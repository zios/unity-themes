using UnityEngine;
using System;
using System.Collections;
[AddComponentMenu("Zios/Singleton/Debugger")]
public class Debugger : MonoBehaviour {
	private int frames = 0;
	private float nextUpdate;
	private string lastFPS;
	public void Awake(){
		Global.Debug = this;
		DontDestroyOnLoad(this.gameObject);
	}
	public void Update(){
		this.frames += 1;
		if(Time.time >= this.nextUpdate){
			this.nextUpdate = Time.time + 1;
			OverlayText.Get("FPS").UpdateText("FPS : " + this.frames);
			this.frames = 0;
		}
	}
}
