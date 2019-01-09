using UnityEngine;
namespace Zios.Unity.Components.Persistent{
	using Zios.Unity.Proxy;
	[AddComponentMenu("Zios/Component/General/Persistent")]
	public class Persistent : MonoBehaviour{
		public bool activateOnLoad = false;
		public void Awake(){
			DontDestroyOnLoad(this.gameObject);
		}
		public void OnEnable(){
			if(!Proxy.IsPlaying() && this.activateOnLoad){
				this.gameObject.SetActive(false);
			}
		}
	}
}