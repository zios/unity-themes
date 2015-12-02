using UnityEngine;
[AddComponentMenu("Zios/Singleton/Debug/FPS (Console)")]
public class FPSConsole : MonoBehaviour{
	public int frames;
	public float nextCheck;
	public void Start(){this.nextCheck = Time.time + 1;}
	public void Update(){
		this.frames += 1;
		if(Time.time >= this.nextCheck){
			Debug.Log(this.frames + " fps");
			this.frames = 0;
			this.Start();
		}
	}
}