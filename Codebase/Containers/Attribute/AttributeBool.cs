using Zios;
using System;
using UnityEngine;
using System.Collections.Generic;
namespace Zios{
	public enum SpecialBool{Copy,Flip};
	public enum CompareNumber{NotZero,Zero,Negative,Positive}
	public enum CompareAgainst{Equals,NotEquals,LessThan,GreaterThan,Range}
	[Serializable]
	public class AttributeBool : Attribute<bool,AttributeBool,AttributeBoolData,SpecialBool>{
		public static Dictionary<Type,string[]> compare = new Dictionary<Type,string[]>(){
			{typeof(AttributeBoolData),new string[]{"And","Or"}}
		};
		public AttributeBool() : this(false){}
		public AttributeBool(bool value){this.delayedValue = value;}
		public static implicit operator AttributeBool(bool current){return new AttributeBool(current);}
		public static implicit operator bool(AttributeBool current){return current.Get();}
		public override bool GetFormulaValue(){
			bool value = false;
			for(int index=0;index<this.data.Length;++index){
				AttributeData raw = this.data[index];
				string sign = AttributeBool.compare[raw.GetType()][raw.sign];
				var data = (AttributeBoolData)raw;
				bool current = data.Get();
				if(index == 0){value = current;}
				else if(sign == "And"){value = value && current;}
				else if(sign == "Or"){value = value || current;}
			}
			return value;
		}
	}
}