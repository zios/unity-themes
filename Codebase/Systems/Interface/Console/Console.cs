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
	using Event;
	using Inputs;
	public struct ConsoleState{
		public const byte closed = 0;
		public const byte closeBegin = 1;
		public const byte closing = 2;
		public const byte open = 3;
		public const byte openBegin = 4;
		public const byte opening = 5;
	}
	public partial class Console : Singleton{
		public static Console singleton;
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
		public static Console Get(){
			Console.singleton = Console.singleton ?? Utility.GetSingleton<Console>();
			return Console.singleton;
		}
		public void OnEnable(){
			Console.singleton = this;
			this.OnValidate();
			Application.logMessageReceived += Console.HandleLog;
			if(!this.logFile.IsEmpty()){
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
			Events.Add("On Application Quit",this.OnApplicationQuit);
			Events.Add("On GUI",this.OnGUI);
			Events.Add("On Start",this.Start);
		}
		public void OnDisable(){Application.logMessageReceived -= Console.HandleLog;}
		public void OnApplicationQuit(){
			Console.SaveCvars();
			if(Console.configOutput.Count > 0){
				Console.SaveBinds();
				Console.SaveConfig();
			}
		}
		public void OnGUI(){
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
			Console.AddShortcut("consoleRepeat","repeat");
			Console.AddShortcut("console","con");
			Console.AddShortcut("consoleResetValue","reset");
			Console.AddShortcut("consoleResetCvars","resetCvars");
			Console.AddShortcut("consoleResetBinds","resetBinds");
			Console.AddShortcut("consoleClear","clear","cls");
			Console.AddShortcut("consoleToggle","keyToggle","switch");
			Console.AddShortcut("consoleBind","bind","trigger");
			Console.AddShortcut("consoleListColors","showColors","listColors");
			Console.AddShortcut("consoleListFonts","showFonts","listFonts");
			Console.AddShortcut("consoleListBinds","showBinds","listBinds");
			Console.LoadBinds();
			Utility.DelayCall(()=>Console.LoadConfig(this.configFile));
		}
	}
	public partial class Console{
		public static FixedList<string> log = new FixedList<string>(256);
		public static FixedList<string> history = new FixedList<string>(256);
		public static Dictionary<string,string> shortcuts = new Dictionary<string,string>();
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
		// Keywords/Shortcuts
		//===========================
		public static void AddShortcut(string term,string shortcut){
			if(!Utility.IsPlaying()){return;}
			if(Console.shortcuts.ContainsKey(shortcut)){
				Debug.LogWarning("[Console] Already has registered Shortcut for -- " + shortcut);
				return;
			}
			Console.shortcuts[shortcut] = term;
		}
		//===========================
		// Internal
		//===========================
		public static void Setup(){
			if(Console.logStyle == null){
				if(Console.Get().skin == null){Console.Get().skin = GUI.skin;}
				Console.logStyle = new GUIStyle(Console.Get().skin.textField);
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
			if(!Utility.IsPlaying()){
				Application.logMessageReceived -= Console.HandleLog;
				Debug.Log(text);
				Application.logMessageReceived += Console.HandleLog;
				if(system){return;}
			}
			if(Console.logFileUsable){
				string logPath = Application.persistentDataPath + "/" + Console.Get().logFile;
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
				int max = text.Length > Console.Get().logLineSize ? Console.Get().logLineSize : text.Length;
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
			if(Button.EventKeyDown(Console.Get().triggerKey)){
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
				if(control){Console.Get().logFontSize -= (int)UnityEvent.current.delta[1];}
				else if(shift){Console.Get().height += Math.Sign(UnityEvent.current.delta[1]) * 0.02f;}
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
			Console.Get().logFontSize = Mathf.Clamp(Console.Get().logFontSize,9,128);
		}
		public static void CheckDrag(){
			float consoleHeight = (Screen.height * Console.offset)*Console.Get().height;
			Rect dragBounds = new Rect(0,consoleHeight,Screen.width,30);
			Vector3 mouse = UnityInput.mousePosition;
			mouse[1] = Screen.height - mouse[1];
			if(Console.dragStart != Vector3.zero){
				if(!UnityInput.GetMouseButton(0)){
					Console.dragStart = Vector3.zero;
				}
				else{
					Vector3 changed = mouse - Console.dragStart;
					Console.Get().height = Console.dragStart[2] + (changed.y/Screen.height);
				}
			}
			else if(UnityInput.GetMouseButtonDown(0)){
				if(dragBounds.Contains(mouse)){
					Console.dragStart = mouse;
					Console.dragStart[2] = Console.Get().height;
				}
			}
			Console.Get().height = Mathf.Clamp(Console.Get().height,0.05f,(((float)Screen.height-30.0f)/(float)Screen.height));
		}
		public static void DrawElements(){
			GUI.skin = Console.Get().skin;
			float consoleHeight = (Screen.height * Console.offset)*Console.Get().height;
			byte logLinesShown = 0;
			int logTextOffset = 15 - Console.Get().logFontSize;
			Rect logBounds = new Rect(-10,5,Screen.width-20,18);
			Rect scrollBounds = new Rect(Screen.width-15,0,20,consoleHeight);
			Rect consoleBounds = new Rect(0,0,Screen.width,consoleHeight);
			Rect inputBounds = new Rect(0,consoleHeight,Screen.width,30);
			Rect inputArrowBounds = new Rect(2,consoleHeight+6,12,12);
			Console.logStyle.fontSize = Console.Get().logFontSize;
			if(UnityEvent.current.type == EventType.Repaint){
				var backgroundTexture = Console.Get().background.GetTexture("textureMap");
				var inputTexture = Console.Get().inputBackground.GetTexture("textureMap");
				if(!backgroundTexture.IsNull()){
					backgroundTexture.wrapMode = TextureWrapMode.Repeat;
					backgroundTexture.filterMode = FilterMode.Bilinear;
				}
				if(!inputTexture.IsNull()){
					inputTexture.wrapMode = TextureWrapMode.Repeat;
					inputTexture.filterMode = FilterMode.Bilinear;
				}
				Graphics.DrawTexture(consoleBounds,Texture2D.whiteTexture,Console.Get().background);
				Graphics.DrawTexture(inputBounds,Texture2D.whiteTexture,Console.Get().inputBackground);
				Graphics.DrawTexture(inputArrowBounds,Texture2D.whiteTexture,Console.Get().textArrow);
			}
			if(Console.status == ConsoleState.open && Console.log.Count > 0){
				Console.logStyle.normal.textColor = Console.color[Console.Get().logFontColor];
				float clampedPosition = Mathf.Clamp(Console.logPosition,0,Console.logScrollLimit);
				byte logPosition = (byte)(clampedPosition * (Console.log.Count+1));
				for(byte lineIndex = 0;lineIndex < Console.log.Count;++lineIndex){
					if(logPosition > lineIndex){continue;}
					if(logBounds.y + Console.Get().logFontSize > consoleBounds.yMax - 5){break;}
					string colorCode = "";
					StringBuilder word = new StringBuilder();
					Console.logStyle.normal.textColor = Console.color[Console.Get().logFontColor];
					foreach(char letter in Console.log[lineIndex]){
						if(logBounds.x > Screen.width){break;}
						if(letter == '^'){
							colorCode = "0";
							continue;
						}
						else if(colorCode.Length > 0){
							if(Char.IsNumber(letter)){
								if(Console.Get().logFontAllowColors){colorCode += letter;}
								continue;
							}
							if(Console.Get().logFontAllowColors){
								if(word.Length > 0){
									logBounds.width = Console.logStyle.CalcSize(new GUIContent(word.ToString()))[0];
									GUI.Label(logBounds,word.ToString().Replace("|",""),Console.logStyle);
									logBounds.x += logBounds.width - (15+logTextOffset/2);
									word = new StringBuilder();
								}
								colorCode = colorCode.Length < 2 ? Console.Get().logFontColor.ToString() : colorCode;
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
			float slideStep = Console.Get().speed * Time.deltaTime;
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