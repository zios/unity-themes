using UnityEngine;
namespace Zios.Unity.Extensions{
	using Zios.Extensions;
	public static class MonoBehaviourExtension{
		public static bool IsEnabled(this MonoBehaviour current){
			return !current.IsNull() && current.enabled && current.gameObject.activeInHierarchy;
		}
		public static Type Get<Type>(this MonoBehaviour current){
			return current.gameObject.GetComponent<Type>();
		}
		public static Mesh GetMesh(this MonoBehaviour current){
			if(current.Get<MeshFilter>()){return current.Get<MeshFilter>().sharedMesh;}
			if(current.Get<SkinnedMeshRenderer>()){return current.Get<SkinnedMeshRenderer>().sharedMesh;}
			return null;
		}
	}
}