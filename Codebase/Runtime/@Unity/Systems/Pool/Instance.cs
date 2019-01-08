using UnityEngine;
namespace Zios.Unity.Pool{
	using Zios.Events;
	using Zios.Extensions;
	//asm Zios.Shortcuts;
	//asm Zios.Unity.Shortcuts;
	[AddComponentMenu("")]
	public class Instance : MonoBehaviour{
		public PoolPrefab prefab;
		public bool free = true;
		public void Awake(){
			Events.Add("On Disable",this.OnDeactivate,this);
		}
		public void OnDeactivate(){
			if(this.gameObject.IsNull()){return;}
			this.gameObject.SetActive(false);
			this.free = true;
		}
	}
}