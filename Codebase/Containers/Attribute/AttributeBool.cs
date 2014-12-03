using Zios;
using System;
using UnityEngine;
using System.Collections.Generic;
namespace Zios{
	public enum OperatorBool{And,Or}
	public enum SpecialBool{Copy,Flip};
	public enum CompareNumber{NotZero,Zero,Negative,Positive}
	public enum CompareAgainst{Equals,NotEquals,LessThan,GreaterThan,Range}
	[Serializable]
	public class AttributeBool : Attribute<bool,AttributeBool,AttributeBoolData,OperatorBool,SpecialBool>{
		public AttributeBool() : this(false){}
		public AttributeBool(bool value){this.delayedValue = value;}
		public static implicit operator AttributeBool(bool current){return new AttributeBool(current);}
		public static implicit operator bool(AttributeBool current){return current.Get();}
		public override bool GetFormulaValue(){
			bool value = false;
			for(int index=0;index<this.data.Length;++index){
				AttributeData raw = this.data[index];
				var data = (AttributeBoolData)raw;
				var sign = data.sign;
				bool current = data.Get();
				if(index == 0){value = current;}
				else if(sign == OperatorBool.And){value = value && current;}
				else if(sign == OperatorBool.Or){value = value || current;}
			}
			return value;
		}
	}
}