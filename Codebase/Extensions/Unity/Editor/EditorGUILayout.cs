using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios.UI{
	public static class EditorGUILayoutExtension{
		public static string Draw(this string current,UnityLabel label=null,GUIStyle style=null,bool indention=false){
			style = style ?? EditorStyles.textField;
			return EditorGUIExtension.Draw<string>(()=>EditorGUILayout.TextField(label,current,style,style.CreateLayout()),indention);
		}
		public static float Draw(this float current,UnityLabel label=null,GUIStyle style=null,bool indention=false){
			style = style ?? EditorStyles.numberField;
			return EditorGUIExtension.Draw<float>(()=>EditorGUILayout.FloatField(label,current,style,style.CreateLayout()),indention);
		}
		public static bool Draw(this bool current,UnityLabel label=null,GUIStyle style=null,bool indention=false){
			style = style ?? EditorStyles.toggle;
			return EditorGUIExtension.Draw<bool>(()=>EditorGUILayout.Toggle(label,current,style,style.CreateLayout()),indention);
		}
		public static Enum Draw(this Enum current,UnityLabel label=null,GUIStyle style=null,bool indention=false){
			style = style ?? EditorStyles.popup;
			return EditorGUIExtension.Draw<Enum>(()=>EditorGUILayout.EnumPopup(label,current,style,style.CreateLayout()),indention);
		}
		public static int Draw(this IList<string> current,int index,UnityLabel label=null,GUIStyle style=null,bool indention=false){
			style = style ?? EditorStyles.popup;
			return EditorGUIExtension.Draw<int>(()=>EditorGUILayout.Popup(label.ToString(),index,current.ToArray(),style),indention);
		}
		public static void Draw(this SerializedProperty current,UnityLabel label=null,bool allowScene=true,bool indention=false){
			if(label != null && label.value.text.IsEmpty()){label = new GUIContent(current.displayName);}
			Action action = ()=>EditorGUILayout.PropertyField(current,label,allowScene);
			EditorGUIExtension.Draw(action,indention);
		}
		public static Rect Draw(this Rect current,UnityLabel label=null,bool indention=false){
			return EditorGUIExtension.Draw<Rect>(()=>EditorGUILayout.RectField(label,current),indention);
		}
		public static AnimationCurve Draw(this AnimationCurve current,UnityLabel label=null,bool indention=false){
			return EditorGUIExtension.Draw<AnimationCurve>(()=>EditorGUILayout.CurveField(label,current),indention);
		}
		public static Color Draw(this Color current,UnityLabel label=null,bool indention=false){
			return EditorGUIExtension.Draw<Color>(()=>EditorGUILayout.ColorField(label,current),indention);
		}
		public static void Draw<T>(this IList<T> current,string header="List"){
			if(header.IsEmpty() || EditorGUILayoutExtensionSpecial.DrawFoldout(header)){
				if(!header.IsEmpty()){EditorGUI.indentLevel += 1;}
				for(int index=0;index<current.Count;++index){
					var item = current[index];
					string label = "#"+index;
					if(item is ICollection){
						var enumerable = item as IEnumerable;
						enumerable.OfType<object>().ToList().Draw(label);
						continue;
					}
					if(!item.IsNull() && item.GetType() != typeof(UnityObject) && item.GetType().IsClass && item.GetType().IsSerializable && !(item is string)){
						item.DrawFields(label);
						continue;
					}
					item.DrawAuto(label);
				}
				if(!header.IsEmpty()){EditorGUI.indentLevel -= 1;}
			}
		}
	}
	public static class EditorGUILayoutExtensionSpecial{
		public static void DrawAuto(this object current,UnityLabel label=null,GUIStyle style=null){
			if(current is string){current.As<string>().Draw(label,style);}
			if(current is int){current.As<int>().DrawInt(label);}
			if(current is float){current.As<float>().Draw(label,style);}
			if(current is Enum){current.As<Enum>().Draw(label,style);}
			if(current is SerializedProperty){current.As<SerializedProperty>().Draw(label);}
			if(current is AnimationCurve){current.As<AnimationCurve>().Draw(label);}
			if(current is Color){current.As<Color>().Draw(label);}
			if(current is Rect){current.As<Rect>().Draw(label);}
			if(current is GameObject){current.As<GameObject>().Draw<GameObject>(label);}
			if(current is Component){current.As<UnityObject>().Draw<Component>(label);}
			if(current is Material){current.As<UnityObject>().Draw<Material>(label);}
			if(current is Shader){current.As<UnityObject>().Draw<Shader>(label);}
			if(current is Vector2){current.As<Vector2>().DrawVector2(label);}
			if(current is Vector3){current.As<Vector3>().DrawVector3(label);}
			if(current is Vector4){current.As<Vector4>().DrawVector4(label);}
		}
		public static void DrawFields(this object current,string header="Fields"){
			if(header.IsEmpty() || EditorGUILayoutExtensionSpecial.DrawFoldout(header)){
				if(!header.IsEmpty()){EditorGUI.indentLevel += 1;}
				foreach(var item in current.GetVariables()){
					string label = item.Key.ToTitle();
					object field = item.Value;
					if(field is ICollection){
						var enumerable = field as IEnumerable;
						enumerable.OfType<object>().ToList().Draw(label);
						continue;
					}
					if(field.GetType().IsClass && field.GetType().IsSerializable && !(field is string)){
						field.DrawFields(label);
						continue;
					}
					string value = Convert.ToString(field);
					if(value != null){
						value.Draw(label);
					}
				}
				if(!header.IsEmpty()){EditorGUI.indentLevel -= 1;}
			}
		}
		//public static bool DrawFoldout(this string current,bool indention=false){return new UnityLabel(current).DrawFoldout(indention);}
		//public static bool DrawFoldout(this GUIContent current,bool indention=false){return new UnityLabel(current).DrawFoldout(indention);}
		public static bool DrawFoldout(this UnityLabel current,bool indention=false){
			string name = current + "Foldout";
			bool previous = EditorPrefs.GetBool(name);
			bool state = EditorGUIExtension.Draw<bool>(()=>EditorGUILayout.Foldout(previous,current),indention);
			if(previous != state){EditorPrefs.SetBool(name,state);}
			return state;
		}
		//public static void DrawLabel(this string current,GUIStyle style=null,bool indention=false){new UnityLabel(current).DrawLabel(style,indention);}
		//public static void DrawLabel(this GUIContent current,GUIStyle style=null,bool indention=false){new UnityLabel(current).DrawLabel(style,indention);}
		public static void DrawLabel(this UnityLabel current,GUIStyle style=null,bool indention=false){
			style = style ?? EditorStyles.label;
			if(indention){
				var options = new List<GUILayoutOption>();
				if(style.fixedWidth != 0){options.Add(GUILayout.Width(style.fixedWidth));}
				EditorGUIExtension.Draw(()=>EditorGUILayout.LabelField(current,style,options.ToArray()),indention);
				return;
			}
			EditorGUIExtension.Draw(()=>GUILayout.Label(current,style),indention);
		}
		public static void DrawHelp(this string current,string textType="Info",bool indention=false){
			MessageType type = MessageType.None;
			if(textType.Contains("Info",true)){type = MessageType.Info;}
			if(textType.Contains("Error",true)){type = MessageType.Error;}
			if(textType.Contains("Warning",true)){type = MessageType.Warning;}
			EditorGUIExtension.Draw(()=>EditorGUILayout.HelpBox(current,type),indention);
		}
		public static string DrawTextArea(this string current,UnityLabel label=null,GUIStyle style=null,bool indention=false){
			style = style ?? EditorStyles.textField;
			return EditorGUIExtension.Draw<string>(()=>EditorGUILayout.TextField(label,current,style),indention);
		}
		//public static bool DrawButton(this string current,GUIStyle style=null,bool indention=false){return new UnityLabel(current).DrawButton(style,indention);}
		//public static bool DrawButton(this GUIContent current,GUIStyle style=null,bool indention=false){return new UnityLabel(current).DrawButton(style,indention);}
		public static bool DrawButton(this UnityLabel current,GUIStyle style=null,bool indention=false){
			style = style ?? GUI.skin.button;
			return EditorGUIExtension.Draw<bool>(()=>GUILayout.Button(current,style),indention);
		}
		public static int DrawInt(this int current,UnityLabel label=null,GUIStyle style=null,bool indention=false){
			style = style ?? EditorStyles.numberField;
			return EditorGUIExtension.Draw<int>(()=>EditorGUILayout.IntField(label,current,style,style.CreateLayout()),indention);
		}
		public static int DrawSlider(this int current,int min,int max,UnityLabel label=null,bool indention=false){
			return EditorGUIExtension.Draw<int>(()=>EditorGUILayout.IntSlider(label,current,min,max),indention);
		}
		public static Type Draw<Type>(this UnityObject current,UnityLabel label=null,bool allowScene=true,bool indention=false) where Type : UnityObject{
			return (Type)EditorGUIExtension.Draw<UnityObject>(()=>EditorGUILayout.ObjectField(label,current,typeof(Type),allowScene),indention);
		}
		public static Enum DrawMask(this Enum current,UnityLabel label=null,GUIStyle style=null,bool indention=false){
			style = style ?? EditorStyles.popup;
			return EditorGUIExtension.Draw<Enum>(()=>EditorGUILayout.EnumMaskField(label,current,style,style.CreateLayout()),indention);
		}
		public static Vector2 DrawVector2(this Vector2 current,UnityLabel label=null,bool indention=false){
			return EditorGUIExtension.Draw<Vector2>(()=>EditorGUILayout.Vector2Field(label,current),indention);
		}
		public static Vector3 DrawVector3(this Vector3 current,UnityLabel label=null,bool indention=false){
			return EditorGUIExtension.Draw<Vector3>(()=>EditorGUILayout.Vector3Field(label,current),indention);
		}
		public static Vector4 DrawVector4(this Vector4 current,UnityLabel label=null,bool indention=false){
			return EditorGUIExtension.Draw<Vector4>(()=>EditorGUILayout.Vector4Field(label.ToString(),current),indention);
		}
	}
}