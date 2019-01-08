using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Zios.Console{
	using Zios.Extensions;
	using Zios.Unity.Button;
	using Zios.Unity.Log;
	using Zios.Unity.Pref;
	using Zios.Unity.Proxy;
	using Zios.Unity.Time;
	public partial class Console{
		public static Dictionary<string,Bind> binds = new Dictionary<string,Bind>();
		public static void AddBind(string key,string action,bool toggle=false,bool repeat=false,float repeatDelay=0){
			if(!Proxy.IsPlaying()){return;}
			List<string> keyCodes = new List<string>(Enum.GetNames(typeof(KeyCode)));
			if(!keyCodes.Contains(key)){
				key = Button.GetName(key);
				if(!keyCodes.Contains(key)){
					Log.Warning("[Console] " + key + " could not be bound. It is not a valid key.");
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
			data.nextRepeat = Time.Get();
			data.released = true;
			data.toggleActive = false;
			if(Console.binds.ContainsKey(key,true)){
				Console.binds.Remove(key);
			}
			Console.binds.Add(key,data);
		}
		public static void LoadBinds(){
			Console.AddBind("BackQuote","console");
			if(Console.Get().configFile != ""){return;}
			if(!PlayerPref.Has("binds")){
				PlayerPref.Set<string>("binds","|");
			}
			string binds = PlayerPref.Get<string>("binds");
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
			if(Console.Get().configFile == ""){
				bindString = bindString.Trim('|') + "|";
				PlayerPref.Set<string>("binds",bindString);
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
				bool keyDown = Button.EventKeyUp(data.key);
				if(keyDown && data.repeat && data.nextRepeat > Time.Get()){
					Console.AddCommand(data.action);
					data.nextRepeat += Time.Get() + data.repeatDelay;
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
}