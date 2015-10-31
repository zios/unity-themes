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
		private static bool setup;
		static SerializerHook(){
			if(Application.isPlaying){return;}
			EditorApplication.delayCall += ()=>SerializerHook.Reset();
		}
		public static void Reset(){
			Events.Add("On Scene Loaded",SerializerHook.Reset).SetPermanent();
			Events.Add("On Hierarchy Changed",SerializerHook.Reset).SetPermanent();
			Events.Add("On Exit Play",SerializerHook.Reset).SetPermanent();
			SerializerHook.setup = false;
			SerializerHook.Create();
			if(Serializer.instance){
				Events.Add("On Enter Play",Serializer.instance.Save);
				Events.Add("On Asset Saving",Serializer.instance.Save);
				Events.Add("On Asset Changed",Serializer.instance.Save);
				Events.Add("On Level Was Loaded",Serializer.instance.Setup);
				Events.Add("On Exit Play",Serializer.instance.Load);
				//Events.Add("On Scene Loaded",Serializer.instance.Load);
			}
		}
		public static void Create(){
			if(SerializerHook.setup || Application.isPlaying){return;}
			SerializerHook.setup = true;
			if(Serializer.instance.IsNull()){
				var path = Locate.GetScenePath("@Main");
				Serializer.instance = path.GetComponent<Serializer>();
				if(Serializer.instance == null){
					Debug.Log("[Serializer] : Auto-creating Serializer Manager GameObject.");
					Serializer.instance = path.AddComponent<Serializer>();
				}
				Serializer.instance.Setup();
			}
		}
	}
	#endif
	[Flags] 
	public enum SerializerDebug : int{
		Build         = 0x001,
		BuildDetailed = 0x002,
		Load          = 0x004,
		LoadDetailed  = 0x008,
		Save          = 0x010,
		SaveType      = 0x020,
		SaveDetailed  = 0x040,
	}
	[ExecuteInEditMode]
	public class Serializer : MonoBehaviour{
		public static Serializer instance;
		public static Serializer Get(){return Serializer.instance;}
		public static Dictionary<Type,Dictionary<string,object>> defaults = new Dictionary<Type,Dictionary<string,object>>();
		[EnumMask] public SerializerDebug debug;
		private StringBuilder contents = new StringBuilder();
		private int tabs;
		private string path;
		[ContextMenu("Refresh")]
		public void Setup(){
			if(Serializer.defaults.Count < 1){
				Serializer.instance = this;
				if(Application.isEditor){
					this.path = Application.dataPath+"/@Serialized/";
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
		public bool Skip(Type type,string name,object value){
			if(!Serializer.defaults.ContainsKey(type) || !Serializer.defaults[type].ContainsKey(name)){
				if(this.debug.Has("SaveDetailed")){Debug.Log("[Serializer] : Skipping save for -- " + type + " -- " + name + " -- " + value);}
				return true;
			}
			var lastValue = Serializer.defaults[type][name];
			if(this.debug.Has("SaveDetailed")){Debug.Log("[Serializer] : " + type + "." + name + " -- " + lastValue + " = " + value);}
			if(lastValue.IsNull() || value.IsNull()){return lastValue == value;}
			return lastValue.Equals(value);
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
			if(Serializer.defaults.ContainsKey(type)){return;}
			if(this.debug.Has("BuildDetailed")){Debug.Log("[Serializer] : Building defaults for type -- " + type.Name);}
			Serializer.defaults.AddNew(type);
			foreach(var item in this.GetVariables(type)){
				Serializer.defaults[type][item.Key] = item.Value;
			}
		}
		//=================
		// Saving
		//=================
		[ContextMenu("Save")]
		public void Save(){
			this.Save(Assembly.Load("Assembly-CSharp"));
			this.Save(Assembly.Load("Assembly-CSharp-Editor"));
		}
		public void Save(Assembly assembly){
			foreach(Type type in assembly.GetTypes()){
				if(type.IsEnum || type == null || type.Name.Contains("_AnonStorey")){continue;}
				this.Save(type);
			}
		}
		public void Save(Type type){
			if(this.debug.Has("SaveType")){Debug.Log("[Serializer] : Serializing type -- " + type.Name);}
			this.tabs = 0;
			this.contents.Clear();
			var file = FileManager.Find(type.Name+".cs",true,false);
			string path = file != null ? file.folder+"/" : this.path;
			string filePath = path+type.Name+".static";
			this.Add(type.FullName,"{");
			bool empty = true;
			foreach(var item in this.GetVariables(type)){
				if(this.Skip(type,item.Key,item.Value)){continue;}
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
			if(File.Exists(filePath)){
				if(this.debug.Has("Save")){Debug.Log("[Serializer] : Removing " + type.Name + ".static");}
				File.Delete(filePath);
				File.Delete(filePath+".meta");
			}
		}
		public bool Save(string name,object value){
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
			this.LoadStatic();
			//this.LoadScene();
			//this.LoadInstance();
		}
		public void LoadStatic(){
			if(this.debug.Has("Load")){Debug.Log("[Serializer] : Loading .static files");}
			foreach(var file in FileManager.FindAll("*.static",true,false)){
				if(this.debug.Has("Load")){Debug.Log("[Serializer] : Loading "+file.fullName);}
				string contents = file.GetText();
				var type = Utility.GetType(contents.Parse("","{"));
				if(type.IsNull()){continue;}
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
						continue;
					}
				}
			}
		}
	}
}