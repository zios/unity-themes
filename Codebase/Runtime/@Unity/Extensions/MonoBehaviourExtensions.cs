using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios.Unity.Extensions{
	using Zios.Extensions;
	public static class MonoBehaviourExtension{
		public static bool IsEnabled(this MonoBehaviour current){
			return !current.IsNull() && current.enabled && current.gameObject.activeInHierarchy;
		}
	}
}