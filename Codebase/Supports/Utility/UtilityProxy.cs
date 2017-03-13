#if UNITY_EDITOR
#pragma warning disable 0618
using System;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;
namespace Zios{
	using Event;
	public static partial class Utility{
		//============================
		// AssetsDatabase
		//============================
		public static bool IsAsset(UnityObject target){
			return !FileManager.GetPath(target).IsEmpty();
		}
		public static void StartAssetEditing(){
			AssetDatabase.StartAssetEditing();
		}
		public static void StopAssetEditing(){
			AssetDatabase.StopAssetEditing();
		}
		public static void RefreshAssets(){
			AssetDatabase.Refresh();
		}
		public static void SaveAssets(){
			AssetDatabase.SaveAssets();
		}
		public static void ImportAsset(string path){
			AssetDatabase.ImportAsset(path);
		}
		public static void DeleteAsset(string path){
			AssetDatabase.DeleteAsset(path);
		}
		//============================
		// Assets
		//============================
		#if !UNITY_THEMES
		[MenuItem("Zios/Reload Scripts &#R")]
		public static void ReloadScripts(){
			Debug.Log("[Utility] : Forced Reload Scripts.");
			typeof(UnityEditorInternal.InternalEditorUtility).CallMethod("RequestScriptReload");
		}
		[MenuItem("Assets/Build AssetBundles")]
		public static void BuildAssetBundles(){
			BuildPipeline.BuildAssetBundles("Assets/",BuildAssetBundleOptions.None,BuildTarget.StandaloneWindows64);
		}
		[MenuItem("Zios/Create Singleton")]
		public static ScriptableObject CreateSingleton(){
			var path = EditorUtility.SaveFilePanelInProject("Create Singleton","","","");
			return Utility.CreateSingleton(path);
		}
		public static ScriptableObject CreateSingleton(string path,bool createPath=true){
			var name = path.GetPathTerm();
			var folder = Application.dataPath + "/" + path.TrimLeft("Assets/").GetDirectory();
			if(createPath){FileManager.Create(folder);}
			AssetDatabase.ImportAsset(folder.GetAssetPath());
			try{
				var instance = ScriptableObject.CreateInstance(name);
				AssetDatabase.CreateAsset(instance,path+".asset");
				AssetDatabase.Refresh();
				return instance;
			}
			catch{Debug.LogWarning("[Utility] No scriptableObject exists named -- " + name + ".asset");}
			return null;
		}
		public static Type GetSingleton<Type>(bool create=true) where Type : ScriptableObject{
			var name = typeof(Type).Name;
			return FileManager.GetAsset<Type>(name+".asset",false) ?? create ? Utility.CreateSingleton("Assets/Settings/"+name).As<Type>() : null;
		}
		#endif
		//============================
		// PrefabUtility
		//============================
		public static UnityObject GetPrefab(UnityObject target){
			return PrefabUtility.GetPrefabObject(target);
		}
		public static GameObject GetPrefabRoot(GameObject target){
			return PrefabUtility.FindPrefabRoot(target);
		}
		public static void ApplyPrefab(GameObject target){
			GameObject root = PrefabUtility.FindPrefabRoot(target);
			PrefabUtility.ReplacePrefab(root,PrefabUtility.GetPrefabParent(root),ReplacePrefabOptions.ConnectToPrefab);
		}
		public static void UpdatePrefab(UnityObject target){
			PrefabUtility.RecordPrefabInstancePropertyModifications(target);
		}
		public static bool ReconnectToLastPrefab(GameObject target){
			return PrefabUtility.ReconnectToLastPrefab(target);
		}
		public static void DisconnectPrefabInstance(UnityObject target){
			PrefabUtility.DisconnectPrefabInstance(target);
		}
		//============================
		// EditorApplication
		//============================
		public static bool IsPaused(){
			return EditorApplication.isPaused;
		}
		public static bool IsSwitching(){
			return EditorApplication.isPlayingOrWillChangePlaymode && !Application.isPlaying;
		}
		public static bool IsBusy(){
			return EventDetector.loading || Application.isLoadingLevel || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating;
		}
		public static bool IsPlaying(){
			return Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;
		}
		//============================
		// Undo
		//============================
		public static void RecordObject(UnityObject target,string name){
			Undo.RecordObject(target,name);
		}
		public static void RegisterCompleteObjectUndo(UnityObject target,string name){
			Undo.RegisterCompleteObjectUndo(target,name);
		}
		//============================
		// Other
		//============================
		public static void UpdateSelection(){
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
		}
		public static void RebuildAll(){
			var windows = Locate.GetAssets<EditorWindow>();
			foreach(var window in windows){
				if(windows.IsNull()){continue;}
				var tracker = window.GetVariable<ActiveEditorTracker>("m_Tracker");
				if(tracker == null || System.Object.Equals(tracker,null)){continue;}
				tracker.ForceRebuild();
			}
		}
		public static void RebuildInspectors(){
			//typeof(EditorUtility).CallMethod("ForceRebuildInspectors");
			Type inspectorType = Utility.GetUnityType("InspectorWindow");
			var windows = inspectorType.CallMethod<EditorWindow[]>("GetAllInspectorWindows");
			for(int index=0;index<windows.Length;++index){
				if(windows[index].IsNull()){continue;}
				var tracker = windows[index].GetVariable<ActiveEditorTracker>("m_Tracker");
				if(tracker == null || System.Object.Equals(tracker,null)){continue;}
				tracker.ForceRebuild();
			}
		}
		public static void ShowInspectors(){
			Type inspectorType = Utility.GetUnityType("InspectorWindow");
			var windows = inspectorType.CallMethod<EditorWindow[]>("GetAllInspectorWindows");
			for(int index=0;index<windows.Length;++index){
				var tracker = windows[index].GetVariable<ActiveEditorTracker>("m_Tracker");
				if(tracker == null || System.Object.Equals(tracker,null)){continue;}
				for(int editorIndex=0;editorIndex<tracker.activeEditors.Length;++editorIndex){
					tracker.SetVisible(editorIndex,1);
				}
			}
		}
		public static void RepaintInspectors(){
			Type inspectorType = Utility.GetUnityType("InspectorWindow");
			inspectorType.CallMethod("RepaintAllInspectors");
		}
		public static void RepaintToolbar(){
			Utility.GetUnityType("Toolbar").CallMethod("RepaintToolbar");
		}
		public static void RepaintAll(){
			//foreach(var window in Locate.GetAssets<EditorWindow>()){window.Repaint();}
			UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
			//typeof(EditorApplication).CallMethod("RequestRepaintAllViews");
			//Utility.GetUnityType("InspectorWindow").CallMethod("RepaintAllInspectors");
			Utility.RepaintToolbar();
		}
		public static void RepaintGameView(){
			Type viewType = Utility.GetUnityType("GameView");
			EditorWindow gameview = EditorWindow.GetWindow(viewType);
			gameview.Repaint();
		}
		public static void RepaintSceneView(){
			if(SceneView.lastActiveSceneView != null){
				SceneView.lastActiveSceneView.Repaint();
			}
		}
		public static void ClearDirty(){
			Utility.delayedDirty.Clear();
		}
		public static void SetDirty(UnityObject target,bool delayed=false,bool forced=false){
			if(Application.isPlaying){return;}
			if(target.IsNull()){return;}
			if(delayed){
				if(!Utility.delayedDirty.Contains(target)){
					Events.AddLimited("On Enter Play",()=>Utility.SetDirty(target),1);
					Events.AddLimited("On Enter Play",Utility.ClearDirty,1);
					Utility.delayedDirty.AddNew(target);
				}
				return;
			}
			EditorUtility.SetDirty(target);
			#if UNITY_5_3_OR_NEWER
			UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
			#else
			EditorApplication.MarkSceneDirty();
			#endif
			//Utility.UpdatePrefab(target);
		}
		public static void SetAssetDirty(UnityObject target){
			string path = FileManager.GetPath(target);
			UnityObject asset = AssetDatabase.LoadMainAssetAtPath(path);
			Utility.SetDirty(asset,false,true);
		}
		public static bool IsDirty(UnityObject target){
			return typeof(EditorUtility).CallMethod<bool>("IsDirty",target.GetInstanceID());
		}
		public static int GetLocalID(int instanceID){
			return Unsupported.GetLocalIdentifierInFile(instanceID);
		}
		public static bool MoveComponentUp(Component component){
			return (bool)Utility.GetUnityType("ComponentUtility").CallMethod("MoveComponentUp",component.AsArray());
		}
		public static bool MoveComponentDown(Component component){
			return (bool)Utility.GetUnityType("ComponentUtility").CallMethod("MoveComponentDown",component.AsArray());
		}
		public static void LoadScene(string name){
			#if UNITY_5_3_OR_NEWER
			UnityEngine.SceneManagement.SceneManager.LoadScene(name);
			#else
			Application.LoadLevel(name);
			#endif
		}
	}
}
#endif