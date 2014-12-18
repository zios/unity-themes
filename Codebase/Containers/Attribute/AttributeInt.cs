using Zios;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace Zios{
	[Serializable]
	public class AttributeInt : Attribute<int,AttributeInt,AttributeIntData>{
		public static string[] specialList = new string[]{"Copy","Flip","Abs","Sign","Floor","Ceil","Cos","Sin","Tan","ATan","Sqrt"};
		public static Dictionary<Type,string[]> compare = new Dictionary<Type,string[]>(){
			{typeof(AttributeIntData),new string[]{"+","-","×","÷","/","Distance","Average","Max","Min"}},
			{typeof(AttributeFloatData),new string[]{"+","-","×","÷","/","Distance","Average","Max","Min"}}
		};
		public AttributeInt() : this(0){}
		public AttributeInt(int value){this.delayedValue = value;}
		public static implicit operator AttributeInt(int current){return new AttributeInt(current);}
		public static implicit operator int(AttributeInt current){return current.Get();}
		/*public static int operator *(AttributeInt current,int amount){return current.Get() * amount;}
		public static int operator +(AttributeInt current,int amount){return current.Get() + amount;}
		public static int operator -(AttributeInt current,int amount){return current.Get() - amount;}
		public static int operator /(AttributeInt current,int amount){return current.Get() / amount;}*/
		public override int GetFormulaValue(){
			int value = 0;
			for(int index=0;index<this.data.Length;++index){
				AttributeData raw = this.data[index];
				string operation = AttributeInt.compare[raw.GetType()][raw.operation];
				int current = raw is AttributeIntData ? ((AttributeIntData)raw).Get() : (int)((AttributeFloatData)raw).Get();
				if(index == 0){value = current;}
				else if(operation == "+"){value += current;}
				else if(operation == "-"){value -= current;}
				else if(operation == "×"){value *= current;}
				else if(operation == "÷"){value /= current;}
				else if(operation == "Average"){value = (value + current) / 2;}
				else if(operation == "Max"){value = Mathf.Max(value,current);}
				else if(operation == "Min"){value = Mathf.Min(value,current);}
			}
			return value;
		}
		public override Type[] GetFormulaTypes(){
			return new Type[]{typeof(AttributeIntData),typeof(AttributeFloatData)};
		}
	}
}
