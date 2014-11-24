using Zios;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
namespace Zios{
	public enum AttributeMode{Normal,Linked,Formula};
	public enum AttributeUsage{Direct,Shaped};
	[Serializable]
	public class Attribute{
		public static List<Attribute> all = new List<Attribute>();
		public static Dictionary<Attribute,bool> setWarning = new Dictionary<Attribute,bool>();
		public static Dictionary<Attribute,bool> getWarning = new Dictionary<Attribute,bool>();
		public static bool ready;
		public string path;
		public string id;
		public string localID;
		public Component parent;
		public AttributeMode mode = AttributeMode.Normal;
		public bool locked;
		public bool canFormula = true;
		public virtual AttributeData[] GetData(){return null;}
		public virtual void Clear(){}
		public virtual void Add(){}
		public virtual void Remove(AttributeData data){}
		public virtual void Setup(string path,Component parent){}
		public virtual void SetupTable(){}
		public virtual void SetupData(){}
	}
	[Serializable]
	public class Attribute<BaseType,Type,DataType,Operator,Special> : Attribute
	where Type : Attribute<BaseType,Type,DataType,Operator,Special>,new()
	where DataType : AttributeData<BaseType,Type,Operator,Special>,new(){
		public static bool editorStart;
		public static Type empty = new Type();
		public static Dictionary<GameObject,Dictionary<string,Type>> lookup = new Dictionary<GameObject,Dictionary<string,Type>>();
		public static Dictionary<GameObject,Dictionary<string,string>> resolve = new Dictionary<GameObject,Dictionary<string,string>>();
		public Func<BaseType> getMethod;
		public Action<BaseType> setMethod;
		public DataType[] data = new DataType[0];
		public Target target{
			get{return this.data[0].target;}
			set{this.data[0].target = value;}
		}
		public AttributeUsage usage{
			get{return this.data[0].usage;}
			set{
				foreach(DataType data in this.data){
					data.usage = value;
				}
			}
		}
		public override AttributeData[] GetData(){return this.data;}
		public virtual BaseType HandleSpecial(Special special,BaseType value){return value;}
		public virtual BaseType GetFormulaValue(){return default(BaseType);}
		public static Type Find(GameObject target,string name){
			if(lookup.ContainsKey(target)){
				if(lookup[target].ContainsKey(name)){
					return lookup[target][name];
				}
			}
			return empty;
		}
		public override void Add(){
			List<DataType> newData = new List<DataType>(this.data);
			newData.Add(new DataType());
			this.data = newData.ToArray();
		}
		public void Add(BaseType value){
			this.Add();
			this.data[this.data.Length-1].value = value;
		}
		public override void Remove(AttributeData data){
			List<DataType> newData = new List<DataType>(this.data);
			foreach(DataType current in newData.Copy()){
				AttributeData currentData = (AttributeData)current;
				if(currentData == data){newData.Remove(current);}
			}
			this.data = newData.ToArray();
		}
		public override void Setup(string path,Component parent){
			if(parent.IsNull()){return;}
			Attribute.all.RemoveAll(x=>x==this);
			this.parent = parent;
			this.path = path.AddRoot(parent);
			string previousID = this.id;
			this.localID = this.localID.IsEmpty() ? Guid.NewGuid().ToString() : this.localID;
			this.id = parent.GetInstanceID()+"/"+this.localID;
			if(!Application.isPlaying){
				bool changed = !previousID.IsEmpty() && this.id != previousID;
				if(this.mode == AttributeMode.Linked){this.usage = AttributeUsage.Shaped;}
				if(changed){
					GameObject root = Utility.FindPrefabRoot(parent.gameObject);
					if(!root.IsNull()){
						string name = root.gameObject.name;
						if(Locate.HasDuplicate(name)){
							Debug.Log("Attribute : Duplicate detected -- " + this.id + " -- " + previousID);
							char lastDigit = name[name.Length-1];
							if(name.Length > 1 && name[name.Length-2] == ' ' && char.IsLetter(lastDigit)){
								char nextLetter = (char)(char.ToUpper(lastDigit)+1);
								root.gameObject.name = name.TrimEnd(lastDigit) + nextLetter;
							}
							else{
								root.gameObject.name = name + " B";
							}
						}
					}
					var resolve = this.GetResolveTable();
					if(!resolve.ContainsKey(parent.gameObject)){
						resolve[parent.gameObject] = new Dictionary<string,string>();
					}
					resolve[parent.gameObject][previousID] = this.id;
					Utility.SetDirty(parent);
				}
			}
			foreach(var data in this.data){
				if(data.usage == AttributeUsage.Direct){continue;}
				data.target.Setup(this.path+"/Target",parent);
				data.target.DefaultSearch("[This]");
			}
			var exists = Attribute.all.Find(item=>item.id == this.id);
			if(exists.IsNull()){
				Attribute.all.Add(this);
			}
		}
		public override void SetupTable(){
			Type self = (Type)this;
			GameObject target = this.parent.gameObject;
			var lookup = this.GetLookupTable();
			if(!lookup.ContainsKey(target)){
				lookup[target] = new Dictionary<string,Type>();
			}
			lookup[target].RemoveValue(self);
			lookup[target][this.id] = self;
		}
		public override void SetupData(){
			var lookup = this.GetLookupTable();
			var resolve = this.GetResolveTable();
			foreach(var data in this.data){
				if(this.mode == AttributeMode.Linked){data.usage = AttributeUsage.Shaped;}
				if(data.usage == AttributeUsage.Direct){continue;}
				GameObject target = data.target.Get();
				if(data.reference.IsNull() && !data.referenceID.IsEmpty() && !target.IsNull()){
					if(!lookup.ContainsKey(target)){
						lookup[target] = new Dictionary<string,Type>();
					}
					if(!Application.isPlaying){
						if(resolve.ContainsKey(target) && resolve[target].ContainsKey(data.referenceID)){
							data.referenceID = resolve[target][data.referenceID];
						}
					}
					if(lookup[target].ContainsKey(data.referenceID)){
						data.reference = lookup[target][data.referenceID];
					}
				}
			}
		}
		public void AddScope(Component parent){
			if(parent.IsNull()){return;}
			var lookup = this.GetLookupTable();
			GameObject target = parent.gameObject;
			if(lookup.ContainsKey(target)){
				Type self = (Type)this;
				lookup[target].RemoveValue(self);
				lookup[target]["*/"+this.path] = self;
			}
		}
		public virtual BaseType Get(){
			if(this.getMethod != null){return this.getMethod();}
			DataType first = this.data[0];
			if(this.mode != AttributeMode.Formula){
				return this.GetValue(first);
			}
			return this.GetFormulaValue();
		}
		public virtual void Set(BaseType value){
			if(this.setMethod != null){
				this.setMethod(value);
				return;
			}
			if(this.data == null || this.data.Length < 1){
				if(!Attribute.setWarning.ContainsKey(this)){
					Debug.LogWarning("Attribute : No data found. (" + this.path + ")",this.target);
					Attribute.setWarning[this] = true;
				}
			}
			else if(this.mode == AttributeMode.Normal){
				if(this.usage == AttributeUsage.Shaped){
					this.usage = AttributeUsage.Direct;
				}
				this.data[0].value = value;
			}
			else if(this.mode == AttributeMode.Linked){
				if(!Attribute.ready && Application.isPlaying){
					Debug.LogWarning("Attribute : Set attempt before attribute data built -- " + this.path);
					return;
				}
				if(data[0].reference == null){
					if(!Attribute.setWarning.ContainsKey(this)){
						string source = "("+this.path+")";
						string goal = (data[0].target.Get().GetPath() + data[0].referencePath).Trim("/");
						Debug.LogWarning("Attribute (Set): No reference found for " + source + " to " + goal,this.parent);
						Attribute.setWarning[this] = true;
					}
					return;
				}
				this.data[0].reference.Set(value);
			}
			else if(this.mode == AttributeMode.Formula){
				if(!Attribute.setWarning.ContainsKey(this)){
					Debug.LogWarning("Attribute (Set): Cannot manually set values for formulas. (" + this.path + ")",this.parent);
					Attribute.setWarning[this] = true;
				}
			}
		}
		public virtual BaseType GetValue(DataType data){
			if(data.usage == AttributeUsage.Direct){
				return data.value;
			}
			else if(this.mode == AttributeMode.Linked || data.usage == AttributeUsage.Shaped){
				if(!Attribute.ready && Application.isPlaying){
					Debug.LogWarning("Attribute : Get attempt before attribute data built -- " + this.path,this.parent);
					return default(BaseType);
				}
				if(data.reference == null){
					if(!Attribute.getWarning.ContainsKey(this)){
						string source = "("+this.path+")";
						string goal = (data.target.Get().GetPath() + data.referencePath).Trim("/");
						Debug.LogWarning("Attribute (Get): No reference found for " + source + " to " + goal,this.parent);
						Attribute.getWarning[this] = true;
					}
					return default(BaseType);
				}
				else if(data.reference == this || data.reference.data[0].reference == this){
					if(!Attribute.getWarning.ContainsKey(this)){
						Debug.LogWarning("Attribute (Get): References self. (" + this.path + ")",this.parent);
						Attribute.getWarning[this] = true;
					}
					return default(BaseType);
				}
				BaseType value = data.reference.Get();
				if(this.mode == AttributeMode.Linked){return value;}
				return this.HandleSpecial(data.special,value);
			}
			if(!Attribute.getWarning.ContainsKey(this)){
				Debug.LogWarning("Attribute (Get): No value found. (" + this.path + ") to " + data.referencePath,this.parent);
				Attribute.getWarning[this] = true;
			}
			return default(BaseType);
		}
		public Dictionary<GameObject,Dictionary<string,Type>> GetLookupTable(){
			System.Type type = typeof(Attribute<BaseType,Type,DataType,Operator,Special>);
			object lookupObject = type.GetField("lookup",BindingFlags.Public|BindingFlags.Static).GetValue(null);
			return (Dictionary<GameObject,Dictionary<string,Type>>)lookupObject;
		}
		public Dictionary<GameObject,Dictionary<string,string>> GetResolveTable(){
			System.Type type = typeof(Attribute<BaseType,Type,DataType,Operator,Special>);
			object resolveObject = type.GetField("resolve",BindingFlags.Public|BindingFlags.Static).GetValue(null);
			return (Dictionary<GameObject,Dictionary<string,string>>)resolveObject;
		}
	}
	[Serializable]
	public class AttributeData{
		public Target target = new Target();
		public AttributeUsage usage;
		public string referenceID;
		public string referencePath;
	}
	[Serializable]
	public class AttributeData<BaseType,AttributeType,Operator,Special> : AttributeData
		where Operator : struct 
		where Special : struct{
		[NonSerialized] public AttributeType reference;
		public BaseType value;
		public Operator sign;
		public Special special;
		public bool clamp;
		public BaseType clampMin;
		public BaseType clampMax;
	}
}
