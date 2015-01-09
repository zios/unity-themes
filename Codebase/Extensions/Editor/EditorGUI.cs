using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;
public static class GUIExtension{
	public static bool render = true;
	public static Type Draw<Type>(Func<Type> method,bool indention=false){
		int indentValue = EditorGUI.indentLevel;
		if(!indention){EditorGUI.indentLevel = 0;}
		Type value = (Type)method();
		EditorGUI.indentLevel = indentValue;
		return value;
	}
	public static void Draw(System.Action method,bool indention=false){
		int indentValue = EditorGUI.indentLevel;
		if(!indention){EditorGUI.indentLevel = 0;}
		if(GUIExtension.render){method();}
		if(!indention){EditorGUI.indentLevel = indentValue;}
	}
	public static void DrawLabel(this GUIContent current,Rect area,GUIStyle style=null,bool indention=false){
		style = style ?? EditorStyles.label;
		GUIExtension.Draw(()=>EditorGUI.LabelField(area,current,style),indention);
	}
	public static void DrawLabel(this string current,Rect area,GUIStyle style=null,bool indention=false){
		style = style ?? EditorStyles.label;
		GUIExtension.Draw(()=>EditorGUI.LabelField(area,current,style),indention);
	}
	public static string Draw(this string current,Rect area,GUIStyle style=null,bool indention=false){
		style = style ?? EditorStyles.textField;
		return GUIExtension.Draw<string>(()=>EditorGUI.TextField(area,current,style),indention);
	}
	public static string DrawArea(this string current,Rect area,GUIStyle style=null,bool indention=false){
		style = style ?? EditorStyles.textField;
		return GUIExtension.Draw<string>(()=>EditorGUI.TextField(area,current,style),indention);
	}
	public static int DrawInt(this int current,Rect area,GUIStyle style=null,bool indention=false){
		style = style ?? EditorStyles.numberField;
		return GUIExtension.Draw<int>(()=>EditorGUI.IntField(area,current,style),indention);
	}
	public static int DrawSlider(this int current,Rect area,int min,int max,bool indention=false){
		return GUIExtension.Draw<int>(()=>EditorGUI.IntSlider(area,current,min,max),indention);
	}
	public static float Draw(this float current,Rect area,GUIStyle style=null,bool indention=false){
		style = style ?? EditorStyles.numberField;
		return GUIExtension.Draw<float>(()=>EditorGUI.FloatField(area,current,style),indention);
	}
	public static bool Draw(this bool current,Rect area,GUIStyle style=null,bool indention=false){
		style = style ?? EditorStyles.toggle;
		return GUIExtension.Draw<bool>(()=>EditorGUI.Toggle(area,current,style),indention);
	}
	public static GameObject DrawObject(this GameObject current,Rect area,bool allowScene=true,bool indention=false){
		return (GameObject)GUIExtension.Draw<UnityObject>(()=>EditorGUI.ObjectField(area,current,typeof(GameObject),allowScene),indention);
	}
	public static Enum Draw(this Enum current,Rect area,GUIStyle style=null,bool indention=false){
		style = style ?? EditorStyles.popup;
		return GUIExtension.Draw<Enum>(()=>EditorGUI.EnumPopup(area,current,style),indention);
	}
	public static Enum DrawMask(this Enum current,Rect area,GUIStyle style=null,bool indention=false){
		style = style ?? EditorStyles.popup;
		return GUIExtension.Draw<Enum>(()=>EditorGUI.EnumMaskField(area,current,style),indention);
	}
	public static int Draw(this string[] current,Rect area,int index,GUIStyle style=null,bool indention=false){
		style = style ?? EditorStyles.popup;
		return GUIExtension.Draw<int>(()=>EditorGUI.Popup(area,index,current,style),indention);
	}
	public static int Draw(this List<string> current,Rect area,int index,GUIStyle style=null,bool indention=false){
		return current.ToArray().Draw(area,index,style,indention);
	}
	public static void Draw(this SerializedProperty current,Rect area,string label="",bool allowScene=true,bool indention=false){
		GUIExtension.Draw(()=>EditorGUI.PropertyField(area,current,new GUIContent(label),allowScene),indention);
		//current.serializedObject.ApplyModifiedProperties();
	}
	public static void Draw(this SerializedProperty current,Rect area,GUIContent label,bool allowScene=true,bool indention=false){
		GUIExtension.Draw(()=>EditorGUI.PropertyField(area,current,label,allowScene),indention);
		//current.serializedObject.ApplyModifiedProperties();
	}
	public static Rect Draw(this Rect current,Rect area,bool indention=false){
		return GUIExtension.Draw<Rect>(()=>EditorGUI.RectField(area,current),indention);
	}
	public static Vector2 DrawVector2(this Vector2 current,Rect area,bool indention=false){
		return GUIExtension.Draw<Vector2>(()=>EditorGUI.Vector2Field(area,"",current),indention);
	}
	public static Vector3 DrawVector3(this Vector3 current,Rect area,bool indention=false){
		return GUIExtension.Draw<Vector3>(()=>EditorGUI.Vector3Field(area,"",current),indention);
	}
	public static Vector4 DrawVector4(this Vector4 current,Rect area,bool indention=false){
		return GUIExtension.Draw<Vector3>(()=>EditorGUI.Vector4Field(area,"",current),indention);
	}
	public static AnimationCurve Draw(this AnimationCurve current,Rect area,bool indention=false){
		return GUIExtension.Draw<AnimationCurve>(()=>EditorGUI.CurveField(area,current),indention);
	}
	public static Color Draw(this Color current,Rect area,bool indention=false){
		return GUIExtension.Draw<Color>(()=>EditorGUI.ColorField(area,current),indention);
	}
	public static Enum DrawLabeledMask(this Enum current,Rect area,GUIContent label,GUIStyle style=null,bool indention=true){
		style = style ?? EditorStyles.popup;
		return GUIExtension.Draw<Enum>(()=>EditorGUI.EnumMaskField(area,label,current,style),indention);
	}
	public static string DrawLabeled(this string current,Rect area,GUIContent label,GUIStyle style=null,bool indention=true){
		style = style ?? EditorStyles.textField;
		return GUIExtension.Draw<string>(()=>EditorGUI.TextField(area,label,current,style),indention);
	}
	public static int DrawLabeledInt(this int current,Rect area,GUIContent label,GUIStyle style=null,bool indention=true){
		style = style ?? EditorStyles.numberField;
		return GUIExtension.Draw<int>(()=>EditorGUI.IntField(area,label,current,style),indention);
	}
	public static float DrawLabeled(this float current,Rect area,GUIContent label,GUIStyle style=null,bool indention=true){
		style = style ?? EditorStyles.numberField;
		return GUIExtension.Draw<float>(()=>EditorGUI.FloatField(area,label,current,style),indention);
	}
	public static bool DrawLabeled(this bool current,Rect area,GUIContent label,GUIStyle style=null,bool indention=true){
		style = style ?? EditorStyles.toggle;
		return GUIExtension.Draw<bool>(()=>EditorGUI.Toggle(area,label,current,style),indention);
	}
	public static Vector3 DrawLabeled(this Vector3 current,Rect area,GUIContent label,bool indention=true){
		return GUIExtension.Draw<Vector3>(()=>EditorGUI.Vector3Field(area,label,current),indention);
	}
	public static GameObject DrawLabeledObject(this GameObject current,Rect area,GUIContent label,bool allowScene=true,bool indention=false){
		return (GameObject)GUIExtension.Draw<UnityObject>(()=>EditorGUI.ObjectField(area,label,current,typeof(GameObject),allowScene),indention);
	}
}