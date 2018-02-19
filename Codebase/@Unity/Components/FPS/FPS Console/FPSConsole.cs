using UnityEngine;
namespace Zios.Unity.Components.FPSConsole{
	using Zios.Console;
	using Zios.Unity.Time;
	//asm Zios.Unity.Supports.Singleton;
	[AddComponentMenu("Zios/Component/Debug/FPS (Console)")]
	public class FPSConsole : MonoBehaviour{
		public int frames;
		public float nextCheck;
		public void Start(){this.nextCheck = Time.Get() + 1;}
		public void Update(){
			this.frames += 1;
			if(Time.Get() >= this.nextCheck){
				Console.AddLog(this.frames + " fps");
				this.frames = 0;
				this.Start();
			}
		}
	}
}