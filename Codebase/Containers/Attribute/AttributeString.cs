using Zios;
using System;
using UnityEngine;
using System.Collections.Generic;
namespace Zios{
	public enum SpecialString{Copy,Lower,Upper,Capitalize};
	[Serializable]
	public class AttributeString : Attribute<string,AttributeString,AttributeStringData,SpecialString>{
		public static Dictionary<Type,string[]> compare = new Dictionary<Type,string[]>(){
			{typeof(AttributeStringData),new string[]{"Prefix","Suffix"}}
		};
		public AttributeString() : this(""){}
		public AttributeString(string value){this.delayedValue = value;}
		public static implicit operator AttributeString(string current){return new AttributeString(current);}
		public static implicit operator string(AttributeString current){return current.Get();}
		//public static string operator +(AttributeString current,string amount){return current.Get() + amount;}
		public override string GetFormulaValue(){
			string value = "";
			for(int index=0;index<this.data.Length;++index){
				AttributeData raw = this.data[index];
				string sign = AttributeString.compare[raw.GetType()][raw.sign];
				var data = (AttributeStringData)raw;
				string current = data.Get();
				if(index == 0){value = current;}
				else if(sign == "Prefix"){value = current + value;}
				else if(sign == "Suffix"){value = value + current;}
			}
			return value;
		}
	}
}
