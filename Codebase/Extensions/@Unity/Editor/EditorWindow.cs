using UnityEditor;
using UnityEngine;
namespace Zios{
	public static class EditorWindowExtensions{
		public static void SetTitle(this EditorWindow current,string title,Texture2D icon=null){
			#if (UNITY_5_3_OR_NEWER) || (UNITY_5 && !UNITY_5_0)
			current.titleContent = new GUIContent(title,icon);
			#else
			current.title = title;
			#endif
		}
	}
}