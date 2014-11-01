using Zios;
using System;
using UnityEngine;
using System.Collections.Generic;
namespace Zios{
	[Serializable]
	public class AttributeBool : Attribute{
		public static Dictionary<GameObject,Dictionary<string,AttributeBool>> lookup = new Dictionary<GameObject,Dictionary<string,AttributeBool>>();
		public AttributeBoolData[] data = new AttributeBoolData[1];
		public AttributeBool() : this(false){}
		public AttributeBool(bool value){
			this.data[0] = new AttributeBoolData();
			this.data[0].value = value;
		}
		public static implicit operator AttributeBool(bool current){return new AttributeBool(current);}
		public static implicit operator bool(AttributeBool current){return current.Get();}
		public void Setup(string name,params MonoBehaviour[] scripts){
			var lookup = AttributeBool.lookup;
			this.script = scripts[0];
			string firstName = "";
			foreach(MonoBehaviour script in scripts){
				if(script == null){continue;}
				GameObject target = script.gameObject;
				string prefix = script is StateInterface ? ((StateInterface)script).alias : script.name;
				string current = prefix + "/" + name;
				if(firstName.IsEmpty()){firstName = current;}
				if(!lookup.ContainsKey(target)){
					lookup[target] = new Dictionary<string,AttributeBool>();
				}
				/*if(Application.isPlaying && lookup[target].ContainsKey(current) && lookup[target][current] != this){
					Debug.LogWarning("AttributeBool : Duplicate setup detected for  (" + this.path + ")");
					return;
				}*/
				lookup[target].RemoveValue(this);
				lookup[target][current] = this;
			}
			this.path = firstName;
			foreach(var data in this.data){
				data.target.Setup(name+"Target",scripts);
			}
		}
		public bool Get(){
			bool value = false;
			var first = this.data[0];
			if(this.mode != AttributeMode.Formula){
				return this.GetValue(first);
			}
			for(int index=0;index<this.data.Length;++index){
				var data = this.data[index];
				bool current = this.GetValue(data);
				if(index == 0){value = current;}
				else if(data.sign == AttributeBoolOperator.And){value = value && current;}
				else if(data.sign == AttributeBoolOperator.Or){value = value || current;}
			}
			return value;
		}
		public void Set(bool value){
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
		private bool GetValue(AttributeBoolData data){
			if(data.usage == AttributeUsage.Direct){
				return data.value;
			}
			else if(this.mode == AttributeMode.Linked || data.usage == AttributeUsage.Shaped){
				if(data.reference == null){
					Debug.LogWarning("Attribute : No reference found. (" + this.path + ")");
					return false;
				}
				bool value = data.reference.Get();
				return this.HandleSpecial(data.special,value);
			}		
			Debug.LogWarning("Attribute : No value found. (" + this.path + ")");
			return false;
		}
		private bool HandleSpecial(AttributeBoolSpecial special,bool value){
			if(this.mode == AttributeMode.Linked){return value;}
			else if(special == AttributeBoolSpecial.Flip){return !value;}
			return value;
		}
	}
}
