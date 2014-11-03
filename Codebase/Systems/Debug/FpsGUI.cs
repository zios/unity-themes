using UnityEngine;
#if UNITY_4_6 || UNITY_5_0
using UnityEngine.UI;
#endif
using System;
using System.Text;
[AddComponentMenu("Zios/Singleton/ShowFPS")]
#if !UNITY_4_6 && !UNITY_5_0
	[RequireComponent(typeof(OverlayText))]
#endif
public class FpsGUI : MonoBehaviour {
	private int frames = 0;
	private float nextUpdate;
	#if UNITY_4_6 || UNITY_5_0
		private Text element;
		public void Start(){
			this.element = this.GetComponent<Text>();
		}
	#else
		private string lastFPS;
		private StringBuilder data = new StringBuilder();
	#endif
	public void Update(){
		this.frames += 1;
		if(Time.time >= this.nextUpdate){
			this.nextUpdate = Time.time + 1;
			#if UNITY_4_6 || UNITY_5_0
				this.element.text = this.frames.ToString();
			#else
				this.data.Clear();
				this.data.Append("FPS : ");
				this.data.Append(this.frames);
				OverlayText.Get("FPS").UpdateText(this.data.ToString());
			#endif
			this.frames = 0;
		}
	}
}