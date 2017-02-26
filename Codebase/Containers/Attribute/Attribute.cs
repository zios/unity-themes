using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Zios.Attributes{
	using Events;
	public enum AttributeMode{Normal,Linked,Formula,Group};
	public enum AttributeUsage{Direct,Shaped};
	public enum LinkType{Both,Get};
	[Serializable]
	public class AttributeInfo{
		public string name;
		public string path;
		public string fullPath;
		public string relativePath;
		public string id;
		public string localID;
		public Component parent;
		public AttributeMode mode = AttributeMode.Normal;
		public LinkType linkType;
		public Type type;
		public AttributeData[] data = new AttributeData[0];
		public AttributeData[] dataB = new AttributeData[0];
		public AttributeData[] dataC = new AttributeData[0];
		[NonSerialized] public Attribute attribute;
		public bool Contains(AttributeData data){return this.data.Contains(data) || this.dataB.Contains(data) || this.dataC.Contains(data);}
	}
	[Flags]
	public enum AttributeDebug : int{
		Issue          = 0x001,
		IssueMinor     = 0x002,
		Add            = 0x004,
		Remove         = 0x008,
		ProcessTime    = 0x010,
		ProcessStage   = 0x020,
		ProcessRefresh = 0x040
	}
	[Flags]
	public enum AttributeRepair : int{
		SamePathAttributes  = 0x001,
		SamePathGameObjects = 0x002,
	}
	[Serializable]
	public class Attribute{
		[EnumMask] public static AttributeDebug debug = 0;
		[EnumMask] public static AttributeRepair repair = (AttributeRepair)(-1);
		[NonSerialized] public static bool ready;
		[NonSerialized] public static List<Attribute> all = new List<Attribute>();
		[NonSerialized] public static Dictionary<GameObject,Dictionary<string,Attribute>> lookup = new Dictionary<GameObject,Dictionary<string,Attribute>>();
		[NonSerialized] public static Dictionary<GameObject,Dictionary<string,string>> resolve = new Dictionary<GameObject,Dictionary<string,string>>();
		[NonSerialized] public static Dictionary<Attribute,bool> setWarning = new Dictionary<Attribute,bool>();
		[NonSerialized] public static Dictionary<AttributeData,bool> getWarning = new Dictionary<AttributeData,bool>();
		public AttributeInfo info = new AttributeInfo();
		public string path{
			get{return this.info.fullPath;}
			set{this.info.fullPath = value;}
		}
		public AttributeData[] data{
			get{return this.info.data;}
			set{this.info.data = value;}
		}
		[NonSerialized] public bool isSetup;
		[NonSerialized] public bool dirty = true;
		[NonSerialized] public bool locked;
		[NonSerialized] public bool showInEditor = true;
		[NonSerialized] public bool canAdvanced = true;
		[NonSerialized] public bool canCache = true;
		[NonSerialized] public bool canFormula = true;
		[NonSerialized] public bool canGroup = false;
		[NonSerialized] public bool canDirect = true;
		[NonSerialized] public bool canShape = true;
		[NonSerialized] public bool canLink = true;
		[NonSerialized] public string defaultSet = "A";
		[NonSerialized] public List<Attribute> dependents = new List<Attribute>();
		public AttributeUsage usage{
			get{
				return this.GetFirst().usage;
			}
			set{
				foreach(AttributeData data in this.info.data){
					data.usage = value;
				}
			}
		}
		public AttributeData GetFirst(){
			var data = this.info.data;
			if(data.Length < 1 || data[0].GetType() == typeof(AttributeData)){this.PrepareData();}
			if(data[0].GetType() == typeof(AttributeData)){
				if(Attribute.debug.Has("Issue")){
					Debug.LogWarning("[Attribute] Could not convert first attribute for -- " + this.info.fullPath);
				}
			}
			if(data.Length < 1){
				if(Attribute.debug.Has("Issue")){
					Debug.LogWarning("[Attribute] Could not retrieve first attribute data for -- " + this.info.fullPath);
				}
				return null;
			}
			return data[0];
		}
		public virtual Type[] GetFormulaTypes(){return null;}
		public virtual bool HasData(){return false;}
		public virtual AttributeData[] GetData(){return null;}
		public virtual void Add<Type>(int index=-1,string set="") where Type : AttributeData,new(){}
		public virtual void Remove(AttributeData data,string set=""){}
		public virtual void Setup(string path,Component parent){}
		public virtual void PrepareData(){}
		public virtual void BuildLookup(){}
		public virtual void BuildData(AttributeData[] dataSet){}
		public AttributeData[] GetDataSet(){
			return this.GetDataSet(this.defaultSet);
		}
		public AttributeData[] GetDataSet(string name){
			if(name == "B"){return this.info.dataB;}
			if(name == "C"){return this.info.dataC;}
			return this.info.data;
		}
	}
	[Serializable]
	public class Attribute<BaseType,AttributeType,DataType> : Attribute,IEnumerable<BaseType>
	where AttributeType : Attribute<BaseType,AttributeType,DataType>,new()
	where DataType : AttributeData<BaseType,AttributeType,DataType>,new(){
		private BaseType cachedValue;
		private IEnumerator<BaseType> cachedEnumerator;
		public BaseType delayedValue = default(BaseType);
		public Func<IEnumerator<BaseType>> enumerateMethod;
		public Func<BaseType> getMethod;
		public Action<BaseType> setMethod;
		public IEnumerator<BaseType> GetEnumerator(){
			/*if(this.cachedEnumerator == null){
				this.cachedEnumerator = this.info.data.Where(x=>x is DataType).Select(x=>x.As<DataType>().Get()).GetEnumerator();
			}
			return this.cachedEnumerator;
			*/
			if(this.enumerateMethod != null){return this.enumerateMethod();}
			if(this.info.mode == AttributeMode.Linked){return ((AttributeType)this.GetFirst().reference).GetEnumerator();}
			return data.Where(x=>x is DataType).Select(x=>x.As<DataType>().Get()).GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator(){return GetEnumerator();}
		// ======================
		// Shortcuts
		// ======================
		public virtual BaseType GetFormulaValue(){return default(BaseType);}
		public override Type[] GetFormulaTypes(){return new Type[]{typeof(AttributeData)};}
		public override bool HasData(){return this.info.data.Length > 0;}
		public void SetDefault(BaseType value){
			this.PrepareData();
			BaseType current = this.GetFirst().As<DataType>().value;
			if(current.IsEmpty()){
				this.GetFirst().As<DataType>().Set(value);
			}
		}
		// ======================
		// Building
		// ======================
		public AttributeData[] CreateData<Type>(AttributeData[] dataArray,int index) where Type : AttributeData,new(){
			if(Attribute.debug.Has("Add")){Debug.Log("[Attribute] Creating attribute data : " + this.info.fullPath);}
			AttributeData data = new Type();
			Event.Add("On Validate Raw",data.Serialize,this.info.parent);
			data.rawType = typeof(Type).FullName;
			data.attribute = this.info;
			if(index == -1){index = dataArray.Length;}
			if(index > dataArray.Length-1){
				dataArray = dataArray.Resize(index+1);
			}
			dataArray[index] = data;
			return dataArray;
		}
		public override void Add<Type>(int index=-1,string set=""){
			if(this.info.parent != null){
				Utility.RecordObject(this.info.parent,"Attribute - Add Data");
				if(set.IsEmpty()){set = this.defaultSet;}
				if(set == "A"){this.info.data = this.CreateData<Type>(this.info.data,index);}
				if(set == "B"){this.info.dataB = this.CreateData<Type>(this.info.dataB,index);}
				if(set == "C"){this.info.dataC = this.CreateData<Type>(this.info.dataC,index);}
				Utility.SetDirty(this.info.parent);
			}
		}
		public void Add(BaseType value){
			if(this.info.parent != null){
				this.Add<DataType>();
				var newData = (DataType)this.info.data.Last();
				newData.Set(value);
			}
		}
		public override void Remove(AttributeData data,string set=""){
			List<AttributeData> entries = new List<AttributeData>();
			if(set.IsEmpty()){set = this.defaultSet;}
			if(set == "A"){entries = this.info.data.Where(x=>x==data).ToList();}
			if(set == "B"){entries = this.info.dataB.Where(x=>x==data).ToList();}
			if(set == "C"){entries = this.info.dataC.Where(x=>x==data).ToList();}
			foreach(AttributeData current in entries){
				if(Attribute.debug.Has("Remove")){Debug.Log("[Attribute] Removing attribute data : " + this.info.fullPath + " : " + data);}
				if(set == "A"){this.info.data = this.info.data.RemoveAll(current);}
				if(set == "B"){this.info.dataB = this.info.dataB.RemoveAll(current);}
				if(set == "C"){this.info.dataC = this.info.dataC.RemoveAll(current);}
			}
		}
		// ======================
		// Setup
		// ======================
		public void Reset(){this.Setup(this.info.relativePath,this.info.parent);}
		public override void Setup(string path,Component parent){
			bool disabled = parent.IsNull() || (Application.isPlaying && !parent.IsEnabled());
			if(disabled){return;}
			if(!Application.isPlaying){
				string previousID = this.info.id;
				this.BuildInfo(path,parent);
				if(!Utility.IsBusy()){
					this.FixDuplicates();
					this.FixChanged(previousID);
				}
				Event.Add("On Validate",this.ValidateDependents,parent);
				Event.AddLimited("On Reset",this.Reset,1,parent);
			}
			this.info.type = typeof(DataType);
			this.PrepareData();
			if(!Attribute.all.Contains(this)){
				Attribute.all.Add(this);
			}
			this.isSetup = true;
		}
		public void BuildInfo(string path,Component parent){
			Utility.RecordObject(parent,"Attribute - Info Changes");
			string previousID = this.info.id;
			this.info.relativePath = path;
			path = (parent.GetAlias() + "/" + path.Trim("/")).Trim("/");
			bool dirty = this.info.parent != parent;
			dirty = dirty || this.info.path != path;
			dirty = dirty || this.info.localID.IsEmpty();
			this.info.attribute = this;
			this.info.parent = parent;
			this.info.path = path;
			this.info.name = path.Split("/").Last();
			this.info.fullPath = parent.GetPath().TrimLeft("/") + path.Trim(parent.GetAlias());
			this.info.localID = this.info.localID.IsEmpty() ? Guid.NewGuid().ToString() : this.info.localID;
			this.info.id = parent.GetInstanceID()+"/"+this.info.localID;
			dirty = dirty || this.info.id != previousID;
			if(dirty){
				Utility.SetDirty(parent);
			}
		}
		public override void BuildLookup(){
			AttributeType self = (AttributeType)this;
			GameObject target = this.info.parent.gameObject;
			var lookup = Attribute.lookup;
			lookup.AddNew(target);
			lookup[target].RemoveValue(self);
			lookup[target][this.info.id] = self;
			this.dependents.Clear();
		}
		public override void BuildData(AttributeData[] dataSet){
			var lookup = Attribute.lookup;
			var resolve = Attribute.resolve;
			foreach(AttributeData data in dataSet){
				if(data.IsNull()){continue;}
				if(this.info.mode == AttributeMode.Linked){data.usage = AttributeUsage.Shaped;}
				if(data.usage == AttributeUsage.Direct){continue;}
				if(this.getMethod != null){continue;}
				GameObject target = data.target.Get();
				if(data.reference.IsNull() && !data.referenceID.IsEmpty() && !target.IsNull()){
					if(!lookup.ContainsKey(target)){
						if(Attribute.debug.Has("IssueMinor")){Debug.Log("[Attribute] Lookup attempted on -- " + target + " before table built");}
						break;
					}
					if(!Application.isPlaying){
						if(resolve.ContainsKey(target) && resolve[target].ContainsKey(data.referenceID)){
							string resolvedID = resolve[target][data.referenceID];
							if(resolvedID != data.referenceID){
								Utility.RecordObject(this.info.parent,"Attribute - Fix ID");
								data.referenceID = resolvedID;
								Utility.SetDirty(this.info.parent);
							}
						}
					}
					if(lookup[target].ContainsKey(data.referenceID)){
						data.reference = lookup[target][data.referenceID];
					}
					if(data.reference.IsNull() && !data.referencePath.IsEmpty()){
						var entries = lookup[target];
						bool resolved = false;
						foreach(var attribute in entries){
							string path = attribute.Value.info.path;
							if(path.Equals(data.referencePath)){
								if(Attribute.debug.Has("IssueMinor")){
									string message = "[Attribute] ID missing : " + data + ".  Resolved via path : " + data.referencePath;
									Debug.Log(message,this.info.parent.gameObject);
								}
								Utility.RecordObject(this.info.parent,"Attribute - Fix Reference");
								Utility.RecordObject(attribute.Value.info.parent,"Attribute - Fix Reference");
								data.referenceID = attribute.Value.info.id;
								data.reference = attribute.Value;
								Utility.SetDirty(this.info.parent);
								Utility.SetDirty(data.reference.info.parent);
								resolved = true;
								break;
							}
						}
						if(!resolved && Attribute.debug.Has("Issue")){
							string message = "[Attribute] ID missing : " + data.path + ".  Unable to find : " + data.referencePath;
							Debug.LogWarning(message,this.info.parent.gameObject);
						}
					}
				}
			}
			this.SetupCaching(dataSet);
		}
		public void SetupCaching(AttributeData[] dataSet){
			this.canCache = this.getMethod == null && this.setMethod == null;
			if(this.canCache){
				foreach(AttributeData data in dataSet){
					if(!data.CanCache()){
						this.canCache = false;
						break;
					}
				}
			}
			if(this.canCache && this.info.mode != AttributeMode.Linked){
				foreach(AttributeData data in dataSet){
					if(!data.reference.IsNull()){
						data.reference.dependents.AddNew(this);
					}
				}
			}
		}
		public void ValidateDependents(){
			foreach(var dependent in this.dependents){
				if(!dependent.dependents.Contains(this)){
					var parent = dependent.info.parent;
					parent.CallEvent("On Validate");
				}
			}
		}
		// ======================
		// Repair
		// ======================
		public void FixDuplicates(){
			Utility.RecordObject(this.info.parent,"Attribute - Fix Duplicates");
			GameObject current = this.info.parent.gameObject;
			string path = this.info.path;
			string name = current.name;
			if(Attribute.repair.Has("SamePathGameObjects")){
				while(Locate.HasDuplicate(current)){
					current.name = current.name.ToLetterSequence();
					Locate.SetDirty();
				}
			}
			if(Attribute.repair.Has("SamePathAttributes")){
				while(Attribute.all.Exists(x=>x != this && x.info.fullPath==this.info.fullPath)){
					if(Attribute.all.Contains(this)){break;}
					this.info.path = this.info.path.ToLetterSequence();
					this.info.name = this.info.path.Split("/").Last();
					this.info.fullPath = this.info.parent.GetPath().TrimLeft("/") + this.info.path.Trim(this.info.parent.GetAlias());
				}
			}
			if(name != current.name && Attribute.debug.Has("Issue")){
				Debug.Log("[Attribute] Resolving same name siblings : " + name,current);
			}
			if(path != this.info.path){
				if(Attribute.debug.Has("IssueMinor")){Debug.Log("[Attribute] Resolving same name sibling attributes : " + this.info.fullPath,this.info.parent.gameObject);}
				this.info.parent.CallEvent("On Validate");
				Utility.SetDirty(this.info.parent);
			}
		}
		public void FixChanged(string previousID){
			bool changedID = !previousID.IsEmpty() && this.info.id != previousID;
			if(changedID){
				if(Attribute.debug.Has("IssueMinor")){Debug.Log("[Attribute] Resolving changed ID : " + this.info.fullPath,this.info.parent.gameObject);}
				var resolve = Attribute.resolve;
				if(!resolve.ContainsKey(this.info.parent.gameObject)){
					resolve[this.info.parent.gameObject] = new Dictionary<string,string>();
				}
				resolve[this.info.parent.gameObject][previousID] = this.info.id;
			}
		}
		public override void PrepareData(){
			if(!Application.isPlaying){
				if(this.info.mode == AttributeMode.Linked){
					this.usage = AttributeUsage.Shaped;
				}
				this.info.data = this.RepairData(this.info.data);
				this.info.dataB = this.RepairData(this.info.dataB);
				this.info.dataC = this.RepairData(this.info.dataC);
			}
			if(this.info.data.Length < 1){
				BaseType value = this.delayedValue != null ? this.delayedValue : default(BaseType);
				if(typeof(BaseType).IsValueType && this.delayedValue == null && Attribute.debug.Has("Issue")){
					Debug.Log("[Attribute] Fixing unprepared data : " + this.info.fullPath);
				}
				this.Add(value);
			}
			this.UpdateData(this.info.data);
			this.UpdateData(this.info.dataB);
			this.UpdateData(this.info.dataC);
		}
		public virtual AttributeData[] RepairData(AttributeData[] dataSet){
			var newData = dataSet.Copy();
			for(int index=0;index<dataSet.Length;++index){
				var data = dataSet[index];
				if(data.rawValue.IsEmpty() && data.rawType.IsEmpty()){
					if(Attribute.debug.Has("Issue")){Debug.LogWarning("[Attribute] : Attribute data is empty. Removing : " + data.path);}
					newData = newData.Remove(data);
				}
			}
			return newData;
		}
		public virtual void UpdateData(AttributeData[] dataSet){
			for(int index=0;index<dataSet.Length;++index){
				var data = dataSet[index];
				if(data.GetType() == typeof(AttributeData)){
					data = dataSet[index] = this.Deserialize(dataSet[index]);
				}
				if(!Application.isPlaying){
					Event.Add("On Enter Play",data.Serialize);
					Event.Add("On Scene Saving",data.Serialize);
				}
				if(data.usage == AttributeUsage.Shaped){data.target.Setup(index + "/Target",this.info.parent);}
				if(data.usage == AttributeUsage.Direct){data.target.Clear();}
				data.attribute = this.info;
				data.path = this.info.fullPath + "/" + index;
			}
		}
		public AttributeData Deserialize(AttributeData data){
			if(Attribute.debug.Has("Issue") && data.rawType.IsEmpty()){
				Debug.LogWarning("[Attribute] : Deserialization type is empty for : " + data.path);
				return data;
			}
			var type = Type.GetType(data.rawType);
			var newData = data;
			if(type == typeof(AttributeBoolData)){
				var boolData = new AttributeBoolData();
				boolData.value = data.rawValue.ToBool();
				newData = boolData;
			}
			else if(type == typeof(AttributeStringData)){
				var stringData = new AttributeStringData();
				stringData.value = data.rawValue.ToString();
				newData = stringData;
			}
			else if(type == typeof(AttributeIntData)){
				var intData = new AttributeIntData();
				intData.value = data.rawValue.ToInt();
				newData = intData;
			}
			else if(type == typeof(AttributeFloatData)){
				var floatData = new AttributeFloatData();
				floatData.value = data.rawValue.ToFloat();
				newData = floatData;
			}
			else if(type == typeof(AttributeVector3Data)){
				var vector3Data = new AttributeVector3Data();
				vector3Data.value = data.rawValue.ToVector3();
				newData = vector3Data;
			}
			else if(type == typeof(AttributeGameObjectData)){
				var gameObjectData = new AttributeGameObjectData();
				gameObjectData.value = Locate.Find(data.rawValue);
				newData = gameObjectData;
			}
			else if(Attribute.debug.Has("Issue")){
				Debug.LogWarning("[Attribute] : Cannot determine deserialization type for : " + data.path);
			}
			newData.target = data.target;
			newData.usage = data.usage;
			newData.path = data.path;
			newData.referenceID = data.referenceID;
			newData.referencePath = data.referencePath;
			newData.operation = data.operation;
			newData.special = data.special;
			newData.rawValue = data.rawValue;
			newData.rawType = data.rawType;
			return newData;
		}
		// ======================
		// Functionality
		// ======================
		public virtual BaseType Get(){
			if(this.getMethod != null){return this.getMethod();}
			if(this.info.data.Length < 1){
				if(Attribute.debug.Has("Issue")){Debug.LogWarning("[Attribute] Get : No data found for : " + this.info.fullPath);}
				return default(BaseType);
			}
			if(this.info.mode != AttributeMode.Formula){
				/*if(this.dirty){
					this.dirty = !this.canCache;
					this.cachedValue = this.GetFirstRaw().Get();
				}
				return this.cachedValue;*/
				return ((DataType)(this.GetFirst())).Get();
			}
			/*if(this.dirty){
				this.dirty = !this.canCache;
				this.cachedValue = this.GetFormulaValue();
			}
			return this.cachedValue;*/
			return this.GetFormulaValue();
		}
		public virtual void Set(BaseType value){
			if(!this.isSetup){
				this.delayedValue = value;
				return;
			}
			if(!this.canCache || !value.Equals(this.cachedValue)){
				this.cachedValue = value;
				foreach(Attribute dependent in this.dependents){
					dependent.dirty = true;
				}
			}
			if(this.setMethod != null){
				this.setMethod(value);
				return;
			}
			DataType data = this.GetFirst().As<DataType>();
			if(data == null){
				if(!Attribute.setWarning.ContainsKey(this)){
					if(Attribute.debug.Has("Issue")){Debug.LogWarning("[Attribute] No data found. (" + this.info.fullPath + ")");}
					Attribute.setWarning[this] = true;
				}
			}
			else if(this.info.mode == AttributeMode.Normal){
				if(this.usage == AttributeUsage.Shaped){
					this.usage = AttributeUsage.Direct;
				}
				data.Set(value);
			}
			else if(this.info.mode == AttributeMode.Linked && this.info.linkType != LinkType.Get){
				if(!Attribute.ready && Application.isPlaying){
					if(Attribute.debug.Has("Issue")){Debug.LogWarning("[Attribute] Set attempt before attribute data built : " + this.info.fullPath);}
					return;
				}
				if(data.reference == null){
					if(!Attribute.setWarning.ContainsKey(this)){
						string source = "("+this.info.fullPath+")";
						string goal = (data.target.Get().GetPath() + data.referencePath).Trim("/");
						if(Attribute.debug.Has("Issue")){Debug.LogWarning("Attribute (Set): No reference found for " + source + " to " + goal,this.info.parent);}
						Attribute.setWarning[this] = true;
					}
					return;
				}
				((AttributeType)data.reference).Set(value);
			}
			else if(this.info.mode == AttributeMode.Formula){
				if(!Attribute.setWarning.ContainsKey(this)){
					if(Attribute.debug.Has("Issue")){Debug.LogWarning("Attribute (Set): Cannot manually set values for formulas. (" + this.info.fullPath + ")",this.info.parent);}
					Attribute.setWarning[this] = true;
				}
			}
		}
	}
}