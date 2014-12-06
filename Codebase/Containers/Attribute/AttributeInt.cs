using Zios;
using System;
using UnityEngine;
namespace Zios{
	[Serializable]
	public class AttributeInt : Attribute<int,AttributeInt,AttributeIntData,OperatorNumeral,SpecialNumeral>{
		public AttributeInt() : this(0){}
		public AttributeInt(int value){this.delayedValue = value;}
		public static implicit operator AttributeInt(int current){return new AttributeInt(current);}
		public static implicit operator int(AttributeInt current){return current.Get();}
		/*public static int operator *(AttributeInt current,int amount){return current.Get() * amount;}
		public static int operator +(AttributeInt current,int amount){return current.Get() + amount;}
		public static int operator -(AttributeInt current,int amount){return current.Get() - amount;}
		public static int operator /(AttributeInt current,int amount){return current.Get() / amount;}*/
		public override int GetFormulaValue(){
			int value = 0;
			for(int index=0;index<this.data.Length;++index){
				AttributeData raw = this.data[index];
				var sign = raw is AttributeIntData ? ((AttributeIntData)raw).sign : ((AttributeFloatData)raw).sign;
				int current = raw is AttributeIntData ? ((AttributeIntData)raw).Get() : (int)((AttributeFloatData)raw).Get();
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
			return new Type[]{typeof(AttributeIntData),typeof(AttributeFloatData)};
		}
	}
}
