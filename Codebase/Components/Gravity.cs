using UnityEngine;
using System.Collections;
[RequireComponent(typeof(ColliderController))]
public class Gravity : MonoBehaviour{
	public Vector3 intensity = new Vector3(0,-9.8f,0);
	public MFloat scale = 1.0f;
	public void Awake(){
		Events.Add("SetGravityScale",this.SetGravityScale);
	}
	public void Update(){
		Vector3 amount = (this.intensity*this.scale)* Time.deltaTime;
		this.gameObject.Call("OnForce",amount);
	}
	public void SetGravityScale(float scale){
		this.scale.Set(scale);
		if(scale == -1){this.scale.Revert();}
	}
}
