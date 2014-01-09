using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
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
[AddComponentMenu("Zios/Singleton/Console")]
public class Console : MonoBehaviour{
	public GUISkin skin;
	public Texture2D background;
	public Texture2D inputBackground;
	public Texture2D textArrow;
	public KeyCode triggerKey = KeyCode.F12;
	public float speed = 5.0f;
	public float height = 0.25f;
	public string configFile = "Game.cfg";
	public string logFile = "Log.txt";
	public int logLineSize = 150;
	public int logFontSize = 15;
	public bool logFontAllowColors = true;
	public byte logFontColor = 7;
	public bool allowLogging = true;
	public FixedList<string> log = new FixedList<string>(256);
	public FixedList<string> history = new FixedList<string>(256);
	public Dictionary<string,Bind> binds = new Dictionary<string,Bind>();
	public Dictionary<string,string> shortcuts = new Dictionary<string,string>();
	public Dictionary<string,ConsoleCallback> keywords = new Dictionary<string,ConsoleCallback>();
	public Dictionary<string,Cvar> cvars = new Dictionary<string,Cvar>();
	private FixedList<string> autocomplete = new FixedList<string>(256);
	private byte status = 0;
	private float logScrollLimit = 0;
	private float logPosition = 0;
	private float offset = -1.0f;
	private Color[] color = Colors.numbers;
	private byte historyIndex = 0;
	private string inputText = "";
	private string keyDetection = "";
	private string lastCommand = "";
	private List<string> configOutput = new List<string>();
	private Vector3 dragStart = Vector3.zero;
	private GUIStyle logStyle;
	private Dictionary<KeyCode,string> keyValues = new Dictionary<KeyCode,string>(){
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
	private string[] help = new string[]{
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
	public void Awake(){
		Global.Console = this;
		DontDestroyOnLoad(this.transform.gameObject);
		if(this.logFile != "" && !Application.isWebPlayer){
			using(StreamWriter file = new StreamWriter(this.logFile,true)){
			    file.WriteLine("-----------------------");
			    file.WriteLine(DateTime.Now);
			    file.WriteLine("-----------------------");
			}
		}
	}
	public void OnEnable(){
		Application.RegisterLogCallback(this.HandleLog);
	}
	public void onDisable(){
		Application.RegisterLogCallback(null);
	}
	public void OnApplicationQuit(){
		this.SaveCvars();
		if(this.configOutput.Count > 0){
			this.SaveBinds();
			this.SaveConfig();
		}
	}
	public void OnGUI(){
		if(Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout){
			Debug.Log(Event.current);
		}
		this.Setup();
		this.CheckTrigger();
		this.CheckBinds();
		if(this.status > 0){
			this.CheckHotkeys();
			this.CheckDrag();
			this.DrawElements();
			this.ManageState();
			this.ManageInput();
		}
	}
	public void Setup(){
		if(this.logStyle == null){
			if(this.skin == null){this.skin = GUI.skin;}
			this.logStyle = new GUIStyle(this.skin.textField);
		}
	}
	public void Start(){
		this.AddCvar("consoleFontColor",this,"logFontColor","Console Font color",this.help[5]);
		this.AddCvar("consoleFontSize",this,"logFontSize","Console Font size",this.help[0]);
		this.AddCvar("consoleSize",this,"height","Console Height percent",this.help[1]);
		this.AddCvar("consoleSpeed",this,"speed","Console Speed",this.help[2]);
		this.AddCvar("consoleFLineSize",this,"logLineSize","Console Line size",this.help[6]);
		this.AddCvar("consoleLogFile",this,"logFile","Console Log name");
		this.AddCvar("consoleConigFile",this,"configFile","Console Config name");
		this.AddKeyword("consoleListFonts",this.ListConsoleFonts,0,this.help[4]);
		this.AddKeyword("consoleListColors",this.ListConsoleColors,0,this.help[3]);
		this.AddKeyword("consoleListBinds",this.ListConsoleBinds,0,this.help[10]);
		this.AddKeyword("consoleBind",this.BindCommand,2,this.help[7]);
		this.AddKeyword("consoleToggle",this.ToggleCommand,2,this.help[8]);
		this.AddKeyword("consoleRepeat",this.RepeatCommand,2,this.help[9]);
		this.AddKeyword("consoleShow",this.ShowConsole,0,this.help[11]);
		this.AddKeyword("consoleResetValue",this.ResetValue,1,this.help[12]);
		this.AddKeyword("consoleResetCvars",this.ResetCvars,0,this.help[13]);
		this.AddKeyword("consoleResetBinds",this.ResetBinds,0,this.help[14]);
		this.AddKeyword("consoleDump",this.SaveConsoleFile,0,this.help[15]);
		this.AddKeyword("consoleClear",this.ClearConsole,0,this.help[16]);
		this.AddKeyword("consoleLoadConfig",this.LoadConfig,1,this.help[17]);
		this.AddShortcut("repeat","consoleRepeat");
		this.AddShortcut("showConsole","consoleShow");
		this.AddShortcut("reset","consoleResetValue");
		this.AddShortcut("resetCvars","consoleResetCvars");
		this.AddShortcut("resetBinds","consoleResetBinds");
		this.AddShortcut(new string[]{"clear","cls"},"consoleClear");
		this.AddShortcut(new string[]{"toggle","switch"},"consoleToggle");
		this.AddShortcut(new string[]{"bind","trigger"},"consoleBind");
		this.AddShortcut(new string[]{"showColors","listColors"},"consoleListColors");
		this.AddShortcut(new string[]{"showFonts","listFonts"},"consoleListFonts");
		this.AddShortcut(new string[]{"showBinds","listBinds"},"consoleListBinds");
		this.LoadBinds();
		this.LoadConfig(this.configFile);
	}
	//===========================
	// Binds
	//===========================
	public void AddBind(string key,string action,bool toggle=false,bool repeat=false,float repeatDelay=0){
		List<string> keyCodes = new List<string>(Enum.GetNames(typeof(KeyCode)));
		if(!keyCodes.Contains(key)){
			key = this.keyValues.ContainsValue(key) ? this.keyValues.GetKey(key) : key;
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
		if(this.binds.ContainsKey(key,true)){
			this.binds.Remove(key);
		}
		this.binds.Add(key,data);
	}
	public void LoadBinds(){
		this.AddBind("BackQuote","showConsole");
		if(!Application.isWebPlayer && this.configFile != ""){return;}
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
			this.AddBind(key,action,toggle,repeat,repeatDelay);
		}
	}
	public void ResetBinds(string[] values){
		this.binds.Clear();
		PlayerPrefs.DeleteKey("binds");
		this.AddLog("^10All stored ^3binds^10 have been cleared.");
	}
	public void BindCommand(string[] values){		
		string data = string.Join(" ",values.Skip(2).ToArray()).Trim();
		this.AddBind(values[1],data);
	}
	public void ToggleCommand(string[] values){
		string data = string.Join(" ",values.Skip(2).ToArray()).Trim();
		this.AddBind(values[1],data,true);
	}
	public void RepeatCommand(string[] values){
		float repeatDelay = values.Length > 2 ? Convert.ToSingle(values.Last()) : 0;
		string data = string.Join(" ",values.Skip(2).Take(values.Length-2).ToArray()).Trim();
		this.AddBind(values[1],data,false,true,repeatDelay);
	}
	public void ListConsoleBinds(string[] values){
		foreach(var item in this.binds){
			Bind data = item.Value;
			string type = data.toggle ? "toggle" : "bind";
			type = data.repeat ? "repeat (" + data.repeatDelay + "s)" : type;
			this.AddLog("  ^17" + type + "^8| " + data.key + " ^7|" + data.action);
		}
	}
	public void SaveBinds(){
		string bindString = "";
		foreach(var item in this.binds){
			Bind data = item.Value;
			bindString += data.name + "-" + data.action;
			if(data.repeat){bindString += "-" + data.repeatDelay;}
			bindString += "|";
		}
		if(Application.isWebPlayer || this.configFile == ""){
			bindString = bindString.Trim('|') + "|";
			PlayerPrefs.SetString("binds",bindString);
		}
		else{
			this.configOutput.Add(bindString.Replace("-"," ").Replace("|","\r\n"));
		}
	}
	public void CheckBinds(){
		KeyShortcut CheckKeyDown = Button.CheckEventKeyDown;
		if(this.keyDetection != ""){return;}
		foreach(var item in this.binds){
			Bind data = item.Value;
			if(this.status > 0 && !data.action.Contains("consoleShow",true)){continue;}
			bool keyDown = CheckKeyDown(data.key);
			if(keyDown && data.repeat && data.nextRepeat > Time.time){
				this.AddCommand(data.action);
				data.nextRepeat += Time.time + data.repeatDelay;
			}
			else if(data.toggle){
				if(data.toggleActive){
					this.AddCommand(data.action);
				}
				if(keyDown && data.released){
					data.toggleActive = !data.toggleActive;
				}
			}
			else if(keyDown && data.released){
				this.AddCommand(data.action);
			}
			if(this.lastCommand.Matches(data.action,true)){
				Event.current.Use();
			}
			data.released = !keyDown;
		}
	}
	//===========================
	// Cvars
	//===========================
	public void AddCvarMethod(string name,object scope,string dataName,string fullName="",string help="",Method method=null){
		this.AddCvar(name,scope,dataName,fullName,help);
		this.cvars[name].method.simple = method;
	}
	public void AddCvarMethod(string name,object scope,string dataName,string fullName="",string help="",ConsoleMethod method=null){
		this.AddCvar(name,scope,dataName,fullName,help);
		this.cvars[name].method.basic = method;
	}
	public void AddCvarMethod(string name,object scope,string dataName,string fullName="",string help="",ConsoleMethodFull method=null){
		this.AddCvar(name,scope,dataName,fullName,help);
		this.cvars[name].method.full = method;
	}
	public void AddCvar(string name,object scope,string dataName,string fullName,Dictionary<string,string> help,bool formatHelp=true){
		if(help.ContainsKey(name)){
			this.AddCvar(name,scope,dataName,fullName,name + " " + help[name],formatHelp);
		}
	}
	public void AddCvar(string name,object scope,string dataName,string fullName="",string help="",bool formatHelp=true){
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
		this.cvars.Add(name,data);
		this.LoadCvar(data);
		this.AddKeyword(name,this.HandleCvar);
	}
	public void LoadCvar(Cvar data){
		if(!Application.isWebPlayer && this.configFile != ""){return;}
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
	public void ResetCvars(string[] values){
		string binds = PlayerPrefs.GetString("binds");
		PlayerPrefs.DeleteAll();
		PlayerPrefs.SetString("binds",binds);
		foreach(var item in this.cvars){
			Cvar data = item.Value;
			data.value.Set(data.defaultValue);
		}
		this.AddLog("^10All stored ^3cvars^10 have been reset to default values.");
	}
	public void SaveCvars(){
		foreach(var item in this.cvars){
			if(item.Key.StartsWith("!")){continue;}
			if(item.Key.StartsWith("#")){continue;}
			Cvar data = item.Value;
			object current = data.value.Get();
			if(Application.isWebPlayer || this.configFile == ""){
				Type type = current.GetType();
				if(type == typeof(float)){PlayerPrefs.SetFloat(data.fullName,(float)current);}
				else if(type == typeof(string)){PlayerPrefs.SetString(data.fullName,(string)current);}
				else if(type == typeof(int)){PlayerPrefs.SetInt(data.fullName,(int)current);}
				else if(type == typeof(byte)){PlayerPrefs.SetInt(data.fullName,Convert.ToInt16(current));}
				else if(type == typeof(bool)){PlayerPrefs.SetInt(data.fullName,(bool)current == true ? 1 : 0);}
			}
			else{
				this.configOutput.Add(item.Key + " " + current);
			}
		}
	}
	public void HandleCvar(string[] values,bool help){
		Cvar data = this.cvars[values[0]];
		string defaultText = "";
		if(help){
			if(data.help != ""){this.AddLog(data.help);}
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
		this.AddLog("^10" + data.fullName + "^10 is :^3 " + data.value.Get() + defaultText);
	}
	//===========================
	// Keywords/Shortcuts
	//===========================
	public void AddKeyword(string name,Method method=null,int minimumParameters=-1,string help=""){
		ConsoleCallback call = new ConsoleCallback();
		call.simple = method;
		call.help = help;
		call.minimumParameters = minimumParameters;
		this.keywords.Add(name,call);
	}
	public void AddKeyword(string name,ConsoleMethod method=null,int minimumParameters=-1,string help=""){
		ConsoleCallback call = new ConsoleCallback();
		call.basic = method;
		call.help = help;
		call.minimumParameters = minimumParameters;
		this.keywords.Add(name,call);
	}
	public void AddKeyword(string name,ConsoleMethodFull method=null,int minimumParameters=-1,string help=""){
		ConsoleCallback call = new ConsoleCallback();
		call.full = method;
		call.help = help;
		call.minimumParameters = minimumParameters;
		this.keywords.Add(name,call);
	}
	public void AddShortcut(string[] names,string replace){
		foreach(string name in names){
			this.shortcuts.Add(name,replace);
		}
	}
	public void AddShortcut(string name,string replace){
		this.shortcuts.Add(name,replace);
	}
	//===========================
	// Config
	//===========================
	public void SaveConfig(){
		if(!Application.isWebPlayer){
			using(StreamWriter file = new StreamWriter(this.configFile,false)){
				foreach(string line in this.configOutput){
					file.WriteLine(line);
				}
			}
		}
	}
	public void LoadConfig(string name){
		if(name != "" && !Application.isWebPlayer && File.Exists(name)){
			using(StreamReader file = new StreamReader(name)){
				string line = "";
				while((line = file.ReadLine()) != null){
					this.AddCommand(line,false);
				}
			}
		}
	}
	public void LoadConfig(string[] values){
		this.LoadConfig(values[1]);
	}
	public void DeleteConfig(string name){
		if(name != "" && File.Exists(name)){
			File.Delete(name);
		}
	}
	//===========================
	// Internal
	//===========================
	public void AddCommand(string text,bool allowLogging=true){
		this.allowLogging = allowLogging;
		this.lastCommand = text;
		this.ManageInput();
		this.allowLogging = true;
	}
	public void AddLog(string text,bool system=false){
		if(!this.allowLogging){return;}
		if(Application.isEditor){
			Application.RegisterLogCallback(null);
			Debug.Log(text);
			Application.RegisterLogCallback(this.HandleLog);
			if(system){return;}
		}
		if(this.logFile != "" && !Application.isWebPlayer){
			string cleanText = text;
			for(int index=this.color.Length-1;index>=0;--index){
				cleanText = cleanText.Replace("^"+index,"").Replace("|","");	
			}
			using(StreamWriter file = new StreamWriter(this.logFile,true)){
			    file.WriteLine(cleanText);
			}
		}
		while(text.Length > 0){
			int max = text.Length > this.logLineSize ? this.logLineSize : text.Length;
			this.log.Add(text.Substring(0,max));
			text = text.Substring(max);
		}
	}
	public void HandleLog(string text,string trace,LogType type){
		if(type == LogType.Error){text = "^1" + text + "^10 -- ^7" + trace;}
		if(type == LogType.Assert){text = "^1" + text + "^10 -- ^7" + trace;}
		if(type == LogType.Warning){text = "^8" + text + "^10 -- ^7" + trace;}
		if(type == LogType.Exception){text = "^6" + text + "^10 -- ^7" + trace;}
		if(type == LogType.Log){}
		text = text.Replace("*"," ");
		text = text.Replace("?"," ");
		text = text.Replace("\n"," ");
		this.AddLog(text,true);
	}
	public void CheckAutocomplete(){
		foreach(var item in this.keywords){
			string name = item.Key.Trim('#');
			if(name.StartsWith("!")){continue;}
			if(name.StartsWith(this.inputText,true)){
				this.autocomplete.Add(name);
			}
		}
		foreach(var item in this.shortcuts){
			string name = item.Key.Trim('#');
			if(name.StartsWith("!")){continue;}
			if(name.StartsWith(this.inputText,true)){
				this.autocomplete.Add(item.Value);
			}
		}
		if(this.autocomplete.Count > 0){
			if(this.autocomplete.Count > 1){
				this.AddLog("^5>> ^7" + this.inputText);
				foreach(string name in this.autocomplete){
					string type = this.cvars.ContainsKey(name,true) ? "^8 " : "^17 ";
					string shortcuts = "^9";
					foreach(var data in this.shortcuts){
						if(data.Value.Matches(name,true)){shortcuts += " / " + data.Key;}
					}
					this.AddLog("\t" + type + name + shortcuts);
				}
			}
			else{
				this.inputText = this.autocomplete[0];
			}
			this.logPosition = 1.0f;
			this.autocomplete.Clear();
		}
	}
	public void CheckTrigger(){
		KeyShortcut CheckKeyDown = Button.CheckEventKeyDown;
		if(CheckKeyDown(this.triggerKey)){
			this.ShowConsole();
			Event.current.Use();
		}
	}
	public void CheckHotkeys(){
		bool control = Event.current.control;
		bool shift = Event.current.shift;
		bool alt = Event.current.alt;
		KeyShortcut CheckKeyDown = Button.CheckEventKeyDown;
		if(control && alt){
			string keyName = Convert.ToString(Event.current.keyCode);
			if(this.keyDetection == ""){this.keyDetection = "###";}
			if(this.keyDetection != keyName && !keyName.ContainsAny("Control","Alt","None")){
				if(this.keyDetection != "" && this.keyDetection != "###"){
					this.inputText = this.inputText.Substring(0,this.inputText.Length-this.keyDetection.Length);
				}
				this.keyDetection = keyName;
				this.inputText += keyName;
			}
			if(Event.current.type == EventType.KeyDown){
				Event.current.Use();
			}
		}
		else{this.keyDetection = "";}
		if(CheckKeyDown(KeyCode.Return)){
			this.inputText = this.inputText.TrimStart('\\').TrimStart('/');
			this.lastCommand = this.inputText == "" ? " " : this.inputText;
			if(this.inputText != ""){
				this.history.Add(this.inputText);
				this.historyIndex = (byte)(this.history.Count);
			}
			this.AddLog("^5>> ^7" + this.inputText);
			this.inputText = "";
		}
		else if(Event.current.type == EventType.ScrollWheel){
			if(control){this.logFontSize -= (int)Event.current.delta[1];}
			else if(shift){this.height += Math.Sign(Event.current.delta[1]) * 0.02f;}
			else{this.logPosition += (float)(Event.current.delta[1]) / (float)(this.log.Count);}
		}
		else if(CheckKeyDown(KeyCode.PageDown)){this.logPosition += this.logScrollLimit * 0.25f;}
		else if(CheckKeyDown(KeyCode.PageUp)){this.logPosition -= this.logScrollLimit * 0.25f;}
		else if(CheckKeyDown(KeyCode.Tab)){this.CheckAutocomplete();}
		if(this.history.Count > 0 && (CheckKeyDown(KeyCode.UpArrow) || CheckKeyDown(KeyCode.DownArrow))){
			if(CheckKeyDown(KeyCode.UpArrow) && this.historyIndex > 0){--this.historyIndex;}
			if(CheckKeyDown(KeyCode.DownArrow)){++this.historyIndex;}
			this.historyIndex = (byte)Mathf.Clamp(this.historyIndex,0,this.history.Count-1);
			this.inputText = this.history[this.historyIndex];
		}
		this.logFontSize = Mathf.Clamp(this.logFontSize,9,128);
	}
	public void CheckDrag(){
		float consoleHeight = (Screen.height * this.offset)*this.height;
		Rect dragBounds = new Rect(0,consoleHeight+this.inputBackground.height-9,Screen.width,9);
		Vector3 mouse = Input.mousePosition;
		mouse[1] = Screen.height - mouse[1];
		if(this.dragStart != Vector3.zero){
			if(!Input.GetMouseButton(0)){
				this.dragStart = Vector3.zero;
			}
			else{
				Vector3 changed = mouse - this.dragStart;
				this.height = this.dragStart[2] + (changed.y/Screen.height);
			}
		}
		else if(Input.GetMouseButtonDown(0)){
			if(dragBounds.ContainsPoint(mouse)){
				this.dragStart = mouse;
				this.dragStart[2] = this.height;
			}
		}
		this.height = Mathf.Clamp(this.height,0.05f,(((float)Screen.height-30.0f)/(float)Screen.height));	
	}
	public void DrawElements(){
		GUI.skin = this.skin;
		float consoleHeight = (Screen.height * this.offset)*this.height;
		float alternate = Time.time % 1.5f;
		byte logLinesShown = 0;
		int logTextOffset = 15 - this.logFontSize;
		Rect logBounds = new Rect(-10,5,Screen.width-20,18);
		Rect scrollBounds = new Rect(Screen.width-15,0,20,consoleHeight);
		Rect consoleBounds = new Rect(0,0,Screen.width,consoleHeight);
		Rect inputBounds = new Rect(0,consoleHeight,Screen.width,30);
		Rect inputArrowBounds = new Rect(2,consoleHeight+6,12,12);
		Rect tiling = new Rect(alternate,alternate,Screen.width/this.background.width,consoleHeight/this.background.height);
		Rect tilingInput = new Rect(0,0,Screen.width/this.inputBackground.width,1);
		this.logStyle.fontSize = this.logFontSize;
		GUI.DrawTextureWithTexCoords(consoleBounds,this.background,tiling);
		GUI.DrawTextureWithTexCoords(inputBounds,this.inputBackground,tilingInput);
		GUI.DrawTexture(inputArrowBounds,this.textArrow);
		if(this.status == ConsoleState.open && this.log.Count > 0){
			this.logStyle.normal.textColor = this.color[this.logFontColor];
			float clampedPosition = Mathf.Clamp(this.logPosition,0,this.logScrollLimit);
			byte logPosition = (byte)(clampedPosition * (this.log.Count+1));
			for(byte lineIndex = 0;lineIndex < this.log.Count;++lineIndex){ 
				if(logPosition > lineIndex){continue;}
				if(logBounds.y + this.logFontSize > consoleBounds.yMax - 5){break;}
				string colorCode = "";
				StringBuilder word = new StringBuilder();
				this.logStyle.normal.textColor = this.color[this.logFontColor];
				foreach(char letter in this.log[lineIndex]){
					if(logBounds.x > Screen.width){break;}
					if(letter == '^'){
						colorCode = "0";
						continue;
					}
					else if(colorCode.Length > 0){
						if(Char.IsNumber(letter)){
							if(this.logFontAllowColors){colorCode += letter;}
							continue;
						}
						if(this.logFontAllowColors){
							if(word.Length > 0){
								logBounds.width = this.logStyle.CalcSize(new GUIContent(word.ToString()))[0];
								GUI.Label(logBounds,word.ToString().Replace("|",""),this.logStyle);
								logBounds.x += logBounds.width - (15+logTextOffset/2);
								word = new StringBuilder();
							}
							colorCode = colorCode.Length < 2 ? this.logFontColor.ToString() : colorCode;
							if(colorCode.IsInt()){
								int colorIndex = Mathf.Clamp(Convert.ToInt32(colorCode),0,this.color.Length-1);
								this.logStyle.normal.textColor = this.color[colorIndex];
							}
						}
						colorCode = "";
					}
					word.Append(letter);
				}
				if(word.Length > 0){
					logBounds.width = this.logStyle.CalcSize(new GUIContent(word.ToString()))[0];
					GUI.Label(logBounds,word.ToString().Replace("|",""),this.logStyle);
				}
				logBounds.x = -10;
				logBounds.y += 17-(logTextOffset/2);
				++logLinesShown;
			}
			this.logScrollLimit = 1.0f - ((float)(logLinesShown) / (float)(this.log.Count));
			if(logScrollLimit > 0.0f){
				this.logPosition = GUI.VerticalScrollbar(scrollBounds,this.logPosition,1.0f-this.logScrollLimit,0.0f,1.0f);
			}
		}
		GUI.SetNextControlName("inputText");
		this.inputText = GUI.TextField(inputBounds,this.inputText);
	}
	public void ManageState(){
		float slideStep = this.speed * Time.deltaTime;
		if(this.status == ConsoleState.open && this.keyDetection == ""){
			GUI.FocusControl("inputText");
		}
		else if(this.keyDetection == ""){
			GUI.SetNextControlName("");
			GUI.FocusControl("");
		}
		if(this.status == ConsoleState.closeBegin){
			this.status = ConsoleState.closing;
		}
		else if(this.status == ConsoleState.closing){
			if(this.offset > -1.0){this.offset -= slideStep;}
			else{this.status = ConsoleState.closed;}
		}
		else if(this.status == ConsoleState.openBegin){
			this.status = ConsoleState.opening;
		}
		else if(this.status == ConsoleState.opening){
			if(this.offset < 1.0){this.offset += slideStep;}
			else{this.status = ConsoleState.open;}
		}
		this.offset = Mathf.Clamp(this.offset,-1.0f,1.0f);
	}
	public void ManageInput(){
		this.lastCommand = this.lastCommand.Trim('#').Trim();
		if(this.lastCommand == " "){
			this.logPosition = 1.0f;
			this.lastCommand = "";
		}
		else if(this.lastCommand != ""){
			foreach(var item in this.shortcuts){
				if(this.lastCommand.IndexOf(item.Key+" ",true) == 0 || this.lastCommand.Matches(item.Key,true)){
					this.lastCommand = item.Value + this.lastCommand.Remove(0,item.Key.Length);
				}
			}
			int endCheck = this.lastCommand.Length > 1 ? 2 : 1;
			string lineEnd = this.lastCommand.Substring(this.lastCommand.Length-endCheck);
			bool commandFound = false;
			bool wildcard = lineEnd.Contains("*");
			bool helpMode = lineEnd.Contains("?");
			if(helpMode){this.lastCommand = this.lastCommand.Replace("?","");}
			if(wildcard){this.lastCommand = this.lastCommand.Replace("*","");}
			string firstWord = this.lastCommand.Split(' ')[0];
			foreach(var item in this.keywords){
				string name = item.Key.Trim('#');
				ConsoleCallback method = item.Value;
				if(wildcard || helpMode){
					if(wildcard && method.full != this.HandleCvar){continue;}
					if(name.StartsWith("!")){continue;}
					if(name.StartsWith(firstWord,true)){
						if(method.minimumParameters > 0 && method.help != ""){
							this.AddLog(method.help);
						}
						else if(method.simple != null){method.simple();}
						else if(method.basic != null){method.basic(new string[2]{item.Key,""});}
						else if(method.full != null){method.full(new string[2]{item.Key,""},helpMode);}
						commandFound = true;
					}
				}
				else if(firstWord.StartsWith(name,true)){
					this.lastCommand = this.lastCommand.Replace(name,"",true).Trim();
					List<string> options = new List<string>();
					options.Add(item.Key);
					if(this.lastCommand != ""){
						options.AddRange(this.lastCommand.Split(' '));
					}
					if(method.minimumParameters > options.Count-1 && method.help != ""){
						this.AddLog(method.help);
					}
					else if(method.simple != null){method.simple();}
					else if(method.basic != null){method.basic(options.ToArray());}
					else if(method.full != null){method.full(options.ToArray(),helpMode);}
					commandFound = true;
					break;
				}
			}
			if(!commandFound){this.AddLog("^1No command found -- " + this.lastCommand);}
			this.logPosition = 1.0f;
			this.lastCommand = "";
		}
	}
	//===========================
	// Commands
	//===========================
	public void ResetValue(string[] values){
		string name = values[1];
		foreach(var data in this.shortcuts){
			if(data.Key.Matches(name,true)){
				name = data.Value;
			}
		}
		if(this.binds.ContainsKey(name,true)){
			this.binds.Remove(name);
			this.AddLog("^3|" + name + "^10 has been unbound as a key.");
		}
		else if(this.cvars.ContainsKey(name,true)){
			Cvar data = this.cvars[name];
			data.value.Set(data.defaultValue);
			this.AddLog("^3|" + data.name + "^10 has been reset to its default value ^7-- ^8|" + data.defaultValue);
		}
		else{
			this.AddLog("^3|" + name + "^10 is not a valid cvar or key bind.");
		}
	}
	public void SaveConsoleFile(string[] values){
		string fileName = values.Length > 1 ? values[1] : "ConsoleDump.txt";
		string path = "";
		if(Application.isWebPlayer){
			this.AddLog("^3Console log dumping not supported in web player.");
			return;
		}
		using(StreamWriter file = new StreamWriter(fileName,true)){
			foreach(string line in this.log){
		    	file.WriteLine(line);
			}
			path = ((FileStream)(file.BaseStream)).Name;
		}
		this.AddLog("^3Console Dump Saved ^7-- " + path);
	}
	public void ClearConsole(){
		this.log = new FixedList<string>(256);
		this.logPosition = 0;
		this.logScrollLimit = 0;
	}
	public void ShowConsole(){
		if(this.status != ConsoleState.openBegin  && this.status != ConsoleState.closeBegin){
			this.status = this.status < ConsoleState.open ? ConsoleState.openBegin : ConsoleState.closeBegin;
		}
	}
	public void ListConsoleColors(){
		string listing = "^10Console Font colors available : ";
		for(byte index = 0;index < this.color.Length;++index){
			listing += " ^" + index + " " + index;
		}
		this.AddLog(listing);
	}
	public void ListConsoleFonts(){
		/*Font[] fonts = FindObjectsOfType(Font) as Font[];
		foreach(Font font in fonts){
		this.AddLog(" ^7 " + font.name);
		}*/
	}
}
