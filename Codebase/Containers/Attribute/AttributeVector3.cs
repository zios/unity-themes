using Zios;
using System;
using UnityEngine;
using System.Collections.Generic;
namespace Zios{
	[Serializable]
	public class AttributeVector3 : Attribute{
		public static Dictionary<GameObject,Dictionary<string,AttributeVector3>> lookup = new Dictionary<GameObject,Dictionary<string,AttributeVector3>>();
		public AttributeVector3Data[] data = new AttributeVector3Data[1];
		public AttributeVector3() : this(default(Vector3)){}
		public AttributeVector3(Vector3 value){
			this.data[0] = new AttributeVector3Data();
			this.data[0].value = value;
		}
		public static implicit operator AttributeVector3(Vector3 current){return new AttributeVector3(current);}
		public static implicit operator Vector3(AttributeVector3 current){return current.Get();}
		public static Vector3 operator *(AttributeVector3 current,float amount){return current.Get() * amount;}
		public static Vector3 operator *(AttributeVector3 current,Vector3 amount){return Vector3.Scale(current.Get(),amount);}
		public static Vector3 operator +(AttributeVector3 current,Vector3 amount){return current.Get() + amount;}
		public static Vector3 operator -(AttributeVector3 current,Vector3 amount){return current.Get() - amount;}
		public float x{get{return this.Get().x;}set{this.SetX(value);}}
		public float y{get{return this.Get().y;}set{this.SetY(value);}}
		public float z{get{return this.Get().z;}set{this.SetZ(value);}}
		public void Setup(string name,params MonoBehaviour[] scripts){
			var lookup = AttributeVector3.lookup;
			this.script = scripts[0];
			string firstName = "";
			foreach(MonoBehaviour script in scripts){
				if(script == null){continue;}
				GameObject target = script.gameObject;
				string prefix = script is StateInterface ? ((StateInterface)script).alias : script.name;
				string current = prefix + "/" + name;
				if(firstName.IsEmpty()){firstName = current;}
				if(!lookup.ContainsKey(target)){
					lookup[target] = new Dictionary<string,AttributeVector3>();
				}
				lookup[target].RemoveValue(this);
				lookup[target][current] = this;
			}
			this.path = firstName;
			foreach(var data in this.data){
				data.target.Setup(name+"Target",scripts);
			}
		}
		public Vector3 Get(){
			Vector3 value = Vector3.zero;
			var first = this.data[0];
			if(this.mode != AttributeMode.Formula){
				return this.GetValue(first);
			}
			for(int index=0;index<this.data.Length;++index){
				var data = this.data[index];
				Vector3 current = this.GetValue(data);
				if(index == 0){value = current;}
				else if(data.sign == AttributeVector3Operator.Addition){value += current;}
				else if(data.sign == AttributeVector3Operator.Subtraction){value -= current;}
				else if(data.sign == AttributeVector3Operator.Multiplication){value = Vector3.Scale(value,current);}
				else if(data.sign == AttributeVector3Operator.Average){value = (value + current) / 2;}
				else if(data.sign == AttributeVector3Operator.Max){value = Vector3.Max(value,current);}
				else if(data.sign == AttributeVector3Operator.Min){value = Vector3.Min(value,current);}
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
		public void Set(Vector3 value){
			if(this.data == null || this.data.Length < 1){
				Debug.LogWarning("Attribute : No data found. (" + this.path + ")");
			}
			else if(this.mode == AttributeMode.Normal){
				this.data[0].value = value;
			}
			else if(this.mode == AttributeMode.Linked){
				if(data[0].reference == null){
					Debug.LogWarning("Attribute : No reference found. (" + this.path + ")");
					return;
				}
				this.data[0].reference.Set(value);
			}
			else if(this.mode == AttributeMode.Formula){
				Debug.LogWarning("Attribute : Cannot manually set values for formulas. (" + this.path + ")");
			}
		}
		private Vector3 GetValue(AttributeVector3Data data){
			if(data.usage == AttributeUsage.Direct){
				return data.value;
			}
			else if(this.mode == AttributeMode.Linked || data.usage == AttributeUsage.Shaped){
				if(data.reference == null){
					Debug.LogWarning("Attribute : No reference found. (" + this.path + ")");
					return Vector3.zero;
				}
				Vector3 value = data.reference.Get();
				return this.HandleSpecial(data.special,value);
			}		
			Debug.LogWarning("Attribute : No value found. (" + this.path + ")");
			return Vector3.zero;
		}
		private Vector3 HandleSpecial(AttributeVector3Special special,Vector3 value){
			if(this.mode == AttributeMode.Linked){return value;}
			else if(special == AttributeVector3Special.Flip){return value * -1;}
			else if(special == AttributeVector3Special.Abs){return value.Abs();}
			else if(special == AttributeVector3Special.Sign){return value.Sign();}
			return value;
		}
	}
}
