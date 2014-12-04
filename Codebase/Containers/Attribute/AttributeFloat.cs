using Zios;
using System;
using UnityEngine;
namespace Zios{
	public enum OperatorNumeral{Addition,Subtraction,Multiplication,Division,Distance,Average,Max,Min}
	public enum SpecialNumeral{Copy,Flip,Abs,Sign,Floor,Ceil,Cos,Sin,Tan,ATan,Sqrt};
	[Serializable]
	public class AttributeFloat : Attribute<float,AttributeFloat,AttributeFloatData,OperatorNumeral,SpecialNumeral>{
		public AttributeFloat() : this(0){}
		public AttributeFloat(float value){this.delayedValue = value;}
		public static implicit operator AttributeFloat(float current){return new AttributeFloat(current);}
		public static implicit operator float(AttributeFloat current){return current.Get();}
		public static float operator *(AttributeFloat current,float amount){return current.Get() * amount;}
		public static float operator +(AttributeFloat current,float amount){return current.Get() + amount;}
		public static float operator -(AttributeFloat current,float amount){return current.Get() - amount;}
		public static float operator /(AttributeFloat current,float amount){return current.Get() / amount;}
		public override float GetFormulaValue(){
			float value = 0;
			for(int index=0;index<this.data.Length;++index){
				AttributeData raw = this.data[index];
				var sign = raw is AttributeIntData ? ((AttributeIntData)raw).sign : ((AttributeFloatData)raw).sign;
				float current = raw is AttributeIntData ? ((AttributeIntData)raw).Get() : ((AttributeFloatData)raw).Get();
				if(index == 0){value = current;}
				else if(sign == OperatorNumeral.Addition){value += current;}
				else if(sign == OperatorNumeral.Subtraction){value -= current;}
				else if(sign == OperatorNumeral.Multiplication){value *= current;}
				else if(sign == OperatorNumeral.Division){value /= current;}
				else if(sign == OperatorNumeral.Average){value = (value + current) / 2;}
				else if(sign == OperatorNumeral.Max){value = Mathf.Max(value,current);}
				else if(sign == OperatorNumeral.Min){value = Mathf.Min(value,current);}
			}
			return value;
		}
		public override Type[] GetFormulaTypes(){
			return new Type[]{typeof(AttributeFloatData),typeof(AttributeIntData)};
		}
	}
}
