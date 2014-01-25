using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
namespace Zios{
	[AddComponentMenu("Zios/Component/General/Pool Controller")]
	public class PoolController : MonoBehaviour{
		public PoolPrefab[] prefabs = new PoolPrefab[4];
		public void Start(){
			this.OnValidate();
			foreach(PoolPrefab prefab in this.prefabs){
				Zios.Pool.Build(prefab);
			}
		}
		public void OnValidate(){
			foreach(PoolPrefab prefab in this.prefabs){
				prefab.name = prefab.prefab.name;
			}
		}
	}
}
