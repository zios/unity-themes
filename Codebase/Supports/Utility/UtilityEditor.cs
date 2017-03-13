#pragma warning disable 0162
#pragma warning disable 0618
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios{
	#if UNITY_EDITOR
	using Event;
	using UnityEditor;
	public class UtilityListener : AssetPostprocessor{
		public static void OnPostprocessAllAssets(string[] imported,string[] deleted,string[] movedTo, string[] movedFrom){
			bool playing = EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;
			if(!playing){Events.Call("On Asset Changed");}
		}
	}
	public class UtilityModificationListener : AssetModificationProcessor{
		public static string[] OnWillSaveAssets(string[] paths){
			//foreach(string path in paths){Debug.Log("Saving Changes : " + path);}
			if(paths.Exists(x=>x.Contains(".unity"))){Events.Call("On Scene Saving");}
			Events.Call("On Asset Saving");
			Events.Call("On Asset Modifying");
			return paths;
		}
		public static string OnWillCreateAssets(string path){
			Debug.Log("Creating : " + path);
			Events.Call("On Asset Creating");
			Events.Call("On Asset Modifying");
			return path;
		}
		public static string[] OnWillDeleteAssets(string[] paths,RemoveAssetOptions option){
			foreach(string path in paths){Debug.Log("Deleting : " + path);}
			Events.Call("On Asset Deleting");
			Events.Call("On Asset Modifying");
			return paths;
		}
		public static string OnWillMoveAssets(string path,string destination){
			Debug.Log("Moving : " + path + " to " + destination);
			Events.Call("On Asset Moving");
			Events.Call("On Asset Modifying");
			return path;
		}
	}
	public static partial class Utility{
		private static EditorWindow inspector;
		private static EditorWindow[] inspectors;
		private static Dictionary<Editor,EditorWindow> editorInspectors = new Dictionary<Editor,EditorWindow>();
		private static List<UnityObject> delayedDirty = new List<UnityObject>();
		private static Dictionary<UnityObject,SerializedObject> serializedObjects = new Dictionary<UnityObject,SerializedObject>();
		public static SerializedObject GetSerializedObject(UnityObject target){
			if(!Utility.serializedObjects.ContainsKey(target)){
				Utility.serializedObjects[target] = new SerializedObject(target);
			}
			return Utility.serializedObjects[target];
		}
		public static SerializedObject GetSerialized(UnityObject target){
			Type type = typeof(SerializedObject);
			return type.CallMethod<SerializedObject>("LoadFromCache",target.GetInstanceID());
		}
		public static void UpdateSerialized(UnityObject target){
			var serialized = Utility.GetSerializedObject(target);
			serialized.Update();
			serialized.ApplyModifiedProperties();
			//Utility.UpdatePrefab(target);
		}
		public static EditorWindow[] GetInspectors(){
			if(Utility.inspectors == null){
				Type inspectorType = Utility.GetUnityType("InspectorWindow");
				Utility.inspectors = inspectorType.CallMethod<EditorWindow[]>("GetAllInspectorWindows");
			}
			return Utility.inspectors;
		}
		public static EditorWindow GetInspector(Editor editor){
			#if UNITY_EDITOR
			if(!Utility.editorInspectors.ContainsKey(editor)){
				Type inspectorType = Utility.GetUnityType("InspectorWindow");
				var windows = inspectorType.CallMethod<EditorWindow[]>("GetAllInspectorWindows");
				for(int index=0;index<windows.Length;++index){
					var tracker = windows[index].GetVariable<ActiveEditorTracker>("m_Tracker");
					if(tracker == null){continue;}
					for(int editorIndex=0;editorIndex<tracker.activeEditors.Length;++editorIndex){
						var current = tracker.activeEditors[editorIndex];
						Utility.editorInspectors[current] = windows[index];
					}
				}
			}
			return Utility.editorInspectors[editor];
			#endif
			return null;
		}
		public static Vector2 GetInspectorScroll(Editor editor){
			#if UNITY_EDITOR
			return Utility.GetInspector(editor).GetVariable<Vector2>("m_ScrollPosition");
			#endif
			return Vector2.zero;
		}
		public static Vector2 GetInspectorScroll(){
			if(Utility.inspector.IsNull()){
				Type inspectorWindow = Utility.GetUnityType("InspectorWindow");
				Utility.inspector = EditorWindow.GetWindow(inspectorWindow);
			}
			return Utility.inspector.GetVariable<Vector2>("m_ScrollPosition");
		}
		public static Vector2 GetInspectorScroll(this Rect current){
			Type inspectorWindow = Utility.GetUnityType("InspectorWindow");
			var window = EditorWindow.GetWindowWithRect(inspectorWindow,current);
			return window.GetVariable<Vector2>("m_ScrollPosition");
		}
		#if !UNITY_THEMES
		//[MenuItem("Zios/Format Code")]
		public static void FormatCode(){
			var output = new StringBuilder();
			var current = "";
			foreach(var file in FileManager.FindAll("*.cs")){
				var contents = file.GetText();
				output.Clear();
				foreach(var line in contents.GetLines()){
					var leading = line.Substring(0,line.TakeWhile(char.IsWhiteSpace).Count()).Replace("    ","\t");
					current = leading+line.Trim().Replace("//","////");
					if(line.Trim().IsEmpty()){continue;}
					output.AppendLine(current);
				}
				file.WriteText(output.ToString().TrimEnd(null));
			}
		}
		[MenuItem("Zios/Unhide GameObjects")]
		public static void UnhideAll(){
			foreach(var target in Locate.GetSceneObjects()){
				target.hideFlags = HideFlags.None;
			}
		}
		#endif
	}
	#endif
}