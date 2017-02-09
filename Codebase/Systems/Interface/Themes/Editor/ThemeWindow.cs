using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEvent = UnityEngine.Event;
namespace Zios.Interface{
	public class ThemeWindow : EditorWindow{
		public Vector2 lastMouse;
		public static bool setup;
		public static Rect hiddenPosition = new Rect(9001,9001,1,1);
		public static Vector2 hiddenSize = new Vector2(1,1);
		public void OnEnable(){ThemeWindow.setup = true;}
		public void OnGUI(){
			Theme.disabled = Utility.GetPref<bool>("EditorTheme-Disabled",false);
			this.Repaint();
			if(Theme.disabled || !ThemeWindow.setup || EditorApplication.isCompiling || EditorApplication.isUpdating){
				return;
			}
			EditorApplication.update -= ThemeWindow.ShowWindow;
			ThemeWindow.ShowWindow();
			Theme.Update();
			ThemeContent.Monitor();
			bool validTheme = !Theme.active.IsNull() && Theme.active.name != "Default";
			bool mouseChanged = this.lastMouse != UnityEvent.current.mousePosition;
			Utility.DelayCall(RelativeColor.UpdateSystem,0.2f,false);
			if(validTheme && mouseChanged){
				this.lastMouse = UnityEvent.current.mousePosition;
				float delay = 0;
				if(Theme.hoverResponse == HoverResponse.None){return;}
				if(Theme.hoverResponse == HoverResponse.Slow){delay = 0.2f;}
				if(Theme.hoverResponse == HoverResponse.Moderate){delay = 0.05f;}
				Utility.DelayCall("Redraw",()=>{
					Theme.UpdateColors();
					var view = Utility.GetUnityType("GUIView").GetVariable("mouseOverView");
					if(!view.IsNull()){
						view.CallMethod("Repaint");
					}
				},delay,false);
			}
		}
		public static void ShowWindow(){
			if(Theme.window.IsNull()){
				Theme.window = Resources.FindObjectsOfTypeAll<ThemeWindow>().FirstOrDefault();
				if(Theme.window.IsNull()){
					Theme.setup = false;
					Theme.window = ScriptableObject.CreateInstance<ThemeWindow>();
					Theme.window.position = ThemeWindow.hiddenPosition;
					Theme.window.minSize = ThemeWindow.hiddenSize;
					Theme.window.wantsMouseMove = Theme.hoverResponse != HoverResponse.None;
					Theme.window.ShowPopup();
				}
			}
			if(Theme.window.position != ThemeWindow.hiddenPosition){Theme.window.position = ThemeWindow.hiddenPosition;}
			if(Theme.window.minSize != ThemeWindow.hiddenSize){Theme.window.minSize = ThemeWindow.hiddenSize;}
		}
		public static void ResetWindow(){
			Theme.window = null;
			ThemeWindow.ShowWindow();
		}
	}
}