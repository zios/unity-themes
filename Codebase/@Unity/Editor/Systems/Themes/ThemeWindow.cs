using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.Themes{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Reflection;
	using Zios.Unity.Call;
	using Zios.Unity.Editor.Pref;
	using Zios.Unity.Locate;
	using Zios.Unity.ProxyEditor;
	public class ThemeWindow : EditorWindow{
		public Vector2 lastMouse;
		public static bool setup;
		public static Rect hiddenPosition = new Rect(9001,9001,1,1);
		public static Vector2 hiddenSize = new Vector2(1,1);
		public static bool Inactive(){return Theme.disabled || !ThemeWindow.setup || EditorApplication.isCompiling || EditorApplication.isUpdating;}
		public void OnEnable(){ThemeWindow.setup = true;}
		public void OnGUI(){
			Theme.disabled = EditorPref.Get<bool>("EditorTheme-Disabled",false);
			this.Repaint();
			if(ThemeWindow.Inactive()){return;}
			Theme.Update();
			if(ThemeWindow.Inactive()){return;}
			ThemeContent.Monitor();
			var validTheme = !Theme.active.IsNull() && Theme.active.name != "Default";
			var mouseChanged = this.lastMouse != Event.current.mousePosition;
			Call.Delay(RelativeColor.UpdateSystem,0.2f,false);
			if(validTheme && mouseChanged){
				this.lastMouse = Event.current.mousePosition;
				float delay = 0;
				if(Theme.hoverResponse == HoverResponse.None){return;}
				if(Theme.hoverResponse == HoverResponse.Slow){delay = 0.2f;}
				if(Theme.hoverResponse == HoverResponse.Moderate){delay = 0.05f;}
				Call.Delay("Redraw",()=>{
					Theme.UpdateColors();
					var view = Reflection.GetUnityType("GUIView").GetVariable("mouseOverView");
					if(!view.IsNull()){
						view.CallMethod("Repaint");
					}
				},delay,false);
			}
		}
		public static void ShowWindow(){
			if(Theme.window.IsNull()){
				Theme.window = Locate.GetAssets<ThemeWindow>().FirstOrDefault();
				if(Theme.window.IsNull()){
					Theme.window = EditorWindow.CreateInstance<ThemeWindow>();
					Theme.window.position = ThemeWindow.hiddenPosition;
					Theme.window.minSize = ThemeWindow.hiddenSize;
					Theme.window.maxSize = ThemeWindow.hiddenSize;
					Theme.window.wantsMouseMove = Theme.hoverResponse != HoverResponse.None;
					Theme.window.ShowPopup();
				}
				if(Theme.window.IsNull()){return;}
			}
			if(Theme.window.position != ThemeWindow.hiddenPosition){Theme.window.position = ThemeWindow.hiddenPosition;}
			if(Theme.window.maxSize != ThemeWindow.hiddenSize){
				Theme.window.minSize = ThemeWindow.hiddenSize;
				Theme.window.maxSize = ThemeWindow.hiddenSize;
			}
		}
		public void OnDestroy(){
			Theme.window = null;
			ThemeWindow.setup = false;
		}
		public void OnDisable(){
			Theme.window = null;
			ThemeWindow.setup = false;
		}
	}
}