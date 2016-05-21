using UnityEditor;
using UnityEngine;
namespace Zios.Interface{
	public static class UI{
		public static int DrawPrompt(this string current,ref string field,GUIStyle titleStyle=null,GUIStyle inputStyle=null){
			int result = 0;
			if(Button.EventKeyDown("KeypadEnter") || Button.EventKeyDown("Return")){result = 1;}
			if(Button.EventKeyDown("Escape")){result = -1;}
			if(titleStyle == null){titleStyle = Style.Get("Prompt","DialogQuestion");}
			if(inputStyle == null){inputStyle = Style.Get("Prompt","DialogInput");}
			float width = (Screen.width/2).Max(150);
			Rect full = new Rect(0,0,Screen.width,Screen.height);
			Rect center = new Rect(Screen.width/2,Screen.height/2,0,0);
			Rect input = center.AddY(-15).SetSize(width,40).AddX(-width/2);
			current.ToLabel().DrawLabel(full,titleStyle);
			GUI.SetNextControlName("PromptField");
			field = field.Draw(input,null,inputStyle);
			EditorGUI.FocusTextInControl("PromptField");
			return result;
		}
		public static int DrawButtonPrompt(this string current,GUIStyle titleStyle=null,GUIStyle buttonStyle=null){
			int result = 0;
			if(Button.EventKeyDown("Escape")){result = -1;}
			if(titleStyle == null){titleStyle = Style.Get("Prompt","DialogQuestion");}
			if(buttonStyle == null){buttonStyle = Style.Get("Prompt","DialogButton");}
			Rect full = new Rect(0,0,Screen.width,Screen.height);
			Rect button = new Rect(Screen.width/2,Screen.height/2,100,40).AddY(-10);
			current.ToLabel().DrawLabel(full,titleStyle);
			if("Yes".ToLabel().DrawButton(button.AddX(-105),buttonStyle)){result = 1;}
			if("No".ToLabel().DrawButton(button.AddX(5),buttonStyle)){result = 2;}
			return result;
		}
	}
}