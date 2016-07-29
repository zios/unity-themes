using UnityEditor;
using UnityEngine;
namespace Zios.Interface{
	public class ThemeWindow : EditorWindow{
		public Vector2 lastMouse;
		public void OnGUI(){
			EditorApplication.update -= Theme.ShowWindow;
			Theme.Step();
			Theme.Update();
			this.Repaint();
			bool validTheme = !Theme.active.IsNull() && Theme.active.name != "Default";
			bool mouseChanged = this.lastMouse != Event.current.mousePosition;
			Debug.Log(GUI.GetNameOfFocusedControl());
			if(validTheme && mouseChanged){
				this.lastMouse = Event.current.mousePosition;
				float delay = 0;
				if(Theme.hoverResponse == HoverResponse.None){return;}
				if(Theme.hoverResponse == HoverResponse.Slow){delay = 0.2f;}
				if(Theme.hoverResponse == HoverResponse.Moderate){delay = 0.05f;}
				Utility.DelayCall("Redraw",()=>{
					ThemeContent.Monitor();
					Theme.UpdateColors();
					Utility.RepaintAll();
				},delay,false);
			}
		}
	}
}