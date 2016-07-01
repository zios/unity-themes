#pragma warning disable 0162
#pragma warning disable 0618
using System;
using System.Collections;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios{
	using Events;
	#if UNITY_EDITOR
	using UnityEditor;
	#endif
	public static partial class Utility{
		//============================
		// EditorUtility
		//============================
		public static bool DisplayCancelableProgressBar(string title,string message,float percent){
			#if UNITY_EDITOR
			return EditorUtility.DisplayCancelableProgressBar(title,message,percent);
			#endif
			return true;
		}
		public static void ClearProgressBar(){
			#if UNITY_EDITOR
			EditorUtility.ClearProgressBar();
			#endif
		}
		//============================
		// AssetDatabase
		//============================
		public static void StartAssetEditing(){
			#if UNITY_EDITOR
			AssetDatabase.StartAssetEditing();
			#endif
		}
		public static void StopAssetEditing(){
			#if UNITY_EDITOR
			AssetDatabase.StopAssetEditing();
			#endif
		}
		public static void RefreshAssets(){
			#if UNITY_EDITOR
			AssetDatabase.Refresh();
			#endif
		}
		public static void SaveAssets(){
			#if UNITY_EDITOR
			AssetDatabase.SaveAssets();
			#endif
		}
		public static void ImportAsset(string path){
			#if UNITY_EDITOR
			AssetDatabase.ImportAsset(path);
			#endif
		}
		public static void DeleteAsset(string path){
			#if UNITY_EDITOR
			AssetDatabase.DeleteAsset(path);
			#endif
		}
		//============================
		// PrefabUtility
		//============================
		public static UnityObject GetPrefab(UnityObject target){
			#if UNITY_EDITOR
			return PrefabUtility.GetPrefabObject(target);
			#endif
			return null;
		}
		public static GameObject GetPrefabRoot(GameObject target){
			#if UNITY_EDITOR
			return PrefabUtility.FindPrefabRoot(target);
			#endif
			return null;
		}
		public static void ApplyPrefab(GameObject target){
			#if UNITY_EDITOR
			GameObject root = PrefabUtility.FindPrefabRoot(target);
			PrefabUtility.ReplacePrefab(root,PrefabUtility.GetPrefabParent(root),ReplacePrefabOptions.ConnectToPrefab);
			#endif
		}
		public static void UpdatePrefab(UnityObject target){
			#if UNITY_EDITOR
			PrefabUtility.RecordPrefabInstancePropertyModifications(target);
			#endif
		}
		public static bool ReconnectToLastPrefab(GameObject target){
			#if UNITY_EDITOR
			return PrefabUtility.ReconnectToLastPrefab(target);
			#endif
			return false;
		}
		public static void DisconnectPrefabInstance(UnityObject target){
			#if UNITY_EDITOR
			PrefabUtility.DisconnectPrefabInstance(target);
			#endif
		}
		//============================
		// EditorApplication
		//============================
		public static bool IsPaused(){
			#if UNITY_EDITOR
			return EditorApplication.isPaused;
			#endif
			return false;
		}
		public static bool IsBusy(){
			#if UNITY_EDITOR
			return EventDetector.loading || Application.isLoadingLevel || EditorApplication.isPlayingOrWillChangePlaymode;
			#endif
			return false;
		}
		public static bool IsPlaying(){
			#if UNITY_EDITOR
			return Application.isPlaying || Utility.IsBusy();
			#endif
			return Application.isPlaying;
		}
		//============================
		// Undo
		//============================
		public static void RecordObject(UnityObject target,string name){
			#if UNITY_EDITOR
			Undo.RecordObject(target,name);
			#endif
		}
		public static void RegisterCompleteObjectUndo(UnityObject target,string name){
			#if UNITY_EDITOR
			Undo.RegisterCompleteObjectUndo(target,name);
			#endif
		}
		//============================
		// Prefs
		//============================
		public static void SavePlayerPref(string name,object value){
			if(value is Vector3){PlayerPrefs.SetString(name,value.As<Vector3>().ToString());}
			else if(value is float){PlayerPrefs.SetFloat(name,value.As<float>());}
			else if(value is int){PlayerPrefs.SetInt(name,value.As<int>());}
			else if(value is bool){PlayerPrefs.SetInt(name,value.As<bool>().ToInt());}
			else if(value is string){PlayerPrefs.SetString(name,value.As<string>().ToString());}
			else if(value is byte){PlayerPrefs.SetString(name,value.As<byte>().ToString());}
			else if(value is short){PlayerPrefs.SetInt(name,value.As<short>().ToInt());}
			else if(value is double){PlayerPrefs.SetFloat(name,value.As<double>().ToFloat());}
			else if(value is ICollection){PlayerPrefs.SetString(name,value.As<IEnumerable>().Serialize());}
		}
		public static Type LoadPlayerPref<Type>(string name,object fallback=null){
			if(typeof(Type) == typeof(Vector3)){return (Type)PlayerPrefs.GetString(name,fallback.Real<Vector3>().Serialize()).Deserialize<Vector3>().Box();}
			else if(typeof(Type) == typeof(float)){return (Type)PlayerPrefs.GetFloat(name,fallback.Real<float>()).Box();}
			else if(typeof(Type) == typeof(int)){return (Type)PlayerPrefs.GetInt(name,fallback.Real<int>()).Box();}
			else if(typeof(Type) == typeof(bool)){return (Type)PlayerPrefs.GetInt(name,fallback.Real<int>()).Box();}
			else if(typeof(Type) == typeof(string)){return (Type)PlayerPrefs.GetString(name,fallback.Real<string>()).Box();}
			else if(typeof(Type) == typeof(byte)){return (Type)PlayerPrefs.GetString(name,fallback.Real<byte>().Serialize()).Box();}
			else if(typeof(Type) == typeof(short)){return (Type)PlayerPrefs.GetInt(name,fallback.Real<short>().ToInt()).Box();}
			else if(typeof(Type) == typeof(double)){return (Type)PlayerPrefs.GetFloat(name,fallback.Real<double>().ToFloat()).Box();}
			else if(typeof(Type).IsCollection()){return (Type)PlayerPrefs.GetString(name,fallback.Real<IEnumerable>().Serialize()).Deserialize<Type>().Box();}
			return default(Type);
		}
		//============================
		// Other
		//============================
		public static void UpdateSelection(){
			#if UNITY_EDITOR
			var targets = Selection.objects;
			var focus = GUI.GetNameOfFocusedControl();
			if(targets.Length > 0){
				Selection.activeObject = null;
				Utility.DelayCall(()=>{
					Selection.objects = targets;
					EditorGUI.FocusTextInControl(focus);
					GUI.FocusControl(focus);
				},0.05f);
			}
			#endif
		}
		public static void RebuildInspectors(){
			#if UNITY_EDITOR
			Type inspectorType = Utility.GetUnityType("InspectorWindow");
			var windows = inspectorType.CallMethod<EditorWindow[]>("GetAllInspectorWindows");
			for(int index=0;index<windows.Length;++index){
				var tracker = windows[index].CallMethod<ActiveEditorTracker>("GetTracker");
				tracker.ForceRebuild();
			}
			#endif
		}
		public static void ShowInspectors(){
			#if UNITY_EDITOR
			Type inspectorType = Utility.GetUnityType("InspectorWindow");
			var windows = inspectorType.CallMethod<EditorWindow[]>("GetAllInspectorWindows");
			for(int index=0;index<windows.Length;++index){
				var tracker = windows[index].CallMethod<ActiveEditorTracker>("GetTracker");
				for(int editorIndex=0;editorIndex<tracker.activeEditors.Length;++editorIndex){
					tracker.SetVisible(editorIndex,1);
				}
			}
			#endif
		}
		public static void RepaintInspectors(){
			#if UNITY_EDITOR
			Type inspectorType = Utility.GetUnityType("InspectorWindow");
			inspectorType.CallMethod("RepaintAllInspectors");
			#endif
		}
		public static void RepaintAll(){
			#if UNITY_EDITOR
			Utility.RepaintInspectors();
			//Utility.GetUnityType("Toolbar").CallMethod("RepaintToolbar");
			UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
			#endif
		}
		public static void RepaintGameView(){
			#if UNITY_EDITOR
			Type viewType = Utility.GetUnityType("GameView");
			EditorWindow gameview = EditorWindow.GetWindow(viewType);
			gameview.Repaint();
			#endif
		}
		public static void RepaintSceneView(){
			#if UNITY_EDITOR
			if(SceneView.lastActiveSceneView != null){
				SceneView.lastActiveSceneView.Repaint();
			}
			#endif
		}
		public static void ClearDirty(){
			#if UNITY_EDITOR
			Utility.delayedDirty.Clear();
			#endif
		}
		public static void SetDirty(UnityObject target,bool delayed=false,bool forced=false){
			#if UNITY_EDITOR
			if(Application.isPlaying){return;}
			if(target.IsNull()){return;}
			if(!forced && target.GetPrefab().IsNull()){return;}
			if(delayed){
				if(!Utility.delayedDirty.Contains(target)){
					Event.AddLimited("On Enter Play",()=>Utility.SetDirty(target),1);
					Event.AddLimited("On Enter Play",Utility.ClearDirty,1);
					Utility.delayedDirty.AddNew(target);
				}
				return;
			}
			EditorUtility.SetDirty(target);
			//Utility.UpdatePrefab(target);
			#endif
		}
		public static void SetAssetDirty(UnityObject target){
			#if UNITY_EDITOR
			string path = FileManager.GetPath(target);
			UnityObject asset = AssetDatabase.LoadMainAssetAtPath(path);
			Utility.SetDirty(asset,false,true);
			#endif
		}
		public static bool IsDirty(UnityObject target){
			#if UNITY_EDITOR
			return typeof(EditorUtility).CallMethod<bool>("IsDirty",target.GetInstanceID());
			#endif
			return false;
		}
		public static int GetLocalID(int instanceID){
			#if UNITY_EDITOR
			return Unsupported.GetLocalIdentifierInFile(instanceID);
			#endif
			return 0;
		}
		public static bool MoveComponentUp(Component component){
			#if UNITY_EDITOR
			return (bool)Utility.GetUnityType("ComponentUtility").CallMethod("MoveComponentUp",component.AsArray());
			#endif
			return false;
		}
		public static bool MoveComponentDown(Component component){
			#if UNITY_EDITOR
			return (bool)Utility.GetUnityType("ComponentUtility").CallMethod("MoveComponentDown",component.AsArray());
			#endif
			return false;
		}
	}
}