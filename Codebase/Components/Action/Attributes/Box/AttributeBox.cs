using UnityEngine;
namespace Zios{
	[AddComponentMenu("")]
	public class AttributeBox<AttributeType> : AttributeBox
	where AttributeType : Attribute,new(){
		public AttributeType value = new AttributeType();
		public bool remember = false;
		public void OnApplicationQuit(){
			if(this.remember){this.Store();}
		}
		public virtual void Load(){}
		public virtual void Store(){}
		public override void Awake(){
			this.alias = this.alias.SetDefault("Attribute");
			base.Awake();
			this.value.Setup("",this);
			if(this.remember){this.Load();}
		}
	}
	[AddComponentMenu("")]
	public class AttributeBox : DataMonoBehaviour{}
}