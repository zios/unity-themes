using UnityEngine;
public static class GameObjectExtension{
	public static void SetVisible(this GameObject current,bool state){
		Renderer[] renderers = current.GetComponentsInChildren<Renderer>();
		foreach(Renderer renderer in renderers){
			renderer.enabled = state;
		}
	}
	public static void SetCollisions(this GameObject current,bool state){
		Collider[] colliders = current.GetComponentsInChildren<Collider>();
		foreach(Collider collider in colliders){
			collider.enabled = state;
		}
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