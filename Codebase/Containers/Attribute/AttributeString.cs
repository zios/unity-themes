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
		public AttributeString(string value){this.Add(value);}
		public static implicit operator AttributeString(string current){return new AttributeString(current);}
		public static implicit operator string(AttributeString current){return current.Get();}
		public static string operator +(AttributeString current,string amount){return current.Get() + amount;}
		public override string HandleSpecial(SpecialString special,string value){
			if(this.mode == AttributeMode.Linked){return value;}
			else if(special == SpecialString.Lower){return value.ToLower();}
			else if(special == SpecialString.Upper){return value.ToUpper();}
			else if(special == SpecialString.Capitalize){return value.Capitalize();}
			return value;
		}
		public override string HandleOperator(OperatorString sign){
			string value = "";
			for(int index=0;index<this.data.Length;++index){
				var data = this.data[index];
				string current = this.GetValue(data);
				if(index == 0){value = current;}
				else if(data.sign == OperatorString.Prefix){value = current + value;}
				else if(data.sign == OperatorString.Suffix){value = value + current;}
			}
			return value;
		}
	}
	[Serializable]
	public class AttributeStringData : AttributeData<string,AttributeString,OperatorString,SpecialString>{
		public int characterLimit;
		public string[] allowed = new string[0];
		public string[] disallowed = new string[0];
	}
}
