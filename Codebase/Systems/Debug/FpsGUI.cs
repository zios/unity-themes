using UnityEngine;
using System;
[AddComponentMenu("Zios/Singleton/ShowFPS")]
[RequireComponent(typeof(OverlayText))]
public class FpsGUI : MonoBehaviour {
	private int frames = 0;
	private float nextUpdate;
	private string lastFPS;
	public void Update(){
		this.frames += 1;
		if(Time.time >= this.nextUpdate){
			this.nextUpdate = Time.time + 1;
			string fps = "<color=yellow>FPS</color>"; 
			OverlayText.Get("FPS").UpdateText(fps + " : " + this.frames);
			this.frames = 0;
		}
	}
}
