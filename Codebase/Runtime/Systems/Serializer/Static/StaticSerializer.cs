using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
namespace Zios.StaticSerializer{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.File;
	using Zios.Reflection;
	using Zios.Unity.Log;
	using Zios.Unity.Proxy;
	using Zios.Unity.Supports.Singleton;
	using Zios.Unity.SystemAttributes;
	using Zios.Unity.Time;
	[Flags]
	public enum SerializerDebug : int{
		Build         = 0x001,
		BuildDetailed = 0x002,
		Load          = 0x004,
		LoadDetailed  = 0x008,
		Save          = 0x010,
		SaveType      = 0x020,
		SaveDetailed  = 0x040,
		Time          = 0x080,
	}
	public class StaticSerializer : Singleton{
		public static StaticSerializer instance;
		public static StaticSerializer Get(){return StaticSerializer.instance;}
		public static Dictionary<Type,Dictionary<string,object>> defaults = new Dictionary<Type,Dictionary<string,object>>();
		public bool disabled = true;
		[EnumMask] public SerializerDebug debug;
		private StringBuilder contents = new StringBuilder();
		private int tabs;
		private string path;
		[ContextMenu("Refresh")]
		public void Setup(){
			if(this.disabled){return;}
			if(StaticSerializer.defaults.Count < 1){
				StaticSerializer.instance = this;
				this.path = Application.dataPath+"/@Serialized/";
				if(Proxy.IsEditor()){
					this.BuildDefault();
				}
			}
			this.Load();
		}
		public void Awake(){this.Setup();}
		//=================
		// Utility
		//=================
		private Dictionary<string,object> GetVariables(object scope,bool getStatic=true,bool getInstance=false){
			var ignoreAttributes = typeof(NonSerializedAttribute).AsList();
			var flags = BindingFlags.Public;
			if(getStatic){flags |= BindingFlags.Static;}
			if(getInstance){flags |= BindingFlags.Instance;}
			return scope.GetVariables(ignoreAttributes,flags);
		}
		public void Add(params string[] contents){
			if(contents.Contains("}")){this.tabs -= 1;}
			string tabs = new String('\t',this.tabs);
			this.contents.Append(tabs);
			foreach(var data in contents){this.contents.Append(data);}
			this.contents.Append("\n");
			if(contents.Contains("{")){this.tabs += 1;}
		}
		public bool Skip(Type type,string name,object value){
			if(!StaticSerializer.defaults.ContainsKey(type) || !StaticSerializer.defaults[type].ContainsKey(name)){
				if(this.debug.Has("SaveDetailed")){Log.Show("[Serializer] : Skipping save for -- " + type + " -- " + name + " -- " + value);}
				return true;
			}
			var lastValue = StaticSerializer.defaults[type][name];
			if(this.debug.Has("SaveDetailed")){Log.Show("[Serializer] : " + type + "." + name + " -- " + lastValue + " = " + value);}
			if(lastValue.IsNull() || value.IsNull()){return lastValue == value;}
			return lastValue.Equals(value);
		}
		//=================
		// Defaults
		//=================
		public void BuildDefault(){
			if(this.disabled){return;}
			var time = Time.Get();
			this.BuildDefault(Assembly.Load("Assembly-CSharp"));
			this.BuildDefault(Assembly.Load("Assembly-CSharp-Editor"));
			if(this.debug.Has("Time")){Log.Show("[Serializer] : Build Default complete -- " + (Time.Get() - time) + " seconds.");}
		}
		public void BuildDefault(Assembly assembly){
			if(this.disabled){return;}
			if(this.debug.Has("Build")){Log.Show("[Serializer] : Building defaults for assembly -- " + assembly.FullName.Split(",")[0]);}
			foreach(Type type in assembly.GetTypes()){
				if(type.IsEnum || type == null || type.Name.Contains("_AnonStorey")){continue;}
				this.BuildDefault(type);
			}
		}
		public void BuildDefault(Type type){
			if(this.disabled){return;}
			if(StaticSerializer.defaults.ContainsKey(type)){return;}
			if(this.debug.Has("BuildDetailed")){Log.Show("[Serializer] : Building defaults for type -- " + type.Name);}
			StaticSerializer.defaults.AddNew(type);
			foreach(var item in this.GetVariables(type)){
				StaticSerializer.defaults[type][item.Key] = item.Value;
			}
		}
		//=================
		// Saving
		//=================
		[ContextMenu("Save")]
		public void Save(){
			if(this.disabled){return;}
			var time = Time.Get();
			this.Save(Assembly.Load("Assembly-CSharp"));
			this.Save(Assembly.Load("Assembly-CSharp-Editor"));
			if(this.debug.Has("Time")){Log.Show("[Serializer] : Save complete -- " + (Time.Get() - time) + " seconds.");}
		}
		public void Save(Assembly assembly){
			if(this.disabled){return;}
			foreach(Type type in assembly.GetTypes()){
				if(type.IsEnum || type == null || type.Name.Contains("_AnonStorey")){continue;}
				this.Save(type);
			}
		}
		public void Save(Type type){
			if(this.disabled){return;}
			if(this.debug.Has("SaveType")){Log.Show("[Serializer] : Serializing type -- " + type.Name);}
			this.tabs = 0;
			this.contents.Clear();
			var file = File.Find(type.Name+".cs",false);
			string path = file != null ? file.directory+"/" : this.path;
			string filePath = path+type.Name+".static";
			this.Add(type.FullName,"{");
			bool empty = true;
			foreach(var item in this.GetVariables(type)){
				if(this.Skip(type,item.Key,item.Value)){continue;}
				if(this.debug.Has("Save")){Log.Show("[Serializer] : " + type.Name + "." + item.Key + " = " + item.Value);}
				bool wasSaved = this.Save(item.Key,item.Value);
				if(wasSaved){empty = false;}
			}
			this.Add("}");
			if(File.Exists(filePath) || !empty){
				File.Create(path).Write(this.contents.ToString());
			}
		}
		public bool Save(string name,object value){
			if(this.disabled){return false;}
			var type = value.GetType();
			if(type.IsValueType || value is string){
				this.Add(name," = ",value.ToString());
				return true;
			}
			if(value is IDictionary){return false;}
			if(value is IEnumerable){return false;}
			/*foreach(var item in this.GetVariables(value,false,true)){
				this.Add("{");
				if(!Serializer.defaults.ContainsKey(type)){
					try{
						var instance = Activator.CreateInstance(type);
						Serializer.defaults.AddNew(type);
						foreach(var instanceItem in this.GetVariables(instance,false,true)){
							Serializer.defaults[type][instanceItem.Key] = instanceItem.Value;
						}
					}
					catch{}
				}
				this.Save(item.Key,item.Value);
			}
			this.Add("}");*/
			return false;
		}
		//=================
		// Loading
		//=================
		[ContextMenu("Load")]
		public void Load(){
			if(this.disabled){return;}
			var time = Time.Get();
			this.LoadStatic();
			//this.LoadScene();
			//this.LoadInstance();
			if(this.debug.Has("Time")){Log.Show("[Serializer] : Load complete -- " + (Time.Get() - time) + " seconds.");}
		}
		public void LoadStatic(){
			if(this.disabled){return;}
			if(this.debug.Has("Load")){Log.Show("[Serializer] : Loading .static files");}
			foreach(var file in File.FindAll("*.static",false)){
				if(this.debug.Has("Load")){Log.Show("[Serializer] : Loading "+file.fullName);}
				string contents = file.ReadText();
				var type = Reflection.GetType(contents.Parse("","{"));
				if(type.IsNull()){continue;}
				foreach(string line in contents.GetLines().Skip(1)){
					if(line.IsEmpty() || line.ContainsAny("{","}")){continue;}
					string name = line.Parse("","=");
					string value = line.Parse("=","");
					var dataType = type.GetVariableType(name);
					if(dataType == typeof(string)){type.SetVariable(name,value);}
					if(dataType == typeof(int)){type.SetVariable(name,value.ToInt());}
					if(dataType == typeof(float)){type.SetVariable(name,value.ToFloat());}
					if(dataType == typeof(bool)){type.SetVariable(name,value.ToBool());}
					if(dataType.IsEnum){
						var parsed = Enum.Parse(dataType,value);
						type.SetVariable(name,(int)parsed);
						continue;
					}
				}
			}
		}
	}
}