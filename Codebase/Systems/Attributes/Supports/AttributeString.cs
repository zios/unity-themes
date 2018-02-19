using System;
using System.Collections.Generic;
namespace Zios.Attributes.Supports{
	using Zios.Extensions.Convert;
	[Serializable]
	public class AttributeString : Attribute<string,AttributeString,AttributeStringData>{
		public static string[] specialList = new string[]{"Copy","Lower","Upper","Capitalize"};
		public static Dictionary<Type,string[]> operators = new Dictionary<Type,string[]>(){
			{typeof(AttributeStringData),new string[]{"Prefix","Suffix"}},
			{typeof(AttributeFloatData),new string[]{"Prefix","Suffix"}},
			{typeof(AttributeIntData),new string[]{"Prefix","Suffix"}},
			{typeof(AttributeBoolData),new string[]{"Prefix","Suffix"}},
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
				Type rawType = raw.GetType();
				string operation = AttributeString.operators[raw.GetType()][raw.operation];
				string current = "";
				if(rawType == typeof(AttributeStringData)){current = raw.As<AttributeStringData>().Get();}
				else if(rawType == typeof(AttributeFloatData)){current = raw.As<AttributeFloatData>().Get().ToString();}
				else if(rawType == typeof(AttributeIntData)){current = raw.As<AttributeIntData>().Get().ToString();}
				else if(rawType == typeof(AttributeBoolData)){current = raw.As<AttributeBoolData>().Get().ToString();}
				if(index == 0){value = current;}
				else if(operation == "Prefix"){value = current + value;}
				else if(operation == "Suffix"){value = value + current;}
			}
			return value;
		}
		public override Type[] GetFormulaTypes(){
			return new Type[]{typeof(AttributeStringData),typeof(AttributeFloatData),typeof(AttributeIntData),typeof(AttributeBoolData)};
		}
	}
}