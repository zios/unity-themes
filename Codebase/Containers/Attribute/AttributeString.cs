using Zios;
using System;
using UnityEngine;
using System.Collections.Generic;
namespace Zios{
	public enum OperatorString{Prefix,Suffix}
	public enum SpecialString{Copy,Lower,Upper,Capitalize};
	[Serializable]
	public class AttributeString : Attribute<string,AttributeString,AttributeStringData,OperatorString,SpecialString>{
		public AttributeString() : this(""){}
		public AttributeString(string value){this.delayedValue = value;}
		public static implicit operator AttributeString(string current){return new AttributeString(current);}
		public static implicit operator string(AttributeString current){return current.Get();}
		//public static string operator +(AttributeString current,string amount){return current.Get() + amount;}
		public override string GetFormulaValue(){
			string value = "";
			for(int index=0;index<this.data.Length;++index){
				AttributeData raw = this.data[index];
				var data = (AttributeStringData)raw;
				var sign = data.sign;
				string current = data.Get();
				if(index == 0){value = current;}
				else if(sign == OperatorString.Prefix){value = current + value;}
				else if(sign == OperatorString.Suffix){value = value + current;}
			}
			return value;
		}
	}
}
