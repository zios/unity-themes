using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios.UI{
	public class UnityLabel{
		public GUIContent value = new GUIContent("");
		public UnityLabel(string value){this.value = new GUIContent(value);}
		public UnityLabel(GUIContent value){this.value = value;}
		public override string ToString(){return this.value.text;}
		public GUIContent ToContent(){return this.value;}
		//public static implicit operator string(UnityLabel current){return current.value.text;}
		public static implicit operator GUIContent(UnityLabel current){
			if(current == null){return GUIContent.none;}
			return current.value;
		}
		public static implicit operator UnityLabel(GUIContent current){return new UnityLabel(current);}
		public static implicit operator UnityLabel(string current){return new UnityLabel(current);}
	}
	public static class EditorGUIExtension{
		public static bool render = true;
		public static Type Draw<Type>(Func<Type> method,bool indention=false){
			int indentValue = EditorGUI.indentLevel;
			if(!indention){EditorGUI.indentLevel = 0;}
			Type value = (Type)method();
			EditorGUI.indentLevel = indentValue;
			return value;
		}
		public static void Draw(Action method,bool indention=false){
			int indentValue = EditorGUI.indentLevel;
			if(!indention){EditorGUI.indentLevel = 0;}
			if(EditorGUIExtension.render){method();}
			if(!indention){EditorGUI.indentLevel = indentValue;}
		}
		public static string Draw(this string current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=false){
			style = style ?? EditorStyles.textField;
			return EditorGUIExtension.Draw<string>(()=>EditorGUI.TextField(area,label,current,style),indention);
		}
		public static float Draw(this float current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=false){
			style = style ?? EditorStyles.numberField;
			return EditorGUIExtension.Draw<float>(()=>EditorGUI.FloatField(area,label,current,style),indention);
		}
		public static bool Draw(this bool current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=false){
			style = style ?? EditorStyles.toggle;
			return EditorGUIExtension.Draw<bool>(()=>EditorGUI.Toggle(area,label,current,style),indention);
		}
		public static Enum Draw(this Enum current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=false){
			style = style ?? EditorStyles.popup;
			return EditorGUIExtension.Draw<Enum>(()=>EditorGUI.EnumPopup(area,label,current,style),indention);
		}
		public static int Draw(this IList<string> current,Rect area,int index,UnityLabel label=null,GUIStyle style=null,bool indention=false){
			style = style ?? EditorStyles.popup;
			string name = label.IsNull() ? "" : label.ToString();
			return EditorGUIExtension.Draw<int>(()=>EditorGUI.Popup(area,name,index,current.ToArray(),style),indention);
		}
		public static void Draw(this SerializedProperty current,Rect area,UnityLabel label=null,bool allowScene=true,bool indention=false){
			if(label != null && label.value.text.IsEmpty()){label = new GUIContent(current.displayName);}
			EditorGUIExtension.Draw(()=>EditorGUI.PropertyField(area,current,label,allowScene),indention);
		}
		public static Rect Draw(this Rect current,Rect area,UnityLabel label=null,bool indention=false){
			return EditorGUIExtension.Draw<Rect>(()=>EditorGUI.RectField(area,label,current),indention);
		}
		public static AnimationCurve Draw(this AnimationCurve current,Rect area,UnityLabel label=null,bool indention=false){
			return EditorGUIExtension.Draw<AnimationCurve>(()=>EditorGUI.CurveField(area,label,current),indention);
		}
		public static Color Draw(this Color current,Rect area,UnityLabel label=null,bool indention=false){
			return EditorGUIExtension.Draw<Color>(()=>EditorGUI.ColorField(area,label,current),indention);
		}
	}
	public static class EditorGUIExtensionSpecial{
		//public static void DrawLabel(this string current,Rect area,GUIStyle style=null,bool indention=false){new UnityLabel(current).DrawLabel(area,style,indention);}
		//public static void DrawLabel(this GUIContent current,Rect area,GUIStyle style=null,bool indention=false){new UnityLabel(current).DrawLabel(area,style,indention);}
		public static void DrawLabel(this UnityLabel current,Rect area,GUIStyle style=null,bool indention=false){
			style = style ?? EditorStyles.label;
			EditorGUIExtension.Draw(()=>EditorGUI.LabelField(area,current,style),indention);
		}
		public static void DrawHelp(this string current,Rect area,string textType,bool indention=false){
			MessageType type = MessageType.None;
			if(textType.Contains("Info",true)){type = MessageType.Info;}
			if(textType.Contains("Error",true)){type = MessageType.Error;}
			if(textType.Contains("Warning",true)){type = MessageType.Warning;}
			EditorGUIExtension.Draw(()=>EditorGUI.HelpBox(area,current,type),indention);
		}
		public static string DrawTextArea(this string current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=false){
			style = style ?? EditorStyles.textField;
			return EditorGUIExtension.Draw<string>(()=>EditorGUI.TextField(area,label,current,style),indention);
		}
		//public static bool DrawButton(this string current,Rect area,GUIStyle style=null,bool indention=false){return new UnityLabel(current).DrawButton(area,style,indention);}
		//public static bool DrawButton(this GUIContent current,Rect area,GUIStyle style=null,bool indention=false){return new UnityLabel(current).DrawButton(area,style,indention);}
		public static bool DrawButton(this UnityLabel current,Rect area,GUIStyle style=null,bool indention=false){
			style = style ?? GUI.skin.button;
			return EditorGUIExtension.Draw<bool>(()=>GUI.Button(area,current,style),indention);
		}
		public static int DrawInt(this int current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=false){
			style = style ?? EditorStyles.numberField;
			return EditorGUIExtension.Draw<int>(()=>EditorGUI.IntField(area,label,current,style),indention);
		}
		public static int DrawSlider(this int current,Rect area,int min,int max,UnityLabel label=null,bool indention=false){
			return EditorGUIExtension.Draw<int>(()=>EditorGUI.IntSlider(area,label,current,min,max),indention);
		}
		public static Type Draw<Type>(this UnityObject current,Rect area,UnityLabel label=null,bool allowScene=true,bool indention=false) where Type : UnityObject{
			return (Type)EditorGUIExtension.Draw<UnityObject>(()=>EditorGUI.ObjectField(area,label,current,typeof(Type),allowScene),indention);
		}
		public static Enum DrawMask(this Enum current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=false){
			style = style ?? EditorStyles.popup;
			return EditorGUIExtension.Draw<Enum>(()=>EditorGUI.EnumMaskField(area,label,current,style),indention);
		}
		public static Vector2 DrawVector2(this Vector2 current,Rect area,UnityLabel label=null,bool indention=false){
			return EditorGUIExtension.Draw<Vector2>(()=>EditorGUI.Vector2Field(area,label,current),indention);
		}
		public static Vector3 DrawVector3(this Vector3 current,Rect area,UnityLabel label=null,bool indention=false){
			return EditorGUIExtension.Draw<Vector3>(()=>EditorGUI.Vector3Field(area,label,current),indention);
		}
		public static Vector4 DrawVector4(this Vector4 current,Rect area,UnityLabel label=null,bool indention=false){
			string name = label.IsNull() ? null : label.ToString();
			return EditorGUIExtension.Draw<Vector3>(()=>EditorGUI.Vector4Field(area,name,current),indention);
		}
	}
}