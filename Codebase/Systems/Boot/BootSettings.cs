using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Zios{
	[AddComponentMenu("Zios/Component/General/BootSettings")]
	public class BootSettings : MonoBehaviour{
		public List<GameObject> persistant = new List<GameObject>();
		public void Awake(){
			Boot.settings = this;
			Boot.Update();
		}
		public void OnLevelWasLoaded(int level){
			Boot.Update();
		}
	}
	public static class Boot{
		public static BootSettings settings;
		public static Dictionary<string,GameObject> shared = new Dictionary<string,GameObject>();
		static Boot(){
			Define(null,(UnityEngine.Object)settings,"BootSettings",true);
		}
		public static GameObject Define(GameObject existing,UnityEngine.Object prefab,string name,bool newOnly=false){
			GameObject target = GameObject.Find(name);
			if(target == null){
				target = (GameObject)GameObject.Instantiate(prefab);
				target.name = name;
				if(newOnly){
					existing = target;
				}
			}
			if(existing != null){
				GameObject[] all = Locate.GetByName(name);
				foreach(GameObject current in all){
					if(current != existing){
						GameObject.Destroy(current);
					}
				}
			}
			return target;
		}
		public static void Update(){
			foreach(GameObject item in Boot.settings.persistant){
				string name = item.name;
				if(!Boot.shared.ContainsKey(name)){
					Boot.shared[name] = null;
				}
				Boot.shared[name] = Boot.Define(Boot.shared[name],item,name);
			}
		}
	}
}

