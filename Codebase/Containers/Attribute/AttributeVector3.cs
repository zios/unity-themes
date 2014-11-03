using Zios;
using System;
using UnityEngine;
using System.Collections.Generic;
namespace Zios{
	public enum OperatorVector3{Addition,Subtraction,Multiplication,Distance,Average,Max,Min}
	public enum SpecialVector3{Copy,Flip,Abs,Sign};
	[Serializable]
	public class AttributeVector3 : Attribute<Vector3,AttributeVector3,AttributeVector3Data,OperatorVector3,SpecialVector3>{
		public AttributeVector3() : this(Vector3.zero){}
		public AttributeVector3(Vector3 value){this.Add(value);}
		public static implicit operator AttributeVector3(Vector3 current){return new AttributeVector3(current);}
		public static implicit operator Vector3(AttributeVector3 current){return current.Get();}
		public static Vector3 operator *(AttributeVector3 current,float amount){return current.Get() * amount;}
		public static Vector3 operator *(AttributeVector3 current,Vector3 amount){return Vector3.Scale(current.Get(),amount);}
		public static Vector3 operator +(AttributeVector3 current,Vector3 amount){return current.Get() + amount;}
		public static Vector3 operator -(AttributeVector3 current,Vector3 amount){return current.Get() - amount;}
		public float x{get{return this.Get().x;}set{this.SetX(value);}}
		public float y{get{return this.Get().y;}set{this.SetY(value);}}
		public float z{get{return this.Get().z;}set{this.SetZ(value);}}
		public override Vector3 HandleSpecial(SpecialVector3 special,Vector3 value){
			if(this.mode == AttributeMode.Linked){return value;}
			else if(special == SpecialVector3.Flip){return value * -1;}
			else if(special == SpecialVector3.Abs){return value.Abs();}
			else if(special == SpecialVector3.Sign){return value.Sign();}
			return value;
		}
		public override Vector3 HandleOperator(OperatorVector3 sign){
			Vector3 value = Vector3.zero;
			for(int index=0;index<this.data.Length;++index){
				var data = this.data[index];
				Vector3 current = this.GetValue(data);
				if(index == 0){value = current;}
				else if(data.sign == OperatorVector3.Addition){value += current;}
				else if(data.sign == OperatorVector3.Subtraction){value -= current;}
				else if(data.sign == OperatorVector3.Multiplication){value = Vector3.Scale(value,current);}
				else if(data.sign == OperatorVector3.Average){value = (value + current) / 2;}
				else if(data.sign == OperatorVector3.Max){value = Vector3.Max(value,current);}
				else if(data.sign == OperatorVector3.Min){value = Vector3.Min(value,current);}
			}
			return value;
		}
		public void SetX(float value){
			Vector3 current = this.Get();
			current.x = value;
			this.Set(current);
		}
		public void SetY(float value){
			Vector3 current = this.Get();
			current.y = value;
			this.Set(current);
		}
		public void SetZ(float value){
			Vector3 current = this.Get();
			current.z = value;
			this.Set(current);
		}
	}
	[Serializable]
	public class AttributeVector3Data : AttributeData<Vector3,AttributeVector3,OperatorVector3,SpecialVector3>{
		//public new bool[] clamp = new bool[3]{false,false,false};
	}
}
