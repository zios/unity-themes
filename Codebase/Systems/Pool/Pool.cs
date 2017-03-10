using System;
using System.Collections.Generic;
using UnityEngine;
namespace Zios{
	using Event;
	[Serializable]
	public class PoolPrefab{
		[Internal] public string name;
		public GameObject prefab;
		public Vector3 offset;
		public Vector3 scale = Vector3.one;
		public int maximum = 8;
		public bool uniqueMaterial;
	}
	[InitializeOnLoad]
	public static class Pool{
		public static Instance empty;
		public static List<PoolPrefab> prefabs = new List<PoolPrefab>();
		public static Dictionary<string,Instance[]> instances = new Dictionary<string,Instance[]>();
		static Pool(){
			if(Application.isPlaying){
				GameObject empty = new GameObject("@Null");
				empty.transform.parent = Locate.GetScenePath("@Instances").transform;
				Pool.empty = empty.AddComponent<Instance>();
				Pool.empty.prefab = new PoolPrefab();
			}
		}
		public static Instance FindAvailable(string name){
			if(Pool.instances.ContainsKey(name)){
				foreach(Instance instance in Pool.instances[name]){
					if(instance.free){
						return instance;
					}
				}
			}
			Debug.LogWarning("[Pool] No instances were available for " + name + "!");
			return Pool.empty;
		}
		public static void Build(PoolPrefab blueprint){
			if(blueprint == null || blueprint.prefab == null){return;}
			Transform instanceGroup = Locate.GetScenePath("@Instances").transform;
			Instance[] slots = Pool.instances[blueprint.name] = new Instance[blueprint.maximum];
			for(int current=0;current<blueprint.maximum;++current){
				GameObject gameObject = (GameObject)GameObject.Instantiate(blueprint.prefab);
				Instance instance = slots[current] = gameObject.AddComponent<Instance>();
				instance.prefab = blueprint;
				instance.gameObject.SetActive(false);
				instance.gameObject.transform.parent = instanceGroup;
				instance.gameObject.transform.localScale = blueprint.scale;
				if(blueprint.uniqueMaterial){
					Material material = instance.gameObject.GetComponent<Renderer>().sharedMaterial;
					instance.gameObject.GetComponent<Renderer>().sharedMaterial = (Material)Material.Instantiate(material);
				}
			}
		}
		public static GameObject AddInstance(string name,Vector3 position,float scale=1.0f,bool mirrorX=false,bool mirrorY=false){
			Instance instance = Pool.FindAvailable(name);
			if(instance != null){
				Vector3 localScale = instance.transform.localScale;
				if(mirrorX){localScale.x *= -1;}
				if(mirrorY){localScale.y *= -1;}
				instance.transform.localScale = localScale * scale;
				instance.transform.position = position + instance.prefab.offset;
				instance.gameObject.SetActive(true);
				instance.free = false;
			}
			instance.gameObject.CallEvent("Spawn");
			return instance.gameObject;
		}
	}
}