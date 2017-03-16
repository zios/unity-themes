using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEvent = UnityEngine.Event;
namespace Zios.Editors{
	using Interface;
	public class ZiosInspector : EditorWindow{
		public GameObject target;
		public Component[] components = new Component[0];
		public Editor[] editors = new Editor[0];
		public bool dirty;
		private Dictionary<Component,Rect> componentArea = new Dictionary<Component,Rect>();
		private Dictionary<Component,Rect> headerArea = new Dictionary<Component,Rect>();
		private Vector2 scrollPosition;
		[MenuItem("Zios/Window/Inspector",false,0)]
		private static void Init(){
			ZiosInspector window = (ZiosInspector)EditorWindow.GetWindow(typeof(ZiosInspector));
			var icon = FileManager.GetAsset<Texture2D>("IconZiosInspector.png");
			window.SetTitle("Inspector",icon);
			window.position = new Rect(100,150,200,200);
			window.wantsMouseMove = true;
			window.autoRepaintOnSceneChange = true;
		}
		public void OnGUI(){
			Theme.Apply("Grayson");
			GUI.skin = FileManager.GetAsset<GUISkin>("EditorStyles-Grayson.guiskin");
			var headerStyle = Style.Get("m_InspectorTitlebar").Padding(32,0,4,0);
			var containerStyle = Style.Get("m_InspectorDefaultMargins");
			var cursorArea = new Rect(UnityEvent.current.mousePosition.x,UnityEvent.current.mousePosition.y,0,0);
			EditorGUIUtility.wideMode = true;
			EditorGUIUtility.labelWidth = 200;
			this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
			GUILayout.Space(4);
			for(int index=0;index<this.components.Length;++index){
				var component = this.components[index];
				var editor = this.editors[index];
				if(component.IsNull() || editor.IsNull()){continue;}
				string name = component.GetAlias();
				if(this.headerArea.AddNew(component).Clicked(1)){
					var clickArea = cursorArea;
					typeof(EditorUtility).CallExactMethod("DisplayObjectContextMenu",clickArea,component,0);
					UnityEvent.current.Use();
				}
				bool enabled = true;
				if(!this.headerArea[component].IsEmpty() && component.HasVariable("enabled")){
					enabled = component.GetVariable<bool>("enabled");
					var toggleButton = this.headerArea[component].AddX(8).AddY(4).SetSize(32,32);
					enabled = enabled.Draw(toggleButton,null,GUI.skin.toggle);
					component.SetVariable("enabled",enabled);
				}
				var label = new UnityLabel(name);
				//var currentStyle = new GUIStyle(titleStyle);
				//if(enabled){currentStyle.normal = currentStyle.onActive;}
				//bool editable = component.HasVariable("alias");
				bool editable = false;
				bool state = label.DrawHeader(name,headerStyle,editable);
				Rect headerArea = GUILayoutUtility.GetLastRect();
				if(!headerArea.IsEmpty() && headerArea != this.headerArea[component]){
					this.headerArea[component] = headerArea;
					this.dirty = true;
				}
				if(state){
					var area = this.componentArea.AddNew(component);
					if(area.IsEmpty() || area.height > 1){
						EditorGUILayout.BeginVertical(containerStyle);
						editor.OnInspectorGUI();
						EditorGUILayout.EndVertical();
						this.componentArea[component] = GUILayoutUtility.GetLastRect();
					}
				}
				GUILayout.Space(1);
			}
			EditorGUILayout.EndScrollView();
			if("Add Component".ToLabel().DrawButton()){
				//var area = GUILayoutUtility.GetLastRect();
				var componentWindow = Utility.GetUnityType("AddComponentWindow");
				componentWindow.CallExactMethod("Show",cursorArea,this.target.AsArray());
			}
			Theme.Apply();
			if(this.dirty){this.Repaint();}
		}
		public void OnSelectionChange(){
			this.target = Selection.activeGameObject;
			this.components = this.target.IsNull() ? new Component[0] : Locate.GetObjectComponents<Component>(this.target).Where(x=>!x.IsNull()).ToArray();
			this.editors = this.components.Select(x=>Editor.CreateEditor(x)).ToArray();
			this.dirty = true;
			this.Repaint();
		}
	}
}