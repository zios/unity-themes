#pragma warning disable 0618
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Zios{
	public static class MonoBehaviourExtension{
		public static string GetGUID(this MonoBehaviour current){
			#if UNITY_EDITOR
			if(Application.isEditor){
				MonoScript scriptFile = MonoScript.FromMonoBehaviour(current);
				string path = FileManager.GetPath(scriptFile);
				return AssetDatabase.AssetPathToGUID(path);
			}
			#endif
			return "";
		}
		public static bool CanValidate(this MonoBehaviour current){
			bool enabled = !current.IsNull() && current.gameObject.activeInHierarchy && current.enabled;
			return !Application.isPlaying && !Utility.IsBusy() && enabled;
		}
		public static bool IsEnabled(this MonoBehaviour current){
			return !current.IsNull() && current.enabled && current.gameObject.activeInHierarchy;
		}
		public static Type Get<Type>(this MonoBehaviour current){
			return current.gameObject.GetComponent<Type>();
		}
	}
}