using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.Attributes.Supports.Target{
	using Zios.Attributes.Supports;
	using Zios.Events;
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.File;
	using Zios.Unity.Colors;
	using Zios.Unity.Components.DataBehaviour;
	using Zios.Unity.Editor.Extensions;
	using Zios.Unity.Editor.Pref;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.EditorUI;
	using Zios.Unity.Extensions;
	//asm Zios.Shortcuts;
	//asm Zios.Unity.Shortcuts;
	[CustomPropertyDrawer(typeof(Target),true)]
	public class TargetDrawer : PropertyDrawer{
		public bool setup;
		public static GUISkin skin;
		public override void OnGUI(Rect area,SerializedProperty property,GUIContent label){
			EditorUI.Reset();
			property.serializedObject.Update();
			Target target = property.GetObject<Target>();
			TargetDrawer.Draw(area,target,label);
		}
		public static void Draw(Rect area,Target target,GUIContent label){
			if(target.parent.IsNull()){return;}
			if(TargetDrawer.skin.IsNull()){
				string skin = EditorGUIUtility.isProSkin || EditorPref.Get<bool>("EditorTheme-Dark",false) ? "Dark" : "Light";
				TargetDrawer.skin = File.GetAsset<GUISkin>("Gentleface-" + skin + ".guiskin");
			}
			Rect toggleRect = new Rect(area);
			Rect propertyRect = new Rect(area);
			float labelWidth = label.text.IsEmpty() ? 0 : EditorGUIUtility.labelWidth;
			propertyRect.x += labelWidth + 18;
			propertyRect.width -= labelWidth + 18;
			toggleRect.x += labelWidth;
			toggleRect.width = 18;
			bool previousMode = target.mode == TargetMode.Direct;
			bool currentMode = previousMode.Draw(toggleRect,"",TargetDrawer.skin.GetStyle("TargetToggle"));
			if(previousMode != currentMode){
				target.mode = target.mode == TargetMode.Direct ? TargetMode.Search : TargetMode.Direct;
			}
			label.ToLabel().DrawLabel(area,null,true);
			ProxyEditor.RecordObject(target.parent,"Target Changes");
			if(target.mode == TargetMode.Direct){
				target.directObject = target.directObject.Draw<GameObject>(propertyRect,"",true);
			}
			else{
				target.Verify();
				var faded = GUI.skin.textField.Background("").TextColor(GUI.skin.textField.normal.textColor.SetAlpha(0.75f)).ContentOffset(-3,0).UseState("normal");
				Rect textRect = propertyRect;
				string result = !target.searchObject.IsNull() ? target.searchObject.GetPath().Trim("/") : "Not Found.";
				Vector2 textSize = TargetDrawer.skin.textField.CalcSize(new GUIContent(target.search));
				Vector2 subtleSize = faded.CalcSize(new GUIContent(result));
				float subtleX = propertyRect.x+propertyRect.width-subtleSize.x;
				float subtleWidth = subtleSize.x;
				float minimumX = propertyRect.x+textSize.x+3;
				if(subtleX < minimumX){
					subtleWidth -= (minimumX-subtleX);
					subtleX = minimumX;
				}
				propertyRect = propertyRect.SetX(subtleX).SetWidth(subtleWidth);
				EditorGUIUtility.AddCursorRect(propertyRect,MouseCursor.Zoom);
				if(!target.searchObject.IsNull() && propertyRect.Clicked(0)){
					Selection.activeGameObject = target.searchObject;
					Event.current.Use();
				}
				target.search = target.search.Draw(textRect);
				result.ToLabel().DrawLabel(propertyRect,faded);
			}
			if(GUI.changed && !target.IsNull()){
				target.Search();
				if(target.parent is DataBehaviour){
					var parent = target.parent.As<DataBehaviour>();
					parent.DelayEvent(parent.path,"On Validate");
					ProxyEditor.SetDirty(parent);
				}
			}
		}
	}
}