using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.Windows{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.File;
	using Zios.Reflection;
	using Zios.Unity.Editor.Extensions;
	using Zios.Unity.EditorUI;
	using Zios.Unity.Extensions;
	using Zios.Unity.Locate;
	using Zios.Unity.Style;
	using Editor = UnityEditor.Editor;
	public class ZiosInspector : EditorWindow{
		public GameObject target;
		public Component[] components = new Component[0];
		public Editor editor;
		public Editor[] editors = new Editor[0];
		public bool dirty;
		private Dictionary<Component,Rect> componentArea = new Dictionary<Component,Rect>();
		private Dictionary<Component,Rect> headerArea = new Dictionary<Component,Rect>();
		private Vector2 scrollPosition;
		[MenuItem("Zios/Window/Inspector",false,0)]
		private static void Init(){
			ZiosInspector window = (ZiosInspector)EditorWindow.GetWindow(typeof(ZiosInspector));
			var icon = File.GetAsset<Texture2D>("IconZiosInspector.png");
			window.SetTitle("Inspector",icon);
			window.position = new Rect(100,150,200,200);
			window.wantsMouseMove = true;
			window.autoRepaintOnSceneChange = true;
		}
		public void OnGUI(){
			if(this.editor.IsNull()){return;}
			GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
			GUILayout.Space(0);
			this.editor.DrawHeader();
			this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
			GUILayout.Space(5);
			for(int index=0;index<this.components.Length;++index){
				var component = this.components[index];
				var editor = this.editors[index];
				if(component.IsNull() || editor.IsNull()){continue;}
				var state = editor.target.IsExpanded();
				editor.target.SetExpanded(EditorGUILayout.InspectorTitlebar(state,editor.target));
				if(state){
					GUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
					EditorGUIUtility.wideMode = true;
					EditorGUIUtility.labelWidth = Screen.width*0.4f;
					editor.OnInspectorGUI();
					GUILayout.EndVertical();
				}
			}
			EditorGUILayout.EndScrollView();
			var line = typeof(EditorStyles).GetVariable<GUIStyle>("inspectorTitlebar").normal.background;
			//GUI.DrawTextureWithTexCoords(GUILayoutUtility.GetControl().SetSize(Screen.width,1),line,new Rect(0,1,1,1));
			//GUILayout.Box(,EditorUI.GenerateLayout(Screen.width,3));
			GUILayout.Space(5);
			if("Add Component".ToLabel().Layout(240,30).DrawButton(new GUIStyle(GUI.skin.button).Center(240))){
				var area = GUILayoutUtility.GetLastRect().SetSize(250,500).Center().AddY(-300);
				var componentWindow = Reflection.GetUnityType("AddComponentWindow");
				componentWindow.CallExactMethod<bool>("Show",area,this.target.AsArray());
			}
			//if(this.dirty){this.Repaint();}
		}
		public void OnFocus(){
			this.OnSelectionChange();
		}
		public void OnSelectionChange(){
			this.target = Selection.activeGameObject;
			this.editor = Editor.CreateEditor(this.target);
			this.components = this.target.IsNull() ? new Component[0] : Locate.GetObjectComponents<Component>(this.target).Where(x=>!x.IsNull()).ToArray();
			this.editors = this.components.Select(x=>Editor.CreateEditor(x)).ToArray();
			this.dirty = true;
			this.Repaint();
		}
	}
}