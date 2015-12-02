using UnityEngine;
namespace Zios{
	[AddComponentMenu("Zios/Component/General/Persistent")]
	public class Persistent : MonoBehaviour{
		public bool activateOnLoad = false;
		public void Awake(){
			DontDestroyOnLoad(this.gameObject);
		}
		public void OnEnable(){
			if(!Application.isPlaying && this.activateOnLoad){
				this.gameObject.SetActive(false);
			}
		}
	}
}