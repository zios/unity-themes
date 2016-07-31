using System;
using System.Collections.Generic;
using UnityEngine;
namespace Zios.Attributes{
	[Serializable]
	public class AttributeFloat : Attribute<float,AttributeFloat,AttributeFloatData>{
		public static string[] specialList = new string[]{"Copy","Flip","Abs","Sign","Floor","Ceil","Cos","Sin","Tan","ATan","Sqrt"};
		public static Dictionary<Type,string[]> operators = new Dictionary<Type,string[]>(){
			{typeof(AttributeFloatData),new string[]{"+","-","×","÷","/","Distance","Average","Max","Min"}},
			{typeof(AttributeIntData),new string[]{"+","-","×","÷","/","Distance","Average","Max","Min"}}
		};
		public AttributeFloat() : this(0){}
		public AttributeFloat(float value){this.delayedValue = value;}
		public static implicit operator AttributeFloat(float current){return new AttributeFloat(current);}
		public static implicit operator float(AttributeFloat current){return current.Get();}
		/*public static float operator *(AttributeFloat current,float amount){return current.Get() * amount;}
		public static float operator +(AttributeFloat current,float amount){return current.Get() + amount;}
		public static float operator -(AttributeFloat current,float amount){return current.Get() - amount;}
		public static float operator /(AttributeFloat current,float amount){return current.Get() / amount;}*/
		public override float GetFormulaValue(){
			float value = 0;
			for(int index=0;index<this.data.Length;++index){
				AttributeData raw = this.data[index];
				string operation = AttributeFloat.operators[raw.GetType()][raw.operation];
				float current = raw is AttributeIntData ? ((AttributeIntData)raw).Get() : ((AttributeFloatData)raw).Get();
				if(index == 0){value = current;}
				else if(operation == "+"){value += current;}
				else if(operation == "-"){value -= current;}
				else if(operation == "×"){value *= current;}
				else if(operation == "÷"){value /= current;}
				else if(operation == "Average"){value = (value + current) / 2;}
				else if(operation == "Max"){value = Mathf.Max(value,current);}
				else if(operation == "Min"){value = Mathf.Min(value,current);}
			}
			return value;
		}
		public override Type[] GetFormulaTypes(){
			return new Type[]{typeof(AttributeFloatData),typeof(AttributeIntData)};
		}
	}
}