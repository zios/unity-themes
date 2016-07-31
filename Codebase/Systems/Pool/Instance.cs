using UnityEngine;
namespace Zios{
	using Events;
	[AddComponentMenu("")]
	public class Instance : MonoBehaviour{
		public PoolPrefab prefab;
		public bool free = true;
		public void Awake(){
			Event.Add("On Disable",this.OnDeactivate,this);
		}
		public void OnDeactivate(){
			this.gameObject.SetActive(false);
			this.free = true;
		}
	}
}