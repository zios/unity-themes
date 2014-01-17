using UnityEngine;
[RequireComponent(typeof(Animation))]
public class AnimationController : MonoBehaviour{
	public string defaultAnimation = "Idle";
	public void Update(){
		if(!this.animation.isPlaying){
			this.animation.Play(this.defaultAnimation);
		}
	}
}
