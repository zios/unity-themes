#if !UNITY_EDITOR
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios{
	public static partial class Utility{
		public static bool IsAsset(UnityObject target){return false;}
		public static void StartAssetEditing(){}
		public static void StopAssetEditing(){ }
		public static void RefreshAssets(){ }
		public static void SaveAssets(){}
		public static void ImportAsset(string path){}
		public static void DeleteAsset(string path){}
		public static void ReloadScripts(){}
		public static void BuildAssetBundles(){}
		public static ScriptableObject CreateSingleton(){return null;}
		public static ScriptableObject CreateSingleton(string path,bool createPath=true){return null;}
		public static Type GetSingleton<Type>(bool create=true) where Type : ScriptableObject{
			var name = typeof(Type).Name;
			return ScriptableObject.FindObjectOfType<Type>();
		}
		//============================
		// PrefabUtility
		//============================
		public static UnityObject GetPrefab(UnityObject target){return null;}
		public static GameObject GetPrefabRoot(GameObject target){return null;}
		public static void ApplyPrefab(GameObject target){}
		public static void UpdatePrefab(UnityObject target){}
		public static bool ReconnectToLastPrefab(GameObject target){return false;}
		public static void DisconnectPrefabInstance(UnityObject target){}
		//============================
		// EditorApplication
		//============================
		public static bool IsPaused(){return false;}
		public static bool IsBusy(){return false;}
		public static bool IsPlaying(){return Application.isPlaying;}
		//============================
		// Undo
		//============================
		public static void RecordObject(UnityObject target,string name){}
		public static void RegisterCompleteObjectUndo(UnityObject target,string name){}
		//============================
		// Other
		//============================
		public static void UpdateSelection(){}
		public static void RebuildAll(){}
		public static void RebuildInspectors(){}
		public static void ShowInspectors(){}
		public static void RepaintInspectors(){}
		public static void RepaintToolbar(){}
		public static void RepaintAll(){}
		public static void RepaintGameView(){}
		public static void RepaintSceneView(){}
		public static void ClearDirty(){}
		public static void SetDirty(UnityObject target,bool delayed=false,bool forced=false){}
		public static void SetAssetDirty(UnityObject target){}
		public static bool IsDirty(UnityObject target){return false;}
		public static int GetLocalID(int instanceID){return 0;}
		public static bool MoveComponentUp(Component component){return false;}
		public static bool MoveComponentDown(Component component){return false;}
		public static void LoadScene(string name){}
	}
}
#endif