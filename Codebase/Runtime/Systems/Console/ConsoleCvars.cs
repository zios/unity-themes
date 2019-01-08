using System;
using System.Collections.Generic;
namespace Zios.Console{
	using Zios.Extensions.Convert;
	using Zios.Shortcuts;
	using Zios.Supports.Accessor;
	using Zios.Unity.Log;
	using Zios.Unity.Pref;
	using Zios.Unity.Proxy;
	public partial class Console{
		public static Dictionary<string,Cvar> cvars = new Dictionary<string,Cvar>();
		public static void AddCvarMethod(string name,object scope,string dataName,string fullName="",string help="",Method method=null){
			if(!Proxy.IsPlaying()){return;}
			Console.AddCvar(name,scope,dataName,fullName,help);
			Console.cvars[name].method.simple = method;
		}
		public static void AddCvarMethod(string name,object scope,string dataName,string fullName="",string help="",ConsoleMethod method=null){
			if(!Proxy.IsPlaying()){return;}
			Console.AddCvar(name,scope,dataName,fullName,help);
			Console.cvars[name].method.basic = method;
		}
		public static void AddCvarMethod(string name,object scope,string dataName,string fullName="",string help="",ConsoleMethodFull method=null){
			if(!Proxy.IsPlaying()){return;}
			Console.AddCvar(name,scope,dataName,fullName,help);
			Console.cvars[name].method.full = method;
		}
		public static void AddCvar(string name,object scope,string dataName,string fullName,Dictionary<string,string> help,bool formatHelp=true){
			if(!Proxy.IsPlaying()){return;}
			if(help.ContainsKey(name)){
				Console.AddCvar(name,scope,dataName,fullName,name + " " + help[name],formatHelp);
			}
		}
		public static void AddCvar(string name,object scope,string dataName,string fullName="",string help="",bool formatHelp=true){
			if(!Proxy.IsPlaying()){return;}
			if(Console.cvars.ContainsKey(name)){
				Log.Warning("[Console] Already has registered Cvar for -- " + name);
				return;
			}
			if(fullName == ""){fullName = scope.GetType().ToString() + "^1 " + dataName;}
			int index = -1;
			if(dataName.Contains("[")){
				string[] words = dataName.Split('[');
				dataName = words[0];
				index = Convert.ToInt16(words[1].Replace("]",""));
			}
			var data = new Cvar();
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
			if(Console.Get().configFile != ""){return;}
			if(PlayerPref.Has(data.fullName)){
				object value = data.value.Get();
				Type type = data.value.type;
				if(type == typeof(float)){value = PlayerPref.Get<float>(data.fullName);}
				else if(type == typeof(string)){value = PlayerPref.Get<string>(data.fullName);}
				else if(type == typeof(int) || type == typeof(byte)){value = PlayerPref.Get<int>(data.fullName);}
				else if(type == typeof(bool)){value = PlayerPref.Get<int>(data.fullName) == 1 ? true : false;}
				data.value.Set(value);
			}
		}
		public static void ResetCvars(string[] values){
			string binds = PlayerPref.Get<string>("binds");
			PlayerPref.ClearAll();
			PlayerPref.Set<string>("binds",binds);
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
				if(Console.Get().configFile == ""){
					PlayerPref.Set(data.fullName,current);
				}
				else{
					Console.configOutput.Add(item.Key + " " + current.SerializeAuto());
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
				try{
					data.value.Set(values[1].Deserialize(data.value.type," "));
				}
				catch{
					Log.Show(values[1] + " -- " + values[1].Deserialize(data.value.type," ").GetType());
					Log.Show("[ConsoleCvars] : Issue setting cvar ["+data.value.type+"] -- " + data.value.name + " to " + values[1]);
				}
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
}