using UnityEngine;
using System.Collections;
[RequireComponent(typeof(ColliderController))]
[AddComponentMenu("Zios/Component/Physics/Gravity")]
public class Gravity : MonoBehaviour{
	public bool disabled;
	public Vector3 intensity = new Vector3(0,-9.8f,0);
	public MFloat scale = 1.0f;
	public void Awake(){
		Events.Add("SetGravityScale",(MethodFloat) this.OnSetGravityScale);
		Events.Add("DisableGravity",this.OnDisableGravity);
		Events.Add("EnableGravity",this.OnEnableGravity);
	}
	public void FixedUpdate(){
		ColliderController controller = ColliderController.Get(this.gameObject);
		if(!this.disabled && !controller.blocked["down"]){
			Vector3 amount = (this.intensity*this.scale)* Time.fixedDeltaTime;
			this.gameObject.Call("AddForce",amount);
		}
	}
	public void OnDisableGravity(){
		this.gameObject.Call("ResetVelocity","y");
		this.disabled = true;
	}
	public void OnEnableGravity(){
		this.gameObject.Call("ResetVelocity","y");
		this.disabled = false;
	}
	public void OnSetGravityScale(float scale){
		this.scale.Set(scale);
		if(scale == -1){this.scale.Revert();}
	}
}
