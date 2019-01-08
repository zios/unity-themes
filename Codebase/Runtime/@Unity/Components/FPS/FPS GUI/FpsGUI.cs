using UnityEngine;
using UnityEngine.UI;
namespace Zios.Unity.Components.FPSGUI{
	using Zios.Unity.Time;
	[AddComponentMenu("Zios/Component/Debug/FPS (GUI)")]
	public class FpsGUI : MonoBehaviour{
		public Text fpsText;
		public Text frameTimeText;
		private int frames = 0;
		private float frameTime;
		private float nextUpdate;
		public void Update(){
			this.frames += 1;
			if(Time.Get() >= this.nextUpdate){
				this.nextUpdate = Time.Get() + 1;
				this.fpsText.text = this.frames.ToString();
				this.frameTimeText.text = ((Time.Get() - this.frameTime) * 1000).ToString("0.0") + " ms";
				this.frames = 0;
			}
			this.frameTime = Time.Get();
		}
	}
}