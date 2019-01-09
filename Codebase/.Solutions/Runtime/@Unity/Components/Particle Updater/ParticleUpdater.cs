using UnityEngine;
namespace Zios.Unity.Components.ParticleUpdater{
	using Zios.Unity.Pref;
	using Zios.Unity.Proxy;
	using Zios.Unity.Time;
	[ExecuteInEditMode][AddComponentMenu("Zios/Component/Rendering/Update Particle")]
	public class UpdateParticle : MonoBehaviour{
		public void OnWillRenderObject(){
			if(Proxy.IsEditor() && !Proxy.IsPlaying() && Camera.current != null){
				if(PlayerPref.Get<bool>("EditorSettings-AlwaysUpdateParticles")){
					float range = PlayerPref.Get<float>("EditorSettings-ParticleUpdateRange");
					Vector3 cameraPosition = Camera.current.transform.position;
					Vector3 objectPosition = this.transform.position;
					if(Vector3.Distance(cameraPosition,objectPosition) <= range){
						this.GetComponent<ParticleSystem>().Simulate(Time.Get()%60+10);
					}
				}
			}
		}
	}
}