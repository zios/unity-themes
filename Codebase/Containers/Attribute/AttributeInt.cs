using Zios;
using System;
using UnityEngine;
using System.Collections.Generic;
namespace Zios{
	[Serializable]
	public class AttributeInt : Attribute{
		public static Dictionary<GameObject,Dictionary<string,AttributeInt>> lookup = new Dictionary<GameObject,Dictionary<string,AttributeInt>>();
		public AttributeIntData[] data = new AttributeIntData[1];
		public AttributeInt() : this(0){}
		public AttributeInt(int value){
			this.data[0] = new AttributeIntData();
			this.data[0].value = value;
		}
		public static implicit operator AttributeInt(int current){return new AttributeInt(current);}
		public static implicit operator int(AttributeInt current){return current.Get();}
		public static int operator *(AttributeInt current,int amount){return current.Get() * amount;}
		public static int operator +(AttributeInt current,int amount){return current.Get() + amount;}
		public static int operator -(AttributeInt current,int amount){return current.Get() - amount;}
		public static int operator /(AttributeInt current,int amount){return current.Get() / amount;}
		public void Setup(string name,params MonoBehaviour[] scripts){
			var lookup = AttributeInt.lookup;
			this.script = scripts[0];
			string firstName = "";
			foreach(MonoBehaviour script in scripts){
				if(script == null){continue;}
				GameObject target = script.gameObject;
				string prefix = script is StateInterface ? ((StateInterface)script).alias : script.name;
				string current = prefix + "/" + name;
				if(firstName.IsEmpty()){firstName = current;}
				if(!lookup.ContainsKey(target)){
					lookup[target] = new Dictionary<string,AttributeInt>();
				}
				lookup[target].RemoveValue(this);
				lookup[target][current] = this;
			}
			this.path = firstName;
			foreach(var data in this.data){
				data.target.Setup(name+"Target",scripts);
			}
		}
		public int Get(){
			int value = 0;
			var first = this.data[0];
			if(this.mode != AttributeMode.Formula){
				return this.GetValue(first);
			}
			for(int index=0;index<this.data.Length;++index){
				var data = this.data[index];
				int current = this.GetValue(data);
				if(index == 0){value = current;}
				else if(data.sign == AttributeNumeralOperator.Addition){value += current;}
				else if(data.sign == AttributeNumeralOperator.Subtraction){value -= current;}
				else if(data.sign == AttributeNumeralOperator.Multiplication){value *= current;}
				else if(data.sign == AttributeNumeralOperator.Division){value /= current;}
				else if(data.sign == AttributeNumeralOperator.Average){value = (value + current) / 2;}
				else if(data.sign == AttributeNumeralOperator.Max){value = Mathf.Max(value,current);}
				else if(data.sign == AttributeNumeralOperator.Min){value = Mathf.Min(value,current);}
			}
			return value;
		}
		public void Set(int value){
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
		private int GetValue(AttributeIntData data){
			if(data.usage == AttributeUsage.Direct){
				return data.value;
			}
			else if(this.mode == AttributeMode.Linked || data.usage == AttributeUsage.Shaped){
				if(data.reference == null){
					Debug.LogWarning("Attribute : No reference found. (" + this.path + ")");
					return 0;
				}
				int value = data.reference.Get();
				return this.HandleSpecial(data.special,value);
			}		
			Debug.LogWarning("Attribute : No value found. (" + this.path + ")");
			return 0;
		}
		private int HandleSpecial(AttributeNumeralSpecial special,int value){
			if(this.mode == AttributeMode.Linked){return value;}
			else if(special == AttributeNumeralSpecial.Flip){return value * -1;}
			else if(special == AttributeNumeralSpecial.Abs){return Mathf.Abs(value);}
			else if(special == AttributeNumeralSpecial.Sign){return (int)Mathf.Sign(value);}
			else if(special == AttributeNumeralSpecial.Floor){return (int)Mathf.Floor(value);}
			else if(special == AttributeNumeralSpecial.Ceil){return (int)Mathf.Ceil(value);}
			else if(special == AttributeNumeralSpecial.Cos){return (int)Mathf.Cos(value);}
			else if(special == AttributeNumeralSpecial.Sin){return (int)Mathf.Sin(value);}
			else if(special == AttributeNumeralSpecial.Tan){return (int)Mathf.Tan(value);}
			else if(special == AttributeNumeralSpecial.ATan){return (int)Mathf.Atan(value);}
			else if(special == AttributeNumeralSpecial.Sqrt){return (int)Mathf.Sqrt(value);}
			return value;
		}
	}
}
