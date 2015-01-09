using Zios;
using System;
using UnityEngine;
using System.Collections.Generic;
namespace Zios{
	[Serializable]
	public class AttributeBool : Attribute<bool,AttributeBool,AttributeBoolData>{
		public static string[] specialList = new string[]{"Copy","Flip"};
		public static Dictionary<Type,string[]> operators = new Dictionary<Type,string[]>(){
			{typeof(AttributeBoolData),new string[]{"And","Or"}},
			{typeof(AttributeIntData),new string[]{"And","Or"}},
			{typeof(AttributeFloatData),new string[]{"And","Or"}},
			//{typeof(AttributeVector3Data),new string[]{"And","Or"}}
		};
		public static Dictionary<string,string[]> comparers = new Dictionary<string,string[]>(){
			{"BoolBool",new string[]{"==","!="}},
			{"NumberNumber",new string[]{"<",">","<=",">=","==","!="}},
			//{"Vector3Vector3",new string[]{"<",">","<=",">=","==","!="}},
		};
		public AttributeBool() : this(false){}
		public AttributeBool(bool value){this.delayedValue = value;}
		public static implicit operator AttributeBool(bool current){return new AttributeBool(current);}
		public static implicit operator bool(AttributeBool current){return current.Get();}
		public override bool GetFormulaValue(){
			bool value = true;
			for(int index=0;index<this.data.Length;++index){
				bool current = false;
				AttributeData compare = this.info.data[index];
				AttributeData against = this.info.dataB[index];
				bool compareIsNumber = compare is AttributeIntData || compare is AttributeFloatData;
				bool againstIsNumber = against is AttributeIntData || against is AttributeFloatData;
				string operation = compare.operation == 0 ? "And" : "Or";
				if(operation == "Or" && value){break;}
				if(compare is AttributeBoolData && against is AttributeBoolData){
					string comparer = AttributeBool.comparers["BoolBool"][against.operation];
					bool compareValue = ((AttributeBoolData)compare).Get();
					bool againstValue = ((AttributeBoolData)against).Get();
					if(comparer == "=="){current = compareValue == againstValue;}
					else if(comparer == "!="){current = compareValue != againstValue;}
				}
				else if(compareIsNumber && againstIsNumber){
					string comparer = AttributeBool.comparers["NumberNumber"][against.operation];
					float compareValue = compare is AttributeIntData ? ((AttributeIntData)compare).Get() : ((AttributeFloatData)compare).Get();
					float againstValue = against is AttributeIntData ? ((AttributeIntData)against).Get() : ((AttributeFloatData)against).Get();
					if(comparer == "<"){current = compareValue < againstValue;}
					else if(comparer == ">"){current = compareValue > againstValue;}
					else if(comparer == "<="){current = compareValue <= againstValue;}
					else if(comparer == ">="){current = compareValue >= againstValue;}
					else if(comparer == "=="){current = compareValue == againstValue;}
					else if(comparer == "!+"){current = compareValue != againstValue;}
				}
				/*else if(compare is AttributeVector3Data && against is AttributeVector3Data){
					string comparer = AttributeBool.compareAgainst["Vector3Vector3"][against.operation];
					Vector3 compareValue = ((AttributeVector3Data)compare).Get();
					Vector3 againstValue = ((AttributeVector3Data)against).Get();
					if(comparer == "<"){current = compareValue < againstValue;}
					else if(comparer == ">"){current = compareValue > againstValue;}
					else if(comparer == "<="){current = compareValue >= againstValue;}
					else if(comparer == ">="){current = compareValue <= againstValue;}
					else if(comparer == "=="){current = compareValue == againstValue;}
					else if(comparer == "!+"){current = compareValue != againstValue;}
				}*/
				if(operation == "And"){value = value && current;}
				else if(operation == "Or"){value = value || current;}
			}
			return value;
		}
		public override Type[] GetFormulaTypes(){
			return new Type[]{typeof(AttributeBoolData),typeof(AttributeIntData),typeof(AttributeFloatData)};
		}
	}
}
