using Zios;
using System;
using UnityEngine;
using System.Collections.Generic;
namespace Zios{
	[Serializable]
	public class AttributeString : Attribute{
		public static Dictionary<GameObject,Dictionary<string,AttributeString>> lookup = new Dictionary<GameObject,Dictionary<string,AttributeString>>();
		public AttributeStringData[] data = new AttributeStringData[1];
		public AttributeString() : this(""){}
		public AttributeString(string value){
			this.data[0] = new AttributeStringData();
			this.data[0].value = value;
		}
		public static implicit operator AttributeString(string current){return new AttributeString(current);}
		public static implicit operator string(AttributeString current){return current.Get();}
		public static string operator +(AttributeString current,string amount){return current.Get() + amount;}
		public void Setup(string name,params MonoBehaviour[] scripts){
			var lookup = AttributeString.lookup;
			this.script = scripts[0];
			string firstName = "";
			foreach(MonoBehaviour script in scripts){
				if(script == null){continue;}
				GameObject target = script.gameObject;
				string prefix = script is StateInterface ? ((StateInterface)script).alias : script.name;
				string current = prefix + "/" + name;
				if(firstName.IsEmpty()){firstName = current;}
				if(!lookup.ContainsKey(target)){
					lookup[target] = new Dictionary<string,AttributeString>();
				}
				lookup[target].RemoveValue(this);
				lookup[target][current] = this;
			}
			this.path = firstName;
			foreach(var data in this.data){
				data.target.Setup(name+"Target",scripts);
			}
		}
		public string Get(){
			string value = "";
			var first = this.data[0];
			if(this.mode != AttributeMode.Formula){
				return this.GetValue(first);
			}
			for(int index=0;index<this.data.Length;++index){
				var data = this.data[index];
				string current = this.GetValue(data);
				if(index == 0){value = current;}
				else if(data.sign == AttributeStringOperator.Prefix){value = current + value;}
				else if(data.sign == AttributeStringOperator.Suffix){value = value + current;}
			}
			return value;
		}
		public void Set(string value){
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
		private string GetValue(AttributeStringData data){
			if(data.usage == AttributeUsage.Direct){
				return data.value;
			}
			else if(this.mode == AttributeMode.Linked || data.usage == AttributeUsage.Shaped){
				if(data.reference == null){
					Debug.LogWarning("Attribute : No reference found. (" + this.path + ")");
					return "";
				}
				string value = data.reference.Get();
				return this.HandleSpecial(data.special,value);
			}		
			Debug.LogWarning("Attribute : No value found. (" + this.path + ")");
			return"";
		}
		private string HandleSpecial(AttributeStringSpecial special,string value){
			if(this.mode == AttributeMode.Linked){return value;}
			else if(special == AttributeStringSpecial.Lower){return value.ToLower();}
			else if(special == AttributeStringSpecial.Upper){return value.ToUpper();}
			else if(special == AttributeStringSpecial.Capitalize){return value.Capitalize();}
			return value;
		}
	}
}
