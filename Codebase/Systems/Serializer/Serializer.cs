using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
namespace Zios{
	#if UNITY_EDITOR 
	using UnityEditor;
	[InitializeOnLoad]
	public static class SerializerHook{
		static SerializerHook(){
			SerializerHook.Create();
			Events.Add("On Scene Loaded",SerializerHook.Create).SetPermanent(true);
			Events.Add("On Hierarchy Changed",SerializerHook.Create).SetPermanent(true);
			Events.Add("On Exit Play",SerializerHook.Create).SetPermanent(true);
		}
		public static void Create(){
			if(Serializer.instance.IsNull()){
				var path = Locate.GetScenePath("@Main");
				if(!path.HasComponent<Serializer>()){
					Debug.Log("[Serializer] : Auto-creating Serializer Manager GameObject.");
					Serializer.instance = path.AddComponent<Serializer>();
				}
				Serializer.instance = path.GetComponent<Serializer>();
				//Serializer.instance.Setup();
			}
			Events.Add("On Enter Play",Serializer.instance.Save);
			Events.Add("On Asset Saving",Serializer.instance.Save);
			Events.Add("On Asset Changed",Serializer.instance.Save);
			Events.Add("On Scene Loaded",Serializer.instance.Load);
		}
	}
	#endif
	[Flags] 
	public enum SerializerDebug : int{
		Build        = 0x001,
		Save         = 0x002,
		Load         = 0x004,
	}
	[ExecuteInEditMode]
	public class Serializer : MonoBehaviour{
		public static Serializer instance;
		public static Serializer Get(){return Serializer.instance;}
		[EnumMask] public SerializerDebug debug;
		private StringBuilder contents = new StringBuilder();
		private int tabs;
		private string path;
		public Dictionary<Type,Dictionary<string,object>> defaults = new Dictionary<Type,Dictionary<string,object>>();
		[ContextMenu("Refresh")]
		public void Setup(){
			if(!Application.isPlaying){
				this.path = Application.dataPath+"/@Serialized/";
				this.defaults.Clear();
				this.BuildDefault();
				/*Profiler.logFile = "mylog.log";
				Profiler.enabled = true;*/
			}
			this.Load();
		}
		//public void OnValidate(){Utility.EditorDelayCall(this.Load);}
		public void Start(){this.Setup();}
		//=================
		// Utility
		//=================
		private Dictionary<string,object> GetVariables(object scope,bool getStatic=true,bool getInstance=false){
			var ignoreAttributes = typeof(NonSerializedAttribute).AsList();
			var flags = BindingFlags.Public;
			if(getStatic){flags |= BindingFlags.Static;}
			if(getInstance){flags |= BindingFlags.Instance;}
			return scope.GetVariables(null,ignoreAttributes,flags);
		}
		public void Add(params string[] contents){
			if(contents.Contains("}")){this.tabs -= 1;}
			string tabs = new String('\t',this.tabs);
			this.contents.Append(tabs);
			foreach(var data in contents){this.contents.Append(data);}
			this.contents.Append("\n");
			if(contents.Contains("{")){this.tabs += 1;}
		}
		//=================
		// Defaults
		//=================
		public void BuildDefault(){
			this.BuildDefault(Assembly.Load("Assembly-CSharp"));
			this.BuildDefault(Assembly.Load("Assembly-CSharp-Editor"));
		}
		public void BuildDefault(Assembly assembly){
			if(this.debug.Has("Build")){Debug.Log("[Serializer] : Building defaults for assembly -- " + assembly.FullName.Split(",")[0]);}
			foreach(Type type in assembly.GetTypes()){
				if(type.IsEnum || type == null || type.Name.Contains("_AnonStorey")){continue;}
				this.BuildDefault(type);
			}
		}
		public void BuildDefault(Type type){
			if(this.defaults.ContainsKey(type)){return;}
			if(this.debug.Has("Build")){Debug.Log("[Serializer] : Building defaults for type -- " + type.Name);}
			this.defaults.AddNew(type);
			foreach(var item in this.GetVariables(type)){
				this.defaults[type][item.Key] = item.Value;
			}
		}
		//=================
		// Saving
		//=================
		[ContextMenu("Save")]
		public void Save(){
			if(!Application.isPlaying){
				this.Save(Assembly.Load("Assembly-CSharp"));
				this.Save(Assembly.Load("Assembly-CSharp-Editor"));
			}
		}
		public void Save(Assembly assembly){
			if(this.debug.Has("Save")){Debug.Log("[Serializer] : Serializing assembly -- " + assembly.FullName.Split(",")[0]);}
			foreach(Type type in assembly.GetTypes()){
				if(type.IsEnum || type == null || type.Name.Contains("_AnonStorey")){continue;}
				this.Save(type);
			}
		}
		public void Save(Type type){
			this.tabs = 0;
			this.contents.Clear();
			var file = FileManager.Find(type.Name+".cs",true,false);
			string path = file != null ? file.folder+"/" : this.path;
			string filePath = path+type.Name+".static";
			this.Add(type.FullName,"{");
			bool empty = true;
			this.BuildDefault(type);
			foreach(var item in this.GetVariables(type)){
				if(!this.defaults.ContainsKey(type) || !this.defaults[type].ContainsKey(item.Key)){continue;}
				bool isNull = this.defaults[type][item.Key].IsNull();
				bool nullMatch = isNull && item.Value.IsNull();
				if(nullMatch || (!isNull && this.defaults[type][item.Key].Equals(item.Value))){continue;}
				if(this.debug.Has("Save")){Debug.Log("[Serializer] : " + type.Name + "." + item.Key + " = " + item.Value);}
				bool wasSaved = this.Save(item.Key,item.Value);
				if(wasSaved){empty = false;}
			}
			this.Add("}");
			if(!empty){
				Directory.CreateDirectory(path);
				File.WriteAllText(filePath,this.contents.ToString());
				return;
			}
			//File.Delete(filePath);
			//File.Delete(filePath+".meta");
		}
		public bool Save(string name,object value){
			var type = value.GetType();
			if(type.IsValueType){
				this.Add(name," = ",value.ToString());
				return true;
			}
			if(value is IDictionary){return false;}
			if(value is IEnumerable){return false;}
			/*foreach(var item in this.GetVariables(value,false,true)){
				this.Add("{");
				if(!this.defaults.ContainsKey(type)){
					try{
						var instance = Activator.CreateInstance(type);
						this.defaults.AddNew(type);
						foreach(var instanceItem in this.GetVariables(instance,false,true)){
							this.defaults[type][instanceItem.Key] = instanceItem.Value;
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
			this.LoadStatic();
			//this.LoadScene();
			//this.LoadInstance();
		}
		public void LoadStatic(){
			if(this.debug.Has("Load")){Debug.Log("[Serializer] : Loading .static files");}
			foreach(var file in FileManager.FindAll("*.static",true,false)){
				if(this.debug.Has("Load")){Debug.Log("[Serializer] : Loading "+file.fullName);}
				string contents = file.GetText();
				var type = Type.GetType(contents.Parse("","{"));
				foreach(string line in contents.Split("\n").Skip(1)){
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
						this.defaults.AddNew(type)[name] = parsed;
						continue;
					}
					this.defaults.AddNew(type)[name] = Convert.ChangeType(value,dataType);
				}
			}
		}
	}
}