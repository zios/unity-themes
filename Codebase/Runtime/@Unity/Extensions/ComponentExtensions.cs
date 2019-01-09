using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Zios.Unity.Extensions{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Reflection;
	public static class ComponentExtension{
		public static Type Get<Type>(this Component current){
			return current.GetComponent<Type>();
		}
		public static Mesh GetMesh(this Component current){
			var filter = current.GetComponentInChildren<MeshFilter>();
			var skinned = current.GetComponentInChildren<SkinnedMeshRenderer>();
			if(filter){return filter.sharedMesh;}
			if(skinned){return skinned.sharedMesh;}
			return null;
		}
		public static Mesh[] GetMeshes(this Component current){
			var filters = current.GetComponentsInChildren<MeshFilter>();
			var skinned = current.GetComponentsInChildren<SkinnedMeshRenderer>();
			var meshes = new List<Mesh>();
			meshes.AddRange(filters.Select(x=>x.sharedMesh));
			meshes.AddRange(skinned.Select(x=>x.sharedMesh));
			return meshes.ToArray();
		}
		public static GameObject GetParent(this Component current){
			if(current.IsNull()){return null;}
			return current.gameObject.GetParent();
		}
		public static string GetPath(this Component current,bool includeSelf=true){
			if(current.IsNull() || current.gameObject.IsNull()){return "Null";}
			string path = current.gameObject.GetPath();
			if(includeSelf){path += current.GetAlias();}
			return path;
		}
		public static bool IsEnabled(this Component current){
			bool enabled = !current.IsNull() && current.gameObject.activeInHierarchy;
			if(current is MonoBehaviour){enabled = enabled && current.As<MonoBehaviour>().enabled;}
			return enabled;
		}
		//====================
		// Interface
		//====================
		public static GameObject Remove(this Component current){return Destroy(current);}
		public static GameObject Destroy(this Component current){
			var source = current.gameObject;
			if(Application.isPlaying){Component.Destroy(current);}
			else{Component.DestroyImmediate(current);}
			return source;
		}
		public static Component[] GetComponentsByInterface<T>(this Component current) where T : Component{
			if(current.IsNull()){return new Component[0];}
			return current.gameObject.GetComponentsByInterface<T>();
		}
		public static T GetComponent<T>(this Component current,bool includeInactive) where T : Component{
			if(current.IsNull()){return null;}
			return current.gameObject.GetComponent<T>(includeInactive);
		}
		public static T[] GetComponents<T>(this Component current,bool includeInactive) where T : Component{
			if(current.IsNull()){return new T[0];}
			return current.gameObject.GetComponents<T>(includeInactive);
		}
		public static T GetComponentInParent<T>(this Component current,bool includeInactive) where T : Component{
			if(current.IsNull()){return null;}
			return current.gameObject.GetComponentInParent<T>(includeInactive);
		}
		public static T GetComponentInChildren<T>(this Component current,bool includeInactive) where T : Component{
			if(current.IsNull()){return null;}
			return current.gameObject.GetComponentInChildren<T>(includeInactive);
		}
	}
}