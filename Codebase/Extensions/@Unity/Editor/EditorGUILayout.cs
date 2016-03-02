using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios.Interface{
	public static class EditorGUILayoutExtension{
		public static string Draw(this string current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.textField;
			return EditorGUIExtension.Draw<string>(()=>EditorGUILayout.TextField(label,current,style,style.CreateLayout()),indention);
		}
		public static float Draw(this float current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.numberField;
			return EditorGUIExtension.Draw<float>(()=>EditorGUILayout.FloatField(label,current,style,style.CreateLayout()),indention);
		}
		public static bool Draw(this bool current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.toggle;
			return EditorGUIExtension.Draw<bool>(()=>EditorGUILayout.Toggle(label,current,style,style.CreateLayout()),indention);
		}
		public static Enum Draw(this Enum current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.popup;
			return EditorGUIExtension.Draw<Enum>(()=>EditorGUILayout.EnumPopup(label,current,style,style.CreateLayout()),indention);
		}
		public static int Draw(this IList<string> current,int index,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.popup;
			return EditorGUIExtension.Draw<int>(()=>EditorGUILayout.Popup(label.ToString(),index,current.ToArray(),style),indention);
		}
		public static void Draw(this SerializedProperty current,UnityLabel label=null,bool allowScene=true,bool indention=true){
			if(label != null && label.value.text.IsEmpty()){label = new GUIContent(current.displayName);}
			Action action = ()=>EditorGUILayout.PropertyField(current,label,allowScene);
			EditorGUIExtension.Draw(action,indention);
		}
		public static Rect Draw(this Rect current,UnityLabel label=null,bool indention=true){
			return EditorGUIExtension.Draw<Rect>(()=>EditorGUILayout.RectField(label,current),indention);
		}
		public static AnimationCurve Draw(this AnimationCurve current,UnityLabel label=null,bool indention=true){
			return EditorGUIExtension.Draw<AnimationCurve>(()=>EditorGUILayout.CurveField(label,current),indention);
		}
		public static Color Draw(this Color current,UnityLabel label=null,bool indention=true){
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
		public static void Draw(this IDictionary current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			var open = EditorGUILayoutExtensionSpecial.DrawFoldout(label.ToString().ToTitleCase());
			if(!open){return;}
			EditorGUI.indentLevel += 1;
			foreach(DictionaryEntry item in current){
				item.Value.DrawAuto(item.Key.ToString(),style,true);
			}
			EditorGUI.indentLevel -= 1;
		}
	}
	public static class EditorGUILayoutExtensionSpecial{
		public static void DrawAuto(this object current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			bool isDictionary = current.GetType().IsGenericType && current.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>);
			if(current is string){current.As<string>().Draw(label,style,indention);}
			if(current is int){current.As<int>().DrawInt(label,style,indention);}
			if(current is float){current.As<float>().Draw(label,style,indention);}
			if(current is Enum){current.As<Enum>().Draw(label,style,indention);}
			if(current is SerializedProperty){current.As<SerializedProperty>().Draw(label,indention);}
			if(current is AnimationCurve){current.As<AnimationCurve>().Draw(label,indention);}
			if(current is Color){current.As<Color>().Draw(label,indention);}
			if(current is Rect){current.As<Rect>().Draw(label,indention);}
			if(current is GameObject){current.As<GameObject>().Draw<GameObject>(label,indention);}
			if(current is Component){current.As<UnityObject>().Draw<Component>(label,indention);}
			if(current is Material){current.As<UnityObject>().Draw<Material>(label,indention);}
			if(current is Shader){current.As<UnityObject>().Draw<Shader>(label,indention);}
			if(current is Vector2){current.As<Vector2>().DrawVector2(label,indention);}
			if(current is Vector3){current.As<Vector3>().DrawVector3(label,indention);}
			if(current is Vector4){current.As<Vector4>().DrawVector4(label,indention);}
			if(isDictionary){current.As<IDictionary>().Draw(label,style,indention);}
		}
		public static void DrawFields(this object current,string header="Fields"){
			if(header.IsEmpty() || EditorGUILayoutExtensionSpecial.DrawFoldout(header)){
				if(!header.IsEmpty()){EditorGUI.indentLevel += 1;}
				foreach(var item in current.GetVariables()){
					string label = item.Key.ToTitleCase();
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
		//public static bool DrawFoldout(this string current,bool indention=true){return new UnityLabel(current).DrawFoldout(indention);}
		//public static bool DrawFoldout(this GUIContent current,bool indention=true){return new UnityLabel(current).DrawFoldout(indention);}
		public static bool DrawFoldout(this UnityLabel current,object key=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.foldout;
			string name = key.IsNull() ? current + "Foldout" : key.GetHashCode().ToString();
			if(key is string){name = (string)key;}
			bool previous = EditorPrefs.GetBool(name);
			bool state = EditorGUIExtension.Draw<bool>(()=>EditorGUILayout.Foldout(previous,current,style),indention);
			if(previous != state){EditorPrefs.SetBool(name,state);}
			return state;
		}
		//public static void DrawLabel(this string current,GUIStyle style=null,bool indention=true){new UnityLabel(current).DrawLabel(style,indention);}
		//public static void DrawLabel(this GUIContent current,GUIStyle style=null,bool indention=true){new UnityLabel(current).DrawLabel(style,indention);}
		public static void DrawLabel(this UnityLabel current,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.label;
			if(indention){
				var options = new List<GUILayoutOption>();
				if(style.fixedWidth != 0){options.Add(GUILayout.Width(style.fixedWidth));}
				EditorGUIExtension.Draw(()=>EditorGUILayout.LabelField(current,style,options.ToArray()),indention);
				return;
			}
			EditorGUIExtension.Draw(()=>GUILayout.Label(current,style),indention);
		}
		public static void DrawHelp(this string current,string textType="Info",bool indention=true){
			MessageType type = MessageType.None;
			if(textType.Contains("Info",true)){type = MessageType.Info;}
			if(textType.Contains("Error",true)){type = MessageType.Error;}
			if(textType.Contains("Warning",true)){type = MessageType.Warning;}
			EditorGUIExtension.Draw(()=>EditorGUILayout.HelpBox(current,type),indention);
		}
		public static string DrawTextArea(this string current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.textField;
			return EditorGUIExtension.Draw<string>(()=>EditorGUILayout.TextField(label,current,style),indention);
		}
		//public static bool DrawButton(this string current,GUIStyle style=null,bool indention=true){return new UnityLabel(current).DrawButton(style,indention);}
		//public static bool DrawButton(this GUIContent current,GUIStyle style=null,bool indention=true){return new UnityLabel(current).DrawButton(style,indention);}
		public static bool DrawButton(this UnityLabel current,GUIStyle style=null,bool indention=true){
			style = style ?? GUI.skin.button;
			return EditorGUIExtension.Draw<bool>(()=>GUILayout.Button(current,style),indention);
		}
		public static int DrawInt(this int current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.numberField;
			return EditorGUIExtension.Draw<int>(()=>EditorGUILayout.IntField(label,current,style,style.CreateLayout()),indention);
		}
		public static int DrawSlider(this int current,int min,int max,UnityLabel label=null,bool indention=true){
			return EditorGUIExtension.Draw<int>(()=>EditorGUILayout.IntSlider(label,current,min,max),indention);
		}
		public static Type Draw<Type>(this UnityObject current,UnityLabel label=null,bool allowScene=true,bool indention=true) where Type : UnityObject{
			return (Type)EditorGUIExtension.Draw<UnityObject>(()=>EditorGUILayout.ObjectField(label,current,typeof(Type),allowScene),indention);
		}
		public static Enum DrawMask(this Enum current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.popup;
			return EditorGUIExtension.Draw<Enum>(()=>EditorGUILayout.EnumMaskField(label,current,style,style.CreateLayout()),indention);
		}
		public static Vector2 DrawVector2(this Vector2 current,UnityLabel label=null,bool indention=true){
			return EditorGUIExtension.Draw<Vector2>(()=>EditorGUILayout.Vector2Field(label,current),indention);
		}
		public static Vector3 DrawVector3(this Vector3 current,UnityLabel label=null,bool indention=true){
			return EditorGUIExtension.Draw<Vector3>(()=>EditorGUILayout.Vector3Field(label,current),indention);
		}
		public static Vector4 DrawVector4(this Vector4 current,UnityLabel label=null,bool indention=true){
			return EditorGUIExtension.Draw<Vector4>(()=>EditorGUILayout.Vector4Field(label.ToString(),current),indention);
		}
	}
}