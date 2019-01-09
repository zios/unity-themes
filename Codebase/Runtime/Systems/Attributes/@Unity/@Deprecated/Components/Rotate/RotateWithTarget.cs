using UnityEngine;
namespace Zios.Attributes.Deprecated.Rotate{
	using Zios.Attributes.Deprecated;
	using Zios.Attributes.Supports;
	using Zios.State;
	//asm Zios.Unity.Components.DataBehaviour;
	//asm Zios.Unity.Components.ManagedBehaviour;
	[AddComponentMenu("Zios/Component/Action/Rotate/Rotate With Target")]
	public class RotateWithTarget : StateBehaviour{
		public AttributeGameObject source = new AttributeGameObject();
		public AttributeGameObject goal = new AttributeGameObject();
		public LerpVector3 rotation = new LerpVector3();
		public override void Awake(){
			base.Awake();
			this.DefaultRate("LateUpdate");
			this.source.Setup("Source",this);
			this.goal.Setup("Goal Target",this);
			this.rotation.Setup("Rotation Target",this);
			this.rotation.isAngle.Set(true);
		}
		public override void Use(){
			Vector3 end = this.goal.Get().transform.localEulerAngles;
			foreach(GameObject source in this.source){
				Vector3 start = source.transform.localEulerAngles;
				source.transform.localEulerAngles = this.rotation.Step(start,end);
			}
			base.Use();
		}
	}
}