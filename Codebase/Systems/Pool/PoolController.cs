using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
namespace Zios{
	[AddComponentMenu("Zios/Component/General/Pool Controller")]
	public class PoolController : MonoBehaviour{
		public PoolPrefab[] prefabs = new PoolPrefab[0];
		public void Awake(){
			foreach(PoolPrefab prefab in this.prefabs){
				if(prefab == null || prefab.prefab == null){
					Debug.LogWarning("[PoolController] Prefab for element is missing/corrupt.");
					continue;
				}
				prefab.name = prefab.prefab.name;
			}
			foreach(PoolPrefab prefab in this.prefabs){
				Zios.Pool.Build(prefab);
			}
		}
	}
}