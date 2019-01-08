using UnityEditor;
namespace Zios.Unity.Editor.Pref{
	using Zios.Unity.Pref;
	public static class PlayerPrefEditor{
		public static void ClearAll(bool prompt){
			if(!prompt || EditorUtility.DisplayDialog("Clear Editor Prefs","Delete all the editor preferences?","Yes","No")){
				PlayerPref.ClearAll();
			}
		}
		#if !ZIOS_MINIMAL
		[MenuItem("Zios/Prefs/Clear Player")]
		public static void ClearAll(){PlayerPrefEditor.ClearAll(true);}
		#endif
	}
}