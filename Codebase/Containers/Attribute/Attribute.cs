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
		public string path;
		public MonoBehaviour script;
		public AttributeMode mode = AttributeMode.Normal;
		public bool locked;
		public virtual void Add(){}
		public virtual void Remove(AttributeData data){}
	}
	[Serializable]
	public class Attribute<BaseType,Type,DataType,Operator,Special> : Attribute
	where Type : Attribute<BaseType,Type,DataType,Operator,Special>
	where DataType : AttributeData<BaseType,Type,Operator,Special>,new(){
		public static Dictionary<GameObject,Dictionary<string,Type>> lookup = new Dictionary<GameObject,Dictionary<string,Type>>();
		public DataType[] data = new DataType[0];
		public AttributeUsage usage{
			get{return this.data[0].usage;}
			set{
				foreach(DataType data in this.data){
					data.usage = value;
				}
			}
		}
		public virtual BaseType HandleSpecial(Special special,BaseType value){return default(BaseType);}
		public virtual BaseType HandleOperator(Operator sign){return default(BaseType);}
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
		public void Setup(string name,params MonoBehaviour[] scripts){
			var lookup = this.GetLookupTable();
			this.script = scripts[0];
			string path = "";
			name = name.Trim();
			foreach(MonoBehaviour script in scripts){
				if(script == null){continue;}
				if(path.IsEmpty() && script is StateInterface){
					StateInterface item = (StateInterface)script;
					path = item.alias + "/";
				}
				GameObject target = script.gameObject;
				string prefix = script is StateInterface ? "" : "*/";
				string current = prefix + path + name;
				if(!lookup.ContainsKey(target)){
					lookup[target] = new Dictionary<string,Type>();
				}
				Type self = (Type)this;
				lookup[target].RemoveValue(self);
				lookup[target][current] = self;
			}
			this.path = path + name;
			foreach(var data in this.data){
				data.target.Setup(name+"Target",scripts);
			}
		}
		public BaseType Get(){
			DataType first = this.data[0];
			if(this.mode != AttributeMode.Formula){
				return this.GetValue(first);
			}
			return this.HandleOperator(first.sign);
		}
		public void Set(BaseType value){
			if(this.data == null || this.data.Length < 1){
				Debug.LogWarning("Attribute : No data found. (" + this.path + ")");
			}
			else if(this.mode == AttributeMode.Normal){
				this.data[0].value = value;
			}
			else if(this.mode == AttributeMode.Linked){
				if(data[0].reference == null){
					Debug.LogWarning("Attribute : No reference found. (" + this.path + ")");
					return;
				}
				this.data[0].reference.Set(value);
			}
			else if(this.mode == AttributeMode.Formula){
				Debug.LogWarning("Attribute : Cannot manually set values for formulas. (" + this.path + ")");
			}
		}
		public BaseType GetValue(DataType data){
			if(data.usage == AttributeUsage.Direct){
				return data.value;
			}
			else if(this.mode == AttributeMode.Linked || data.usage == AttributeUsage.Shaped){
				if(data.reference == null){
					Debug.LogWarning("Attribute : No reference found. (" + this.path + ")");
					return default(BaseType);
				}
				BaseType value = data.reference.Get();
				return this.HandleSpecial(data.special,value);
			}		
			Debug.LogWarning("Attribute : No value found. (" + this.path + ")");
			return default(BaseType);
		}
		public Dictionary<GameObject,Dictionary<string,Type>> GetLookupTable(){
			System.Type type = typeof(Attribute<BaseType,Type,DataType,Operator,Special>);
			object lookupObject = type.GetField("lookup",BindingFlags.Public|BindingFlags.Static).GetValue(null);
			return (Dictionary<GameObject,Dictionary<string,Type>>)lookupObject;
		}
	}
	[Serializable]
	public class AttributeData{
		public Target target = new Target();
		public AttributeUsage usage;
	}
	[Serializable]
	public class AttributeData<Type,AttributeType,Operator,Special> : AttributeData
		where Operator : struct 
		where Special : struct{
		public AttributeType reference;
		public Type value;
		public Operator sign;
		public Special special;
		public bool clamp;
		public Type clampMin;
		public Type clampMax;
	}
}