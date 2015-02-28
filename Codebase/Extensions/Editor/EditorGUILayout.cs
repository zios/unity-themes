using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;
public static class EditorGUILayoutExtension{
	public static string Draw(this string current,GUIStyle style=null){
		style = style ?? EditorStyles.textField;
		return EditorGUIExtension.Draw<string>(()=>EditorGUILayout.TextField(current,style));
	}
	public static float Draw(this float current,GUIStyle style=null){
		style = style ?? EditorStyles.numberField;
		return EditorGUIExtension.Draw<float>(()=>EditorGUILayout.FloatField(current,style));
	}
	public static bool Draw(this bool current,GUIStyle style=null){
		style = style ?? EditorStyles.toggle;
		return EditorGUIExtension.Draw<bool>(()=>EditorGUILayout.Toggle(current,style));
	}
	public static Enum Draw(this Enum current,GUIStyle style=null){
		style = style ?? EditorStyles.popup;
		return EditorGUIExtension.Draw<Enum>(()=>EditorGUILayout.EnumPopup(current,style));
	}
	public static int Draw(this string[] current,int index,GUIStyle style=null){
		style = style ?? EditorStyles.popup;
		return EditorGUIExtension.Draw<int>(()=>EditorGUILayout.Popup(index,current,style));
	}
	public static int Draw(this List<string> current,int index,GUIStyle style=null){
		return current.ToArray().Draw(index,style);
	}
	public static void Draw(this SerializedProperty current,string label="",bool allowScene=true){
		Action action = ()=>EditorGUILayout.PropertyField(current,new GUIContent(label),allowScene);
		EditorGUIExtension.Draw(action);
	}
	public static void Draw(this SerializedProperty current,GUIContent label,bool allowScene=true){
		Action action = ()=>EditorGUILayout.PropertyField(current,label,allowScene);
		EditorGUIExtension.Draw(action);
	}
	public static Rect Draw(this Rect current){
		return EditorGUIExtension.Draw<Rect>(()=>EditorGUILayout.RectField(current));
	}
	public static AnimationCurve Draw(this AnimationCurve current){
		return EditorGUIExtension.Draw<AnimationCurve>(()=>EditorGUILayout.CurveField(current));
	}
	public static Color Draw(this Color current){
		return EditorGUIExtension.Draw<Color>(()=>EditorGUILayout.ColorField(current));
	}
}
public static class EditorGUILayoutExtensionSpecial{
	public static void DrawLabel(this GUIContent current,GUIStyle style=null,bool indention=false){
		style = style ?? EditorStyles.label;
		EditorGUIExtension.Draw(()=>EditorGUILayout.LabelField(current,style),indention);
	}
	public static void DrawLabel(this string current,GUIStyle style=null,bool indention=false){
		style = style ?? EditorStyles.label;
		EditorGUIExtension.Draw(()=>EditorGUILayout.LabelField(current,style),indention);
	}
	public static void DrawHelp(this string current,string textType,bool indention=false){
		MessageType type = MessageType.None;
		if(textType.Contains("Info",true)){type = MessageType.Info;}
		if(textType.Contains("Error",true)){type = MessageType.Error;}
		if(textType.Contains("Warning",true)){type = MessageType.Warning;}
		EditorGUIExtension.Draw(()=>EditorGUILayout.HelpBox(current,type),indention);
	}
	public static string DrawArea(this string current,GUIStyle style=null,bool indention=false){
		style = style ?? EditorStyles.textField;
		return EditorGUIExtension.Draw<string>(()=>EditorGUILayout.TextField(current,style),indention);
	}
	public static bool DrawButton(this string current,GUIStyle style=null,bool indention=false){
		style = style ?? GUI.skin.button;
		return EditorGUIExtension.Draw<bool>(()=>GUILayout.Button(current,style),indention);
	}
	public static int DrawInt(this int current,GUIStyle style=null,bool indention=false){
		style = style ?? EditorStyles.numberField;
		return EditorGUIExtension.Draw<int>(()=>EditorGUILayout.IntField(current,style),indention);
	}
	public static int DrawSlider(this int current,int min,int max,bool indention=false){
		return EditorGUIExtension.Draw<int>(()=>EditorGUILayout.IntSlider(current,min,max),indention);
	}
	public static GameObject DrawObject(this GameObject current,bool allowScene=true,bool indention=false){
		return (GameObject)EditorGUIExtension.Draw<UnityObject>(()=>EditorGUILayout.ObjectField(current,typeof(GameObject),allowScene),indention);
	}
	public static Enum DrawMask(this Enum current,GUIStyle style=null,bool indention=false){
		style = style ?? EditorStyles.popup;
		return EditorGUIExtension.Draw<Enum>(()=>EditorGUILayout.EnumMaskField(current,style),indention);
	}
	public static Vector2 DrawVector2(this Vector2 current,bool indention=false){
		return EditorGUIExtension.Draw<Vector2>(()=>EditorGUILayout.Vector2Field("",current),indention);
	}
	public static Vector3 DrawVector3(this Vector3 current,bool indention=false){
		return EditorGUIExtension.Draw<Vector3>(()=>EditorGUILayout.Vector3Field("",current),indention);
	}
	public static Vector4 DrawVector4(this Vector4 current,bool indention=false){
		return EditorGUIExtension.Draw<Vector3>(()=>EditorGUILayout.Vector4Field("",current),indention);
	}
}
public static class EditorGUILayoutExtensionLabeled{
	public static string DrawLabeled(this string current,GUIContent label,GUIStyle style=null,bool indention=true){
		style = style ?? EditorStyles.textField;
		return EditorGUIExtension.Draw<string>(()=>EditorGUILayout.TextField(label,current,style),indention);
	}
	public static Enum DrawLabeledMask(this Enum current,GUIContent label,GUIStyle style=null,bool indention=true){
		style = style ?? EditorStyles.popup;
		return EditorGUIExtension.Draw<Enum>(()=>EditorGUILayout.EnumMaskField(label,current,style),indention);
	}
	public static int DrawLabeledInt(this int current,GUIContent label,GUIStyle style=null,bool indention=true){
		style = style ?? EditorStyles.numberField;
		return EditorGUIExtension.Draw<int>(()=>EditorGUILayout.IntField(label,current,style),indention);
	}
	public static float DrawLabeled(this float current,GUIContent label,GUIStyle style=null,bool indention=true){
		style = style ?? EditorStyles.numberField;
		return EditorGUIExtension.Draw<float>(()=>EditorGUILayout.FloatField(label,current,style),indention);
	}
	public static bool DrawLabeled(this bool current,GUIContent label,GUIStyle style=null,bool indention=true){
		style = style ?? EditorStyles.toggle;
		return EditorGUIExtension.Draw<bool>(()=>EditorGUILayout.Toggle(label,current,style),indention);
	}
	public static Vector3 DrawLabeled(this Vector3 current,GUIContent label,bool indention=true){
		return EditorGUIExtension.Draw<Vector3>(()=>EditorGUILayout.Vector3Field(label,current),indention);
	}
	public static GameObject DrawLabeledObject(this GameObject current,GUIContent label,bool allowScene=true,bool indention=false){
		return (GameObject)EditorGUIExtension.Draw<UnityObject>(()=>EditorGUILayout.ObjectField(label,current,typeof(GameObject),allowScene),indention);
	}
}