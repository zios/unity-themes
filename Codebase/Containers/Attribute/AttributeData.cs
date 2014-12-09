using Zios;
using System;
using UnityEngine;
namespace Zios{	
	[Serializable][AddComponentMenu("")]
	public class AttributeData : DataMonoBehaviour{
		public Target target = new Target();
		public AttributeUsage usage;
		public string referenceID;
		public string referencePath;
		public AttributeInfo attribute;
		public int sign;
		[NonSerialized] public Attribute reference;
		public void Validate(){
			bool noAttribute = this.attribute.IsNull();
			bool wrongParent = noAttribute || !this.attribute.data.Contains(this);
			bool emptyParent = noAttribute || this.attribute.parent.IsNull();
			bool emptyRoot = emptyParent || this.attribute.parent.gameObject.IsNull();
			bool wrongPlace = emptyParent || (this.attribute.parent.gameObject != this.gameObject);
			//bool notActive = noAttribute || !this.attribute.setup;
			//bool notActive = Attribute.ready && !Attribute.all.Contains(this.attribute);
			if(noAttribute || wrongParent || emptyParent || emptyRoot || wrongPlace){
				Debug.Log("AttributeData : Clearing defunct data.");
				Utility.Destroy(this);
				return;
			}
		}
		public virtual AttributeData Copy(GameObject target){return default(AttributeData);}
	}
	[Serializable]
	public class AttributeData<BaseType,AttributeType,DataType,Special> : AttributeData
		where DataType : AttributeData<BaseType,AttributeType,DataType,Special>
		where AttributeType : Attribute<BaseType,AttributeType,DataType,Special>
		where Special : struct{
		public BaseType value;
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
			AttributeInfo attribute = this.attribute;
			if(this.usage == AttributeUsage.Direct){
				if(attribute.mode == AttributeMode.Formula){
					return this.HandleSpecial();
				}
				return this.value;
			}
			else if(attribute.mode == AttributeMode.Linked || this.usage == AttributeUsage.Shaped){	
				if(!Attribute.ready && Application.isPlaying){
					Debug.LogWarning("[Attribute] Get attempt before attribute data built : " + attribute.path,attribute.parent);
					return default(BaseType);
				}
				else if(this.reference == null){
					if(!Attribute.getWarning.ContainsKey(this)){
						string source = "("+attribute.path+")";
						string goal = (this.target.Get().GetPath() + this.referencePath).Trim("/");
						Debug.LogWarning("[Attribute] Get : No reference found for " + source + " to " + goal,attribute.parent);
						Attribute.getWarning[this] = true;
					}
					return default(BaseType);
				}
				else if(this.reference.info == attribute){
					if(!Attribute.getWarning.ContainsKey(this)){
						Debug.LogWarning("[Attribute] Get : References self. (" + attribute.path + ")",attribute.parent);
						Attribute.getWarning[this] = true;
					}
					return default(BaseType);
				}
				this.value = ((AttributeType)this.reference).Get();
				if(attribute.mode == AttributeMode.Linked){return this.value;}
				return this.HandleSpecial();
			}
			if(!Attribute.getWarning.ContainsKey(this)){
				Debug.LogWarning("[Attribute] Get : No value found. (" + attribute.path + ") to " + this.referencePath,attribute.parent);
				Attribute.getWarning[this] = true;
			}
			return default(BaseType);
		}
	}
}