using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Zios{
    public static class MonoBehaviourExtension{
	    public static string GetGUID(this MonoBehaviour current){
		    #if UNITY_EDITOR
		    if(Application.isEditor){
			    MonoScript scriptFile = MonoScript.FromMonoBehaviour(current);
			    string path = AssetDatabase.GetAssetPath(scriptFile);
			    return AssetDatabase.AssetPathToGUID(path);
		    }
		    #endif
		    return "";

		}
		public static bool CanValidate(this MonoBehaviour current){
			return !Application.isPlaying && !Application.isLoadingLevel && current.gameObject.activeInHierarchy && current.enabled;
		}
		public static bool IsEnabled(this MonoBehaviour current){
			return current.enabled && current.gameObject.activeInHierarchy;
		}
    }
}