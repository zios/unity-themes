using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
[AddComponentMenu("Zios/Singleton/ShowFPS")]
public class FpsGUI : MonoBehaviour {
	private int frames = 0;
	private Text element;
	private float nextUpdate;
	public void Start(){
		this.element = this.GetComponent<Text>();
	}
	public void Update(){
		this.frames += 1;
		if(Time.time >= this.nextUpdate){
			this.nextUpdate = Time.time + 1;
			this.element.text = this.frames.ToString();
			this.frames = 0;
		}
	}
}