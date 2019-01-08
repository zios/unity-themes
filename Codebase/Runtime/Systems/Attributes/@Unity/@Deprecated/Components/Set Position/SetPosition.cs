using UnityEngine;
namespace Zios.Attributes.Deprecated.SetPosition{
	using Zios.Attributes.Supports;
	using Zios.Extensions;
	using Zios.State;
	//asm Zios.Unity.Components.DataBehaviour;
	//asm Zios.Unity.Components.ManagedBehaviour;
	[AddComponentMenu("Zios/Component/Action/Move/Set Position")]
	public class SetPosition : StateBehaviour{
		public PositionMode mode;
		public AttributeGameObject target = new AttributeGameObject();
		public AttributeVector3 position = Vector3.zero;
		public override void Awake(){
			base.Awake();
			this.target.Setup("Target",this);
			this.position.Setup("Position",this);
			this.warnings.AddNew("Deprecated. Consider using AttributeModify with ExposeTransform components.");
		}
		public override void Use(){
			Vector3 position = this.position.Get();
			foreach(GameObject target in this.target){
				if(this.mode == PositionMode.World){target.transform.position = position;}
				if(this.mode == PositionMode.Local){target.transform.localPosition = position;}
			}
			base.Use();
		}
	}
	public enum PositionMode{World,Local}
}