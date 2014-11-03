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
		public AttributeBool(bool value){this.Add(value);}
		public static implicit operator AttributeBool(bool current){return new AttributeBool(current);}
		public static implicit operator bool(AttributeBool current){return current.Get();}
		public override bool HandleSpecial(SpecialBool special,bool value){
			if(this.mode == AttributeMode.Linked){return value;}
			else if(special == SpecialBool.Flip){return !value;}
			return value;
		}
		public override bool HandleOperator(OperatorBool sign){
			bool value = false;
			var first = this.data[0];
			if(this.mode != AttributeMode.Formula){
				return this.GetValue(first);
			}
			for(int index=0;index<this.data.Length;++index){
				var data = this.data[index];
				bool current = this.GetValue(data);
				if(index == 0){value = current;}
				else if(data.sign == OperatorBool.And){value = value && current;}
				else if(data.sign == OperatorBool.Or){value = value || current;}
			}
			return value;
		}
	}
	[Serializable]
	public class AttributeBoolData : AttributeData<bool,AttributeBool,OperatorBool,SpecialBool>{}
}
