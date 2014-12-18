using Zios;
using System;
using UnityEngine;
using System.Collections.Generic;
namespace Zios{
	[Serializable]
	public class AttributeVector3 : Attribute<Vector3,AttributeVector3,AttributeVector3Data>{
		public static string[] specialList = new string[]{"Copy","Flip","Abs","Sign","Floor","Ceil","Normalized","Magnitude","SqrMagnitude"};
		public static Dictionary<Type,string[]> compare = new Dictionary<Type,string[]>(){
			{typeof(AttributeVector3Data),new string[]{"+","-","×","/","Average","Max","Min","Distance","Dot","Cross","Angle","Reflect","Project","ProjectOnPlane"}},
			{typeof(AttributeIntData),new string[]{"+","-","×","/","Average","Max","Min"}},
			{typeof(AttributeFloatData),new string[]{"+","-","×","/","Average","Max","Min"}},
		};
		public AttributeVector3() : this(Vector3.zero){}
		public AttributeVector3(Vector3 value){this.delayedValue = value;}
		public static implicit operator AttributeVector3(Vector3 current){return new AttributeVector3(current);}
		public static implicit operator Vector3(AttributeVector3 current){return current.Get();}
		public static Vector3 operator *(AttributeVector3 current,float amount){return current.Get() * amount;}
		public static Vector3 operator *(AttributeVector3 current,Vector3 amount){return Vector3.Scale(current.Get(),amount);}
		/*public static Vector3 operator +(AttributeVector3 current,Vector3 amount){return current.Get() + amount;}
		public static Vector3 operator -(AttributeVector3 current,Vector3 amount){return current.Get() - amount;}*/
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
				string operation = AttributeVector3.compare[raw.GetType()][raw.operation];
				if(raw is AttributeVector3Data){
					var data = (AttributeVector3Data)raw;
					Vector3 current = data.Get();
					if(index == 0){value = current;}
					else if(operation == "+"){value += current;}
					else if(operation == "-"){value -= current;}
					else if(operation == "×"){value = Vector3.Scale(value,current);}
					else if(operation == "Average"){value = (value + current) / 2;}
					else if(operation == "Max"){value = Vector3.Max(value,current);}
					else if(operation == "Min"){value = Vector3.Min(value,current);}
					else if(operation == "Distance"){
						float distance = Vector3.Distance(value,current);
						value = new Vector3(distance,distance,distance);
					}
					else if(operation == "Dot"){
						float dot = Vector3.Dot(value,current);
						value = new Vector3(dot,dot,dot);
					}
					else if(operation == "Angle"){
						float angle = Vector3.Angle(value,current);
						value = new Vector3(angle,angle,angle);
					}
					else if(operation == "Cross"){value = Vector3.Cross(value,current);}
					else if(operation == "Reflect"){value = Vector3.Reflect(value,current);}
					else if(operation == "Project"){value = Vector3.Project(value,current);}
					else if(operation == "ProjectOnPlane"){value = Vector3.ProjectOnPlane(value,current);}
				}
				else if(raw is AttributeIntData || raw is AttributeFloatData){
					float current = raw is AttributeIntData ? ((AttributeIntData)raw).Get() : ((AttributeFloatData)raw).Get();
					if(index == 0){value = new Vector3(current,current,current);}
					else if(operation == "+"){value += new Vector3(current,current,current);}
					else if(operation == "-"){value -= new Vector3(current,current,current);}
					else if(operation == "×"){value = value * current;}
					else if(operation == "Average"){value = (value + new Vector3(current,current,current)) / 2;}
					else if(operation == "Max"){value = Vector3.Max(value,new Vector3(current,current,current));}
					else if(operation == "Min"){value = Vector3.Min(value,new Vector3(current,current,current));}
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
