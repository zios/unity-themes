using System;
using System.Collections.Generic;
namespace Zios.Attributes{
	[Serializable]
	public class AttributeString : Attribute<string,AttributeString,AttributeStringData>{
		public static string[] specialList = new string[]{"Copy","Lower","Upper","Capitalize"};
		public static Dictionary<Type,string[]> operators = new Dictionary<Type,string[]>(){
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
				string operation = AttributeString.operators[raw.GetType()][raw.operation];
				var data = (AttributeStringData)raw;
				string current = data.Get();
				if(index == 0){value = current;}
				else if(operation == "Prefix"){value = current + value;}
				else if(operation == "Suffix"){value = value + current;}
			}
			return value;
		}
	}
}