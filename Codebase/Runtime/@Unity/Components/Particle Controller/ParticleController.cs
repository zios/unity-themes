using UnityEngine;
namespace Zios.Unity.Components.ParticleController{
	using Zios.Shortcuts;
	[AddComponentMenu("Zios/Component/Rendering/Particle Controller")]
	public class ParticleController : MonoBehaviour{
		public Method onLast;
		public ParticleSystem instance;
		public void Update(){
			if(this.instance != null && !this.instance.IsAlive()){
				if(this.onLast != null){
					this.onLast();
				}
			}
		}
	}
}