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
		public static Dictionary<GameObject,Dictionary<string,Attribute>> lookup = new Dictionary<GameObject,Dictionary<string,Attribute>>();
		public static Dictionary<GameObject,Dictionary<string,string>> resolve = new Dictionary<GameObject,Dictionary<string,string>>();
		public static Dictionary<Attribute,bool> setWarning = new Dictionary<Attribute,bool>();
		public static Dictionary<AttributeData,bool> getWarning = new Dictionary<AttributeData,bool>();
		public static bool ready = false;
		public string path;
		public string id;
		public string localID;
		public Component parent;
		public AttributeMode mode = AttributeMode.Normal;
		public AttributeData[] data = new AttributeData[0];
		public Type dataType;
		[NonSerialized] public bool locked;
		[NonSerialized] public bool showInEditor = true;
		[NonSerialized] public bool canFormula = true;
		[NonSerialized] public bool canDirect = true;
		[NonSerialized] public bool canShape = true;
		[NonSerialized] public bool canLink = true;	
		public virtual Type[] GetFormulaTypes(){return null;}
		public virtual AttributeData[] GetData(){return null;}
		public virtual void Clear(){}
		public virtual void Add<Type>() where Type : AttributeData{}
		public virtual void Remove(AttributeData data){}
		public virtual void Setup(string path,Component parent){}
		public virtual void SetupTable(){}
		public virtual void SetupData(){}
	}
	[Serializable]
	public class Attribute<BaseType,AttributeType,DataType,Operator,Special> : Attribute
	where AttributeType : Attribute<BaseType,AttributeType,DataType,Operator,Special>
	where DataType : AttributeData<BaseType,AttributeType,DataType,Operator,Special>{
		public static bool editorStart;
		protected BaseType delayedValue = default(BaseType);
		public Func<BaseType> getMethod;
		public Action<BaseType> setMethod;
		public BaseType value{
			get{return this.GetFirst().value;}
			set{this.GetFirst().value = value;}
		}
		public Target target{
			get{return this.GetFirst().target;}
			set{this.GetFirst().target = value;}
		}
		public AttributeUsage usage{
			get{return this.GetFirst().usage;}
			set{
				foreach(AttributeData data in this.data){
					data.usage = value;
				}
			}
		}
		public override AttributeData[] GetData(){return this.data;}
		public void Prepare(){
			if(this.data.Length < 1){
				BaseType value = this.delayedValue != null ? this.delayedValue : default(BaseType);
				if(this.delayedValue == null){
					Debug.Log("Attribute : Fixing unprepared data -- " + this.path);
				}
				this.Add(value);
			}
		}
		public DataType GetFirst(){
			if(!Application.isPlaying){
				if(this.data.Length > 0 && this.data[0].IsNull()){
					Debug.Log("Attribute : Purging null data -- " + this.path);
					this.data = this.data.RemoveAt(0);
				}
				this.Prepare();
			}
			return (DataType)this.data[0];
		}
		public virtual BaseType GetFormulaValue(){return default(BaseType);}
		public override Type[] GetFormulaTypes(){return new Type[]{typeof(AttributeData)};}
		public void SetDefault(BaseType value){
			this.Prepare();
			BaseType current = this.GetFirst().value;
			if(current.IsEmpty()){
				this.value = value;
			}
		}
		public override void Add<Type>(){
			if(this.parent != null){
				AttributeData data = this.parent.gameObject.AddComponent<Type>();
				data.hideFlags = HideFlags.HideInInspector;
				data.attribute = this;
				this.data = this.data.Add(data);
			}
		}
		public void Add(BaseType value){
			if(this.parent != null){
				this.Add<DataType>();
				DataType last = (DataType)this.data[this.data.Length-1];
				last.value = value;
			}
		}
		public override void Remove(AttributeData data){
			foreach(AttributeData current in this.data.Copy()){
				if(current == data){
					Debug.Log("Attribute : Removing attribute data -- " + this.path + " -- " + data);
					this.data.Remove(current);
					Utility.Destroy(current);
				}
			}
		}
		public override void Setup(string path,Component parent){
			if(parent.IsNull()){return;}
			this.dataType = typeof(DataType);
			this.parent = parent;
			this.path = path.AddRoot(parent);
			string previousID = this.id;
			this.localID = this.localID.IsEmpty() ? Guid.NewGuid().ToString() : this.localID;
			this.id = parent.GetInstanceID()+"/"+this.localID;
			if(!Application.isPlaying){
				Attribute.all.RemoveAll(x=>x==this);
				for(int index=0;index<this.data.Length;++index){
					AttributeData data = this.data[index];
					if(data.IsNull()){
						Debug.Log("Attribute : Removing null attribute data in " + this.path + ".");
						this.data = this.data.RemoveAt(index);
						index -= 1;
						continue;
					}
					bool wrongParent = (data.attribute != this) || (data.attribute.id != this.id);
					bool emptyParent = data.attribute.parent.IsNull();
					bool emptyAttribute = data.attribute.IsNull();
					if(wrongParent || emptyParent || emptyAttribute){
						this.data[index].attribute = this;
						Utility.SetDirty(this.data[index]);
					}
				}
				bool changed = !previousID.IsEmpty() && this.id != previousID;
				GameObject root = Utility.FindPrefabRoot(parent.gameObject);
				if(!root.IsNull()){
					string name = root.gameObject.name;
					if(Locate.HasDuplicate(name)){
						Debug.Log("Attribute : Duplicate detected -- " + this.path,this.parent.gameObject);
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
				if(this.mode == AttributeMode.Linked){this.usage = AttributeUsage.Shaped;}
				if(changed){
					var resolve = Attribute.resolve;
					if(!resolve.ContainsKey(parent.gameObject)){
						resolve[parent.gameObject] = new Dictionary<string,string>();
					}
					resolve[parent.gameObject][previousID] = this.id;
				}
			}
			this.Prepare();
			for(int index=0;index<this.data.Length;++index){
				AttributeData data = this.data[index];
				if(!Application.isPlaying){data.reference = null;}
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
			AttributeType self = (AttributeType)this;
			GameObject target = this.parent.gameObject;
			var lookup = Attribute.lookup;
			lookup.AddNew(target);
			lookup[target].RemoveValue(self);
			lookup[target][this.id] = self;
		}
		public override void SetupData(){
			var lookup = Attribute.lookup;
			var resolve = Attribute.resolve;
			foreach(AttributeData data in this.data){
				if(data.IsNull()){continue;}
				if(this.mode == AttributeMode.Linked){data.usage = AttributeUsage.Shaped;}
				if(data.usage == AttributeUsage.Direct){continue;}
				GameObject target = data.target.Get();
				if(data.reference.IsNull() && !data.referenceID.IsEmpty() && !target.IsNull()){
					lookup.AddNew(target);
					if(!Application.isPlaying){
						if(resolve.ContainsKey(target) && resolve[target].ContainsKey(data.referenceID)){
							data.referenceID = resolve[target][data.referenceID];
						}
					}
					if(lookup[target].ContainsKey(data.referenceID)){
						data.reference = lookup[target][data.referenceID];
					}
					if(data.reference.IsNull() && !data.referencePath.IsEmpty()){
						var entries = lookup[target];
						foreach(var attribute in entries){
							if(attribute.Value.path == data.referencePath){
								Debug.Log("Attribute : ID missing.  Resolved via path -- " + data.referencePath,this.parent.gameObject);
								data.referenceID = attribute.Value.id;
								data.reference = attribute.Value;
								break;
							}
						}
					}
				}
			}
		}
		public void AddScope(Component parent){
			if(parent.IsNull()){return;}
			var lookup = Attribute.lookup;
			GameObject target = parent.gameObject;
			if(lookup.ContainsKey(target)){
				AttributeType self = (AttributeType)this;
				lookup[target].RemoveValue(self);
				lookup[target]["*/"+this.path] = self;
			}
		}
		public virtual BaseType Get(){
			if(this.data.Length < 1){return default(BaseType);}
			if(this.getMethod != null){return this.getMethod();}
			if(this.mode != AttributeMode.Formula){
				return this.GetFirst().Get();
			}
			return this.GetFormulaValue();
		}
		public virtual void Set(BaseType value){
			DataType data = this.GetFirst();
			if(this.setMethod != null){
				this.setMethod(value);
				return;
			}
			if(data == null){
				if(!Attribute.setWarning.ContainsKey(this)){
					Debug.LogWarning("Attribute : No data found. (" + this.path + ")",this.target);
					Attribute.setWarning[this] = true;
				}
			}
			else if(this.mode == AttributeMode.Normal){
				if(this.usage == AttributeUsage.Shaped){
					this.usage = AttributeUsage.Direct;
				}
				data.value = value;
			}
			else if(this.mode == AttributeMode.Linked){
				if(!Attribute.ready && Application.isPlaying){
					Debug.LogWarning("Attribute : Set attempt before attribute data built -- " + this.path);
					return;
				}
				if(data.reference == null){
					if(!Attribute.setWarning.ContainsKey(this)){
						string source = "("+this.path+")";
						string goal = (data.target.Get().GetPath() + data.referencePath).Trim("/");
						Debug.LogWarning("Attribute (Set): No reference found for " + source + " to " + goal,this.parent);
						Attribute.setWarning[this] = true;
					}
					return;
				}
				((AttributeType)data.reference).Set(value);
			}
			else if(this.mode == AttributeMode.Formula){
				if(!Attribute.setWarning.ContainsKey(this)){
					Debug.LogWarning("Attribute (Set): Cannot manually set values for formulas. (" + this.path + ")",this.parent);
					Attribute.setWarning[this] = true;
				}
			}
		}

	}
	[Serializable]
	public class AttributeData : DataMonoBehaviour{
		public Target target = new Target();
		public AttributeUsage usage;
		public string referenceID;
		public string referencePath;
		public Attribute attribute;
		[NonSerialized] public Attribute reference;
		public override void Awake(){
			if(!this.attribute.IsNull()){
				bool wrongParent = !this.attribute.data.Contains(this);
				bool emptyParent = this.attribute.parent.IsNull();
				bool emptyRoot = emptyParent || this.attribute.parent.gameObject.IsNull();
				if(wrongParent || emptyParent || emptyRoot){
					//Debug.Log("AttributeData : Clearing defunct data.");
					Utility.Destroy(this);
				}
			}
		}
		public virtual AttributeData Copy(GameObject target){return default(AttributeData);}
	}
	[Serializable]
	public class AttributeData<BaseType,AttributeType,DataType,Operator,Special> : AttributeData
		where DataType : AttributeData<BaseType,AttributeType,DataType,Operator,Special>
		where AttributeType : Attribute<BaseType,AttributeType,DataType,Operator,Special>
		where Operator : struct 
		where Special : struct{
		public BaseType value;
		public Operator sign;
		public Special special;
		/*public bool clamp;
		public BaseType clampMin;
		public BaseType clampMax;*/
		public virtual BaseType HandleSpecial(){return default(BaseType);}
		public override AttributeData Copy(GameObject target){
			DataType data = target.AddComponent<DataType>();
			data.target = this.target.Clone();
			data.usage = this.usage;
			data.referenceID = this.referenceID;
			data.referencePath = this.referencePath;
			data.sign = this.sign;
			data.special = this.special;
			data.value = this.value;
			return data;
		}
		public BaseType Get(){
			Attribute attribute = this.attribute;
			if(this.usage == AttributeUsage.Direct){
				if(attribute.mode == AttributeMode.Formula){
					return this.HandleSpecial();
				}
				return this.value;
			}
			else if(attribute.mode == AttributeMode.Linked || this.usage == AttributeUsage.Shaped){	
				if(!Attribute.ready && Application.isPlaying){
					Debug.LogWarning("Attribute : Get attempt before attribute data built -- " + attribute.path,attribute.parent);
					return default(BaseType);
				}
				else if(this.reference == null){
					if(!Attribute.getWarning.ContainsKey(this)){
						string source = "("+attribute.path+")";
						string goal = (this.target.Get().GetPath() + this.referencePath).Trim("/");
						Debug.LogWarning("Attribute (Get): No reference found for " + source + " to " + goal,attribute.parent);
						Attribute.getWarning[this] = true;
					}
					return default(BaseType);
				}
				else if(this.reference == attribute){
					if(!Attribute.getWarning.ContainsKey(this)){
						Debug.LogWarning("Attribute (Get): References self. (" + attribute.path + ")",attribute.parent);
						Attribute.getWarning[this] = true;
					}
					return default(BaseType);
				}
				BaseType value = ((AttributeType)this.reference).Get();
				if(attribute.mode == AttributeMode.Linked){return value;}
				return this.HandleSpecial();
			}
			if(!Attribute.getWarning.ContainsKey(this)){
				Debug.LogWarning("Attribute (Get): No value found. (" + attribute.path + ") to " + this.referencePath,attribute.parent);
				Attribute.getWarning[this] = true;
			}
			return default(BaseType);
		}
	}
}
