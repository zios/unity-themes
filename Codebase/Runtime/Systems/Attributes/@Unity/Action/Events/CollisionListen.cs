using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.Attributes.Supports;
	using Zios.Events;
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.State;
	using Zios.SystemAttributes;
	using Zios.Unity.Components.ColliderController;
	using Zios.Unity.Extensions;
	using Zios.Unity.SystemAttributes;
	using Zios.Shortcuts;
	//asm Zios.Unity.Components.DataBehaviour;
	//asm Zios.Unity.Components.ManagedBehaviour;
	//asm Zios.Unity.Shortcuts;
	[AddComponentMenu("Zios/Component/Action/Event/Collision Listen")]
	public class CollisionListen : StateBehaviour{
		public CollisionEvent collisionEvent;
		[EnumMask] public CollisionSource sourceCause = (CollisionSource)(-1);
		//[EnumMask] public CollisionDirection direction = (CollisionDirection)(-1);
		public LayerMask layer = -1;
		public AttributeGameObject target = new AttributeGameObject();
		//public AttributeBool forceRequired = true;
		[Internal] public AttributeGameObject lastCollision = new AttributeGameObject();
		public override void Awake(){
			base.Awake();
			this.DefaultRate("LateUpdate");
			this.lastCollision.Setup("Last Collision",this);
			this.target.Setup("Target",this);
		}
		public override void Start(){
			base.Start();
			if(Application.isPlaying){
				string triggerName = this.collisionEvent.ToString().ToTitleCase();
				foreach(GameObject target in this.target){
					Events.Add(triggerName,(MethodObject)this.Collision,target);
					if(triggerName == "On Collision"){
						Events.Add("On Collision End",this.EndCollision,target);
					}
				}
			}
		}
		public override void Use(){}
		public void EndCollision(){base.End();}
		public void Collision(object data){
			CollisionData collision = (CollisionData)data;
			CollisionSource sourceCause = collision.isSource ? CollisionSource.Self : CollisionSource.Target;
			this.lastCollision.Set(collision.hitObject);
			bool layerMatch = this.layer.Contains(collision.hitObject.layer);
			bool sourceMatch = this.sourceCause.Contains(sourceCause);
			if(sourceMatch && layerMatch /*&& directionMatch && this.forceRequired.Get()*/){
				base.Use();
			}
		}
	}
	public enum CollisionEvent{OnCollision,OnCollisionStart,OnCollisionEnd}
	public enum CollisionDirection : int{
		Above     = 0x001,
		Below     = 0x002,
		Front     = 0x004,
		Behind    = 0x008,
		Left      = 0x010,
		Right     = 0x020,
	}
	public enum CollisionSource : int{
		Self      = 0x001,
		Target    = 0x002,
	}
}