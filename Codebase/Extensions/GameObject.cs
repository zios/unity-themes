using UnityEngine;
using System;
public static class GameObjectExtension{
	public static void EnableComponents(this GameObject current,params Type[] types){
		current.SetComponents(true,types);
	}
	public static void DisableComponents(this GameObject current,params Type[] types){
		current.SetComponents(false,types);
	}
	public static void SetComponents(this GameObject current,bool state,params Type[] types){
		foreach(Type type in types){
			if(type == typeof(Renderer)){
				Renderer[] items = current.GetComponentsInChildren<Renderer>(true);
				foreach(var item in items){item.enabled = state;}
			}
			else if(type == typeof(Collider)){
				Collider[] items = current.GetComponentsInChildren<Collider>(true);
				foreach(var item in items){item.enabled = state;}
			}
			else if(type == typeof(MonoBehaviour)){
				MonoBehaviour[] items = current.GetComponentsInChildren<MonoBehaviour>(true);
				foreach(var item in items){item.enabled = state;}
			}
			else if(type == typeof(Animation)){
				Animation[] items = current.GetComponentsInChildren<Animation>(true);
				foreach(var item in items){item.enabled = state;}
			}
		}
	}
	public static void SetVisible(this GameObject current,bool state){
		current.SetComponents(state,typeof(Renderer));
	}
	public static void SetCollisions(this GameObject current,bool state){
		current.SetComponents(state,typeof(Collider));
	}
	public static void SetTriggers(this GameObject current,bool state){
		Collider[] colliders = current.GetComponentsInChildren<Collider>();
		foreach(Collider collider in colliders){
			collider.isTrigger = state;
		}
	}
	public static void MoveTo(this GameObject current,Vector3 location,bool useX=true,bool useY=true,bool useZ=true){
		Vector3 position = current.transform.position;
		if(useX){position.x = location.x;}
		if(useY){position.y = location.y;}
		if(useZ){position.z = location.z;}
		current.transform.position = position;
	}
}