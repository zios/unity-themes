using Zios;
using System;
using UnityEngine;
namespace Zios{	
	[Serializable]
	public class AttributeData : DataMonoBehaviour{
		public Target target = new Target();
		public AttributeUsage usage;
		public string referenceID;
		public string referencePath;
		public Attribute attribute;
		[NonSerialized] public Attribute reference;
		public override void Awake(){
			if(!Application.isPlaying && !this.attribute.IsNull()){
				bool wrongParent = !this.attribute.data.Contains(this);
				bool emptyParent = this.attribute.parent.IsNull();
				bool emptyRoot = emptyParent || this.attribute.parent.gameObject.IsNull();
				//bool notActive = Attribute.ready && !this.attribute.setup;
				//bool notActive = Attribute.ready && !Attribute.all.Contains(this.attribute);
				if(wrongParent || emptyParent || emptyRoot/* || notActive*/){
					Debug.Log("AttributeData : Clearing defunct data -- " + this.referencePath);
					Utility.Destroy(this);
					return;
				}
			}
		}
		public virtual AttributeData Copy(GameObject target){return default(AttributeData);}
	}
	[Serializable]
	public class AttributeData<BaseType,AttributeType,DataType,Operator,Special> : AttributeData
		where DataType : AttributeData<BaseType,AttributeType,DataType,Operator,Special>
		where AttributeType : Attribute<BaseType,AttributeType,DataType,Operator,Special>
		where Operator : struct 
		where Special : struct{
		public BaseType value;
		public Operator sign;
		public Special special;
		public virtual BaseType HandleSpecial(){return default(BaseType);}
		public override AttributeData Copy(GameObject target){
			DataType data = target.AddComponent<DataType>();
			data.target = this.target.Clone();
			data.usage = this.usage;
			data.referenceID = this.referenceID;
			data.referencePath = this.referencePath;
			data.sign = this.sign;
			data.special = this.special;
			data.value = this.value;
			return data;
		}
		public BaseType Get(){
			Attribute attribute = this.attribute;
			if(this.usage == AttributeUsage.Direct){
				if(attribute.mode == AttributeMode.Formula){
					return this.HandleSpecial();
				}
				return this.value;
			}
			else if(attribute.mode == AttributeMode.Linked || this.usage == AttributeUsage.Shaped){	
				if(!Attribute.ready && Application.isPlaying){
					Debug.LogWarning("Attribute : Get attempt before attribute data built -- " + attribute.path,attribute.parent);
					return default(BaseType);
				}
				else if(this.reference == null){
					if(!Attribute.getWarning.ContainsKey(this)){
						string source = "("+attribute.path+")";
						string goal = (this.target.Get().GetPath() + this.referencePath).Trim("/");
						Debug.LogWarning("Attribute (Get): No reference found for " + source + " to " + goal,attribute.parent);
						Attribute.getWarning[this] = true;
					}
					return default(BaseType);
				}
				else if(this.reference == attribute){
					if(!Attribute.getWarning.ContainsKey(this)){
						Debug.LogWarning("Attribute (Get): References self. (" + attribute.path + ")",attribute.parent);
						Attribute.getWarning[this] = true;
					}
					return default(BaseType);
				}
				BaseType value = ((AttributeType)this.reference).Get();
				if(attribute.mode == AttributeMode.Linked){return value;}
				return this.HandleSpecial();
			}
			if(!Attribute.getWarning.ContainsKey(this)){
				Debug.LogWarning("Attribute (Get): No value found. (" + attribute.path + ") to " + this.referencePath,attribute.parent);
				Attribute.getWarning[this] = true;
			}
			return default(BaseType);
		}
	}
}