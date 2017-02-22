#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios.Interface{
	//============================
	// Core
	//============================
	public static partial class EditorUI{
		public static bool allowIndention = true;
		public static Type Draw<Type>(Func<Type> method,bool indention=true){
			int indentValue = EditorGUI.indentLevel;
			indention = EditorUI.allowIndention && indention;
			if(indention && EditorUI.space!=0){GUILayout.Space(EditorUI.space);}
			if(!indention){EditorGUI.indentLevel = 0;}
			bool wasChanged = GUI.changed;
			GUI.changed = false;
			Type value = (Type)method();
			EditorUI.lastChanged = GUI.changed;
			EditorUI.anyChanged = GUI.changed = GUI.changed || wasChanged;
			EditorGUI.indentLevel = indentValue;
			if(EditorUI.resetField){EditorUI.SetFieldSize(EditorUI.resetFieldSize,false);}
			if(EditorUI.resetLayout){EditorUI.ResetLayout();}
			return value;
		}
		public static void Draw(Action method,bool indention=true){
			int indentValue = EditorGUI.indentLevel;
			if(!indention){EditorGUI.indentLevel = 0;}
			if(EditorUI.render){method();}
			if(!indention){EditorGUI.indentLevel = indentValue;}
		}
	}
	//============================
	// Proxy
	//============================
	public static partial class EditorUI{
		public static string Draw(this string current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.textField;
			return EditorUI.Draw<string>(()=>EditorGUI.TextField(area,label,current,style),indention);
		}
		public static float Draw(this float current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.numberField;
			return EditorUI.Draw<float>(()=>EditorGUI.FloatField(area,label,current,style),indention);
		}
		public static bool Draw(this bool current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.toggle;
			return EditorUI.Draw<bool>(()=>EditorGUI.Toggle(area,label,current,style),indention);
		}
		public static Enum Draw(this Enum current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.popup;
			return EditorUI.Draw<Enum>(()=>EditorGUI.EnumPopup(area,label,current,style),indention);
		}
		public static int Draw(this IEnumerable<string> current,Rect area,int index,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.popup;
			string name = label.IsNull() ? "" : label.ToString();
			return EditorUI.Draw<int>(()=>EditorGUI.Popup(area,name,index,current.ToArray(),style),indention);
		}
		public static void Draw(this SerializedProperty current,Rect area,UnityLabel label=null,bool allowScene=true,bool indention=true){
			if(label != null && label.value.text.IsEmpty()){label = new GUIContent(current.displayName);}
			EditorUI.Draw(()=> EditorGUI.PropertyField(area,current,label,allowScene),indention);
		}
		public static Rect Draw(this Rect current,Rect area,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw<Rect>(()=> EditorGUI.RectField(area,label,current),indention);
		}
		public static AnimationCurve Draw(this AnimationCurve current,Rect area,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw<AnimationCurve>(()=> EditorGUI.CurveField(area,label,current),indention);
		}
		public static Color Draw(this Color current,Rect area,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw<Color>(()=> EditorGUI.ColorField(area,label,current),indention);
		}
		public static void DrawLabel(this UnityLabel current,Rect area,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.label;
			EditorUI.Draw(()=> EditorGUI.LabelField(area,current,style),indention);
		}
		public static void DrawHelp(this string current,Rect area,string textType,bool indention=true){
			MessageType type = MessageType.None;
			if(textType.Contains("Info",true)){type = MessageType.Info;}
			if(textType.Contains("Error",true)){type = MessageType.Error;}
			if(textType.Contains("Warning",true)){type = MessageType.Warning;}
			EditorUI.Draw(()=> EditorGUI.HelpBox(area,current,type),indention);
		}
		public static string DrawTextArea(this string current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.textField;
			return EditorUI.Draw<string>(()=> EditorGUI.TextField(area,label,current,style),indention);
		}
		public static bool DrawButton(this UnityLabel current,Rect area,GUIStyle style=null,bool indention=true){
			style = style ?? GUI.skin.button;
			return EditorUI.Draw<bool>(()=> GUI.Button(area,current,style),indention);
		}
		public static int DrawInt(this int current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.numberField;
			return EditorUI.Draw<int>(()=> EditorGUI.IntField(area,label,current,style),indention);
		}
		public static float DrawSlider(this float current,Rect area,float min,float max,bool indention=true){
			var value = EditorUI.Draw<float>(()=> GUI.HorizontalSlider(area,current,min,max),indention);
			if(value != current){
				GUI.FocusControl("");}
			return value;
		}
		public static Enum DrawMaskField(this Enum current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.popup;
			return EditorUI.Draw<Enum>(()=> EditorGUI.EnumMaskField(area,label,current,style),indention);
		}
		public static Vector2 DrawVector2(this Vector2 current,Rect area,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw<Vector2>(()=> EditorGUI.Vector2Field(area,label,current),indention);
		}
		public static Vector3 DrawVector3(this Vector3 current,Rect area,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw<Vector3>(()=> EditorGUI.Vector3Field(area,label,current),indention);
		}
		public static Vector4 DrawVector4(this Vector4 current,Rect area,UnityLabel label=null,bool indention=true){
			string name = label.IsNull() ? null : label.ToString();
			return EditorUI.Draw<Vector3>(()=> EditorGUI.Vector4Field(area,name,current),indention);
		}
		public static Type Draw<Type>(this UnityObject current,Rect area,UnityLabel label=null,bool allowScene=true,bool indention=true) where Type : UnityObject{
			return (Type)EditorUI.Draw<UnityObject>(()=> EditorGUI.ObjectField(area,label,current,typeof(Type),allowScene),indention);
		}
	}
	public static partial class EditorUI{
		public static Rect menuArea;
		public static object menuValue;
		public static void DrawAuto(this object current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			if(current is string){current.As<string>().Draw(area,label,style,indention);}
			if(current is int){current.As<int>().DrawInt(area,label,style,indention);}
			if(current is float){current.As<float>().Draw(area,label,style,indention);}
			if(current is Enum){current.As<Enum>().Draw(area,label,style,indention);}
			if(current is SerializedProperty){current.As<SerializedProperty>().Draw(area,label,indention);}
			if(current is AnimationCurve){current.As<AnimationCurve>().Draw(area,label,indention);}
			if(current is Color){current.As<Color>().Draw(area,label,indention);}
			if(current is Rect){current.As<Rect>().Draw(area,label,indention);}
			if(current is GameObject){current.As<GameObject>().Draw<GameObject>(area,label,indention);}
			if(current is Component){current.As<UnityObject>().Draw<Component>(area,label,indention);}
			if(current is Material){current.As<UnityObject>().Draw<Material>(area,label,indention);}
			if(current is Shader){current.As<UnityObject>().Draw<Shader>(area,label,indention);}
			if(current is Vector2){current.As<Vector2>().DrawVector2(area,label,indention);}
			if(current is Vector3){current.As<Vector3>().DrawVector3(area,label,indention);}
			if(current is Vector4){current.As<Vector4>().DrawVector4(area,label,indention);}
		}
		public static bool DrawFoldout(this UnityLabel current,Rect area,object key,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.foldout;
			string name = key.IsNull() ? current + "Foldout" : key.GetHashCode().ToString();
			if(key is string){name = (string)key;}
			bool previous = Utility.GetPref<bool>(name);
			bool state = EditorUI.Draw<bool>(()=> EditorGUI.Foldout(area,previous,current,style),indention);
			if(previous != state){Utility.SetPref<bool>(name,state);}
			return state;
		}
		public static bool DrawHeader(this UnityLabel current,Rect area,object key,GUIStyle style=null,bool indention=true){
			string stateName = key.IsNull() ? current + "Foldout" : key.GetHashCode().ToString();
			if(key is string){stateName = (string)key;}
			bool state = Utility.GetPref<bool>(stateName);
			current = state ? "▼ " + current : "▶ " + current;
			var currentStyle = style.IsNull() ? null : new GUIStyle(style);
			if(state){currentStyle.normal = currentStyle.active;}
			if(current.DrawButton(area,currentStyle,indention)){
				state = !state;
				Utility.SetPref<bool>(stateName,state);
			}
			return state;
		}
		public static Enum DrawMask(this Enum current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.popup;
			string value = current.ToName().Replace(" "," | ").ToTitleCase();
			Rect valueArea = area;
			if(!label.IsNull()){
				Rect labelArea = area.AddWidth(-EditorGUIUtility.labelWidth);
				valueArea = labelArea.AddX(EditorGUIUtility.labelWidth);
				if(value.IsEmpty()){value = "None";}
				label.DrawLabel(labelArea,null,true);
			}
			if(GUI.Button(valueArea,value.Trim("| "),style)){
				var items = current.ToName().Split(" ").ToTitleCase();
				GenericMenu.MenuFunction2 callback = index=>{
					EditorUI.menuArea = area;
					EditorUI.menuValue = current.GetValues().GetValue((int)index);
				};
				current.GetNames().ToTitleCase().DrawMenu(valueArea,callback,items);
			}
			if(EditorUI.menuArea == area && !EditorUI.menuValue.IsNull()){
				var menuValue = (Enum)EditorUI.menuValue;
				var newValue = current.ToInt() ^ menuValue.ToInt();
				current = (Enum)Enum.ToObject(current.GetType(),newValue);
				EditorUI.menuValue = null;
				EditorUI.menuArea = new Rect();
				GUI.changed = true;
			}
			return current;
		}
	}
}
#endif