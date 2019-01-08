#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios.Unity.EditorUI{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Reflection;
	using Zios.Unity.Extensions;
	using Zios.Unity.Extensions.Convert;
	using Zios.Unity.Pref;
	//============================
	// Proxy
	//============================
	public static partial class EditorUI{
		public static string Draw(this string current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.textField;
			return EditorUI.Draw<string>(()=>EditorGUI.TextField(area,label,current,style),indention,area);
		}
		public static float Draw(this float current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.numberField;
			return EditorUI.Draw<float>(()=>EditorGUI.FloatField(area,label,current,style),indention,area);
		}
		public static bool Draw(this bool current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.toggle;
			return EditorUI.Draw<bool>(()=>EditorGUI.Toggle(area,label,current,style),indention,area);
		}
		public static Enum Draw(this Enum current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.popup;
			return EditorUI.Draw<Enum>(()=>EditorGUI.EnumPopup(area,label,current,style),indention,area);
		}
		public static int Draw(this IEnumerable<string> current,Rect area,int index,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.popup;
			string name = label.IsNull() ? "" : label.ToString();
			return EditorUI.Draw<int>(()=>EditorGUI.Popup(area,name,index,current.ToArray(),style),indention,area);
		}
		public static void Draw(this SerializedProperty current,Rect area,UnityLabel label=null,bool allowScene=true,bool indention=true){
			if(label != null && label.value.text.IsEmpty()){label = new GUIContent(current.displayName);}
			EditorUI.Draw(()=>EditorGUI.PropertyField(area,current,label,allowScene),indention,area);
		}
		public static Rect Draw(this Rect current,Rect area,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw<Rect>(()=>EditorGUI.RectField(area,label,current),indention,area);
		}
		public static AnimationCurve Draw(this AnimationCurve current,Rect area,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw<AnimationCurve>(()=>EditorGUI.CurveField(area,label,current),indention,area);
		}
		public static Color Draw(this Color current,Rect area,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw<Color>(()=>EditorGUI.ColorField(area,label,current),indention,area);
		}
		public static void DrawLabel(this UnityLabel current,Rect area,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.label;
			EditorUI.Draw(()=>EditorGUI.LabelField(area,current,style),indention,area);
		}
		public static void DrawHelp(this string current,Rect area,string textType,bool indention=true){
			MessageType type = MessageType.None;
			if(textType.Contains("Info",true)){type = MessageType.Info;}
			if(textType.Contains("Error",true)){type = MessageType.Error;}
			if(textType.Contains("Warning",true)){type = MessageType.Warning;}
			EditorUI.Draw(()=>EditorGUI.HelpBox(area,current,type),indention,area);
		}
		public static string DrawTextArea(this string current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.textField;
			return EditorUI.Draw<string>(()=>EditorGUI.TextField(area,label,current,style),indention,area);
		}
		public static bool DrawButton(this UnityLabel current,Rect area,GUIStyle style=null,bool indention=true){
			style = style ?? GUI.skin.button;
			return EditorUI.Draw<bool>(()=>GUI.Button(area,current,style),indention,area);
		}
		public static int DrawInt(this int current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.numberField;
			return EditorUI.Draw<int>(()=>EditorGUI.IntField(area,label,current,style),indention,area);
		}
		public static float DrawSlider(this float current,Rect area,float min,float max,bool indention=true){
			var value = EditorUI.Draw<float>(()=>GUI.HorizontalSlider(area,current,min,max),indention,area);
			if(value != current){GUI.FocusControl("");}
			return value;
		}
		public static Enum DrawMaskField(this Enum current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.popup;
			#if UNITY_2017_3_OR_NEWER
			return EditorUI.Draw<Enum>(()=>EditorGUI.EnumFlagsField(area,label,current,style),indention,area);
			#else
			return EditorUI.Draw<Enum>(()=>EditorGUI.EnumMaskField(area,label,current,style),indention,area);
			#endif
		}
		public static Vector2 DrawVector2(this Vector2 current,Rect area,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw<Vector2>(()=>EditorGUI.Vector2Field(area,label,current),indention,area);
		}
		public static Vector3 DrawVector3(this Vector3 current,Rect area,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw<Vector3>(()=>EditorGUI.Vector3Field(area,label,current),indention,area);
		}
		public static Vector4 DrawVector4(this Vector4 current,Rect area,UnityLabel label=null,bool indention=true){
			string name = label.IsNull() ? null : label.ToString();
			return EditorUI.Draw<Vector3>(()=>EditorGUI.Vector4Field(area,name,current),indention,area);
		}
		public static Type Draw<Type>(this UnityObject current,Rect area,UnityLabel label=null,bool allowScene=true,bool indention=true) where Type : UnityObject{
			return (Type)EditorUI.Draw<UnityObject>(()=>EditorGUI.ObjectField(area,label,current,typeof(Type),allowScene),indention,area);
		}
	}
	//============================
	// Special
	//============================
	public static partial class EditorUI{
		public static Rect lastRect;
		public static Rect menuArea;
		public static object menuValue;
		public static Type Draw<Type>(Func<Type> method,bool indention,Rect area){
			EditorUI.lastRect = area;
			return EditorUI.Draw<Type>(method,indention);
		}
		public static void Draw(Action method,bool indention,Rect area){
			EditorUI.lastRect = area;
			EditorUI.Draw(method,indention);
		}
		public static void DrawMenu(this IEnumerable<string> current,GenericMenu.MenuFunction2 callback,IEnumerable<string> selected=null,IEnumerable<string> disabled=null){
			current.DrawMenu(GUILayoutUtility.GetLastRect(),callback,selected,disabled);
		}
		public static void DrawMenu(this IEnumerable<string> current,Rect area,GenericMenu.MenuFunction2 callback,IEnumerable<string> selected=null,IEnumerable<string> disabled=null){
			if(selected.IsNull()){selected = new List<string>();}
			if(disabled.IsNull()){disabled = new List<string>();}
			var menu = new GenericMenu();
			var index = 0;
			foreach(var item in current){
				++index;
				if(!disabled.Contains(item)){
					menu.AddItem(item.ToContent(),selected.Contains(item),callback,index-1);
					continue;
				}
				menu.AddDisabledItem(item.ToContent());
			}
			menu.DropDown(area);
		}
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
		public static Rect DrawFields<Type>(this Dictionary<string,Type> current,Rect area,string key,string header="Fields",int maxDepth=9){
			bool open = header.IsEmpty();
			if(!open){
				open = header.ToLabel().DrawFoldout(area,key.Box());
				area.y += EditorGUIUtility.singleLineHeight;
			}
			if(open){
				if(!header.IsEmpty()){area.x += 12;}
				foreach(var item in current){
					string label = item.Key.ToTitleCase();
					string subKey = key+label;
					object field = item.Value;
					if(item.HasVariable("propertyHeight")){
						item.CallMethod("OnGUI",subKey,area,item,label);
						area.y += item.GetVariable<float>("propertyHeight");
						continue;
					}
					if(field is ICollection){
						if(maxDepth > 0){
							var enumerable = field as IEnumerable;
							area = enumerable.OfType<object>().ToList().Draw(area,subKey,label,maxDepth-1);
						}
						continue;
					}
					if(field.GetType().IsClass && field.GetType().IsSerializable && !(field is string)){
						if(maxDepth > 0){
							area = field.DrawFields(area,subKey,label,maxDepth-1);
						}
						continue;
					}
					string value = Convert.ToString(field);
					if(value != null){
						value.Draw(area,label);
						area.y += EditorGUIUtility.singleLineHeight;
					}
				}
				if(!header.IsEmpty()){area.x -= 12;}
			}
			return area;
		}
		public static Rect DrawFields(this object current,Rect area,string key,string header="Fields",int maxDepth=9){
			return current.GetVariables().DrawFields(area,key,header,maxDepth);
		}
		public static Rect Draw<Type>(this IList<Type> current,Rect area,string key,string header="List",int maxDepth=9){
			return current.ToList().ToDictionary().DrawFields(area,key,header,maxDepth);
		}
		public static bool DrawFoldout(this UnityLabel current,Rect area,object key,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.foldout;
			string name = key.IsNull() ? current + "Foldout" : key.GetHashCode().ToString();
			if(key is string){name = (string)key;}
			bool previous = PlayerPref.Get<bool>(name);
			bool state = EditorUI.Draw<bool>(()=>EditorGUI.Foldout(area,previous,current,style),indention,area);
			if(previous != state){PlayerPref.Set<bool>(name,state);}
			return state;
		}
		public static bool DrawHeader(this UnityLabel current,Rect area,object key,GUIStyle style=null,bool indention=true){
			string stateName = key.IsNull() ? current + "Foldout" : key.GetHashCode().ToString();
			if(key is string){stateName = (string)key;}
			bool state = PlayerPref.Get<bool>(stateName);
			current = state ? "▼ " + current : "▶ " + current;
			var currentStyle = style.IsNull() ? null : new GUIStyle(style);
			if(state){currentStyle.normal = currentStyle.active;}
			if(current.DrawButton(area,currentStyle,indention)){
				state = !state;
				PlayerPref.Set<bool>(stateName,state);
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
#else
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios.Unity.EditorUI{
	//============================
	// Proxy
	//============================
	public static partial class EditorUI{
		public static string Draw(this string current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){return "";}
		public static float Draw(this float current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){return 0;}
		public static bool Draw(this bool current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){return false;}
		public static Enum Draw(this Enum current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){return default(Enum);}
		public static int Draw(this IEnumerable<string> current,Rect area,int index,UnityLabel label=null,GUIStyle style=null,bool indention=true){return 0;}
		//public static void Draw(this SerializedProperty current,Rect area,UnityLabel label=null,bool allowScene=true,bool indention=true){}
		public static Rect Draw(this Rect current,Rect area,UnityLabel label=null,bool indention=true){return Rect.zero;}
		//public static AnimationCurve Draw(this AnimationCurve current,Rect area,UnityLabel label=null,bool indention=true){return default(AnimationCurve);}
		public static Color Draw(this Color current,Rect area,UnityLabel label=null,bool indention=true){return Color.white;}
		public static void DrawLabel(this UnityLabel current,Rect area,GUIStyle style=null,bool indention=true){}
		public static void DrawHelp(this string current,Rect area,string textType,bool indention=true){}
		public static string DrawTextArea(this string current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){return "";}
		public static bool DrawButton(this UnityLabel current,Rect area,GUIStyle style=null,bool indention=true){return false;}
		public static int DrawInt(this int current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){return 0;}
		public static float DrawSlider(this float current,Rect area,float min,float max,bool indention=true){return 0;}
		public static Enum DrawMaskField(this Enum current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){return default(Enum);}
		public static Vector2 DrawVector2(this Vector2 current,Rect area,UnityLabel label=null,bool indention=true){return Vector2.zero;}
		public static Vector3 DrawVector3(this Vector3 current,Rect area,UnityLabel label=null,bool indention=true){return Vector3.zero;}
		public static Vector4 DrawVector4(this Vector4 current,Rect area,UnityLabel label=null,bool indention=true){return Vector4.zero;}
		public static Type Draw<Type>(this UnityObject current,Rect area,UnityLabel label=null,bool allowScene=true,bool indention=true) where Type : UnityObject{return default(Type);}
	}
	//============================
	// Special
	//============================
	public static partial class EditorUI{
		public static Rect lastRect;
		public static Rect menuArea;
		public static object menuValue;
		public static Type Draw<Type>(Func<Type> method,bool indention,Rect area){return default(Type);}
		public static void Draw(Action method,bool indention,Rect area){}
		public static void DrawMenu(this IEnumerable<string> current,MenuFunction2 callback,IEnumerable<string> selected=null,IEnumerable<string> disabled=null){}
		public static void DrawMenu(this IEnumerable<string> current,Rect area,MenuFunction2 callback,IEnumerable<string> selected=null,IEnumerable<string> disabled=null){}
		public static void DrawAuto(this object current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){}
		public static Rect DrawFields<Type>(this Dictionary<string,Type> current,Rect area,string key,string header="Fields",int maxDepth=9){return Rect.zero;}
		public static Rect DrawFields(this object current,Rect area,string key,string header="Fields",int maxDepth=9){return Rect.zero;}
		public static Rect Draw<Type>(this IList<Type> current,Rect area,string key,string header="List",int maxDepth=9){return Rect.zero;}
		public static bool DrawFoldout(this UnityLabel current,Rect area,object key,GUIStyle style=null,bool indention=true){return false;}
		public static bool DrawHeader(this UnityLabel current,Rect area,object key,GUIStyle style=null,bool indention=true){return false;}
		public static Enum DrawMask(this Enum current,Rect area,UnityLabel label=null,GUIStyle style=null,bool indention=true){return default(Enum);}
	}
}
#endif