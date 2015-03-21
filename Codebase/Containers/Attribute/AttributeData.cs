using Zios;
using System;
using UnityEngine;
namespace Zios{	
	[AddComponentMenu("")]
	public class AttributeData : DataMonoBehaviour{
		public Target target = new Target();
		public AttributeUsage usage;
		public string path;
		public string referenceID;
		public string referencePath;
		public AttributeInfo attribute;
		public Attribute attributeRaw;
		public int operation;
		public int special;
		[NonSerialized] public Attribute reference;
		public void Setup(){
			if(!this.attribute.IsNull() && !this.attribute.parent.IsNull()){
				Method destroyMethod = ()=>{
					bool exists = !this.IsNull() && !this.gameObject.IsNull();
					if(exists){Utility.EditorDelayCall(()=>Utility.Destroy(this));}
				};
				this.attribute.parent.AddEvent("On Destroy",destroyMethod);
				//Events.Add("On Destroy",destroyMethod,this.attribute.parent);
			}
		}
		public override void OnDestroy(){
			base.OnDestroy();
			if(this.attribute != null){
				this.attribute.data = this.attribute.data.Remove(this);
				this.attribute.dataB = this.attribute.dataB.Remove(this);
				this.attribute.dataC = this.attribute.dataC.Remove(this);
			}
		}
		public virtual void Validate(){
			if(this.attribute.IsNull()){this.Purge("Null Attribute");}
			else if(this.attribute.parent.IsNull()){this.Purge("Null Parent");}
			else if(this.attribute.parent.gameObject.IsNull()){this.Purge("Null GameObject");}
			else if(this.attribute.parent.gameObject != this.gameObject){this.Purge("Wrong Scope");}
			else if(!this.hideFlags.Contains(HideFlags.HideInInspector) && PlayerPrefs.GetInt("ShowAttributeData") == 0){this.Purge("Visible");}
			else if(!this.attribute.data.Contains(this) && !this.attribute.dataB.Contains(this) && !this.attribute.dataC.Contains(this)){this.Purge("Not In Attribute");}
		}
		public void Purge(string reason){
			if(AttributeManager.debug){Debug.Log("[AttributeData] Clearing defunct data -- " + reason + ".");}
			Utility.Destroy(this);
		}
		public virtual bool CanCache(){return true;}
		public virtual AttributeData Copy(GameObject target){return default(AttributeData);}
	}
	public class AttributeData<BaseType,AttributeType,DataType> : AttributeData
		where DataType : AttributeData<BaseType,AttributeType,DataType>
		where AttributeType : Attribute<BaseType,AttributeType,DataType>{
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
					if(AttributeManager.debug){Debug.LogWarning("[AttributeData] Get attempt before attribute data built : " + attribute.path,attribute.parent);}
					return default(BaseType);
				}
				else if(this.reference == null){
					if(!Attribute.getWarning.ContainsKey(this)){
						string source = "("+attribute.path+")";
						string goal = (this.target.Get().GetPath() + this.referencePath).Trim("/");
						if(AttributeManager.debug){Debug.LogWarning("[AttributeData] Get : No reference found for " + source + " to " + goal,attribute.parent);}
						Attribute.getWarning[this] = true;
					}
					return default(BaseType);
				}
				else if(this.reference.info == attribute){
					if(!Attribute.getWarning.ContainsKey(this)){
						if(AttributeManager.debug){Debug.LogWarning("[AttributeData] Get : References self. (" + attribute.path + ")",attribute.parent);}
						Attribute.getWarning[this] = true;
					}
					return default(BaseType);
				}
				this.value = ((AttributeType)this.reference).Get();
				if(attribute.mode == AttributeMode.Linked){return this.value;}
				return this.HandleSpecial();
			}
			if(!Attribute.getWarning.ContainsKey(this)){
				if(AttributeManager.debug){Debug.LogWarning("[AttributeData] Get : No value found. (" + attribute.path + ") to " + this.referencePath,attribute.parent);}
				Attribute.getWarning[this] = true;
			}
			return default(BaseType);
		}
	}
}