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
			if(!Application.isPlaying){
				Events.Add("On Scene Loaded",SerializerHook.Create).SetPermanent(true);
				Events.Add("On Hierarchy Changed",SerializerHook.Create).SetPermanent(true);
				SerializerHook.Create();
			}
		}
		public static void Create(){
			if(!Application.isPlaying && Serializer.instance.IsNull()){
				var path = Locate.GetScenePath("@Main");
				if(!path.HasComponent<Serializer>()){
					Debug.Log("[Serializer] : Auto-creating Serializer Manager GameObject.");
					Serializer.instance = path.AddComponent<Serializer>();
				}
				Serializer.instance = path.GetComponent<Serializer>();
				Events.Add("On Asset Saving",Serializer.instance.Save);
				Events.Add("On Asset Changed",Serializer.instance.Load);
				Events.Add("On Scene Loaded",Serializer.instance.Load);
			}
			Serializer.instance.Setup();
		}
	}
	#endif
	[Flags] 
	public enum SerializerDebug : int{
		Build        = 0x001,
		Save         = 0x002,
		Load         = 0x004,
	}
	[Serializable]
	public class DefaultType{
		public string name;
		public Type type;
		public List<DefaultVariable> variables = new List<DefaultVariable>();
		public DefaultType(Type type){
			this.type = type;
			this.name = type.Name;
		}
	}
	[Serializable]
	public class DefaultVariable{
		public string name;
		public object value;
		public DefaultVariable(string name,object value){
			this.name = name;
			this.value = value;
		}
	}
	[ExecuteInEditMode]
	public class Serializer : MonoBehaviour{
		public static Serializer instance;
		public static Serializer Get(){return Serializer.instance;}
		[EnumMask] public SerializerDebug debug;
		public bool separateStatic = true;
		private StringBuilder contents = new StringBuilder();
		private int tabs;
		private string path;
		public List<DefaultType> defaults = new List<DefaultType>();
		//public Dictionary<Type,Dictionary<string,object>> defaults = new Dictionary<Type,Dictionary<string,object>>();
		[ContextMenu("Refresh")]
		public void Setup(){
			this.path = Application.dataPath+"/@Serialized/";
			this.defaults.Clear();
			this.BuildDefault();
		}
		public void Awake(){this.Load();}
		public void BuildDefault(){
			this.BuildDefault(Assembly.Load("Assembly-CSharp"));
			this.BuildDefault(Assembly.Load("Assembly-CSharp-Editor"));
		}
		public void BuildDefault(Assembly assembly){
			if(this.debug.Has("Build")){Debug.Log("[Serializer] : Building defaults for assembly -- " + assembly.FullName.Split(",")[0]);}
			foreach(Type type in assembly.GetTypes()){
				if(type == null || type.Name.Contains("_AnonStorey")){continue;}
				this.BuildDefault(type);
			}
		}
		public void BuildDefault(Type type){
			if(this.defaults.Exists(x=>x.type==type)){return;}
			if(this.debug.Has("Build")){Debug.Log("[Serializer] : Building defaults for type -- " + type.Name);}
			var entry = new DefaultType(type);
			foreach(var item in type.GetVariables(null,typeof(NonSerializedAttribute).AsList(),ObjectExtension.staticFlags)){
				entry.variables.Add(new DefaultVariable(item.Key,item.Value));
			}
			this.defaults.Add(entry);
		}
		[ContextMenu("Save")]
		public void Save(){
			this.Save(Assembly.Load("Assembly-CSharp"));
			this.Save(Assembly.Load("Assembly-CSharp-Editor"));
		}
		public void Save(Assembly assembly){
			if(this.debug.Has("Save")){Debug.Log("[Serializer] : Serializing assembly -- " + assembly.FullName.Split(",")[0]);}
			foreach(Type type in assembly.GetTypes()){
				if(type == null || type.Name.Contains("_AnonStorey")){continue;}
				this.Save(type);
			}
		}
		public void Save(Type type){
			this.tabs = 0;
			this.contents.Clear();
			var file = FileManager.Find(type.Name+".cs",true,false);
			string path = file != null ? file.folder+"/" : this.path;
			this.Add(type.FullName,"{");
			this.tabs += 1;
			bool empty = true;
			this.BuildDefault(type);
			foreach(var item in type.GetVariables(null,typeof(NonSerializedAttribute).AsList(),ObjectExtension.staticFlags)){
				var entry = this.defaults.Find(x=>x.type==type);
				if(entry.IsNull() || item.Key.IsNull() || item.Value.IsNull()){continue;}
				string name = item.Key;
				object value = item.Value;
				object lastValue = entry.variables.First(x=>x.name==name).value;
				if(lastValue.Equals(value)){continue;}
				if(this.debug.Has("Save")){Debug.Log("[Serializer] : " + type.Name + "." + name + " = " + value);}
				this.Save(name,value);
				empty = false;
			}
			this.tabs -= 1;
			this.Add("}");
			if(!empty){
				Directory.CreateDirectory(path);
				File.WriteAllText(path+type.Name+".static",this.contents.ToString());
			}
		}
		public void Save(string name,object value){
			var type = value.GetType();
			if(type.IsValueType){
				this.Add(name," = ",value.ToString());
				return;
			}
			if(value is IDictionary){return;}
			if(value is IEnumerable){return;}
			foreach(var item in value.GetVariables(null,typeof(NonSerializedAttribute).AsList(),ObjectExtension.publicFlags)){
				this.Add("{");
				this.tabs += 1;
				if(!this.defaults.Exists(x=>x.type==type)){
					try{
						var instance = Activator.CreateInstance(type);
						var entry = new DefaultType(type);
						foreach(var instanceItem in instance.GetVariables(null,typeof(NonSerializedAttribute).AsList(),ObjectExtension.publicFlags)){
							entry.variables.Add(new DefaultVariable(instanceItem.Key,instanceItem.Value));
						}
						this.defaults.Add(entry);
					}
					catch{}
				}
				this.Save(item.Key,item.Value);
			}
			this.tabs -= 1;
			this.Add("}");
		}
		public void Add(params string[] contents){
			string tabs = new String('\t',this.tabs);
			this.contents.Append(tabs);
			foreach(var data in contents){this.contents.Append(data);}
			this.contents.Append("\n");
		}
		[ContextMenu("Load")]
		public void Load(){
			this.LoadStatic();
			//this.LoadScene();
			//this.LoadInstance();
		}
		public void LoadStatic(){
			if(this.debug.Has("Load")){Debug.Log("[Serializer] : Loading .static files");}
			foreach(var file in FileManager.FindAll("*.static")){
				if(this.debug.Has("Load")){Debug.Log("[Serializer] : Loading "+file.fullName);}
				string contents = file.GetText();
				var type = Type.GetType(contents.Parse("","{"));
				foreach(string line in contents.Split("\n").Skip(1)){
					if(line.IsEmpty() || line.ContainsAny("{","}")){continue;}
					string name = line.Parse("","=");
					string value = line.Parse("=","");
					var dataType = type.GetVariableType(name);
					Debug.Log(name + " -- " + value + " -- " + dataType);
					var accessor = new Accessor(type,name);
					if(dataType == typeof(string)){type.SetVariable(name,value);}
					if(dataType == typeof(int)){type.SetVariable(name,value.ToInt());}
					if(dataType == typeof(float)){type.SetVariable(name,value.ToFloat());}
					if(dataType == typeof(bool)){type.SetVariable(name,value.ToBool());}
					if(dataType.IsEnum){
						//Debug.Log(type + " -- " + name + " -- " + Enum.Parse(dataType,value));
						type.SetVariable(name,Enum.Parse(dataType,value));
						//accessor.Set(-1);
					}	
				}
			}
		}
	}
}