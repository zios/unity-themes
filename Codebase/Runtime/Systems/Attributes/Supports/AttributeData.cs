using System;
using UnityEngine;
namespace Zios.Attributes.Supports{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Unity.Extensions;
	using AttributeTarget = Zios.Attributes.Supports.Target;
	[Serializable][AddComponentMenu("")]
	public class AttributeData{
		public AttributeTarget target = new AttributeTarget();
		public AttributeUsage usage;
		public string path;
		public string referenceID;
		public string referencePath;
		public int operation;
		public int special;
		public string rawValue = "";
		public string rawType;
		[NonSerialized] public AttributeInfo attribute;
		[NonSerialized] public Attribute reference;
		public virtual bool CanCache(){return true;}
		public virtual void Serialize(){}
	}
	public class AttributeData<BaseType,AttributeType,DataType> : AttributeData
		where DataType : AttributeData<BaseType,AttributeType,DataType>,new()
		where AttributeType : Attribute<BaseType,AttributeType,DataType>,new(){
		public BaseType value;
		public virtual BaseType HandleSpecial(){return default(BaseType);}
		public override bool CanCache(){
			bool shaped = this.usage == AttributeUsage.Shaped && !this.reference.IsNull();
			if(shaped){
				AttributeType attribute = (AttributeType)this.reference;
				return attribute.getMethod == null && attribute.setMethod == null;
			}
			return false;
		}
		public virtual void Set(BaseType value){
			this.value = value;
			if(!Application.isPlaying){this.Serialize();}
		}
		public virtual BaseType Get(){
			AttributeInfo attribute = this.attribute;
			if(this.usage == AttributeUsage.Direct){
				if(attribute.mode == AttributeMode.Formula){
					return this.HandleSpecial();
				}
				return this.value;
			}
			else if(attribute.mode == AttributeMode.Linked || this.usage == AttributeUsage.Shaped){
				if(Application.isPlaying && !Attribute.ready){
					if(Attribute.debug.Has("Issue")){Debug.LogWarning("[AttributeData] Get attempt before attribute data built : " + attribute.fullPath,attribute.parent);}
					return default(BaseType);
				}
				else if(this.reference.IsNull()){
					if(Application.isPlaying && !this.target.Get().IsNull() && !Attribute.getWarning.ContainsKey(this)){
						string source = "("+attribute.fullPath+")";
						string goal = (this.target.Get().GetPath() + this.referencePath).Trim("/");
						if(Attribute.debug.Has("Issue")){Debug.LogWarning("[AttributeData] Get : No reference found for " + source + " to " + goal,attribute.parent);}
						Attribute.getWarning[this] = true;
					}
					return default(BaseType);
				}
				else if(this.reference.info == attribute){
					if(!Attribute.getWarning.ContainsKey(this)){
						if(Attribute.debug.Has("Issue")){Debug.LogWarning("[AttributeData] Get : References self. (" + attribute.fullPath + ")",attribute.parent);}
						Attribute.getWarning[this] = true;
					}
					return default(BaseType);
				}
				this.value = ((AttributeType)this.reference).Get();
				if(attribute.mode == AttributeMode.Linked){return this.value;}
				return this.HandleSpecial();
			}
			if(!Attribute.getWarning.ContainsKey(this)){
				if(Attribute.debug.Has("Issue")){Debug.LogWarning("[AttributeData] Get : No value found. (" + attribute.fullPath + ") to " + this.referencePath,attribute.parent);}
				Attribute.getWarning[this] = true;
			}
			return default(BaseType);
		}
		public override void Serialize(){
			if(!this.value.IsEmpty()){
				this.rawValue = this.value.ToString();
			}
		}
	}
}