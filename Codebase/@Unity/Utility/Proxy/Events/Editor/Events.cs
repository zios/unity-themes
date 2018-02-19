using UnityEditor;
using UnityEngine;
using CallbackFunction = UnityEditor.EditorApplication.CallbackFunction;
using UnityAction = UnityEngine.Events.UnityAction;
using UnityUndo = UnityEditor.Undo;
namespace Zios.Unity.Editor.EditorEvents{
	using Zios.Events;
	using Zios.Extensions;
	using Zios.Reflection;
	using Zios.Unity.Call;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.Log;
	using Zios.Unity.Proxy;
	using Zios.Unity.UnityEvents;
	//asm Zios.Shortcuts;
	//asm Zios.Unity.Shortcuts;
	[InitializeOnLoad]
	public static class EditorEvents{
		static EditorEvents(){
			Events.Register("On Global Event");
			Events.Register("On Editor Update");
			Events.Register("On Prefab Changed");
			Events.Register("On Lightmap Baked");
			Events.Register("On Windows Reordered");
			Events.Register("On Hierarchy Changed");
			Events.Register("On Asset Changed");
			Events.Register("On Asset Saving");
			Events.Register("On Asset Creating");
			Events.Register("On Asset Deleting");
			Events.Register("On Asset Moving");
			Events.Register("On Scene Loaded");
			Events.Register("On Editor Scene Loaded");
			Events.Register("On Editor Quit");
			Events.Register("On Mode Changed");
			Events.Register("On Enter Play");
			Events.Register("On Exit Play");
			Events.Register("On Undo Flushing");
			Events.Register("On Undo");
			Events.Register("On Redo");
			#if UNITY_5 || UNITY_2017_1_OR_NEWER
			Camera.onPostRender += (Camera camera)=>Events.Call("On Camera Post Render",camera);
			Camera.onPreRender += (Camera camera)=>Events.Call("On Camera Pre Render",camera);
			Camera.onPreCull += (Camera camera)=>Events.Call("On Camera Pre Cull",camera);
			Lightmapping.completed += ()=>Events.Call("On Lightmap Baked");
			#endif
			UnityUndo.willFlushUndoRecord += ()=>Events.Call("On Undo Flushing");
			UnityUndo.undoRedoPerformed += ()=>Events.Call("On Undo");
			UnityUndo.undoRedoPerformed += ()=>Events.Call("On Redo");
			PrefabUtility.prefabInstanceUpdated += (GameObject target)=>Events.Call("On Prefab Changed",target);
			EditorApplication.projectWindowChanged += ()=>Events.Call("On Project Changed");
			EditorApplication.playModeStateChanged += (PlayModeStateChange state)=>{
				Events.Call("On Mode Changed");
				bool changing = ProxyEditor.IsChanging();
				bool playing = Proxy.IsPlaying();
				if(changing && !playing){Events.Call("On Enter Play");}
				if(!changing && playing){Events.Call("On Exit Play");}
			};
			EditorApplication.hierarchyWindowChanged += ()=>Events.DelayCall("On Hierarchy Changed",0.25f);
			EditorApplication.update += ()=>Events.Call("On Editor Update");
			EditorApplication.update += ()=>UnityEvents.CheckLoaded(true);
			EditorApplication.update += ()=>Call.CheckDelayed(true);
			UnityAction editorQuitEvent = new UnityAction(()=>Events.Call("On Editor Quit"));
			CallbackFunction windowEvent = ()=>Events.Call("On Window Reordered");
			CallbackFunction globalEvent = ()=>Events.Call("On Global Event");
			var windowsReordered = typeof(EditorApplication).GetVariable<CallbackFunction>("windowsReordered");
			typeof(EditorApplication).SetVariable("windowsReordered",windowsReordered+windowEvent);
			var globalEventHandler = typeof(EditorApplication).GetVariable<CallbackFunction>("globalEventHandler");
			typeof(EditorApplication).SetVariable("globalEventHandler",globalEventHandler+globalEvent);
			var editorQuitHandler = typeof(EditorApplication).GetVariable<UnityAction>("editorApplicationQuit");
			typeof(EditorApplication).SetVariable("editorApplicationQuit",editorQuitHandler+editorQuitEvent);
		}
	}
	public class UtilityListener : AssetPostprocessor{
		public static void OnPostprocessAllAssets(string[] imported,string[] deleted,string[] movedTo, string[] movedFrom){
			bool playing = ProxyEditor.IsPlaying() || ProxyEditor.IsChanging();
			if(!playing){Events.Call("On Asset Changed");}
		}
	}
	public class UtilityModificationListener : UnityEditor.AssetModificationProcessor{
		public static string[] OnWillSaveAssets(string[] paths){
			//foreach(string path in paths){Log.Show("Saving Changes : " + path);}
			if(paths.Exists(x=>x.Contains(".unity"))){Events.Call("On Scene Saving");}
			Events.Call("On Asset Saving");
			Events.Call("On Asset Modifying");
			return paths;
		}
		public static string OnWillCreateAssets(string path){
			Log.Show("Creating : " + path);
			Events.Call("On Asset Creating");
			Events.Call("On Asset Modifying");
			return path;
		}
		public static string[] OnWillDeleteAssets(string[] paths,RemoveAssetOptions option){
			foreach(string path in paths){Log.Show("Deleting : " + path);}
			Events.Call("On Asset Deleting");
			Events.Call("On Asset Modifying");
			return paths;
		}
		public static string OnWillMoveAssets(string path,string destination){
			Log.Show("Moving : " + path + " to " + destination);
			Events.Call("On Asset Moving");
			Events.Call("On Asset Modifying");
			return path;
		}
	}
}