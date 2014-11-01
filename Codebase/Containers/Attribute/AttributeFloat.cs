using Zios;
using System;
using UnityEngine;
using System.Collections.Generic;
namespace Zios{
	[Serializable]
	public class AttributeFloat : Attribute{
		public static Dictionary<GameObject,Dictionary<string,AttributeFloat>> lookup = new Dictionary<GameObject,Dictionary<string,AttributeFloat>>();
		public AttributeFloatData[] data = new AttributeFloatData[1];
		public AttributeFloat() : this(0){}
		public AttributeFloat(float value){
			this.data[0] = new AttributeFloatData();
			this.data[0].value = value;
		}
		public static implicit operator AttributeFloat(float current){return new AttributeFloat(current);}
		public static implicit operator float(AttributeFloat current){return current.Get();}
		public static float operator *(AttributeFloat current,float amount){return current.Get() * amount;}
		public static float operator +(AttributeFloat current,float amount){return current.Get() + amount;}
		public static float operator -(AttributeFloat current,float amount){return current.Get() - amount;}
		public static float operator /(AttributeFloat current,float amount){return current.Get() / amount;}
		public void Setup(string name,params MonoBehaviour[] scripts){
			var lookup = AttributeFloat.lookup;
			this.script = scripts[0];
			string firstName = "";
			foreach(MonoBehaviour script in scripts){
				if(script == null){continue;}
				GameObject target = script.gameObject;
				string prefix = script is StateInterface ? ((StateInterface)script).alias : script.name;
				string current = prefix + "/" + name;
				if(firstName.IsEmpty()){firstName = current;}
				if(!lookup.ContainsKey(target)){
					lookup[target] = new Dictionary<string,AttributeFloat>();
				}
				lookup[target].RemoveValue(this);
				lookup[target][current] = this;
			}
			this.path = firstName;
			foreach(var data in this.data){
				data.target.Setup(name+"Target",scripts);
			}
		}
		public float Get(){
			float value = 0;
			var first = this.data[0];
			if(this.mode != AttributeMode.Formula){
				return this.GetValue(first);
			}
			for(int index=0;index<this.data.Length;++index){
				var data = this.data[index];
				float current = this.GetValue(data);
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
		public void Set(float value){
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
		private float GetValue(AttributeFloatData data){
			if(data.usage == AttributeUsage.Direct){
				return data.value;
			}
			else if(this.mode == AttributeMode.Linked || data.usage == AttributeUsage.Shaped){
				if(data.reference == null){
					Debug.LogWarning("Attribute : No reference found. (" + this.path + ")");
					return 0;
				}
				float value = data.reference.Get();
				return this.HandleSpecial(data.special,value);
			}		
			Debug.LogWarning("Attribute : No value found. (" + this.path + ")");
			return 0;
		}
		private float HandleSpecial(AttributeNumeralSpecial special,float value){
			if(this.mode == AttributeMode.Linked){return value;}
			else if(special == AttributeNumeralSpecial.Flip){return value * -1;}
			else if(special == AttributeNumeralSpecial.Abs){return Mathf.Abs(value);}
			else if(special == AttributeNumeralSpecial.Sign){return Mathf.Sign(value);}
			else if(special == AttributeNumeralSpecial.Floor){return Mathf.Floor(value);}
			else if(special == AttributeNumeralSpecial.Ceil){return Mathf.Ceil(value);}
			else if(special == AttributeNumeralSpecial.Cos){return Mathf.Cos(value);}
			else if(special == AttributeNumeralSpecial.Sin){return Mathf.Sin(value);}
			else if(special == AttributeNumeralSpecial.Tan){return Mathf.Tan(value);}
			else if(special == AttributeNumeralSpecial.ATan){return Mathf.Atan(value);}
			else if(special == AttributeNumeralSpecial.Sqrt){return Mathf.Sqrt(value);}
			return value;
		}
	}
}
