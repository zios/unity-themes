using UnityEngine;
using System;
using System.Text;
[AddComponentMenu("Zios/Singleton/ShowFPS")]
[RequireComponent(typeof(OverlayText))]
public class FpsGUI : MonoBehaviour {
	private int frames = 0;
	private float nextUpdate;
	private string lastFPS;
	private StringBuilder data = new StringBuilder();
	public void Update(){
		this.frames += 1;
		if(Time.time >= this.nextUpdate){
			this.nextUpdate = Time.time + 1;
			this.data.Clear();
			this.data.Append("<color=yellow>FPS</color> : ");
			this.data.Append(this.frames);
			OverlayText.Get("FPS").UpdateText(this.data.ToString());
			this.frames = 0;
		}
	}
}
