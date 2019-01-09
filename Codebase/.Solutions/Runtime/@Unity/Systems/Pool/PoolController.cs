using UnityEngine;
namespace Zios.Unity.Pool{
	using Zios.Unity.Log;
	[AddComponentMenu("Zios/Component/General/Pool Controller")]
	public class PoolController : MonoBehaviour{
		public PoolPrefab[] prefabs = new PoolPrefab[0];
		public void Awake(){
			foreach(PoolPrefab prefab in this.prefabs){
				if(prefab == null || prefab.prefab == null){
					Log.Warning("[PoolController] Prefab for element is missing/corrupt.");
					continue;
				}
				prefab.name = prefab.prefab.name;
			}
			foreach(PoolPrefab prefab in this.prefabs){
				Zios.Unity.Pool.Pool.Build(prefab);
			}
		}
	}
}