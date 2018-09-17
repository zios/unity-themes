#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using CallbackFunction = UnityEditor.EditorApplication.CallbackFunction;
using UnityObject = UnityEngine.Object;
using UnityUndo = UnityEditor.Undo;
namespace Zios.Unity.ProxyEditor{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Reflection;
	using Zios.Unity.Call;
	using Zios.Unity.Log;
	using Zios.Unity.Proxy;
	using Editor = UnityEditor.Editor;
	[InitializeOnLoad]
	public static class ProxyEditor{
		private static List<UnityObject> delayedDirty = new List<UnityObject>();
		private static EditorWindow inspector;
		private static EditorWindow[] inspectors;
		private static Dictionary<Editor,EditorWindow> editorInspectors = new Dictionary<Editor,EditorWindow>();
		private static Dictionary<UnityObject,SerializedObject> serializedObjects = new Dictionary<UnityObject,SerializedObject>();
		static ProxyEditor(){
			Proxy.busyMethods.Add(ProxyEditor.IsChanging);
			Proxy.busyMethods.Add(ProxyEditor.IsCompiling);
			Proxy.busyMethods.Add(ProxyEditor.IsUpdating);
		}
		//============================
		// AssetDatabase
		//============================
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
		public static void CreateAsset(UnityObject target,string path){
			AssetDatabase.CreateAsset(target,path);
		}
		public static void DeleteAsset(string path){
			AssetDatabase.DeleteAsset(path);
		}
		public static string GetGUID(string path){
			return AssetDatabase.AssetPathToGUID(path);
		}
		public static string GetAssetPath(UnityObject target){
			return AssetDatabase.GetAssetPath(target);
		}
		public static string GetAssetPath(string guid){
			return AssetDatabase.GUIDToAssetPath(guid);
		}
		public static UnityObject LoadAsset(string path,Type type){
			return AssetDatabase.LoadAssetAtPath(path,type);
		}
		public static UnityObject LoadMainAsset(string path){
			return AssetDatabase.LoadMainAssetAtPath(path);
		}
		public static Type LoadAsset<Type>(string path) where Type : UnityObject{
			return AssetDatabase.LoadAssetAtPath<Type>(path);
		}
		public static MonoScript GetMonoScript(MonoBehaviour behaviour){
			return MonoScript.FromMonoBehaviour(behaviour);
		}
		public static bool WriteImportSettings(string path){
			return AssetDatabase.WriteImportSettingsIfDirty(path);
		}
		public static bool CopyAsset(string from,string to){
			return AssetDatabase.CopyAsset(from,to);
		}
		//============================
		// Assets
		//============================
		#if !ZIOS_MINIMAL
		[MenuItem("Zios/Reload Scripts &#R")]
		public static void ReloadScripts(){
			Log.Show("[Utility] : Forced Reload Scripts.");
			typeof(UnityEditorInternal.InternalEditorUtility).CallMethod("RequestScriptReload");
		}
		[MenuItem("Assets/Build AssetBundles")]
		public static void BuildAssetBundles(){
			BuildPipeline.BuildAssetBundles("Assets/",BuildAssetBundleOptions.None,BuildTarget.StandaloneWindows64);
		}
		#endif
		public static void ClearDirty(){
			ProxyEditor.delayedDirty.Clear();
		}
		public static void MarkSceneDirty(){
			#if UNITY_5_3_OR_NEWER
			UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
			#else
			EditorApplication.MarkSceneDirty();
			#endif
		}
		public static void SetDirty(UnityObject target,bool delayed=false,bool forced=false){
			if(ProxyEditor.IsPlaying()){return;}
			if(target.IsNull()){return;}
			if(delayed){
				if(!ProxyEditor.delayedDirty.Contains(target)){
					//Events.AddLimited("On Enter Play",()=>ProxyEditor.SetDirty(target),1);
					//Events.AddLimited("On Enter Play",Proxy.ClearDirty,1);
					ProxyEditor.delayedDirty.AddNew(target);
				}
				return;
			}
			EditorUtility.SetDirty(target);
			ProxyEditor.MarkSceneDirty();
			//Utility.UpdatePrefab(target);
		}
		public static void SetAssetDirty(UnityObject target){
			string path = AssetDatabase.GetAssetPath(target);
			UnityObject asset = AssetDatabase.LoadMainAssetAtPath(path);
			ProxyEditor.SetDirty(asset,false,true);
		}
		public static bool IsDirty(UnityObject target){
			return typeof(EditorUtility).CallMethod<bool>("IsDirty",target.GetInstanceID());
		}
		//============================
		// PrefabUtility
		//============================
		public static UnityObject GetPrefab(UnityObject target){
			return PrefabUtility.GetPrefabObject(target);
		}
		public static GameObject GetPrefabRoot(GameObject target){
			return PrefabUtility.FindPrefabRoot(target);
		}
		public static PrefabType GetPrefabType(UnityObject target){
			return PrefabUtility.GetPrefabType(target);
		}
		public static void ApplyPrefab(GameObject target){
			GameObject root = PrefabUtility.FindPrefabRoot(target);
			#if UNITY_2018_2_OR_NEWER
			PrefabUtility.ReplacePrefab(root,PrefabUtility.GetCorrespondingObjectFromSource(root),ReplacePrefabOptions.ConnectToPrefab);
			#else
			PrefabUtility.ReplacePrefab(root,PrefabUtility.GetPrefabParent(root),ReplacePrefabOptions.ConnectToPrefab);
			#endif
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
		// ShaderUtil
		//============================
		public static string GetPropertyName(Shader shader,int index){
			return ShaderUtil.GetPropertyName(shader,index);
		}
		public static ShaderUtil.ShaderPropertyType GetPropertyType(Shader shader,int index){
			return ShaderUtil.GetPropertyType(shader,index);
		}
		public static int GetPropertyCount(Shader shader){
			return ShaderUtil.GetPropertyCount(shader);
		}
		//============================
		// EditorApplication
		//============================
		public static bool IsChanging(){
			return EditorApplication.isPlayingOrWillChangePlaymode;
		}
		public static bool IsUpdating(){
			return EditorApplication.isUpdating;
		}
		public static bool IsPaused(){
			return EditorApplication.isPaused;
		}
		public static bool IsPlaying(){
			return Application.isPlaying || ProxyEditor.IsChanging();
		}
		public static bool IsCompiling(){
			return EditorApplication.isCompiling;
		}
		public static bool IsSwitching(){
			return ProxyEditor.IsChanging() || ProxyEditor.IsCompiling();
		}
		public static void AddUpdate(CallbackFunction method){
			EditorApplication.update -= method;
			EditorApplication.update += method;
		}
		public static void RemoveUpdate(CallbackFunction method){
			EditorApplication.update -= method;
		}
		public static void AddModeChange(CallbackFunction method){
			#if UNITY_2017_2_OR_NEWER
			EditorApplication.playModeStateChanged += (x)=>method();
			#else
			EditorApplication.playmodeStateChanged += method;
			#endif
		}
		public static void RemoveModeChange(CallbackFunction method){
			#if UNITY_2017_2_OR_NEWER
			#else
			EditorApplication.playmodeStateChanged -= method;
			#endif
		}
		#if UNITY_2018_1_OR_NEWER
		public static void HierarchyChanged(Action method){
			EditorApplication.hierarchyChanged += method;
		}
		#else
		public static void HierarchyChanged(CallbackFunction method){
			EditorApplication.hierarchyWindowChanged += method;
		}
		#endif
		#if UNITY_2018_1_OR_NEWER
		public static void ProjectChanged(Action method){
			EditorApplication.projectChanged += method;
		}
		#else
		public static void ProjectChanged(CallbackFunction method){
			EditorApplication.projectWindowChanged += method;
		}
		#endif
		//============================
		// EditorUtility
		//============================
		public static string SaveFilePanel(string title,string directory,string fallback,string extension){
			return EditorUtility.SaveFilePanel(title,directory,fallback,extension);
		}
		public static SerializedObject GetSerializedObject(UnityObject target){
			if(!ProxyEditor.serializedObjects.ContainsKey(target)){
				ProxyEditor.serializedObjects[target] = new SerializedObject(target);
			}
			return ProxyEditor.serializedObjects[target];
		}
		public static SerializedObject GetSerialized(UnityObject target){
			Type type = typeof(SerializedObject);
			return type.CallMethod<SerializedObject>("LoadFromCache",target.GetInstanceID());
		}
		public static void UpdateSerialized(UnityObject target){
			var serialized = ProxyEditor.GetSerializedObject(target);
			serialized.Update();
			serialized.ApplyModifiedProperties();
			//ProxyEditor.UpdatePrefab(target);
		}
		public static EditorWindow[] GetInspectors(){
			if(ProxyEditor.inspectors == null){
				Type inspectorType = Reflection.GetUnityType("InspectorWindow");
				ProxyEditor.inspectors = inspectorType.CallMethod<EditorWindow[]>("GetAllInspectorWindows");
			}
			return ProxyEditor.inspectors;
		}
		public static EditorWindow GetInspector(Editor editor){
			if(!ProxyEditor.editorInspectors.ContainsKey(editor)){
				Type inspectorType = Reflection.GetUnityType("InspectorWindow");
				var windows = inspectorType.CallMethod<EditorWindow[]>("GetAllInspectorWindows");
				for(int index=0;index<windows.Length;++index){
					var tracker = windows[index].GetVariable<ActiveEditorTracker>("m_Tracker");
					if(tracker == null){continue;}
					for(int editorIndex=0;editorIndex<tracker.activeEditors.Length;++editorIndex){
						var current = tracker.activeEditors[editorIndex];
						ProxyEditor.editorInspectors[current] = windows[index];
					}
				}
			}
			return ProxyEditor.editorInspectors[editor];;
		}
		public static Vector2 GetInspectorScroll(Editor editor){
			return ProxyEditor.GetInspector(editor).GetVariable<Vector2>("m_ScrollPosition");
		}
		public static Vector2 GetInspectorScroll(){
			if(ProxyEditor.inspector.IsNull()){
				Type inspectorWindow = Reflection.GetUnityType("InspectorWindow");
				ProxyEditor.inspector = EditorWindow.GetWindow(inspectorWindow);
			}
			return ProxyEditor.inspector.GetVariable<Vector2>("m_ScrollPosition");
		}
		public static Vector2 GetInspectorScroll(this Rect current){
			Type inspectorWindow = Reflection.GetUnityType("InspectorWindow");
			var window = EditorWindow.GetWindowWithRect(inspectorWindow,current);
			return window.GetVariable<Vector2>("m_ScrollPosition");
		}
		#if !ZIOS_MINIMAL
		[MenuItem("Zios/Unhide GameObjects")]
		public static void UnhideAll(){
			foreach(var target in Resources.FindObjectsOfTypeAll<GameObject>()){
				if(!target.name.ContainsAny("SceneCamera","SceneLight","InternalIdentityTransform","Reflection Probes Camera")){
					target.hideFlags = HideFlags.None;
				}
			}
		}
		#endif
		//============================
		// Undo
		//============================
		public static void RecordObject(UnityObject target,string name){
			if(target.IsNull()){return;}
			UnityUndo.RecordObject(target,name);
		}
		public static void RecordObjects(UnityObject[] targets,string name){
			if(targets.IsNull()){return;}
			UnityUndo.RecordObjects(targets,name);
		}
		public static void RegisterCompleteObjectUndo(UnityObject target,string name){
			UnityUndo.RegisterCompleteObjectUndo(target,name);
		}
		public static void RegisterCreatedObjectUndo(UnityObject target,string name){
			UnityUndo.RegisterCreatedObjectUndo(target,name);
		}
		public static void RegisterSceneUndo(UnityObject target,string name){
			#if UNITY_3_0 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
			UnityUndo.RegisterSceneUndo(name);
			#else
			UnityUndo.RegisterCompleteObjectUndo(target,name);
			#endif
		}
		public static void RegisterUndo(UnityObject target,string name){
			#if UNITY_3_0 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
			UnityUndo.RegisterUndo(target,name);
			#else
			UnityUndo.RecordObject(target,name);
			#endif
		}
		public static void RegisterUndo(UnityObject[] targets,string name){
			#if UNITY_3_0 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
			UnityUndo.RegisterUndo(targets,name);	
			#else
			UnityUndo.RecordObjects(targets,name);
			#endif
		}
		//============================
		// Other
		//============================
		public static void UpdateSelection(){
			var targets = Selection.objects;
			var focus = GUI.GetNameOfFocusedControl();
			if(targets.Length > 0){
				Selection.activeObject = null;
				Call.Delay(()=>{
					Selection.objects = targets;
					EditorGUI.FocusTextInControl(focus);
					GUI.FocusControl(focus);
				},0.05f);
			}
		}
		public static void RebuildInspectors(){
			//typeof(EditorUtility).CallMethod("ForceRebuildInspectors");
			Type inspectorType = Reflection.GetUnityType("InspectorWindow");
			var windows = inspectorType.CallMethod<EditorWindow[]>("GetAllInspectorWindows");
			for(int index=0;index<windows.Length;++index){
				if(windows[index].IsNull()){continue;}
				var tracker = windows[index].GetVariable<ActiveEditorTracker>("m_Tracker");
				if(tracker == null || System.Object.Equals(tracker,null)){continue;}
				tracker.ForceRebuild();
			}
		}
		public static void ShowInspectors(){
			Type inspectorType = Reflection.GetUnityType("InspectorWindow");
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
			Type inspectorType = Reflection.GetUnityType("InspectorWindow");
			inspectorType.CallMethod("RepaintAllInspectors");
		}
		public static void RepaintToolbar(){
			Reflection.GetUnityType("Toolbar").CallMethod("RepaintToolbar");
		}
		public static void RepaintAll(){
			//foreach(var window in Locate.GetAssets<EditorWindow>()){window.Repaint();}
			UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
			//typeof(EditorApplication).CallMethod("RequestRepaintAllViews");
			//Reflection.GetUnityType("InspectorWindow").CallMethod("RepaintAllInspectors");
			ProxyEditor.RepaintToolbar();
		}
		public static void RepaintGameView(){
			Type viewType = Reflection.GetUnityType("GameView");
			var gameview = EditorWindow.GetWindow(viewType);
			gameview.Repaint();
		}
		public static void RepaintSceneView(){
			if(SceneView.lastActiveSceneView != null){
				SceneView.lastActiveSceneView.Repaint();
			}
		}
		public static int GetLocalID(int instanceID){
			return Unsupported.GetLocalIdentifierInFile(instanceID);
		}
		public static bool MoveComponentUp(Component component){
			return (bool)Reflection.GetUnityType("ComponentUtility").CallMethod("MoveComponentUp",component.AsArray());
		}
		public static bool MoveComponentDown(Component component){
			return (bool)Reflection.GetUnityType("ComponentUtility").CallMethod("MoveComponentDown",component.AsArray());
		}
	}
}
namespace Zios.Unity.ProxyEditor{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Reflection;
	public static class GameObjectExtensions{
		public static void UpdateSerialized(this Component current){
			ProxyEditor.UpdateSerialized(current);
		}
		public static GameObject GetPrefabRoot(this GameObject current){
			return ProxyEditor.GetPrefabRoot(current);
		}
		public static GameObject GetPrefabRoot(this Component current){
			if(current.IsNull()){return null;}
			return current.gameObject.GetPrefabRoot();
		}
	}
	public static class ComponentExtensions{
		public static void Move(this Component current,int amount){
			if(current.IsNull()){return;}
			ProxyEditor.DisconnectPrefabInstance(current);
			while(amount != 0){
				if(amount > 0){
					ProxyEditor.MoveComponentDown(current);
					amount -= 1;
				}
				if(amount < 0){
					ProxyEditor.MoveComponentUp(current);
					amount += 1;
				}
			}
		}
		public static void MoveUp(this Component current){
			if(current.IsNull()){return;}
			Component[] components = current.GetComponents<Component>();
			int position = components.IndexOf(current);
			int amount = 1;
			if(position != 0){
				while(components[position-1].hideFlags.Contains(HideFlags.HideInInspector)){
					position -= 1;
					amount += 1;
				}
			}
			current.Move(-amount);
		}
		public static void MoveDown(this Component current){
			if(current.IsNull()){return;}
			Component[] components = current.GetComponents<Component>();
			int position = components.IndexOf(current);
			int amount = 1;
			if(position < components.Length-1){
				while(components[position+1].hideFlags.Contains(HideFlags.HideInInspector)){
					position += 1;
					amount += 1;
				}
			}
			current.Move(amount);
		}
		public static void MoveToTop(this Component current){
			if(current.IsNull()){return;}
			ProxyEditor.DisconnectPrefabInstance(current);
			Component[] components = current.GetComponents<Component>();
			int position = components.IndexOf(current);
			current.Move(-position);
		}
		public static void MoveToBottom(this Component current){
			if(current.IsNull()){return;}
			ProxyEditor.DisconnectPrefabInstance(current);
			Component[] components = current.GetComponents<Component>();
			int position = components.IndexOf(current);
			current.Move(components.Length-position);
		}
	}
	public static class UnityObjectExtensions{
		public static UnityObject GetPrefab(this UnityObject current){
			return ProxyEditor.GetPrefab(current);
		}
	}
	public static class RectExtensions{
		public static EditorWindow GetInspectorWindow(this Rect current){
			Type inspectorWindow = Reflection.GetUnityType("InspectorWindow");
			return EditorWindow.GetWindowWithRect(inspectorWindow,current);
		}
		public static Rect GetInspectorArea(this Rect current,EditorWindow window=null){
			var windowRect = new Rect(0,0,Screen.width,Screen.height);
			//var window = current.GetInspectorWindow();
			//Log.Show(window.GetVariable<Rect>("position"));
			if(window == null){window = ProxyEditor.GetInspectors().First();}
			Vector2 scroll = window.GetVariable<Vector2>("m_ScrollPosition");
			windowRect.x = scroll.x;
			windowRect.y = scroll.y;
			return windowRect;
		}
		public static bool InInspectorWindow(this Rect current,EditorWindow window=null){
			if(current.IsEmpty()){return false;}
			Rect windowRect = current.GetInspectorArea(window);
			return current.Overlaps(windowRect);
		}
	}
}
#else
using System;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios.Unity.ProxyEditor{
	public enum PrefabType{
		None,
		Prefab,
		ModelPrefab,
		PrefabInstance,
		ModelPrefabInstance,
		MissingPrefabInstance,
		DisconnectedPrefabInstance,
		DisconnectedModelPrefabInstance
	}
	public static class ProxyEditor{
		//============================
		// AssetsDatabase
		//============================
		public static void StartAssetEditing(){}
		public static void StopAssetEditing(){}
		public static void RefreshAssets(){}
		public static void SaveAssets(){}
		public static void ImportAsset(string path){}
		public static void CreateAsset(UnityObject target,string path){}
		public static void DeleteAsset(string path){}
		public static string GetGUID(string path){return "";}
		public static string GetAssetPath(UnityObject target){return "";}
		public static string GetAssetPath(string guid){return "";}
		public static UnityObject LoadAsset(string path,Type type){return null;}
		public static UnityObject LoadMainAsset(string path){return null;}
		public static Type LoadAsset<Type>(string path) where Type : UnityObject{return default(Type);}
		public static UnityObject GetMonoScript(MonoBehaviour behaviour){return null;}
		public static bool WriteImportSettings(string path){return false;}
		public static bool CopyAsset(string from,string to){return false;}
		//============================
		// Assets
		//============================
		public static void ReloadScripts(){}
		public static void BuildAssetBundles(){}
		public static void ClearDirty(){}
		public static void MarkSceneDirty(){}
		public static void SetDirty(UnityObject target,bool delayed=false,bool forced=false){}
		public static void SetAssetDirty(UnityObject target){}
		public static bool IsDirty(UnityObject target){return false;}
		//============================
		// PrefabUtility
		//============================
		public static UnityObject GetPrefab(UnityObject target){return target;}
		public static PrefabType GetPrefabType(UnityObject target){return PrefabType.None;}
		public static GameObject GetPrefabRoot(GameObject target){return target;}
		public static void ApplyPrefab(GameObject target){}
		public static void UpdatePrefab(UnityObject target){}
		public static bool ReconnectToLastPrefab(GameObject target){return true;}
		public static void DisconnectPrefabInstance(UnityObject target){}
		//============================
		// ShaderUtil
		//============================
		public static string GetPropertyName(Shader shader,int index){return "";}
		public static object GetPropertyType(Shader shader,int index){return null;}
		public static int GetPropertyCount(Shader shader){return 0;}
		//============================
		// EditorApplication
		//============================
		public static bool IsChanging(){return false;}
		public static bool IsUpdating(){return false;}
		public static bool IsPaused(){return false;}
		public static bool IsPlaying(){return true;}
		public static bool IsCompiling(){return false;}
		public static bool IsSwitching(){return false;}
		public static void AddUpdate(Action method){}
		public static void RemoveUpdate(Action method){}
		public static void AddModeChange(Action method){}
		//============================
		// EditorUtility
		//============================
		//public static SerializedObject GetSerializedObject(UnityObject target){return null;}
		//public static SerializedObject GetSerialized(UnityObject target){return null;}
		//public static EditorWindow[] GetInspectors(){return null;}
		//public static EditorWindow GetInspector(Editor editor){return null;}
		public static void UpdateSerialized(UnityObject target){}
		//public static Vector2 GetInspectorScroll(Editor editor){return Vector2.zero;}
		public static Vector2 GetInspectorScroll(){return Vector2.zero;}
		public static Vector2 GetInspectorScroll(this Rect current){return Vector2.zero;}
		public static void UnhideAll(){}
		//============================
		// Undo
		//============================
		public static void RecordObject(UnityObject target,string name){}
		public static void RecordObjects(UnityObject[] target,string name){}
		public static void RegisterCompleteObjectUndo(UnityObject target,string name){}
		public static void RegisterSceneUndo(string name){}
		public static void RegisterUndo(UnityObject target,string name){}
		public static void RegisterUndo(UnityObject[] targets,string name){}
		//============================
		// Other
		//============================
		public static void UpdateSelection(){}
		public static void RebuildInspectors(){}
		public static void ShowInspectors(){}
		public static void RepaintInspectors(){}
		public static void RepaintToolbar(){}
		public static void RepaintAll(){}
		public static void RepaintGameView(){}
		public static void RepaintSceneView(){}
		public static int GetLocalID(int instanceID){return -1;}
		public static bool MoveComponentUp(Component component){return false;}
		public static bool MoveComponentDown(Component component){return false;}
	}
}
namespace Zios.Unity.ProxyEditor{
	using Zios.Extensions;
	using Zios.Reflection;
	public static class GameObjectExtensions{
		public static void UpdateSerialized(this Component current){}
		public static GameObject GetPrefabRoot(this GameObject current){return current;}
		public static GameObject GetPrefabRoot(this Component current){return current.gameObject;}
	}
	public static class ComponentExtensions{
		public static void Move(this Component current,int amount){}
		public static void MoveUp(this Component current){}
		public static void MoveDown(this Component current){}
		public static void MoveToTop(this Component current){}
		public static void MoveToBottom(this Component current){}
	}
	public static class UnityObjectExtensions{
		public static UnityObject GetPrefab(this UnityObject current){return current;}
	}
}
#endif