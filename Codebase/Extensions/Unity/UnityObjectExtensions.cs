using System;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios{
	public static class UnityObjectExtension{
		#if UNITY_EDITOR
		public static UnityObject GetPrefab(this UnityObject current){
			return Utility.GetPrefab(current);
		}
		public static bool IsExpanded(this UnityObject current){
			Type editorUtility = Utility.GetInternalType("InternalEditorUtility");
			return editorUtility.CallMethod<bool>("GetIsInspectorExpanded",current);
		}
		public static void SetExpanded(this UnityObject current,bool state){
			Type editorUtility = Utility.GetInternalType("InternalEditorUtility");
			editorUtility.CallMethod("SetIsInspectorExpanded",current,state);
		}
		#endif
	}
}