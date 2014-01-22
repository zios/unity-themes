using UnityEngine;
namespace Zios{
	public class Instance : MonoBehaviour{
		public PoolPrefab prefab;
		public bool free = true;
		public void Awake(){
			Events.Add("Disable",this.OnDisable);
		}
		public void OnDisable(){
			this.gameObject.SetActive(false);
			this.free = true;
		}
	}
}