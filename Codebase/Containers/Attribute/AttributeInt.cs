using Zios;
using System;
using UnityEngine;
namespace Zios{
	[Serializable]
	public class AttributeInt : Attribute<int,AttributeInt,AttributeIntData,OperatorNumeral,SpecialNumeral>{
		public AttributeInt() : this(0){}
		public AttributeInt(int value){this.Add(value);}
		public static implicit operator AttributeInt(int current){return new AttributeInt(current);}
		public static implicit operator int(AttributeInt current){return current.Get();}
		public static int operator *(AttributeInt current,int amount){return current.Get() * amount;}
		public static int operator +(AttributeInt current,int amount){return current.Get() + amount;}
		public static int operator -(AttributeInt current,int amount){return current.Get() - amount;}
		public static int operator /(AttributeInt current,int amount){return current.Get() / amount;}
		public override int HandleSpecial(SpecialNumeral special,int value){
			if(this.mode == AttributeMode.Linked){return value;}
			else if(special == SpecialNumeral.Flip){return value * -1;}
			else if(special == SpecialNumeral.Abs){return Mathf.Abs(value);}
			else if(special == SpecialNumeral.Sign){return (int)Mathf.Sign(value);}
			else if(special == SpecialNumeral.Floor){return (int)Mathf.Floor(value);}
			else if(special == SpecialNumeral.Ceil){return (int)Mathf.Ceil(value);}
			else if(special == SpecialNumeral.Cos){return (int)Mathf.Cos(value);}
			else if(special == SpecialNumeral.Sin){return (int)Mathf.Sin(value);}
			else if(special == SpecialNumeral.Tan){return (int)Mathf.Tan(value);}
			else if(special == SpecialNumeral.ATan){return (int)Mathf.Atan(value);}
			else if(special == SpecialNumeral.Sqrt){return (int)Mathf.Sqrt(value);}
			return value;
		}
		public override int HandleOperator(OperatorNumeral sign){
			int value = 0;
			for(int index=0;index<this.data.Length;++index){
				var data = this.data[index];
				int current = this.GetValue(data);
				if(index == 0){value = current;}
				else if(data.sign == OperatorNumeral.Addition){value += current;}
				else if(data.sign == OperatorNumeral.Subtraction){value -= current;}
				else if(data.sign == OperatorNumeral.Multiplication){value *= current;}
				else if(data.sign == OperatorNumeral.Division){value /= current;}
				else if(data.sign == OperatorNumeral.Average){value = (value + current) / 2;}
				else if(data.sign == OperatorNumeral.Max){value = Mathf.Max(value,current);}
				else if(data.sign == OperatorNumeral.Min){value = Mathf.Min(value,current);}
			}
			return value;
		}
	}
	[Serializable]
	public class AttributeIntData : AttributeData<int,AttributeInt,OperatorNumeral,SpecialNumeral>{}
}
