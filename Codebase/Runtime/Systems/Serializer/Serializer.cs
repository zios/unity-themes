using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Zios.Serializer{
	using Zios.Events;
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.File;
	using Zios.Reflection;
	using Zios.Shortcuts;
	using Zios.Supports.Hierarchy;
	using Zios.Unity.Extensions;
	using Zios.Unity.Locate;
	using Zios.Unity.Supports.Singleton;
	using Zios.Unity.SystemAttributes;
	//asm Zios.Unity.Shortcuts;
	[InitializeOnLoad]
	public class Serializer : Singleton{
		public static Serializer singleton;
		public static string scenePath;
		public static string referencePath;
		public string path = "[shared]/.Serialized/";
		public SerializeFormat format = new SerializeFormat();
		public static Serializer Get(){
			Serializer.singleton = Serializer.singleton ?? Singleton.Get<Serializer>();
			return Serializer.singleton;
		}
		public void OnEnable(){
			Serializer.singleton = this;
			Events.Add<Method>("On Awake",this.format.LoadScene);
			Events.Add<Method>("On Enter Play",this.format.SaveScene);
			Events.Add<Method>("On Editor Quit",this.format.SaveScene);
		}
	}
	public class SerializeFormat{
		public Dictionary<object,string> hashes = new Dictionary<object,string>();
		public Dictionary<string,object> references = new Dictionary<string,object>();
		public Hierarchy<object,string,string> links = new Hierarchy<object,string,string>();
		private StringBuilder buffer = new StringBuilder();
		//===================
		// Options
		//===================
		public virtual bool ChangesOnly(){return false;}
		public virtual string Header(){return "[Default]";}
		public virtual string Hash(string value){return "["+value+"]";}
		public virtual string Separator(){return "\n";}
		public virtual string SeparatorItem(){return ",";}
		//===================
		// Saving
		//===================
		public virtual string Format(string name,string value){
			if(value.IsEmpty()){return "";}
			this.buffer.Clear();
			this.buffer.Append(name," = ",value,this.Separator());
			return this.buffer.ToString();
		}
		public virtual string Save(string name,string value){return this.Format(name,value.Serialize(this.ChangesOnly()));}
		public virtual string Save(string name,byte value){return this.Format(name,value.Serialize(this.ChangesOnly()));}
		public virtual string Save(string name,short value){return this.Format(name,value.Serialize(this.ChangesOnly()));}
		public virtual string Save(string name,int value){return this.Format(name,value.Serialize(this.ChangesOnly()));}
		public virtual string Save(string name,float value){return this.Format(name,value.Serialize(this.ChangesOnly()));}
		public virtual string Save(string name,double value){return this.Format(name,value.Serialize(this.ChangesOnly()));}
		public virtual string Save(string name,bool value){return this.Format(name,value.Serialize(this.ChangesOnly()));}
		public virtual string Save(string name,Enum value){return this.Format(name,value.Serialize(this.ChangesOnly()));}
		public virtual string Save<Type>(string name,Type value){return this.Format(name,value.CallExactMethod<string>("Serialize",this.ChangesOnly()));}
		public virtual string Save<Type>(string name,Type[] value){return this.Format(name,value.Serialize(this.SeparatorItem(),this.ChangesOnly()));}
		public virtual string Save<Type>(string name,List<Type> value){return this.Format(name,value.Serialize(this.SeparatorItem(),this.ChangesOnly()));}
		public virtual string Save<Key,Value>(string name,Dictionary<Key,Value> value){return this.Format(name,value.Serialize(this.SeparatorItem(),this.ChangesOnly()));}
		public virtual string Save(string name,object value){
			if(value is string){return this.Save(name,value.As<string>());}
			if(value is byte){return this.Save(name,value.As<byte>());}
			if(value is short){return this.Save(name,value.As<short>());}
			if(value is int){return this.Save(name,value.As<int>());}
			if(value is float){return this.Save(name,value.As<float>());}
			if(value is double){return this.Save(name,value.As<double>());}
			if(value is bool){return this.Save(name,value.As<bool>());}
			if(value is Enum){return this.Save(name,value.As<Enum>());}
			if(value.IsArray()){return this.Save(name,value.As<Array>().Cast<object>().ToArray());}
			if(value.IsList()){return this.Save(name,value.As<List<object>>());}
			if(value.IsDictionary()){return this.Save(name,value.As<Dictionary<object,object>>());}
			return this.SaveReference(name,value);
		}
		public virtual string SaveReference(string name,object value){
			var output = new StringBuilder();
			foreach(var variable in value.GetVariables()){
				var key = variable.Key;
				var data = variable.Value;
				output.Append(this.Save(key,data));
			}
			var contents = output.ToString().TrimRight(this.Separator());
			if(!contents.IsEmpty()){
				var hash = this.GetHash(value);
				var typeName = "["+value.GetType().FullName +"]";
				output.Prepend(typeName,this.Separator());
				this.GetFile(value).Write(output.ToString().Trim(this.Separator()));
				return this.Format(name,"@"+hash);
			}
			return "";
		}
		public virtual string GetHash(object target){
			if(!this.hashes.ContainsKey(target)){
				this.hashes[target] = target.GetHashCode().ToString().Replace("-","n");
			}
			return this.hashes[target];
		}
		public virtual void SaveScene(){this.SaveScene("");}
		public virtual void SaveScene(string state){
			this.SetPath(state);
			Debug.Log("Saving " + Serializer.scenePath);
			foreach(var script in Locate.GetSceneComponents<MonoBehaviour>()){
				var serializable = script.GetAttributedVariables(typeof(Store).AsArray());
				if(serializable.Count < 1){continue;}
				var output = new StringBuilder();
				foreach(var target in serializable){
					var name = target.Key;
					var value = target.Value;
					output.Append(this.Save(name,value));
				}
				var contents = output.ToString().TrimRight(this.Separator());
				if(!contents.IsEmpty()){
					var id = script.GetInstanceID().ToString().Replace("-","n");
					output.Prepend(this.Hash(id),this.Separator());
					this.GetFile(script).Write(output.ToString().Trim(this.Separator()));
				}
			}
			File.CheckSave();
		}
		//===================
		// Loading
		//===================
		public virtual KeyValuePair<string,string> Parse(string line){
			var data = line.Split("=");
			var name = data[0].Trim();
			var value = data[1].Trim();
			return new KeyValuePair<string,string>(name,value);
		}
		public virtual Type Load<Type>(object scope,string name,string value){
			var result = value.Deserialize<Type>();
			scope.SetVariable(name,result);
			return result;
		}
		public virtual object Load(object scope,string name,string value,Type enumType){
			var result = value.ToEnum(enumType);
			scope.SetVariable(name,result);
			return result;
		}
		public virtual void LoadScene(){
			this.SetPath();
			Debug.Log("Loading " + Serializer.scenePath + " | " + Serializer.referencePath);
			this.LoadReferences();
			this.LoadInstances();
			this.LinkReferences();
		}
		public virtual void LoadReferences(){
			var references = File.FindAll(Serializer.referencePath+"/",false);
			foreach(var referenceFile in references){
				object target = null;
				var lines = referenceFile.ReadText().Split(this.Separator());
				var typeName = lines.First().Trim("[","]");
				Type type = Type.GetType(typeName);
				target = Activator.CreateInstance(type);
				foreach(var line in lines.Skip(1)){
					if(line.Trim().IsEmpty()){continue;}
					var data = this.Parse(line);
					this.LoadValue(target,data.Key,data.Value);
				}
				Debug.Log("Creating Reference -- |" + referenceFile.name+"|");
				this.references[referenceFile.name] = target;
			}
		}
		public virtual void LoadInstances(){
			var instances = File.FindAll(Serializer.scenePath+"/",false);
			var lookup = new Dictionary<int,MonoBehaviour>();
			foreach(var script in Locate.GetSceneComponents<MonoBehaviour>()){
				lookup[script.GetInstanceID()] = script;
			}
			foreach(var instanceFile in instances){
				Debug.Log("Parsing : " + instanceFile.path);
				var lines = instanceFile.ReadText().Split(this.Separator());
				var instanceID = lines.First().Trim("[","]").ToInt();
				if(lookup.ContainsKey(instanceID)){
					var instance = lookup[instanceID];
					foreach(var line in lines.Skip(1)){
						if(line.Trim().IsEmpty()){continue;}
						var data = this.Parse(line);
						this.LoadValue(instance,data.Key,data.Value);
					}
				}
			}
		}
		public virtual void LinkReferences(){
			foreach(var target in this.links){
				foreach(var data in target.Value){
					var name = data.Key;
					var id = data.Value;
					Debug.Log("Assigning Reference -- |" + this.references[id]+"|");
					target.Key.SetVariable(name,this.references[id]);
					this.hashes[this.references[id]] = id.Replace("-","n");
				}
			}
		}
		public virtual object LoadValue(object target,string name,string value){
			if(target.HasVariable(name)){
				var type = target.GetVariableType(name);
				if(type.Is<string>()){return this.Load<string>(target,name,value);}
				else if(type.Is<byte>()){return this.Load<byte>(target,name,value);}
				else if(type.Is<short>()){return this.Load<short>(target,name,value);}
				else if(type.Is<int>()){return this.Load<int>(target,name,value);}
				else if(type.Is<float>()){return this.Load<float>(target,name,value);}
				else if(type.Is<double>()){return this.Load<double>(target,name,value);}
				else if(type.Is<bool>()){return this.Load<bool>(target,name,value);}
				else if(type.IsEnum()){return this.Load(target,name,value,type);}
				else if(value.StartsWith("@")){
					var id = value.Trim("@");
					Debug.Log("Assigning Link -- |" + id+"|");
					this.links.AddNew(target)[name] = id;
				}
			}
			return null;
		}
		//===================
		// Utility
		//===================
		public virtual string SetPath(string state=""){
			var path =  Serializer.Get().path;
			path = path.Replace("[shared]",File.dataPath,true);
			path = path.Replace("[data]",Application.dataPath,true);
			path = path.Replace("[persistentData]",Application.persistentDataPath,true);
			path = path.Replace("[streamingAssets]",Application.streamingAssetsPath,true);
			path = path.Replace("[temporaryCache]",Application.temporaryCachePath,true);
			Serializer.referencePath = path+"[References]";
			state = state.IsEmpty() ? "" : "-" + state;
			path += SceneManager.GetActiveScene().name + state;
			Serializer.scenePath = path;
			return path;
		}
		public virtual FileData GetFile(object value){
			var hash = this.GetHash(value);
			var path = Serializer.referencePath+"/"+hash+"."+value.GetType().Name.ToCamelCase();
			return File.AddNew(path);
		}
		public virtual FileData GetFile(MonoBehaviour script){
			var fileName = script.GetPath(false).Replace(".","_").Replace("/",".").TrimLeft(".") + script.GetType().Name.ToCamelCase();
			return File.AddNew(Serializer.scenePath+"/"+fileName);
		}
	}
	public class XMLFormat : SerializeFormat{
		public override string Header(){return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";}
		public override string Format(string name,string value){return "<"+name+">"+value+"</"+name+">";}
		public override KeyValuePair<string,string> Parse(string line){
			var name = line.Split(">").First().Trim("<");
			var value = line.Split(">").Skip(1).ToString().Split("<").First();
			return new KeyValuePair<string,string>(name,value);
		}
	}
	public class Store : Attribute{}
}