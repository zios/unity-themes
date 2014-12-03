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
		public AttributeVector3(Vector3 value){this.delayedValue = value;}
		public static implicit operator AttributeVector3(Vector3 current){return new AttributeVector3(current);}
		public static implicit operator Vector3(AttributeVector3 current){return current.Get();}
		public static Vector3 operator *(AttributeVector3 current,float amount){return current.Get() * amount;}
		public static Vector3 operator *(AttributeVector3 current,Vector3 amount){return Vector3.Scale(current.Get(),amount);}
		public static Vector3 operator +(AttributeVector3 current,Vector3 amount){return current.Get() + amount;}
		public static Vector3 operator -(AttributeVector3 current,Vector3 amount){return current.Get() - amount;}
		public AttributeFloat x = 0;
		public AttributeFloat y = 0;
		public AttributeFloat z = 0;
		public override void Setup(string path,Component component){
			base.Setup(path,component);
			this.x.Setup(path+"/X",component);
			this.y.Setup(path+"/Y",component);
			this.z.Setup(path+"/Z",component);
			this.x.getMethod = ()=>this.Get().x;
			this.y.getMethod = ()=>this.Get().y;
			this.z.getMethod = ()=>this.Get().z;
			this.x.setMethod = this.SetX;
			this.y.setMethod = this.SetY;
			this.z.setMethod = this.SetZ;
		}
		public override Vector3 GetFormulaValue(){
			Vector3 value = Vector3.zero;
			for(int index=0;index<this.data.Length;++index){
				AttributeData raw = this.data[index];
				if(raw is AttributeVector3Data){
					var data = (AttributeVector3Data)raw;
					var sign = data.sign;
					Vector3 current = data.Get();
					if(index == 0){value = current;}
					else if(sign == OperatorVector3.Addition){value += current;}
					else if(sign == OperatorVector3.Subtraction){value -= current;}
					else if(sign == OperatorVector3.Multiplication){value = Vector3.Scale(value,current);}
					else if(sign == OperatorVector3.Average){value = (value + current) / 2;}
					else if(sign == OperatorVector3.Max){value = Vector3.Max(value,current);}
					else if(sign == OperatorVector3.Min){value = Vector3.Min(value,current);}
				}
				else if(raw is AttributeIntData || raw is AttributeFloatData){
					var sign = raw is AttributeIntData ? ((AttributeIntData)raw).sign : ((AttributeFloatData)raw).sign;
					float current = raw is AttributeIntData ? ((AttributeIntData)raw).Get() : ((AttributeFloatData)raw).Get();
					if(index == 0){value = new Vector3(current,current,current);}
					else if(sign == OperatorNumeral.Addition){value += new Vector3(current,current,current);}
					else if(sign == OperatorNumeral.Subtraction){value -= new Vector3(current,current,current);}
					else if(sign == OperatorNumeral.Multiplication){value = value * current;}
					else if(sign == OperatorNumeral.Average){value = (value + new Vector3(current,current,current)) / 2;}
					else if(sign == OperatorNumeral.Max){value = Vector3.Max(value,new Vector3(current,current,current));}
					else if(sign == OperatorNumeral.Min){value = Vector3.Min(value,new Vector3(current,current,current));}
				}
			}
			return value;
		}
		public override Type[] GetFormulaTypes(){
			return new Type[]{typeof(AttributeVector3Data),typeof(AttributeIntData),typeof(AttributeFloatData)};
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
}
