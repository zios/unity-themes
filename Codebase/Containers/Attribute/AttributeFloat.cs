using Zios;
using System;
using UnityEngine;
namespace Zios{
	public enum OperatorNumeral{Addition,Subtraction,Multiplication,Division,Distance,Average,Max,Min}
	public enum SpecialNumeral{Copy,Flip,Abs,Sign,Floor,Ceil,Cos,Sin,Tan,ATan,Sqrt};
	[Serializable]
	public class AttributeFloat : Attribute<float,AttributeFloat,AttributeFloatData,OperatorNumeral,SpecialNumeral>{
		public AttributeFloat() : this(0){}
		public AttributeFloat(float value){this.Add(value);}
		public static implicit operator AttributeFloat(float current){return new AttributeFloat(current);}
		public static implicit operator float(AttributeFloat current){return current.Get();}
		public static float operator *(AttributeFloat current,float amount){return current.Get() * amount;}
		public static float operator +(AttributeFloat current,float amount){return current.Get() + amount;}
		public static float operator -(AttributeFloat current,float amount){return current.Get() - amount;}
		public static float operator /(AttributeFloat current,float amount){return current.Get() / amount;}
		public override float HandleSpecial(SpecialNumeral special,float value){
			if(this.mode == AttributeMode.Linked){return value;}
			else if(special == SpecialNumeral.Flip){return value * -1;}
			else if(special == SpecialNumeral.Abs){return Mathf.Abs(value);}
			else if(special == SpecialNumeral.Sign){return Mathf.Sign(value);}
			else if(special == SpecialNumeral.Floor){return Mathf.Floor(value);}
			else if(special == SpecialNumeral.Ceil){return Mathf.Ceil(value);}
			else if(special == SpecialNumeral.Cos){return Mathf.Cos(value);}
			else if(special == SpecialNumeral.Sin){return Mathf.Sin(value);}
			else if(special == SpecialNumeral.Tan){return Mathf.Tan(value);}
			else if(special == SpecialNumeral.ATan){return Mathf.Atan(value);}
			else if(special == SpecialNumeral.Sqrt){return Mathf.Sqrt(value);}
			return value;
		}
		public override float HandleOperator(OperatorNumeral sign){
			float value = 0;
			for(int index=0;index<this.data.Length;++index){
				var data = this.data[index];
				float current = this.GetValue(data);
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
	}
	[Serializable]
	public class AttributeFloatData : AttributeData<float,AttributeFloat,OperatorNumeral,SpecialNumeral>{}
}
