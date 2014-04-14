using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
namespace Zios{
	public delegate void ConsoleMethod(string[] values);
	public delegate void ConsoleMethodFull(string[] values,bool help);
	public struct ConsoleState{
		public const byte closed = 0;
		public const byte closeBegin = 1;
		public const byte closing = 2;
		public const byte open = 3;
		public const byte openBegin = 4;
		public const byte opening = 5;
	}
	public class ConsoleCallback{
		public Method simple;
		public ConsoleMethod basic;
		public ConsoleMethodFull full;
		public int minimumParameters = -1;
		public string help;
	}
	public struct Cvar{
		public Accessor value;
		public object defaultValue;
		public object scope;
		public string name;
		public string fullName;
		public string help;
		public ConsoleCallback method;
	}
	public struct Bind{
		public string name;
		public KeyCode key;
		public string action;
		public bool toggle;
		public bool repeat;
		public float repeatDelay;
		public float nextRepeat;
		public bool released;
		public bool toggleActive;
	}
	public static class Console{
		public static ConsoleSettings settings;
		public static FixedList<string> log = new FixedList<string>(256);
		public static FixedList<string> history = new FixedList<string>(256);
		public static Dictionary<string,Bind> binds = new Dictionary<string,Bind>();
		public static Dictionary<string,string> shortcuts = new Dictionary<string,string>();
		public static Dictionary<string,ConsoleCallback> keywords = new Dictionary<string,ConsoleCallback>();
		public static Dictionary<string,Cvar> cvars = new Dictionary<string,Cvar>();
		private static FixedList<string> autocomplete = new FixedList<string>(256);
		private static byte status = 0;
		private static float logScrollLimit = 0;
		private static float logPosition = 0;
		private static float offset = -1.0f;
		private static Color[] color = Colors.numbers;
		private static byte historyIndex = 0;
		private static string inputText = "";
		private static string keyDetection = "";
		private static string lastCommand = "";
		private static List<string> configOutput = new List<string>();
		private static Vector3 dragStart = Vector3.zero;
		private static GUIStyle logStyle;
		private static Dictionary<KeyCode,string> keyValues = new Dictionary<KeyCode,string>(){
			{KeyCode.Keypad0,"0"},
			{KeyCode.Keypad1,"1"},
			{KeyCode.Keypad2,"2"},
			{KeyCode.Keypad3,"3"},
			{KeyCode.Keypad4,"4"},
			{KeyCode.Keypad5,"5"},
			{KeyCode.Keypad6,"6"},
			{KeyCode.Keypad7,"7"},
			{KeyCode.Keypad8,"8"},
			{KeyCode.Keypad9,"9"},
			{KeyCode.KeypadPeriod,"."},
			{KeyCode.KeypadDivide,"/"},
			{KeyCode.KeypadMultiply,"*"},
			{KeyCode.KeypadMinus,"-"},
			{KeyCode.KeypadPlus,"+"},
			{KeyCode.KeypadEquals,"="},
			{KeyCode.Alpha0,"0"},
			{KeyCode.Alpha1,"1"},
			{KeyCode.Alpha2,"2"},
			{KeyCode.Alpha3,"3"},
			{KeyCode.Alpha4,"4"},
			{KeyCode.Alpha5,"5"},
			{KeyCode.Alpha6,"6"},
			{KeyCode.Alpha7,"7"},
			{KeyCode.Alpha8,"8"},
			{KeyCode.Alpha9,"9"},
			{KeyCode.Exclaim,"!"},
			{KeyCode.DoubleQuote,"\""},
			{KeyCode.Hash,"#"},
			{KeyCode.Dollar,"$"},
			{KeyCode.Ampersand,"&"},
			{KeyCode.Quote,"'"},
			{KeyCode.LeftParen,"("},
			{KeyCode.RightParen,")"},
			{KeyCode.Asterisk,"*"},
			{KeyCode.Plus,"+"},
			{KeyCode.Comma,","},
			{KeyCode.Minus,"-"},
			{KeyCode.Period,"."},
			{KeyCode.Slash,"/"},
			{KeyCode.Colon,":"},
			{KeyCode.Semicolon,";"},
			{KeyCode.Less,"<"},
			{KeyCode.Equals,"="},
			{KeyCode.Greater,">"},
			{KeyCode.Question,"?"},
			{KeyCode.At,"@"},
			{KeyCode.LeftBracket,"["},
			{KeyCode.Backslash,"\\"},
			{KeyCode.RightBracket,"]"},
			{KeyCode.Caret,"^"},
			{KeyCode.Underscore,"_"},
			{KeyCode.BackQuote,"`"}
		};
		private static string[] help = new string[]{
			"^3consoleFontSize ^9<^7number^9> :^10 The font size of the log.",
			"^3consoleSize ^9<^7decimal^9> :^10 The height percent that the console is visible.",
			"^3consoleSpeed ^9<^7number^9> :^10 The amount of speed (in pixels) that the console opens with.",
			"^3consoleListColors :^10 Displays all allowed colors with their index.",
			"^3consoleListFonts :^10 Displays all allowed fonts by name and index.",
			"^3consoleFontColor ^9<^7number^9> :^10 The default font color by the chosen color index.",
			"^3consoleLineSize ^9<^7number^9> :^10 The maximum number of characters displayed on a single line before auto-splitting.",
			"^3consoleBind ^9<^7key^9><^7action^9> :^10 Binds a target key to process an action string in the console when pressed.",
			"^3consoleToggle ^9<^7key^9><^7action^9> :^10 Binds a target key to toggle processing an action string in the console when pressed.",
			"^3consoleRepeat ^9<^7key^9><^7action^9> :^10 Binds a target key to continually process an action string in the console while held.",
			"^3consoleListBinds :^10 Displays all bound keys with their relative action.",
			"^3consoleShow :^10 Toggles display of the console.",
			"^3consoleResetValue ^9<^7cvar/bind^9> :^10 Resets the given cvar/bind name to its default state.",
			"^3consoleResetCvars :^10 Resets all console cvars to their default value states.",
			"^3consoleResetBinds :^10 Removes all existing console binds.",
			"^3consoleDump ^9[^7name^9] :^10 Saves the current console log to a text file (optionally named).",
			"^3consoleClear :^10 Removes all current console log history.",
			"^3consoleLoadConfig ^9<^7name^9>:^10 Adds an existing file's contents as console commands."
		};
		//===========================
		// Unity Specific
		//===========================
		public static void Awake(){
			if(Console.settings.allowLogging && !Application.isWebPlayer){
				string logPath = Application.persistentDataPath + "/" + Console.settings.logFile;
				using(StreamWriter file = new StreamWriter(logPath,true)){
					file.WriteLine("-----------------------");
					file.WriteLine(DateTime.Now);
					file.WriteLine("-----------------------");
				}
			}
		}
		public static void OnEnable(){
			Application.RegisterLogCallback(Console.HandleLog);
		}
		public static void OnDisable(){
			Application.RegisterLogCallback(null);
		}
		public static void OnApplicationQuit(){
			Console.SaveCvars();
			if(Console.configOutput.Count > 0){
				Console.SaveBinds();
				Console.SaveConfig();
			}
		}
		public static void OnGUI(){
			Console.Setup();
			Console.CheckTrigger();
			Console.CheckBinds();
			if(Console.status > 0){
				Console.CheckHotkeys();
				Console.CheckDrag();
				Console.DrawElements();
				Console.ManageState();
				Console.ManageInput();
			}
		}
		public static void Start(){
			Console.AddCvar("consoleFontColor",Console.settings,"logFontColor","Console Font color",Console.help[5]);
			Console.AddCvar("consoleFontSize",Console.settings,"logFontSize","Console Font size",Console.help[0]);
			Console.AddCvar("consoleSize",Console.settings,"height","Console Height percent",Console.help[1]);
			Console.AddCvar("consoleSpeed",Console.settings,"speed","Console Speed",Console.help[2]);
			Console.AddCvar("consoleLineSize",Console.settings,"logLineSize","Console Line size",Console.help[6]);
			Console.AddCvar("consoleLogFile",Console.settings,"logFile","Console Log name");
			Console.AddCvar("consoleConigFile",Console.settings,"configFile","Console Config name");
			Console.AddKeyword("consoleListFonts",Console.ListConsoleFonts,0,Console.help[4]);
			Console.AddKeyword("consoleListColors",Console.ListConsoleColors,0,Console.help[3]);
			Console.AddKeyword("consoleListBinds",Console.ListConsoleBinds,0,Console.help[10]);
			Console.AddKeyword("consoleBind",Console.BindCommand,2,Console.help[7]);
			Console.AddKeyword("consoleToggle",Console.ToggleCommand,2,Console.help[8]);
			Console.AddKeyword("consoleRepeat",Console.RepeatCommand,2,Console.help[9]);
			Console.AddKeyword("consoleShow",Console.ShowConsole,0,Console.help[11]);
			Console.AddKeyword("consoleResetValue",Console.ResetValue,1,Console.help[12]);
			Console.AddKeyword("consoleResetCvars",Console.ResetCvars,0,Console.help[13]);
			Console.AddKeyword("consoleResetBinds",Console.ResetBinds,0,Console.help[14]);
			Console.AddKeyword("consoleDump",Console.SaveConsoleFile,0,Console.help[15]);
			Console.AddKeyword("consoleClear",Console.ClearConsole,0,Console.help[16]);
			Console.AddKeyword("consoleLoadConfig",Console.LoadConfig,1,Console.help[17]);
			Console.AddShortcut("repeat","consoleRepeat");
			Console.AddShortcut("showConsole","consoleShow");
			Console.AddShortcut("reset","consoleResetValue");
			Console.AddShortcut("resetCvars","consoleResetCvars");
			Console.AddShortcut("resetBinds","consoleResetBinds");
			Console.AddShortcut(new string[]{"clear","cls"},"consoleClear");
			Console.AddShortcut(new string[]{"toggle","switch"},"consoleToggle");
			Console.AddShortcut(new string[]{"bind","trigger"},"consoleBind");
			Console.AddShortcut(new string[]{"showColors","listColors"},"consoleListColors");
			Console.AddShortcut(new string[]{"showFonts","listFonts"},"consoleListFonts");
			Console.AddShortcut(new string[]{"showBinds","listBinds"},"consoleListBinds");
			Console.LoadBinds();
			Console.LoadConfig(Console.settings.configFile);
		}
		//===========================
		// Binds
		//===========================
		public static void AddBind(string key,string action,bool toggle=false,bool repeat=false,float repeatDelay=0){
			List<string> keyCodes = new List<string>(Enum.GetNames(typeof(KeyCode)));
			if(!keyCodes.Contains(key)){
				key = Console.keyValues.ContainsValue(key) ? Console.keyValues.GetKey(key) : key;
				if(!keyCodes.Contains(key)){ 
					Debug.LogWarning(key + " could not be bound. It is not a valid key.");
					return;
				}
			}
			Bind data;
			data.key = (KeyCode)Enum.Parse(typeof(KeyCode),key);
			data.name = "bind-"+key;
			if(toggle){data.name = "toggle-"+key;}
			if(repeat){data.name = "repeat-"+key;}
			data.action = action.Replace("showconsole","consoleshow",true);
			data.toggle = toggle;
			data.repeat = repeat;
			data.repeatDelay = repeatDelay;
			data.nextRepeat = Time.time;
			data.released = true;
			data.toggleActive = false;
			if(Console.binds.ContainsKey(key,true)){
				Console.binds.Remove(key);
			}
			Console.binds.Add(key,data);
		}
		public static void LoadBinds(){
			Console.AddBind("BackQuote","showConsole");
			if(!Application.isWebPlayer && Console.settings.configFile != ""){return;}
			if(!PlayerPrefs.HasKey("binds")){
				PlayerPrefs.SetString("binds","|");
			}
			string binds = PlayerPrefs.GetString("binds");
			string[] bindList = binds.Split('|');
			foreach(string item in bindList){
				string[] dataList = item.Split('-');
				if(dataList.Length < 3){continue;}
				string key = dataList[1];
				string action = dataList[2];
				bool toggle = dataList[0] == "toggle";
				bool repeat = dataList[0] == "repeat";
				float repeatDelay = dataList.Length > 3 ? Convert.ToSingle(dataList[3]) : 0;
				Console.AddBind(key,action,toggle,repeat,repeatDelay);
			}
		}
		public static void ResetBinds(string[] values){
			Console.binds.Clear();
			PlayerPrefs.DeleteKey("binds");
			Console.AddLog("^10All stored ^3binds^10 have been cleared.");
		}
		public static void BindCommand(string[] values){		
			string data = string.Join(" ",values.Skip(2).ToArray()).Trim();
			Console.AddBind(values[1],data);
		}
		public static void ToggleCommand(string[] values){
			string data = string.Join(" ",values.Skip(2).ToArray()).Trim();
			Console.AddBind(values[1],data,true);
		}
		public static void RepeatCommand(string[] values){
			float repeatDelay = values.Length > 2 ? Convert.ToSingle(values.Last()) : 0;
			string data = string.Join(" ",values.Skip(2).Take(values.Length-2).ToArray()).Trim();
			Console.AddBind(values[1],data,false,true,repeatDelay);
		}
		public static void ListConsoleBinds(string[] values){
			foreach(var item in Console.binds){
				Bind data = item.Value;
				string type = data.toggle ? "toggle" : "bind";
				type = data.repeat ? "repeat (" + data.repeatDelay + "s)" : type;
				Console.AddLog("  ^17" + type + "^8| " + data.key + " ^7|" + data.action);
			}
		}
		public static void SaveBinds(){
			string bindString = "";
			foreach(var item in Console.binds){
				Bind data = item.Value;
				bindString += data.name + "-" + data.action;
				if(data.repeat){bindString += "-" + data.repeatDelay;}
				bindString += "|";
			}
			if(Application.isWebPlayer || Console.settings.configFile == ""){
				bindString = bindString.Trim('|') + "|";
				PlayerPrefs.SetString("binds",bindString);
			}
			else{
				Console.configOutput.Add(bindString.Replace("-"," ").Replace("|","\r\n"));
			}
		}
		public static void CheckBinds(){
			KeyShortcut CheckKeyDown = Button.CheckEventKeyDown;
			if(Console.keyDetection != ""){return;}
			foreach(var item in Console.binds){
				Bind data = item.Value;
				if(Console.status > 0 && !data.action.Contains("consoleShow",true)){continue;}
				bool keyDown = CheckKeyDown(data.key);
				if(keyDown && data.repeat && data.nextRepeat > Time.time){
					Console.AddCommand(data.action);
					data.nextRepeat += Time.time + data.repeatDelay;
				}
				else if(data.toggle){
					if(data.toggleActive){
						Console.AddCommand(data.action);
					}
					if(keyDown && data.released){
						data.toggleActive = !data.toggleActive;
					}
				}
				else if(keyDown && data.released){
					Console.AddCommand(data.action);
				}
				if(Console.lastCommand.Matches(data.action,true)){
					Event.current.Use();
				}
				data.released = !keyDown;
			}
		}
		//===========================
		// Cvars
		//===========================
		public static void AddCvarMethod(string name,object scope,string dataName,string fullName="",string help="",Method method=null){
			Console.AddCvar(name,scope,dataName,fullName,help);
			Console.cvars[name].method.simple = method;
		}
		public static void AddCvarMethod(string name,object scope,string dataName,string fullName="",string help="",ConsoleMethod method=null){
			Console.AddCvar(name,scope,dataName,fullName,help);
			Console.cvars[name].method.basic = method;
		}
		public static void AddCvarMethod(string name,object scope,string dataName,string fullName="",string help="",ConsoleMethodFull method=null){
			Console.AddCvar(name,scope,dataName,fullName,help);
			Console.cvars[name].method.full = method;
		}
		public static void AddCvar(string name,object scope,string dataName,string fullName,Dictionary<string,string> help,bool formatHelp=true){
			if(help.ContainsKey(name)){
				Console.AddCvar(name,scope,dataName,fullName,name + " " + help[name],formatHelp);
			}
		}
		public static void AddCvar(string name,object scope,string dataName,string fullName="",string help="",bool formatHelp=true){
			if(fullName == ""){fullName = scope.GetType().ToString() + "^1 " + dataName;}
			int index = -1;
			if(dataName.Contains("[")){
				string[] words = dataName.Split('[');
				dataName = words[0];
				index = Convert.ToInt16(words[1].Replace("]",""));
			}
			Cvar data;
			data.scope = scope;
			data.name = dataName;
			data.help = help;
			data.fullName = fullName;
			data.method = new ConsoleCallback();
			data.value = new Accessor(scope,dataName,index);
			data.defaultValue = data.value.Get().ToString();
			Console.cvars.Add(name,data);
			Console.LoadCvar(data);
			Console.AddKeyword(name,Console.HandleCvar);
		}
		public static void LoadCvar(Cvar data){
			if(!Application.isWebPlayer && Console.settings.configFile != ""){return;}
			if(PlayerPrefs.HasKey(data.fullName)){
				object value = data.value.Get();
				Type type = data.value.type;
				if(type == typeof(float)){value = PlayerPrefs.GetFloat(data.fullName);}
				else if(type == typeof(string)){value = PlayerPrefs.GetString(data.fullName);}
				else if(type == typeof(int) || type == typeof(byte)){value = PlayerPrefs.GetInt(data.fullName);}
				else if(type == typeof(bool)){value = PlayerPrefs.GetInt(data.fullName) == 1 ? true : false;}
				data.value.Set(value);
			}
		}
		public static void ResetCvars(string[] values){
			string binds = PlayerPrefs.GetString("binds");
			PlayerPrefs.DeleteAll();
			PlayerPrefs.SetString("binds",binds);
			foreach(var item in Console.cvars){
				Cvar data = item.Value;
				data.value.Set(data.defaultValue);
			}
			Console.AddLog("^10All stored ^3cvars^10 have been reset to default values.");
		}
		public static void SaveCvars(){
			foreach(var item in Console.cvars){
				if(item.Key.StartsWith("!")){continue;}
				if(item.Key.StartsWith("#")){continue;}
				Cvar data = item.Value;
				object current = data.value.Get();
				if(Application.isWebPlayer || Console.settings.configFile == ""){
					Type type = current.GetType();
					if(type == typeof(float)){PlayerPrefs.SetFloat(data.fullName,(float)current);}
					else if(type == typeof(string)){PlayerPrefs.SetString(data.fullName,(string)current);}
					else if(type == typeof(int)){PlayerPrefs.SetInt(data.fullName,(int)current);}
					else if(type == typeof(byte)){PlayerPrefs.SetInt(data.fullName,Convert.ToInt16(current));}
					else if(type == typeof(bool)){PlayerPrefs.SetInt(data.fullName,(bool)current == true ? 1 : 0);}
				}
				else{
					Console.configOutput.Add(item.Key + " " + current);
				}
			}
		}
		public static void HandleCvar(string[] values,bool help){
			Cvar data = Console.cvars[values[0]];
			string defaultText = "";
			if(help){
				if(data.help != ""){Console.AddLog(data.help);}
				return;
			}
			if(values.Length > 1 && values[1] != ""){
				data.value.Set(values[1]);
			}
			if(data.value.Get().ToString() != data.defaultValue.ToString()){
				string value = data.defaultValue.ToString() != "" ? data.defaultValue.ToString() : "empty";
				defaultText = "^7 (default : ^8|" + value + "^7)";
			}
			if(data.method.simple != null){data.method.simple();}
			if(data.method.basic != null){data.method.basic(values);}
			if(data.method.full != null){data.method.full(values,false);}
			Console.AddLog("^10" + data.fullName + "^10 is :^3 " + data.value.Get() + defaultText);
		}
		//===========================
		// Keywords/Shortcuts
		//===========================
		public static void AddKeyword(string name,Method method=null,int minimumParameters=-1,string help=""){
			ConsoleCallback call = new ConsoleCallback();
			call.simple = method;
			call.help = help;
			call.minimumParameters = minimumParameters;
			Console.keywords.Add(name,call);
		}
		public static void AddKeyword(string name,ConsoleMethod method=null,int minimumParameters=-1,string help=""){
			ConsoleCallback call = new ConsoleCallback();
			call.basic = method;
			call.help = help;
			call.minimumParameters = minimumParameters;
			Console.keywords.Add(name,call);
		}
		public static void AddKeyword(string name,ConsoleMethodFull method=null,int minimumParameters=-1,string help=""){
			ConsoleCallback call = new ConsoleCallback();
			call.full = method;
			call.help = help;
			call.minimumParameters = minimumParameters;
			Console.keywords.Add(name,call);
		}
		public static void AddShortcut(string[] names,string replace){
			foreach(string name in names){
				Console.shortcuts.Add(name,replace);
			}
		}
		public static void AddShortcut(string name,string replace){
			Console.shortcuts.Add(name,replace);
		}
		//===========================
		// Config
		//===========================
		public static void SaveConfig(){
			if(!Application.isWebPlayer){
				using(StreamWriter file = new StreamWriter(Console.settings.configFile,false)){
					foreach(string line in Console.configOutput){
						file.WriteLine(line);
					}
				}
			}
		}
		public static void LoadConfig(string name){
			if(name != "" && !Application.isWebPlayer && File.Exists(name)){
				using(StreamReader file = new StreamReader(name)){
					string line = "";
					while((line = file.ReadLine()) != null){
						Console.AddCommand(line,false);
					}
				}
			}
		}
		public static void LoadConfig(string[] values){
			Console.LoadConfig(values[1]);
		}
		public static void DeleteConfig(string name){
			if(name != "" && File.Exists(name)){
				File.Delete(name);
			}
		}
		//===========================
		// Internal
		//===========================
		public static void Setup(){
			if(Console.logStyle == null){
				if(Console.settings.skin == null){Console.settings.skin = GUI.skin;}
				Console.logStyle = new GUIStyle(Console.settings.skin.textField);
			}
		}
		public static void AddCommand(string text,bool allowLogging=true){
			Console.settings.allowLogging = allowLogging;
			Console.lastCommand = text;
			Console.ManageInput();
			Console.settings.allowLogging = true;
		}
		public static void AddLog(string text,bool system=false){
			if(!Console.settings.allowLogging){return;}
			if(Application.isEditor){
				Application.RegisterLogCallback(null);
				Debug.Log(text);
				Application.RegisterLogCallback(Console.HandleLog);
				if(system){return;}
			}
			if(Console.settings.allowLogging && !Application.isWebPlayer){
				string logPath = Application.persistentDataPath + "/" + Console.settings.logFile;
				string cleanText = text;
				for(int index=Console.color.Length-1;index>=0;--index){
					cleanText = cleanText.Replace("^"+index,"").Replace("|","");	
				}
				using(StreamWriter file = new StreamWriter(logPath,true)){
					file.WriteLine(cleanText);
				}
			}
			while(text.Length > 0){
				int max = text.Length > Console.settings.logLineSize ? Console.settings.logLineSize : text.Length;
				Console.log.Add(text.Substring(0,max));
				text = text.Substring(max);
			}
		}
		public static void HandleLog(string text,string trace,LogType type){
			if(type == LogType.Error){text = "^1" + text + "^10 -- ^7" + trace;}
			if(type == LogType.Assert){text = "^1" + text + "^10 -- ^7" + trace;}
			if(type == LogType.Warning){text = "^8" + text + "^10 -- ^7" + trace;}
			if(type == LogType.Exception){text = "^6" + text + "^10 -- ^7" + trace;}
			if(type == LogType.Log){}
			if(text.StartsWith("@")){
				Console.AddCommand(text.Replace("@",""));
			}
			else{
				text = text.Replace("*"," ");
				text = text.Replace("?"," ");
				text = text.Replace("\n"," ");
				Console.AddLog(text,true);
			}
		}
		public static void CheckAutocomplete(){
			foreach(var item in Console.keywords){
				string name = item.Key.Trim('#');
				if(name.StartsWith("!")){continue;}
				if(name.StartsWith(Console.inputText,true)){
					Console.autocomplete.Add(name);
				}
			}
			foreach(var item in Console.shortcuts){
				string name = item.Key.Trim('#');
				if(name.StartsWith("!")){continue;}
				if(name.StartsWith(Console.inputText,true)){
					Console.autocomplete.Add(item.Value);
				}
			}
			if(Console.autocomplete.Count > 0){
				if(Console.autocomplete.Count > 1){
					Console.AddLog("^5>> ^7" + Console.inputText);
					foreach(string name in Console.autocomplete){
						string type = Console.cvars.ContainsKey(name,true) ? "^8 " : "^17 ";
						string shortcuts = "^9";
						foreach(var data in Console.shortcuts){
							if(data.Value.Matches(name,true)){shortcuts += " / " + data.Key;}
						}
						Console.AddLog("\t" + type + name + shortcuts);
					}
				}
				else{
					Console.inputText = Console.autocomplete[0];
				}
				Console.logPosition = 1.0f;
				Console.autocomplete.Clear();
			}
		}
		public static void CheckTrigger(){
			KeyShortcut CheckKeyDown = Button.CheckEventKeyDown;
			if(CheckKeyDown(Console.settings.triggerKey)){
				Console.ShowConsole();
				Event.current.Use();
			}
		}
		public static void CheckHotkeys(){
			bool control = Event.current.control;
			bool shift = Event.current.shift;
			bool alt = Event.current.alt;
			KeyShortcut CheckKeyDown = Button.CheckEventKeyDown;
			if(control && alt){
				string keyName = Convert.ToString(Event.current.keyCode);
				if(Console.keyDetection == ""){Console.keyDetection = "###";}
				if(Console.keyDetection != keyName && !keyName.ContainsAny("Control","Alt","None")){
					if(Console.keyDetection != "" && Console.keyDetection != "###"){
						Console.inputText = Console.inputText.Substring(0,Console.inputText.Length-Console.keyDetection.Length);
					}
					Console.keyDetection = keyName;
					Console.inputText += keyName;
				}
				if(Event.current.type == EventType.KeyDown){
					Event.current.Use();
				}
			}
			else{Console.keyDetection = "";}
			if(CheckKeyDown(KeyCode.Return)){
				Console.inputText = Console.inputText.TrimStart('\\').TrimStart('/');
				Console.lastCommand = Console.inputText == "" ? " " : Console.inputText;
				if(Console.inputText != ""){
					Console.history.Add(Console.inputText);
					Console.historyIndex = (byte)(Console.history.Count);
				}
				Console.AddLog("^5>> ^7" + Console.inputText);
				Console.inputText = "";
			}
			else if(Event.current.type == EventType.ScrollWheel){
				if(control){Console.settings.logFontSize -= (int)Event.current.delta[1];}
				else if(shift){Console.settings.height += Math.Sign(Event.current.delta[1]) * 0.02f;}
				else{Console.logPosition += (float)(Event.current.delta[1]) / (float)(Console.log.Count);}
			}
			else if(CheckKeyDown(KeyCode.PageDown)){Console.logPosition += Console.logScrollLimit * 0.25f;}
			else if(CheckKeyDown(KeyCode.PageUp)){Console.logPosition -= Console.logScrollLimit * 0.25f;}
			else if(CheckKeyDown(KeyCode.Tab)){Console.CheckAutocomplete();}
			if(Console.history.Count > 0 && (CheckKeyDown(KeyCode.UpArrow) || CheckKeyDown(KeyCode.DownArrow))){
				if(CheckKeyDown(KeyCode.UpArrow) && Console.historyIndex > 0){--Console.historyIndex;}
				if(CheckKeyDown(KeyCode.DownArrow)){++Console.historyIndex;}
				Console.historyIndex = (byte)Mathf.Clamp(Console.historyIndex,0,Console.history.Count-1);
				Console.inputText = Console.history[Console.historyIndex];
			}
			Console.settings.logFontSize = Mathf.Clamp(Console.settings.logFontSize,9,128);
		}
		public static void CheckDrag(){
			float consoleHeight = (Screen.height * Console.offset)*Console.settings.height;
			Rect dragBounds = new Rect(0,consoleHeight+Console.settings.inputBackground.height-9,Screen.width,9);
			Vector3 mouse = Input.mousePosition;
			mouse[1] = Screen.height - mouse[1];
			if(Console.dragStart != Vector3.zero){
				if(!Input.GetMouseButton(0)){
					Console.dragStart = Vector3.zero;
				}
				else{
					Vector3 changed = mouse - Console.dragStart;
					Console.settings.height = Console.dragStart[2] + (changed.y/Screen.height);
				}
			}
			else if(Input.GetMouseButtonDown(0)){
				if(dragBounds.ContainsPoint(mouse)){
					Console.dragStart = mouse;
					Console.dragStart[2] = Console.settings.height;
				}
			}
			Console.settings.height = Mathf.Clamp(Console.settings.height,0.05f,(((float)Screen.height-30.0f)/(float)Screen.height));	
		}
		public static void DrawElements(){
			GUI.skin = Console.settings.skin;
			float consoleHeight = (Screen.height * Console.offset)*Console.settings.height;
			float alternate = Time.time % 1.5f;
			byte logLinesShown = 0;
			int logTextOffset = 15 - Console.settings.logFontSize;
			Rect logBounds = new Rect(-10,5,Screen.width-20,18);
			Rect scrollBounds = new Rect(Screen.width-15,0,20,consoleHeight);
			Rect consoleBounds = new Rect(0,0,Screen.width,consoleHeight);
			Rect inputBounds = new Rect(0,consoleHeight,Screen.width,30);
			Rect inputArrowBounds = new Rect(2,consoleHeight+6,12,12);
			Rect tiling = new Rect(alternate,alternate,Screen.width/Console.settings.background.width,consoleHeight/Console.settings.background.height);
			Rect tilingInput = new Rect(0,0,Screen.width/Console.settings.inputBackground.width,1);
			Console.logStyle.fontSize = Console.settings.logFontSize;
			GUI.DrawTextureWithTexCoords(consoleBounds,Console.settings.background,tiling);
			GUI.DrawTextureWithTexCoords(inputBounds,Console.settings.inputBackground,tilingInput);
			GUI.DrawTexture(inputArrowBounds,Console.settings.textArrow);
			if(Console.status == ConsoleState.open && Console.log.Count > 0){
				Console.logStyle.normal.textColor = Console.color[Console.settings.logFontColor];
				float clampedPosition = Mathf.Clamp(Console.logPosition,0,Console.logScrollLimit);
				byte logPosition = (byte)(clampedPosition * (Console.log.Count+1));
				for(byte lineIndex = 0;lineIndex < Console.log.Count;++lineIndex){ 
					if(logPosition > lineIndex){continue;}
					if(logBounds.y + Console.settings.logFontSize > consoleBounds.yMax - 5){break;}
					string colorCode = "";
					StringBuilder word = new StringBuilder();
					Console.logStyle.normal.textColor = Console.color[Console.settings.logFontColor];
					foreach(char letter in Console.log[lineIndex]){
						if(logBounds.x > Screen.width){break;}
						if(letter == '^'){
							colorCode = "0";
							continue;
						}
						else if(colorCode.Length > 0){
							if(Char.IsNumber(letter)){
								if(Console.settings.logFontAllowColors){colorCode += letter;}
								continue;
							}
							if(Console.settings.logFontAllowColors){
								if(word.Length > 0){
									logBounds.width = Console.logStyle.CalcSize(new GUIContent(word.ToString()))[0];
									GUI.Label(logBounds,word.ToString().Replace("|",""),Console.logStyle);
									logBounds.x += logBounds.width - (15+logTextOffset/2);
									word = new StringBuilder();
								}
								colorCode = colorCode.Length < 2 ? Console.settings.logFontColor.ToString() : colorCode;
								if(colorCode.IsInt()){
									int colorIndex = Mathf.Clamp(Convert.ToInt32(colorCode),0,Console.color.Length-1);
									Console.logStyle.normal.textColor = Console.color[colorIndex];
								}
							}
							colorCode = "";
						}
						word.Append(letter);
					}
					if(word.Length > 0){
						logBounds.width = Console.logStyle.CalcSize(new GUIContent(word.ToString()))[0];
						GUI.Label(logBounds,word.ToString().Replace("|",""),Console.logStyle);
					}
					logBounds.x = -10;
					logBounds.y += 17-(logTextOffset/2);
					++logLinesShown;
				}
				Console.logScrollLimit = 1.0f - ((float)(logLinesShown) / (float)(Console.log.Count));
				if(logScrollLimit > 0.0f){
					Console.logPosition = GUI.VerticalScrollbar(scrollBounds,Console.logPosition,1.0f-Console.logScrollLimit,0.0f,1.0f);
				}
			}
			GUI.SetNextControlName("inputText");
			Console.inputText = GUI.TextField(inputBounds,Console.inputText);
		}
		public static void ManageState(){
			float slideStep = Console.settings.speed * Time.deltaTime;
			if(Console.status == ConsoleState.open && Console.keyDetection == ""){
				GUI.FocusControl("inputText");
			}
			else if(Console.keyDetection == ""){
				GUI.SetNextControlName("");
				GUI.FocusControl("");
			}
			if(Console.status == ConsoleState.closeBegin){
				Console.status = ConsoleState.closing;
			}
			else if(Console.status == ConsoleState.closing){
				if(Console.offset > -1.0){Console.offset -= slideStep;}
				else{Console.status = ConsoleState.closed;}
			}
			else if(Console.status == ConsoleState.openBegin){
				Console.status = ConsoleState.opening;
			}
			else if(Console.status == ConsoleState.opening){
				if(Console.offset < 1.0){Console.offset += slideStep;}
				else{Console.status = ConsoleState.open;}
			}
			Console.offset = Mathf.Clamp(Console.offset,-1.0f,1.0f);
		}
		public static void ManageInput(){
			Console.lastCommand = Console.lastCommand.Trim('#').Trim();
			if(Console.lastCommand == " "){
				Console.logPosition = 1.0f;
				Console.lastCommand = "";
			}
			else if(Console.lastCommand != ""){
				foreach(var item in Console.shortcuts){
					if(Console.lastCommand.IndexOf(item.Key+" ",true) == 0 || Console.lastCommand.Matches(item.Key,true)){
						Console.lastCommand = item.Value + Console.lastCommand.Remove(0,item.Key.Length);
					}
				}
				int endCheck = Console.lastCommand.Length > 1 ? 2 : 1;
				string lineEnd = Console.lastCommand.Substring(Console.lastCommand.Length-endCheck);
				bool commandFound = false;
				bool wildcard = lineEnd.Contains("*");
				bool helpMode = lineEnd.Contains("?");
				if(helpMode){Console.lastCommand = Console.lastCommand.Replace("?","");}
				if(wildcard){Console.lastCommand = Console.lastCommand.Replace("*","");}
				string firstWord = Console.lastCommand.Split(' ')[0];
				foreach(var item in Console.keywords){
					string name = item.Key.Trim('#');
					ConsoleCallback method = item.Value;
					if(wildcard || helpMode){
						if(wildcard && method.full != Console.HandleCvar){continue;}
						if(name.StartsWith("!")){continue;}
						if(name.StartsWith(firstWord,true)){
							if(method.minimumParameters > 0 && method.help != ""){
								Console.AddLog(method.help);
							}
							else if(method.simple != null){method.simple();}
							else if(method.basic != null){method.basic(new string[2]{item.Key,""});}
							else if(method.full != null){method.full(new string[2]{item.Key,""},helpMode);}
							commandFound = true;
						}
					}
					else if(firstWord.StartsWith(name,true)){
						Console.lastCommand = Console.lastCommand.Replace(name,"",true).Trim();
						List<string> options = new List<string>();
						options.Add(item.Key);
						if(Console.lastCommand != ""){
							options.AddRange(Console.lastCommand.Split(' '));
						}
						if(method.minimumParameters > options.Count-1 && method.help != ""){
							Console.AddLog(method.help);
						}
						else if(method.simple != null){method.simple();}
						else if(method.basic != null){method.basic(options.ToArray());}
						else if(method.full != null){method.full(options.ToArray(),helpMode);}
						commandFound = true;
						break;
					}
				}
				if(!commandFound){Console.AddLog("^1No command found -- " + Console.lastCommand);}
				Console.logPosition = 1.0f;
				Console.lastCommand = "";
			}
		}
		//===========================
		// Commands
		//===========================
		public static void ResetValue(string[] values){
			string name = values[1];
			foreach(var data in Console.shortcuts){
				if(data.Key.Matches(name,true)){
					name = data.Value;
				}
			}
			if(Console.binds.ContainsKey(name,true)){
				Console.binds.Remove(name);
				Console.AddLog("^3|" + name + "^10 has been unbound as a key.");
			}
			else if(Console.cvars.ContainsKey(name,true)){
				Cvar data = Console.cvars[name];
				data.value.Set(data.defaultValue);
				Console.AddLog("^3|" + data.name + "^10 has been reset to its default value ^7-- ^8|" + data.defaultValue);
			}
			else{
				Console.AddLog("^3|" + name + "^10 is not a valid cvar or key bind.");
			}
		}
		public static void SaveConsoleFile(string[] values){
			string fileName = values.Length > 1 ? values[1] : "ConsoleDump.txt";
			string path = "";
			if(Application.isWebPlayer){
				Console.AddLog("^3Console log dumping not supported in web player.");
				return;
			}
			if(fileName == ""){return;}
			using(StreamWriter file = new StreamWriter(fileName,true)){
				foreach(string line in Console.log){
					file.WriteLine(line);
				}
				path = ((FileStream)(file.BaseStream)).Name;
			}
			Console.AddLog("^3Console Dump Saved ^7-- " + path);
		}
		public static void ClearConsole(){
			Console.log = new FixedList<string>(256);
			Console.logPosition = 0;
			Console.logScrollLimit = 0;
		}
		public static void ShowConsole(){
			if(Console.status != ConsoleState.openBegin  && Console.status != ConsoleState.closeBegin){
				Console.status = Console.status < ConsoleState.open ? ConsoleState.openBegin : ConsoleState.closeBegin;
			}
		}
		public static void ListConsoleColors(){
			string listing = "^10Console Font colors available : ";
			for(byte index = 0;index < Console.color.Length;++index){
				listing += " ^" + index + " " + index;
			}
			Console.AddLog(listing);
		}
		public static void ListConsoleFonts(){
			/*Font[] fonts = FindObjectsOfType(Font) as Font[];
			foreach(Font font in fonts){
			Console.AddLog(" ^7 " + font.name);
			}*/
		}
	}
}