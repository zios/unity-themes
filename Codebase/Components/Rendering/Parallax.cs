using UnityEngine;
[AddComponentMenu("Zios/Rendering/Parallax")]
public class Parallax : MonoBehaviour{
	public float scrollSpeedX = 0.5f;
	public float scrollSpeedY = 0.0f;
	private Vector3 cameraStart;
	private Vector3 selfStart;
	private bool ready = false;
	public void LateUpdate(){
		if(!this.ready){
			this.selfStart = this.transform.position;
			this.cameraStart = Camera.main.transform.position;
			this.ready = true;
		}
		var difference = Vector3.zero;
		if(this.scrollSpeedX != 0){difference.x = (Camera.main.transform.position.x-this.cameraStart.x)*this.scrollSpeedX;}
		if(this.scrollSpeedY != 0){difference.y = (Camera.main.transform.position.y-this.cameraStart.y)*this.scrollSpeedY;}
		this.transform.position = this.selfStart + difference;
	}
}