using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEvent = UnityEngine.Event;
using UnityInput = UnityEngine.Input;
namespace Zios.Interface{
	using Containers;
	using Events;
	public delegate void ConsoleMethod(string[] values);
	public delegate void ConsoleMethodFull(string[] values,bool help);
	public class ConsoleCallback{
		public Method simple;
		public ConsoleMethod basic;
		public ConsoleMethodFull full;
		public int minimumParameters = -1;
		public string help;
	}
	public struct ConsoleState{
		public const byte closed = 0;
		public const byte closeBegin = 1;
		public const byte closing = 2;
		public const byte open = 3;
		public const byte openBegin = 4;
		public const byte opening = 5;
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
	[InitializeOnLoad]
	public static class ConsoleHook{
		public static Hook<Console> hook;
		static ConsoleHook(){
			if(Application.isPlaying){return;}
			new Hook<Console>();
		}
	}
	[ExecuteInEditMode][AddComponentMenu("Zios/Singleton/Console")]
	public partial class Console : MonoBehaviour{
		public GUISkin skin;
		public Material background;
		public Material inputBackground;
		public Material textArrow;
		public KeyCode triggerKey = KeyCode.F12;
		public float speed = 5.0f;
		public float height = 0.25f;
		public string configFile = "Game.cfg";
		public string logFile = "Log.txt";
		public int logLineSize = 150;
		public int logFontSize = 15;
		public byte logFontColor = 7;
		public bool logFontAllowColors = true;
		public void OnValidate(){
			if(this.skin.IsNull()){
				this.skin = FileManager.GetAsset<GUISkin>("Console.guiskin");
				this.background = FileManager.GetAsset<Material>("ConsoleBackground.mat");
				this.inputBackground = FileManager.GetAsset<Material>("ConsoleInput.mat");
				this.textArrow = FileManager.GetAsset<Material>("ConsoleArrow.mat");
			}
		}
		public void Awake(){
			this.OnValidate();
			Console.instance = this;
			if(!this.logFile.IsEmpty() && !Application.isWebPlayer){
				string logPath = Application.persistentDataPath + "/" + this.logFile;
				try{
					using(StreamWriter file = new StreamWriter(logPath,true)){
						file.WriteLine("-----------------------");
						file.WriteLine(DateTime.Now);
						file.WriteLine("-----------------------");
					}
					Console.logFileUsable = true;
				}
				catch{
					Console.logFileUsable = false;
					Console.AddLog("Log file is not writable. File in use or has bad path.");
				}
			}
		}
		public void OnEnable(){Application.logMessageReceived += Console.HandleLog;}
		public void OnDisable(){Application.logMessageReceived -= Console.HandleLog;}
		public void OnApplicationQuit(){
			Console.SaveCvars();
			if(Console.configOutput.Count > 0){
				Console.SaveBinds();
				Console.SaveConfig();
			}
		}
		public void OnGUI(){
			if(Console.instance.IsNull()){return;}
			Console.Setup();
			Console.CheckTrigger();
			Console.CheckBinds();
			InputState.disabled = Console.status >= 3;
			if(!Console.hidden && Console.status > 0){
				Console.CheckHotkeys();
				Console.CheckDrag();
				Console.DrawElements();
				Console.ManageState();
				Console.ManageInput();
			}
		}
		public void Start(){
			Console.AddCvar("consoleFontColor",this,"logFontColor","Console Font color",Console.help[5]);
			Console.AddCvar("consoleFontSize",this,"logFontSize","Console Font size",Console.help[0]);
			Console.AddCvar("consoleSize",this,"height","Console Height percent",Console.help[1]);
			Console.AddCvar("consoleSpeed",this,"speed","Console Speed",Console.help[2]);
			Console.AddCvar("consoleLineSize",this,"logLineSize","Console Line size",Console.help[6]);
			Console.AddCvar("consoleLogFile",this,"logFile","Console Log name");
			Console.AddCvar("consoleConfigFile",this,"configFile","Console Config name");
			Console.AddKeyword("console",Console.Toggle,0,Console.help[11]);
			Console.AddKeyword("consoleListFonts",Console.ListConsoleFonts,0,Console.help[4]);
			Console.AddKeyword("consoleListColors",Console.ListConsoleColors,0,Console.help[3]);
			Console.AddKeyword("consoleListBinds",Console.ListConsoleBinds,0,Console.help[10]);
			Console.AddKeyword("consoleBind",Console.BindCommand,2,Console.help[7]);
			Console.AddKeyword("consoleToggle",Console.ToggleCommand,2,Console.help[8]);
			Console.AddKeyword("consoleRepeat",Console.RepeatCommand,2,Console.help[9]);
			Console.AddKeyword("consoleResetValue",Console.ResetValue,1,Console.help[12]);
			Console.AddKeyword("consoleResetCvars",Console.ResetCvars,0,Console.help[13]);
			Console.AddKeyword("consoleResetBinds",Console.ResetBinds,0,Console.help[14]);
			Console.AddKeyword("consoleDump",Console.SaveConsoleFile,0,Console.help[15]);
			Console.AddKeyword("consoleClear",Console.ClearConsole,0,Console.help[16]);
			Console.AddKeyword("consoleLoadConfig",Console.LoadConfig,1,Console.help[17]);
			Console.AddShortcut("repeat","consoleRepeat");
			Console.AddShortcut("con","console");
			Console.AddShortcut("reset","consoleResetValue");
			Console.AddShortcut("resetCvars","consoleResetCvars");
			Console.AddShortcut("resetBinds","consoleResetBinds");
			Console.AddShortcut(new string[]{"clear","cls"},"consoleClear");
			Console.AddShortcut(new string[]{"keyToggle","switch"},"consoleToggle");
			Console.AddShortcut(new string[]{"bind","trigger"},"consoleBind");
			Console.AddShortcut(new string[]{"showColors","listColors"},"consoleListColors");
			Console.AddShortcut(new string[]{"showFonts","listFonts"},"consoleListFonts");
			Console.AddShortcut(new string[]{"showBinds","listBinds"},"consoleListBinds");
			Console.LoadBinds();
			Utility.DelayCall(()=>Console.LoadConfig(this.configFile));
		}
	}
	public partial class Console{
		public static Console instance;
		public static FixedList<string> log = new FixedList<string>(256);
		public static FixedList<string> history = new FixedList<string>(256);
		public static Dictionary<string,Bind> binds = new Dictionary<string,Bind>();
		public static Dictionary<string,string> shortcuts = new Dictionary<string,string>();
		public static Dictionary<string,ConsoleCallback> keywords = new Dictionary<string,ConsoleCallback>();
		public static Dictionary<string,Cvar> cvars = new Dictionary<string,Cvar>();
		private static List<string> autocomplete = new List<string>();
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
		private static bool logFileUsable;
		private static bool disableLogging;
		private static bool mouseHeld;
		private static bool hidden;
		private static bool moveCursor;
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
			"^3console :^10 Toggles display of the console.",
			"^3consoleResetValue ^9<^7cvar/bind^9> :^10 Resets the given cvar/bind name to its default state.",
			"^3consoleResetCvars :^10 Resets all console cvars to their default value states.",
			"^3consoleResetBinds :^10 Removes all existing console binds.",
			"^3consoleDump ^9[^7name^9] :^10 Saves the current console log to a text file (optionally named).",
			"^3consoleClear :^10 Removes all current console log history.",
			"^3consoleLoadConfig ^9<^7name^9>:^10 Adds an existing file's contents as console commands."
		};
		//===========================
		// Binds
		//===========================
		public static void AddBind(string key,string action,bool toggle=false,bool repeat=false,float repeatDelay=0){
			if(!Application.isPlaying){return;}
			List<string> keyCodes = new List<string>(Enum.GetNames(typeof(KeyCode)));
			if(!keyCodes.Contains(key)){
				key = Button.GetName(key);
				if(!keyCodes.Contains(key)){
					Debug.LogWarning("[Console] " + key + " could not be bound. It is not a valid key.");
					return;
				}
			}
			Bind data;
			data.key = (KeyCode)Enum.Parse(typeof(KeyCode),key);
			data.name = "bind-"+key;
			if(toggle){data.name = "toggle-"+key;}
			if(repeat){data.name = "repeat-"+key;}
			data.action = action;
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
			Console.AddBind("BackQuote","console");
			if(!Application.isWebPlayer && Console.instance.configFile != ""){return;}
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
			if(Application.isWebPlayer || Console.instance.configFile == ""){
				bindString = bindString.Trim('|') + "|";
				PlayerPrefs.SetString("binds",bindString);
			}
			else{
				Console.configOutput.Add(bindString.Replace("-"," ").Replace("|","\r\n"));
			}
		}
		public static void CheckBinds(){
			if(Console.keyDetection != ""){return;}
			foreach(var item in Console.binds){
				Bind data = item.Value;
				if(Console.status > 0 && !data.action.Contains("console",true)){continue;}
				bool keyDown = Button.EventKeyDown(data.key);
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
					UnityEvent.current.Use();
				}
				data.released = !keyDown;
			}
		}
		//===========================
		// Cvars
		//===========================
		public static void AddCvarMethod(string name,object scope,string dataName,string fullName="",string help="",Method method=null){
			if(!Application.isPlaying){return;}
			Console.AddCvar(name,scope,dataName,fullName,help);
			Console.cvars[name].method.simple = method;
		}
		public static void AddCvarMethod(string name,object scope,string dataName,string fullName="",string help="",ConsoleMethod method=null){
			if(!Application.isPlaying){return;}
			Console.AddCvar(name,scope,dataName,fullName,help);
			Console.cvars[name].method.basic = method;
		}
		public static void AddCvarMethod(string name,object scope,string dataName,string fullName="",string help="",ConsoleMethodFull method=null){
			if(!Application.isPlaying){return;}
			Console.AddCvar(name,scope,dataName,fullName,help);
			Console.cvars[name].method.full = method;
		}
		public static void AddCvar(string name,object scope,string dataName,string fullName,Dictionary<string,string> help,bool formatHelp=true){
			if(!Application.isPlaying){return;}
			if(help.ContainsKey(name)){
				Console.AddCvar(name,scope,dataName,fullName,name + " " + help[name],formatHelp);
			}
		}
		public static void AddCvar(string name,object scope,string dataName,string fullName="",string help="",bool formatHelp=true){
			if(!Application.isPlaying){return;}
			if(Console.cvars.ContainsKey(name)){
				Debug.LogWarning("[Console] Already has registered Cvar for -- " + name);
				return;
			}
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
			if(!Application.isWebPlayer && Console.instance.configFile != ""){return;}
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
				if(Application.isWebPlayer || Console.instance.configFile == ""){
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
		public static void AddKeyword(string name,ConsoleCallback call){
			if(!Application.isPlaying){return;}
			if(Console.keywords.ContainsKey(name)){
				Debug.LogWarning("[Console] Already has registered Keyword for -- " + name);
				return;
			}
			Console.keywords.Add(name,call);
		}
		public static void AddKeyword(string name,Method method=null,int minimumParameters=-1,string help=""){
			if(!Application.isPlaying){return;}
			ConsoleCallback call = new ConsoleCallback();
			call.simple = method;
			call.help = help;
			call.minimumParameters = minimumParameters;
			Console.AddKeyword(name,call);
		}
		public static void AddKeyword(string name,ConsoleMethod method=null,int minimumParameters=-1,string help=""){
			if(!Application.isPlaying){return;}
			ConsoleCallback call = new ConsoleCallback();
			call.basic = method;
			call.help = help;
			call.minimumParameters = minimumParameters;
			Console.AddKeyword(name,call);
		}
		public static void AddKeyword(string name,ConsoleMethodFull method=null,int minimumParameters=-1,string help=""){
			if(!Application.isPlaying){return;}
			ConsoleCallback call = new ConsoleCallback();
			call.full = method;
			call.help = help;
			call.minimumParameters = minimumParameters;
			Console.AddKeyword(name,call);
		}
		public static void AddShortcut(string[] names,string replace){
			if(!Application.isPlaying){return;}
			foreach(string name in names){
				Console.AddShortcut(name,replace);
			}
		}
		public static void AddShortcut(string name,string replace){
			if(!Application.isPlaying){return;}
			if(Console.shortcuts.ContainsKey(name)){
				Debug.LogWarning("[Console] Already has registered Shortcut for -- " + name);
				return;
			}
			Console.shortcuts.Add(name,replace);
		}
		//===========================
		// Config
		//===========================
		public static void SaveConfig(){
			if(!Application.isWebPlayer){
				using(StreamWriter file = new StreamWriter(Console.instance.configFile,false)){
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
						Console.AddCommand(line,true);
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
				if(Console.instance.skin == null){Console.instance.skin = GUI.skin;}
				Console.logStyle = new GUIStyle(Console.instance.skin.textField);
			}
		}
		public static void AddCommand(string text,bool disableLogging=false){
			var usable = Console.disableLogging;
			Console.disableLogging = disableLogging;
			Console.lastCommand = text;
			Console.ManageInput();
			Console.disableLogging = usable;
		}
		public static void AddLog(string text,bool system=false){
			if(Console.disableLogging){return;}
			if(!Application.isPlaying){
				Application.logMessageReceived -= Console.HandleLog;
				Debug.Log(text);
				Application.logMessageReceived += Console.HandleLog;
				if(system){return;}
			}
			if(Console.logFileUsable){
				string logPath = Application.persistentDataPath + "/" + Console.instance.logFile;
				string cleanText = text;
				for(int index=Console.color.Length-1;index>=0;--index){
					cleanText = cleanText.Replace("^"+index,"").Replace("|","");
				}
				using(StreamWriter file = new StreamWriter(logPath,true)){
					file.WriteLine(cleanText);
				}
			}
			float lastPosition = Console.log.Count * Console.logPosition;
			while(text.Length > 0){
				int max = text.Length > Console.instance.logLineSize ? Console.instance.logLineSize : text.Length;
				Console.log.Add(text.Substring(0,max));
				text = text.Substring(max);
			}
			Console.logPosition = !Console.mouseHeld ? 1.0f : lastPosition / Console.log.Count;
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
				var lastPosition = Console.log.Count * Console.logPosition;
				if(Console.autocomplete.Count > 1){
					Console.AddLog("^5>> ^7" + Console.inputText);
					foreach(string name in Console.autocomplete.Distinct()){
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
				Console.moveCursor = true;
				Console.logPosition = !Console.mouseHeld ? 1.0f : lastPosition / Console.log.Count;
				Console.autocomplete.Clear();
			}
		}
		public static void CheckTrigger(){
			if(Button.EventKeyDown(Console.instance.triggerKey)){
				Console.Toggle();
				UnityEvent.current.Use();
			}
		}
		public static void CheckHotkeys(){
			bool control = UnityEvent.current.control;
			bool shift = UnityEvent.current.shift;
			bool alt = UnityEvent.current.alt;
			if(control && alt){
				string keyName = Convert.ToString(UnityEvent.current.keyCode);
				if(Console.keyDetection == ""){Console.keyDetection = "###";}
				if(Console.keyDetection != keyName && !keyName.ContainsAny("Control","Alt","None")){
					if(Console.keyDetection != "" && Console.keyDetection != "###"){
						Console.inputText = Console.inputText.Substring(0,Console.inputText.Length-Console.keyDetection.Length);
					}
					Console.keyDetection = keyName;
					Console.inputText += keyName;
				}
				if(UnityEvent.current.type == EventType.KeyDown){
					UnityEvent.current.Use();
				}
			}
			else{Console.keyDetection = "";}
			if(Button.EventKeyDown(KeyCode.Return)){
				Console.inputText = Console.inputText.TrimStart('\\').TrimStart('/');
				Console.lastCommand = Console.inputText == "" ? " " : Console.inputText;
				if(Console.inputText != ""){
					Console.history.Add(Console.inputText);
					Console.historyIndex = (byte)(Console.history.Count);
				}
				Console.AddLog("^5>> ^7" + Console.inputText);
				Console.inputText = "";
			}
			else if(UnityEvent.current.type == EventType.MouseDown){Console.mouseHeld = true;}
			else if(UnityEvent.current.type == EventType.MouseUp){Console.mouseHeld = false;}
			else if(UnityEvent.current.type == EventType.ScrollWheel){
				if(control){Console.instance.logFontSize -= (int)UnityEvent.current.delta[1];}
				else if(shift){Console.instance.height += Math.Sign(UnityEvent.current.delta[1]) * 0.02f;}
				else{Console.logPosition += (float)(UnityEvent.current.delta[1]) / (float)(Console.log.Count);}
				UnityEvent.current.Use();
			}
			else if(Button.EventKeyDown(KeyCode.PageDown)){Console.logPosition += Console.logScrollLimit * 0.25f;}
			else if(Button.EventKeyDown(KeyCode.PageUp)){Console.logPosition -= Console.logScrollLimit * 0.25f;}
			else if(Button.EventKeyDown(KeyCode.Tab)){Console.CheckAutocomplete();}
			if(Console.history.Count > 0 && (Button.EventKeyDown(KeyCode.UpArrow) || Button.EventKeyDown(KeyCode.DownArrow))){
				if(Button.EventKeyDown(KeyCode.UpArrow) && Console.historyIndex > 0){--Console.historyIndex;}
				if(Button.EventKeyDown(KeyCode.DownArrow)){++Console.historyIndex;}
				Console.historyIndex = (byte)Mathf.Clamp(Console.historyIndex,0,Console.history.Count-1);
				Console.inputText = Console.history[Console.historyIndex];
			}
			Console.instance.logFontSize = Mathf.Clamp(Console.instance.logFontSize,9,128);
		}
		public static void CheckDrag(){
			float consoleHeight = (Screen.height * Console.offset)*Console.instance.height;
			Rect dragBounds = new Rect(0,consoleHeight,Screen.width,30);
			Vector3 mouse = UnityInput.mousePosition;
			mouse[1] = Screen.height - mouse[1];
			if(Console.dragStart != Vector3.zero){
				if(!UnityInput.GetMouseButton(0)){
					Console.dragStart = Vector3.zero;
				}
				else{
					Vector3 changed = mouse - Console.dragStart;
					Console.instance.height = Console.dragStart[2] + (changed.y/Screen.height);
				}
			}
			else if(UnityInput.GetMouseButtonDown(0)){
				if(dragBounds.Contains(mouse)){
					Console.dragStart = mouse;
					Console.dragStart[2] = Console.instance.height;
				}
			}
			Console.instance.height = Mathf.Clamp(Console.instance.height,0.05f,(((float)Screen.height-30.0f)/(float)Screen.height));
		}
		public static void DrawElements(){
			GUI.skin = Console.instance.skin;
			float consoleHeight = (Screen.height * Console.offset)*Console.instance.height;
			byte logLinesShown = 0;
			int logTextOffset = 15 - Console.instance.logFontSize;
			Rect logBounds = new Rect(-10,5,Screen.width-20,18);
			Rect scrollBounds = new Rect(Screen.width-15,0,20,consoleHeight);
			Rect consoleBounds = new Rect(0,0,Screen.width,consoleHeight);
			Rect inputBounds = new Rect(0,consoleHeight,Screen.width,30);
			Rect inputArrowBounds = new Rect(2,consoleHeight+6,12,12);
			Console.logStyle.fontSize = Console.instance.logFontSize;
			if(UnityEvent.current.type == EventType.Repaint){
				var backgroundTexture = Console.instance.background.GetTexture("textureMap");
				var inputTexture = Console.instance.inputBackground.GetTexture("textureMap");
				if(!backgroundTexture.IsNull()){
					backgroundTexture.wrapMode = TextureWrapMode.Repeat;
					backgroundTexture.filterMode = FilterMode.Bilinear;
				}
				if(!inputTexture.IsNull()){
					inputTexture.wrapMode = TextureWrapMode.Repeat;
					inputTexture.filterMode = FilterMode.Bilinear;
				}
				Graphics.DrawTexture(consoleBounds,Texture2D.whiteTexture,Console.instance.background);
				Graphics.DrawTexture(inputBounds,Texture2D.whiteTexture,Console.instance.inputBackground);
				Graphics.DrawTexture(inputArrowBounds,Texture2D.whiteTexture,Console.instance.textArrow);
			}
			if(Console.status == ConsoleState.open && Console.log.Count > 0){
				Console.logStyle.normal.textColor = Console.color[Console.instance.logFontColor];
				float clampedPosition = Mathf.Clamp(Console.logPosition,0,Console.logScrollLimit);
				byte logPosition = (byte)(clampedPosition * (Console.log.Count+1));
				for(byte lineIndex = 0;lineIndex < Console.log.Count;++lineIndex){
					if(logPosition > lineIndex){continue;}
					if(logBounds.y + Console.instance.logFontSize > consoleBounds.yMax - 5){break;}
					string colorCode = "";
					StringBuilder word = new StringBuilder();
					Console.logStyle.normal.textColor = Console.color[Console.instance.logFontColor];
					foreach(char letter in Console.log[lineIndex]){
						if(logBounds.x > Screen.width){break;}
						if(letter == '^'){
							colorCode = "0";
							continue;
						}
						else if(colorCode.Length > 0){
							if(Char.IsNumber(letter)){
								if(Console.instance.logFontAllowColors){colorCode += letter;}
								continue;
							}
							if(Console.instance.logFontAllowColors){
								if(word.Length > 0){
									logBounds.width = Console.logStyle.CalcSize(new GUIContent(word.ToString()))[0];
									GUI.Label(logBounds,word.ToString().Replace("|",""),Console.logStyle);
									logBounds.x += logBounds.width - (15+logTextOffset/2);
									word = new StringBuilder();
								}
								colorCode = colorCode.Length < 2 ? Console.instance.logFontColor.ToString() : colorCode;
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
			if(Console.moveCursor){
				GUIUtility.GetStateObject(typeof(TextEditor),GUIUtility.keyboardControl).As<TextEditor>().MoveTextEnd();
				Console.moveCursor = false;
			}
		}
		public static void ManageState(){
			float slideStep = Console.instance.speed * Time.deltaTime;
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
			var command = Console.lastCommand.Trim('#').Trim();
			if(command == " "){
				Console.logPosition = 1.0f;
				command = "";
			}
			else if(!command.IsEmpty()){
				foreach(var item in Console.shortcuts){
					if(command.IndexOf(item.Key+" ",true) == 0 || command.Matches(item.Key,true)){
						command = item.Value + command.Remove(0,item.Key.Length);
					}
				}
				bool commandFound = false;
				bool wildcard = command.EndsWith("*");
				bool helpMode = command.EndsWith("?");
				command = command.Remove("?","*");
				string firstWord = command.TrySplit(' ',0);
				foreach(var item in Console.keywords){
					string name = item.Key.Trim('#');
					ConsoleCallback method = item.Value;
					if(wildcard || helpMode){
						if(wildcard && method.full != Console.HandleCvar){continue;}
						if(name.StartsWith("!")){continue;}
						if(name.StartsWith(firstWord,true)){
							if(!helpMode){
								if(!method.simple.IsNull()){method.simple();}
								if(!method.basic.IsNull()){method.basic(new string[2]{item.Key,""});}
								if(!method.full.IsNull()){method.full(new string[2]{item.Key,""},false);}
								continue;
							}
							if(!method.help.IsEmpty()){Console.AddLog(method.help);}
							if(!method.full.IsNull()){method.full(new string[2]{item.Key,""},true);}
							commandFound = true;
						}
					}
					else if(name.Matches(firstWord,true)){
						command = command.Replace(name,"",true).Trim();
						List<string> options = new List<string>();
						options.Add(item.Key);
						if(command != ""){
							options.AddRange(command.Split(' '));
						}
						if(method.minimumParameters > options.Count-1 && !method.help.IsEmpty()){
							Console.AddLog(method.help);
						}
						else if(method.simple != null){method.simple();}
						else if(method.basic != null){method.basic(options.ToArray());}
						else if(method.full != null){method.full(options.ToArray(),helpMode);}
						commandFound = true;
						break;
					}
				}
				if(!wildcard && !commandFound){Console.AddLog("^1No command found -- " + command);}
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
		public static void Toggle(){
			if(Console.status != ConsoleState.openBegin  && Console.status != ConsoleState.closeBegin){
				if(Console.status < ConsoleState.open){Console.Open();}
				else{Console.Close();}
			}
		}
		public static void Open(bool immediate=false){
			Console.status = immediate ? ConsoleState.open : ConsoleState.openBegin;
		}
		public static void Close(bool immediate=false){
			Console.status = immediate ? ConsoleState.closed : ConsoleState.closeBegin;
		}
		public static void Hide(){Console.hidden = true;}
		public static void Show(){Console.hidden = false;}
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