using Zios;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Zios{
	public enum AttributeMode{Normal,Linked,Formula,Group};
	public enum AttributeUsage{Direct,Shaped};
	public enum LinkType{Both,Get};
	[Serializable]
	public class AttributeInfo{
		public string name;
		public string path;
		public string id;
		public string localID;
		public Type dataType;
		public Component parent;
		public AttributeMode mode = AttributeMode.Normal;
		public LinkType linkType;
		public AttributeData[] data = new AttributeData[0];
		public AttributeData[] dataB = new AttributeData[0];
		public AttributeData[] dataC = new AttributeData[0];
	}
	[Flags]
	public enum AttributeDebug : int{
		Issue          = 0x001,
		Add            = 0x002,
		Remove         = 0x004,
		ProcessTime    = 0x008,
		ProcessStage   = 0x010,
		ProcessRefresh = 0x020
	}
	[Serializable]
	public class Attribute{
		[EnumMask] public static AttributeDebug debug;
		[NonSerialized] public static bool ready;
		[NonSerialized] public static List<Attribute> all = new List<Attribute>();
		[NonSerialized] public static Dictionary<GameObject,Dictionary<string,Attribute>> lookup = new Dictionary<GameObject,Dictionary<string,Attribute>>();
		[NonSerialized] public static Dictionary<GameObject,Dictionary<string,string>> resolve = new Dictionary<GameObject,Dictionary<string,string>>();
		[NonSerialized] public static Dictionary<Attribute,bool> setWarning = new Dictionary<Attribute,bool>();
		[NonSerialized] public static Dictionary<AttributeData,bool> getWarning = new Dictionary<AttributeData,bool>();
		public AttributeInfo info = new AttributeInfo();
		public AttributeData[] data{
			get{return this.info.data;}
			set{this.info.data = value;}
		}
		public int Length{
			get{return this.info.data.Length;}
			set{}
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
		[NonSerialized] public bool isDefault = true;
		[NonSerialized] public string defaultSet = "A";
		[NonSerialized] public List<Attribute> dependents = new List<Attribute>();
		public virtual Type[] GetFormulaTypes(){return null;}
		public virtual bool HasData(){return false;}
		public virtual AttributeData[] GetData(){return null;}
		public virtual void Clear(){}
		public virtual void Add<Type>(int index=-1,string set="") where Type : AttributeData{}
		public virtual void Remove(AttributeData data,string set=""){}
		public virtual void Setup(string path,Component parent){}
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
	where AttributeType : Attribute<BaseType,AttributeType,DataType>
	where DataType : AttributeData<BaseType,AttributeType,DataType>{
		public static bool editorStart;
		private BaseType cachedValue;
		private IEnumerator<BaseType> cachedEnumerator;
		protected BaseType delayedValue = default(BaseType);
		public Func<IEnumerator<BaseType>> enumerateMethod;
		public Func<BaseType> getMethod;
		public Action<BaseType> setMethod;
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
		public IEnumerator<BaseType> GetEnumerator(){
			/*if(this.cachedEnumerator == null){
				this.cachedEnumerator = this.info.data.Where(x=>x is DataType).Select(x=>x.As<DataType>().Get()).GetEnumerator();
			}
			return this.cachedEnumerator;
			*/
			if(this.enumerateMethod != null){return this.enumerateMethod();}
			if(this.info.mode == AttributeMode.Linked){return ((AttributeType)this.GetFirstRaw().reference).GetEnumerator();}
			return data.Where(x=>x is DataType).Select(x=>x.As<DataType>().Get()).GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator(){return GetEnumerator();}
		// ======================
		// Shortcuts
		// ======================
		public virtual BaseType GetFormulaValue(){return default(BaseType);}
		public override Type[] GetFormulaTypes(){return new Type[]{typeof(AttributeData)};}
		public override AttributeData[] GetData(){return this.info.data;}
		public override bool HasData(){return this.info.data.Length > 0;}
		public AttributeData GetFirst(){
			if(this.info.data.Length < 1){this.PrepareData();}
			if(this.info.data.Length < 1){
				if(Attribute.debug.Has("Issue")){
					Debug.Log("[Attribute] Could not retrieve first attribute data for -- " + this.info.name);
				}
				return null;
			}
			return this.info.data[0];
		}
		public DataType GetFirstRaw(){
			return (DataType)this.GetFirst();
		}
		public void SetDefault(BaseType value){
			this.PrepareData();
			BaseType current = this.GetFirstRaw().value;
			if(current.IsEmpty()){
				this.GetFirstRaw().value = value;
			}
		}
		// ======================
		// Building
		// ======================
		public AttributeData[] CreateData<Type>(AttributeData[] dataArray,int index) where Type : AttributeData{
			if(Attribute.debug.Has("Add")){Debug.Log("[Attribute] Creating attribute data : " + this.info.path);}
			AttributeData data = this.info.parent.gameObject.AddComponent<Type>();
			data.hideFlags = HideFlags.HideInInspector;
			data.attribute = this.info;
			data.Setup();
			if(index == -1){index = dataArray.Length;}
			if(index > dataArray.Length-1){
				dataArray = dataArray.Resize(index+1);
			}
			if(dataArray[index] != null){
				Utility.Destroy(dataArray[index]);
			}
			dataArray[index] = data;
			Utility.SetDirty(data);
			return dataArray;
		}
		public override void Add<Type>(int index=-1,string set=""){
			this.isDefault = false;
			if(this.info.parent != null){
				if(set.IsEmpty()){set = this.defaultSet;}
				if(set == "A"){this.info.data = this.CreateData<Type>(this.info.data,index);}
				if(set == "B"){this.info.dataB = this.CreateData<Type>(this.info.dataB,index);}
				if(set == "C"){this.info.dataC = this.CreateData<Type>(this.info.dataC,index);}
				Utility.SetDirty(this.info.parent);
			}
		}
		public void Add(BaseType value){
			this.isDefault = false;
			if(this.info.parent != null){
				this.Add<DataType>();
				var newData = (DataType)this.info.data.Last();
				newData.value = value;
			}
		}
		public override void Remove(AttributeData data,string set=""){
			List<AttributeData> entries = new List<AttributeData>();
			if(set.IsEmpty()){set = this.defaultSet;}
			if(set == "A"){entries = this.info.data.Where(x=>x==data).ToList();}
			if(set == "B"){entries = this.info.dataB.Where(x=>x==data).ToList();}
			if(set == "C"){entries = this.info.dataC.Where(x=>x==data).ToList();}
			foreach(AttributeData current in entries){
				if(Attribute.debug.Has("Remove")){Debug.Log("[Attribute] Removing attribute data : " + this.info.path + " : " + data);}
				if(set == "A"){this.info.data = this.info.data.RemoveAll(current);}
				if(set == "B"){this.info.dataB = this.info.dataB.RemoveAll(current);}
				if(set == "C"){this.info.dataC = this.info.dataC.RemoveAll(current);}
				Utility.Destroy(current);
			}
		}
		// ======================
		// Setup
		// ======================
		public override void Setup(string path,Component parent){
			if(parent.IsNull()){return;}
			this.info.dataType = typeof(DataType);
			if(!Application.isPlaying){
				string previousID = this.info.id;
				this.BuildInfo(path,parent);
				this.FixDuplicates();
				this.FixConflicts(previousID);
				Events.Add("On Validate",this.ValidateDependents,parent);
				Events.AddLimited("On Reset",()=>this.Setup(path,parent),1,parent);
			}
			this.PrepareData();
			if(!Attribute.all.Contains(this)){
				Attribute.all.Add(this);
			}
			this.isSetup = true;
		}
		public void BuildInfo(string path,Component parent){
			string previousID = this.info.id;
			path = (parent.GetAlias() + "/" + path.Trim("/")).Trim("/");
			bool dirty = this.info.parent != parent;
			dirty = dirty || this.info.path != path;
			dirty = dirty || this.info.localID.IsEmpty();
			this.info.parent = parent;
			this.info.path = path;
			this.info.name = path.Split("/").Last();
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
						if(Attribute.debug.Has("Issue")){Debug.Log("[Attribute] Lookup attempted on -- " + target + " before table built");}
						break;
					}
					if(!Application.isPlaying){
						if(resolve.ContainsKey(target) && resolve[target].ContainsKey(data.referenceID)){
							string resolvedID = resolve[target][data.referenceID];
							if(resolvedID != data.referenceID){
								data.referenceID = resolvedID;
								Utility.SetDirty(this.info.parent);
								Utility.SetDirty(data);
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
							if(attribute.Value.info.path == data.referencePath){
								if(Attribute.debug.Has("Issue")){
									string message = "[Attribute] ID missing : " + data.attribute.path + ".  Resolved via path : " + data.referencePath;
									Debug.Log(message,this.info.parent.gameObject);
								}
								data.referenceID = attribute.Value.info.id;
								data.reference = attribute.Value;
								Utility.SetDirty(this.info.parent);
								Utility.SetDirty(data.reference.info.parent);
								Utility.SetDirty(data);
								resolved = true;
								break;
							}
						}
						if(!resolved && Attribute.debug.Has("Issue")){
							string message = "[Attribute] ID missing : " + data.attribute.path + ".  Unable to resolve : " + data.referencePath;
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
			if(this.canCache){
				foreach(AttributeData data in dataSet){
					if(!data.reference.IsNull()){
						data.reference.dependents.AddNew(this);
					}
				}
			}
		}
		public void ValidateDependents(){
			foreach(var dependent in this.dependents){
				var parent = dependent.info.parent;
				parent.CallMethod("OnValidate");
			}
		}
		// ======================
		// Repair
		// ======================
		public void FixDuplicates(){
			GameObject current = this.info.parent.gameObject;
			string name = current.name;
			if(Locate.HasDuplicate(current)){
				if(Attribute.debug.Has("Issue")){Debug.Log("[Attribute] Resolving same name siblings : " + name,this.info.parent.gameObject);}
				char lastDigit = name[name.Length-1];
				if(name.Length > 1 && name[name.Length-2] == ' ' && char.IsLetter(lastDigit)){
					char nextLetter = (char)(char.ToUpper(lastDigit)+1);
					current.gameObject.name = name.TrimEnd(lastDigit) + nextLetter;
				}
				else{
					current.gameObject.name = name + " B";
				}
				AttributeManager.PerformRefresh();
			}
		}
		public void FixConflicts(string previousID){
			bool changedID = !previousID.IsEmpty() && this.info.id != previousID;
			if(changedID){
				if(Attribute.debug.Has("Issue")){Debug.Log("[Attribute] Resolving id conflict : " + this.info.path,this.info.parent.gameObject);}
				var resolve = Attribute.resolve;
				if(!resolve.ContainsKey(this.info.parent.gameObject)){
					resolve[this.info.parent.gameObject] = new Dictionary<string,string>();
				}
				resolve[this.info.parent.gameObject][previousID] = this.info.id;
			}
		}
		public void PrepareData(){
			if(!Application.isPlaying){
				this.RepairData("A");
				this.RepairData("B");
				this.RepairData("C");
				if(this.info.mode == AttributeMode.Linked){
					this.usage = AttributeUsage.Shaped;
				}
			}
			if(this.info.data.Length < 1){
				BaseType value = this.delayedValue != null ? this.delayedValue : default(BaseType);
				if(typeof(BaseType).IsValueType && this.delayedValue == null && Attribute.debug.Has("Issue")){Debug.Log("[Attribute] Fixing unprepared data : " + this.info.path);}
				//else{Debug.Log("[Attribute] Delayed add : " + this.info.path);}
				this.Add(value);
			}
			this.UpdateData(this.info.data);
			this.UpdateData(this.info.dataB);
			this.UpdateData(this.info.dataC);
		}
		public void RepairData(string set="A"){
			AttributeData[] dataSet = this.info.data;
			if(set == "B"){dataSet = this.info.dataB;}
			if(set == "C"){dataSet = this.info.dataC;}
			List<AttributeData> corrupt = new List<AttributeData>();
			foreach(var data in dataSet){
				if(data.IsNull()){
					if(Attribute.debug.Has("Issue")){Debug.Log("[Attribute] Removing null attribute data in " + this.info.path + ".");}
					corrupt.Add(data);
				}
				else{
					bool kidnapped = data.attribute.parent.gameObject != data.gameObject;
					bool amnesia = data.attribute.IsNull();
					bool orphanned = data.attribute.parent.IsNull();
					if(kidnapped || orphanned || amnesia){
						if(Attribute.debug.Has("Issue")){
							if(kidnapped){Debug.LogWarning("[Attribute] Data was kidnapped.  Call the cops! : " + data.referencePath);}
							if(orphanned){Debug.LogWarning("[Attribute] Data was orphanned.  What a travesty! : " + data.referencePath);}
							if(amnesia){Debug.LogWarning("[Attribute] Data has amnesia.  Who am I?! : " + data.referencePath);}
						}
						corrupt.Add(data);
					}
				}
			}
			if(corrupt.Count > 0){
				foreach(var data in corrupt){
					if(set == "A"){this.info.data = this.info.data.Remove(data);}
					if(set == "B"){this.info.dataB = this.info.dataB.Remove(data);}
					if(set == "C"){this.info.dataC = this.info.dataC.Remove(data);}
				}
				Utility.SetDirty(this.info.parent);
			}
		}
		public virtual void UpdateData(AttributeData[] dataSet){
			for(int index=0;index<dataSet.Length;++index){
				AttributeData data = dataSet[index];
				data.attribute = this.info;
				data.path = this.info.path + "/" + index;
				data.hideFlags = PlayerPrefs.GetInt("Attribute-ShowData") == 1 ? 0 : HideFlags.HideInInspector;
				data.Setup();
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
			if(!this.isSetup){this.Setup("",null);}
			if(this.getMethod != null){return this.getMethod();}
			if(this.info.data.Length < 1){
				if(Attribute.debug.Has("Issue")){Debug.LogWarning("[Attribute] Get : No data found for : " + this.info.path);}
				return default(BaseType);
			}
			if(this.info.mode != AttributeMode.Formula){
				/*if(this.dirty){
					this.dirty = !this.canCache;
					this.cachedValue = this.GetFirstRaw().Get();
				}
				return this.cachedValue;*/
				return this.GetFirstRaw().Get();
			}
			/*if(this.dirty){
				this.dirty = !this.canCache;
				this.cachedValue = this.GetFormulaValue();
			}
			return this.cachedValue;*/
			return this.GetFormulaValue();
		}
		public virtual void Set(BaseType value){
			this.isDefault = false;
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
			if(this.info.data.Length < 1){
				if(Attribute.debug.Has("Issue")){Debug.LogWarning("[Attribute] Set : No data found for : " + this.info.path);}
				return;
			}
			DataType data = this.GetFirstRaw();
			if(data == null){
				if(!Attribute.setWarning.ContainsKey(this)){
					if(Attribute.debug.Has("Issue")){Debug.LogWarning("[Attribute] No data found. (" + this.info.path + ")",this.target);}
					Attribute.setWarning[this] = true;
				}
			}
			else if(this.info.mode == AttributeMode.Normal){
				if(this.usage == AttributeUsage.Shaped){
					this.usage = AttributeUsage.Direct;
				}
				data.value = value;
			}
			else if(this.info.mode == AttributeMode.Linked && this.info.linkType != LinkType.Get){
				if(!Attribute.ready && Application.isPlaying){
					if(Attribute.debug.Has("Issue")){Debug.LogWarning("[Attribute] Set attempt before attribute data built : " + this.info.path);}
					return;
				}
				if(data.reference == null){
					if(!Attribute.setWarning.ContainsKey(this)){
						string source = "("+this.info.path+")";
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
					if(Attribute.debug.Has("Issue")){Debug.LogWarning("Attribute (Set): Cannot manually set values for formulas. (" + this.info.path + ")",this.info.parent);}
					Attribute.setWarning[this] = true;
				}
			}
		}
	}
}