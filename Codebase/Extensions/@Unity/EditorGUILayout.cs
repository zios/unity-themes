#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using Class = Zios.Interface.EditorUI;
namespace Zios.Interface{
	public static partial class EditorUI{
		public static float space = 0;
		public static float width = 0;
		public static float height = 0;
		public static float minWidth = 0;
		public static float minHeight = 0;
		public static float maxWidth = 0;
		public static float maxHeight = 0;
		public static bool? autoWidth = null;
		public static bool? autoHeight = null;
		public static bool autoLayout = true;
		public static bool autoLayoutReset = false;
		public static void SetLayout(float width,float height,float maxWidth,float maxHeight,float minWidth,float minHeight,bool autoWidth,bool autoHeight){
			EditorUI.SetLayout(width,height,maxWidth,maxHeight,minWidth,minHeight,(bool?)autoWidth,(bool?)autoHeight);
		}
		public static void SetLayout(float width=0,float height=0,float maxWidth=0,float maxHeight=0,float minWidth=0,float minHeight=0,bool? autoWidth=null,bool? autoHeight=null){
			EditorUI.width = width;
			EditorUI.height = height;
			EditorUI.maxWidth = maxWidth;
			EditorUI.maxHeight = maxHeight;
			EditorUI.minWidth = minWidth;
			EditorUI.minHeight = minHeight;
			EditorUI.autoWidth = autoWidth;
			EditorUI.autoHeight = autoHeight;
		}
		public static GUILayoutOption[] CreateLayout(){
			var options = new List<GUILayoutOption>();
			if(EditorUI.autoLayout){
				if(EditorUI.width != 0){options.Add(GUILayout.Width(EditorUI.width));}
				if(EditorUI.height != 0){options.Add(GUILayout.Height(EditorUI.height));}
				if(EditorUI.maxWidth != 0){options.Add(GUILayout.MaxWidth(EditorUI.maxWidth));}
				if(EditorUI.maxHeight != 0){options.Add(GUILayout.MaxHeight(EditorUI.maxHeight));}
				if(EditorUI.minWidth != 0){options.Add(GUILayout.MinWidth(EditorUI.minWidth));}
				if(EditorUI.minHeight != 0){options.Add(GUILayout.MinHeight(EditorUI.minHeight));}
				if(EditorUI.autoWidth != null){options.Add(GUILayout.ExpandWidth(EditorUI.autoWidth.As<bool>()));}
				if(EditorUI.autoHeight != null){options.Add(GUILayout.ExpandHeight(EditorUI.autoHeight.As<bool>()));}
			}
			if(EditorUI.autoLayoutReset){
				EditorUI.width = 0;
				EditorUI.height = 0;
				EditorUI.maxWidth = 0;
				EditorUI.maxHeight = 0;
				EditorUI.minWidth = 0;
				EditorUI.minHeight = 0;
				EditorUI.autoWidth = null;
				EditorUI.autoHeight = null;
			}
			return options.ToArray();
		}
		public static string Draw(this string current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.textField;
			var layout = !style.IsNull() ? style.CreateLayout() : Class.CreateLayout();
			return EditorUI.Draw<string>(()=>EditorGUILayout.TextField(label,current,style,layout),indention);
		}
		public static float Draw(this float current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.numberField;
			var layout = !style.IsNull() ? style.CreateLayout() : Class.CreateLayout();
			return EditorUI.Draw<float>(()=>EditorGUILayout.FloatField(label,current,style,layout),indention);
		}
		public static bool Draw(this bool current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.toggle;
			var layout = !style.IsNull() ? style.CreateLayout() : Class.CreateLayout();
			return EditorUI.Draw<bool>(()=>EditorGUILayout.Toggle(label,current,style,layout),indention);
		}
		public static Enum Draw(this Enum current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.popup;
			var layout = !style.IsNull() ? style.CreateLayout() : Class.CreateLayout();
			return EditorUI.Draw<Enum>(()=>EditorGUILayout.EnumPopup(label,current,style,layout),indention);
		}
		public static int Draw(this IEnumerable<string> current,int index,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.popup;
			var labelValue = label.IsNull() ? null : label.ToString();
			return EditorUI.Draw<int>(()=>EditorGUILayout.Popup(labelValue,index,current.ToArray(),style),indention);
		}
		public static void Draw(this SerializedProperty current,UnityLabel label=null,bool allowScene=true,bool indention=true){
			if(label != null && label.value.text.IsEmpty()){label = new GUIContent(current.displayName);}
			Action action = ()=>EditorGUILayout.PropertyField(current,label,allowScene,(GUILayoutOption[])EditorUI.CreateLayout());
			EditorUI.Draw(action,indention);
		}
		public static Rect Draw(this Rect current,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw<Rect>(()=>EditorGUILayout.RectField(label,current,(GUILayoutOption[])EditorUI.CreateLayout()),indention);
		}
		public static AnimationCurve Draw(this AnimationCurve current,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw<AnimationCurve>(()=>EditorGUILayout.CurveField(label,current,(GUILayoutOption[])EditorUI.CreateLayout()),indention);
		}
		public static Color Draw(this Color current,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw<Color>(()=>EditorGUILayout.ColorField(label,current,(GUILayoutOption[])EditorUI.CreateLayout()),indention);
		}
		public static void Draw<T>(this IList<T> current,string header="List"){
			if(header.IsEmpty() || EditorUI.DrawFoldout(header)){
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
			var open = EditorUI.DrawFoldout(label.ToString().ToTitleCase(),Class.CreateLayout());
			if(!open){return;}
			EditorGUI.indentLevel += 1;
			foreach(DictionaryEntry item in current){
				item.Value.DrawAuto(item.Key.ToString(),style,true);
			}
			EditorGUI.indentLevel -= 1;
		}
	}
	public static partial class EditorUI{
		public static string DrawDelayed(this string current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.textField;
			var layout = !style.IsNull() ? style.CreateLayout() : Class.CreateLayout();
			#if UNITY_5_5_OR_NEWER
			return EditorUI.Draw<string>(()=>EditorGUILayout.DelayedTextField(label,current,style,layout),indention);
			#else
			return EditorUI.Draw<string>(()=>EditorGUILayout.TextField(label,current,style,layout),indention);
			#endif
		}
		public static float DrawDelayed(this float current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.numberField;
			var layout = !style.IsNull() ? style.CreateLayout() : Class.CreateLayout();
			#if UNITY_5_5_OR_NEWER
			return EditorUI.Draw<float>(()=>EditorGUILayout.DelayedFloatField(label,current,style,layout),indention);
			#else
			return EditorUI.Draw<float>(()=>EditorGUILayout.FloatField(label,current,style,layout),indention);
			#endif
		}
		public static int DrawIntDelayed(this int current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.numberField;
			var layout = !style.IsNull() ? style.CreateLayout() : Class.CreateLayout();
			#if UNITY_5_5_OR_NEWER
			return EditorUI.Draw<int>(()=>EditorGUILayout.DelayedIntField(label,current,style,layout),indention);
			#else
			return EditorUI.Draw<int>(()=>EditorGUILayout.IntField(label,current,style,layout),indention);
			#endif
		}
	}
	public static partial class EditorUI{
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
			if(current is Texture){current.As<UnityObject>().Draw<Texture>(label,indention);}
			if(current is Material){current.As<UnityObject>().Draw<Material>(label,indention);}
			if(current is Shader){current.As<UnityObject>().Draw<Shader>(label,indention);}
			if(current is Vector2){current.As<Vector2>().DrawVector2(label,indention);}
			if(current is Vector3){current.As<Vector3>().DrawVector3(label,indention);}
			if(current is Vector4){current.As<Vector4>().DrawVector4(label,indention);}
			if(isDictionary){current.As<IDictionary>().Draw(label,style,indention);}
		}
		public static void DrawFields(this object current,string header="Fields"){
			if(header.IsEmpty() || EditorUI.DrawFoldout(header)){
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
		public static bool DrawFoldout(this UnityLabel current,object key=null,GUIStyle style=null,bool indention=true){
			var lastState = GUI.changed;
			style = style ?? EditorStyles.foldout;
			string name = key.IsNull() ? current + "Header" : key.GetHashCode().ToString();
			if(key is string){name = (string)key;}
			bool previous = Utility.GetPref<bool>(name);
			#if UNITY_5_5_OR_NEWER
			bool state = EditorUI.Draw<bool>(()=>EditorGUILayout.Foldout(previous,current,true,style),indention);
			#else
			bool state = EditorUI.Draw<bool>(()=>EditorGUILayout.Foldout(previous,current,style),indention);
			#endif
			if(previous != state){
				Utility.SetPref<bool>(name,state);
				EditorUI.foldoutChanged = true;
				GUI.FocusControl("");
			}
			GUI.changed = lastState;
			return state;
		}
		public static bool DrawHeader(this UnityLabel current,object key=null,GUIStyle style=null,bool editable=false,Action callback=null,bool indention=true){
			string stateName = key.IsNull() ? current + "Header" : key.GetHashCode().ToString();
			if(key is string){stateName = (string)key;}
			bool state = Utility.GetPref<bool>(stateName);
			//current = state ? "▼ " + current : "▶ " + current;
			var fallback = editable ? EditorStyles.textField : EditorStyles.label;
			var currentStyle = style.IsNull() ? fallback.Copy() : new GUIStyle(style);
			if(state){currentStyle.normal = currentStyle.onNormal;}
			if(!editable){
				if(current.DrawButton(currentStyle,indention)){
					state = !state;
					Utility.SetPref<bool>(stateName,state);
					if(!callback.IsNull()){callback();}
				}
			}
			else{
				current.value.text = current.value.text.Draw(null,currentStyle,indention);
			}
			return state;
		}
		public static void DrawLabel(this UnityLabel current,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.label;
			var layout = !style.IsNull() ? style.CreateLayout() : Class.CreateLayout();
			if(indention){
				EditorUI.Draw(()=>EditorGUILayout.LabelField(current,style,layout),indention);
				return;
			}
			EditorUI.Draw(()=>GUILayout.Label(current,style,layout),indention);
		}
		public static void DrawHelp(this string current,string textType="Info",bool indention=true){
			MessageType type = MessageType.None;
			if(textType.Contains("Info",true)){type = MessageType.Info;}
			if(textType.Contains("Error",true)){type = MessageType.Error;}
			if(textType.Contains("Warning",true)){type = MessageType.Warning;}
			EditorUI.Draw(()=>EditorGUILayout.HelpBox(current,type),indention);
		}
		public static string DrawTextArea(this string current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.textField;
			return EditorUI.Draw<string>(()=>EditorGUILayout.TextField(label,current,style,(GUILayoutOption[])EditorUI.CreateLayout()),indention);
		}
		public static bool DrawButton(this UnityLabel current,GUIStyle style=null,bool indention=true){
			style = style ?? GUI.skin.button;
			return EditorUI.Draw<bool>(()=>GUILayout.Button(current,style,Class.CreateLayout()),indention);
		}
		public static int DrawInt(this int current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.numberField;
			var layout = !style.IsNull() ? style.CreateLayout() : Class.CreateLayout();
			return EditorUI.Draw<int>(()=>EditorGUILayout.IntField(label,current,style,layout),indention);
		}
		public static int DrawSlider(this int current,int min,int max,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw<int>(()=>EditorGUILayout.IntSlider(label,current,min,max,(GUILayoutOption[])EditorUI.CreateLayout()),indention);
		}
		public static float DrawSlider(this float current,float min,float max,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw<float>(()=>EditorGUILayout.Slider(label,current,min,max,(GUILayoutOption[])EditorUI.CreateLayout()),indention);
		}
		public static Type Draw<Type>(this UnityObject current,UnityLabel label=null,bool allowScene=true,bool indention=true) where Type : UnityObject{
			return (Type)EditorUI.Draw<UnityObject>(()=>EditorGUILayout.ObjectField(label,current,typeof(Type),allowScene,(GUILayoutOption[])EditorUI.CreateLayout()),indention);
		}
		public static Enum DrawMask(this Enum current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.popup;
			var layout = !style.IsNull() ? style.CreateLayout() : Class.CreateLayout();
			return EditorUI.Draw<Enum>(()=>EditorGUILayout.EnumMaskField(label,current,style,layout),indention);
		}
		public static Vector2 DrawVector2(this Vector2 current,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw<Vector2>(()=>EditorGUILayout.Vector2Field(label,current,(GUILayoutOption[])EditorUI.CreateLayout()),indention);
		}
		public static Vector3 DrawVector3(this Vector3 current,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw<Vector3>(()=>EditorGUILayout.Vector3Field(label,current,(GUILayoutOption[])EditorUI.CreateLayout()),indention);
		}
		public static Vector4 DrawVector4(this Vector4 current,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw<Vector4>(()=>EditorGUILayout.Vector4Field(label.ToString(),current,(GUILayoutOption[])EditorUI.CreateLayout()),indention);
		}
	}
}
#endif