using Zios;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
namespace Zios{
	public enum AttributeMode{Normal,Linked,Formula};
	public enum AttributeUsage{Direct,Shaped};
	[Serializable]
	public class AttributeInfo{
		public string path;
		public string id;
		public string localID;
		public Type dataType;
		public Component parent;
		public AttributeMode mode = AttributeMode.Normal;
		public AttributeData[] data = new AttributeData[0];
	}
	[Serializable]
	public class Attribute{
		public static List<Attribute> all = new List<Attribute>();
		public static Dictionary<GameObject,Dictionary<string,Attribute>> lookup = new Dictionary<GameObject,Dictionary<string,Attribute>>();
		public static Dictionary<GameObject,Dictionary<string,string>> resolve = new Dictionary<GameObject,Dictionary<string,string>>();
		public static Dictionary<Attribute,bool> setWarning = new Dictionary<Attribute,bool>();
		public static Dictionary<AttributeData,bool> getWarning = new Dictionary<AttributeData,bool>();
		public static bool ready = false;
		public AttributeInfo info;
		public AttributeData[] data{
			get{return this.info.data;}
			set{this.info.data = value;}
		}
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
		public virtual void BuildLookup(){}
		public virtual void BuildData(){}
	}
	[Serializable]
	public class Attribute<BaseType,AttributeType,DataType,Special> : Attribute
	where AttributeType : Attribute<BaseType,AttributeType,DataType,Special>
	where DataType : AttributeData<BaseType,AttributeType,DataType,Special>{
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
				foreach(AttributeData data in this.info.data){
					data.usage = value;
				}
			}
		}
		// ======================
		// Shortcuts
		// ======================
		public virtual BaseType GetFormulaValue(){return default(BaseType);}
		public override Type[] GetFormulaTypes(){return new Type[]{typeof(AttributeData)};}
		public override AttributeData[] GetData(){return this.info.data;}
		public DataType GetFirst(){
			if(!Application.isPlaying){
				this.PrepareData();
			}
			return (DataType)this.info.data[0];
		}
		public void SetDefault(BaseType value){
			this.PrepareData();
			BaseType current = this.GetFirst().value;
			if(current.IsEmpty()){
				this.value = value;
			}
		}
		// ======================
		// Building
		// ======================
		public override void Add<Type>(){
			if(this.info.parent != null){
				AttributeData data = this.info.parent.gameObject.AddComponent<Type>();
				data.hideFlags = HideFlags.HideInInspector;
				data.attribute = this.info;
				this.info.data = this.info.data.Add(data);
				Utility.SetDirty(data);
				Utility.SetDirty(this.info.parent);
			}
		}
		public void Add(BaseType value){
			if(this.info.parent != null){
				this.Add<DataType>();
				DataType last = (DataType)this.info.data[this.info.data.Length-1];
				last.value = value;
			}
		}
		public override void Remove(AttributeData data){
			foreach(AttributeData current in this.info.data.Copy()){
				if(current == data){
					Debug.Log("[Attribute] Removing attribute data : " + this.info.path + " : " + data);
					this.info.data.Remove(current);
					Utility.Destroy(current);
				}
			}
		}
		// ======================
		// Setup
		// ======================
		public override void Setup(string path,Component parent){
			if(parent.IsNull()){return;}
			this.info.dataType = typeof(DataType);
			if(!Application.isPlaying){
				this.info.parent = parent;
				this.info.path = path.AddRoot(parent);
				string previousID = this.info.id;
				this.info.localID = this.info.localID.IsEmpty() ? Guid.NewGuid().ToString() : this.info.localID;
				this.info.id = parent.GetInstanceID()+"/"+this.info.localID;
				//Attribute.all.RemoveAll(x=>x==this);
				this.FixDuplicates();
				this.FixIDConflict(previousID);
				Utility.SetDirty(parent);
			}
			this.PrepareData();
			if(!Attribute.all.Contains(this)){
				Attribute.all.Add(this);
			}
		}
		public override void BuildLookup(){
			AttributeType self = (AttributeType)this;
			GameObject target = this.info.parent.gameObject;
			var lookup = Attribute.lookup;
			lookup.AddNew(target);
			lookup[target].RemoveValue(self);
			lookup[target][this.info.id] = self;
		}
		public override void BuildData(){
			var lookup = Attribute.lookup;
			var resolve = Attribute.resolve;
			foreach(AttributeData data in this.info.data){
				if(data.IsNull()){continue;}
				if(this.info.mode == AttributeMode.Linked){data.usage = AttributeUsage.Shaped;}
				if(data.usage == AttributeUsage.Direct){continue;}
				GameObject target = data.target.Get();
				if(data.reference.IsNull() && !data.referenceID.IsEmpty() && !target.IsNull()){
					if(!lookup.ContainsKey(target)){
						Debug.Log("[Attribute] Lookup attempted on -- " + target + " before table built");
						break;
					}
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
						bool resolved = false;
						foreach(var attribute in entries){
							if(attribute.Value.info.path == data.referencePath){
								Debug.Log("[Attribute] ID missing : " + data.attribute.path + ".  Resolved via path : " + data.referencePath,this.info.parent.gameObject);
								data.referenceID = attribute.Value.info.id;
								data.reference = attribute.Value;
								Utility.SetDirty(attribute.Value.info.parent);
								Utility.SetDirty(data);
								Utility.SetDirty(this.info.parent);
								resolved = true;
								break;
							}
						}
						if(!resolved){
							Debug.LogWarning("[Attribute] ID missing : " + data.attribute.path + ".  Unable to resolve : " + data.referencePath,this.info.parent.gameObject);
						}
					}
				}
			}
		}
		// ======================
		// Repair
		// ======================
		public void PrepareData(){
			if(this.info.data.Length < 1){
				BaseType value = this.delayedValue != null ? this.delayedValue : default(BaseType);
				if(this.delayedValue == null){Debug.Log("[Attribute] Fixing unprepared data : " + this.info.path);}
				//else{Debug.Log("[Attribute] Delayed add : " + this.info.path);}
				this.Add(value);
			}
			if(!Application.isPlaying){
				this.RepairData();
				if(this.info.mode == AttributeMode.Linked){
					this.usage = AttributeUsage.Shaped;
				}
			}
			for(int index=0;index<this.info.data.Length;++index){
				AttributeData data = this.info.data[index];
				data.attribute = this.info;
				data.hideFlags = PlayerPrefs.GetInt("ShowAttributeData") == 1 ? 0 : HideFlags.HideInInspector;
				if(data.usage == AttributeUsage.Direct){continue;}
				data.target.Setup(this.info.path+"/Target",this.info.parent);
				data.target.DefaultSearch("[This]");
			}
		}
		public void FixDuplicates(){
			GameObject root = Utility.FindPrefabRoot(this.info.parent.gameObject);
			if(!root.IsNull()){
				string name = root.gameObject.name;
				if(Locate.HasDuplicate(name)){
					Debug.Log("[Attribute] Resolving same name siblings : " + this.info.path,this.info.parent.gameObject);
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
		}
		public void FixIDConflict(string previousID){
			bool changedID = !previousID.IsEmpty() && this.info.id != previousID;
			if(changedID){
				var resolve = Attribute.resolve;
				if(!resolve.ContainsKey(this.info.parent.gameObject)){
					resolve[this.info.parent.gameObject] = new Dictionary<string,string>();
				}
				resolve[this.info.parent.gameObject][previousID] = this.info.id;
			}
		}
		public void RepairData(){
			for(int index=0;index<this.info.data.Length;++index){
				AttributeData data = this.info.data[index];
				if(data.IsNull()){
					Debug.Log("[Attribute] Removing null attribute data in " + this.info.path + ".");
					this.info.data = this.info.data.RemoveAt(index);
					index -= 1;
					Utility.SetDirty(this.info.parent);
					continue;
				}
				bool kidnapped = data.attribute.parent.gameObject != data.gameObject;
				bool amnesia = data.attribute.IsNull();
				bool orphanned = data.attribute.parent.IsNull();
				//bool wrongParent = (data.attribute != this) || (data.attribute.id != this.info.id);
				if(kidnapped || orphanned || amnesia){
					if(kidnapped){Debug.LogWarning("[Attribute] Data was kidnapped.  Call the cops! : " + data.referencePath);}
					if(orphanned){Debug.LogWarning("[Attribute] Data was orphanned.  What a travesty! : " + data.referencePath);}
					if(amnesia){Debug.LogWarning("[Attribute] Data has amnesia.  Who am I?! : " + data.referencePath);}
					this.info.data = this.info.data.RemoveAt(index);
					index -= 1;
					Utility.SetDirty(this.info.parent);
					//this.info.data[index].attribute = this;
					//Utility.SetDirty(this.info.data[index]);
				}
			}
		}
		// ======================
		// Functionality
		// ======================
		public void AddScope(Component parent){
			if(parent.IsNull()){return;}
			var lookup = Attribute.lookup;
			GameObject target = parent.gameObject;
			if(lookup.ContainsKey(target)){
				AttributeType self = (AttributeType)this;
				lookup[target].RemoveValue(self);
				lookup[target]["*/"+this.info.path] = self;
			}
		}
		public virtual BaseType Get(){
			if(this.getMethod != null){return this.getMethod();}
			if(this.info.data.Length < 1){
				Debug.LogWarning("[Attribute] Get : No data found for : " + this.info.path);
				return default(BaseType);
			}
			if(this.info.mode != AttributeMode.Formula){
				return this.GetFirst().Get();
			}
			return this.GetFormulaValue();
		}
		public virtual void Set(BaseType value){
			if(this.setMethod != null){
				this.setMethod(value);
				return;
			}
			if(this.info.data.Length < 1){
				Debug.LogWarning("[Attribute] Set : No data found for : " + this.info.path);
				return;
			}
			DataType data = this.GetFirst();
			if(data == null){
				if(!Attribute.setWarning.ContainsKey(this)){
					Debug.LogWarning("[Attribute] No data found. (" + this.info.path + ")",this.target);
					Attribute.setWarning[this] = true;
				}
			}
			else if(this.info.mode == AttributeMode.Normal){
				if(this.usage == AttributeUsage.Shaped){
					this.usage = AttributeUsage.Direct;
				}
				data.value = value;
			}
			else if(this.info.mode == AttributeMode.Linked){
				if(!Attribute.ready && Application.isPlaying){
					Debug.LogWarning("[Attribute] Set attempt before attribute data built : " + this.info.path);
					return;
				}
				if(data.reference == null){
					if(!Attribute.setWarning.ContainsKey(this)){
						string source = "("+this.info.path+")";
						string goal = (data.target.Get().GetPath() + data.referencePath).Trim("/");
						Debug.LogWarning("Attribute (Set): No reference found for " + source + " to " + goal,this.info.parent);
						Attribute.setWarning[this] = true;
					}
					return;
				}
				((AttributeType)data.reference).Set(value);
			}
			else if(this.info.mode == AttributeMode.Formula){
				if(!Attribute.setWarning.ContainsKey(this)){
					Debug.LogWarning("Attribute (Set): Cannot manually set values for formulas. (" + this.info.path + ")",this.info.parent);
					Attribute.setWarning[this] = true;
				}
			}
		}

	}
}
