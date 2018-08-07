#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios.Unity.EditorUI{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Reflection;
	using Zios.Unity.Button;
	using Zios.Unity.Call;
	using Zios.Unity.Extensions;
	using Zios.Unity.Log;
	using Zios.Unity.Pref;
	using Zios.Unity.Proxy;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.Style;
	//============================
	// Proxy
	//============================
	public static partial class EditorUI{
		public static string Draw(this string current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.textField;
			var layout = style.CreateLayout() ?? EditorUI.CreateLayout();
			return EditorUI.Draw(()=>EditorGUILayout.TextField(label,current,style,layout),indention);
		}
		public static float Draw(this float current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.numberField;
			var layout = style.CreateLayout() ?? EditorUI.CreateLayout();
			return EditorUI.Draw(()=>EditorGUILayout.FloatField(label,current,style,layout),indention);
		}
		public static bool Draw(this bool current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.toggle;
			var layout = style.CreateLayout() ?? EditorUI.CreateLayout();
			return EditorUI.Draw(()=>EditorGUILayout.Toggle(label,current,style,layout),indention);
		}
		public static Enum Draw(this Enum current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.popup;
			var layout = style.CreateLayout() ?? EditorUI.CreateLayout();
			return EditorUI.Draw(()=>EditorGUILayout.EnumPopup(label,current,style,layout),indention);
		}
		public static int Draw(this IEnumerable<string> current,int index,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.popup;
			var labelValue = label.IsNull() ? null : label.ToString();
			return EditorUI.Draw(()=>EditorGUILayout.Popup(labelValue,index,current.ToArray(),style),indention);
		}
		public static void Draw(this SerializedProperty current,UnityLabel label=null,bool allowScene=true,bool indention=true){
			if(label != null && label.value.text.IsEmpty()){label = new GUIContent(current.displayName);}
			Action action = ()=>EditorGUILayout.PropertyField(current,label,allowScene,EditorUI.CreateLayout());
			EditorUI.Draw(action,indention);
		}
		public static Rect Draw(this Rect current,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw(()=>EditorGUILayout.RectField(label,current,EditorUI.CreateLayout()),indention);
		}
		public static AnimationCurve Draw(this AnimationCurve current,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw(()=>EditorGUILayout.CurveField(label,current,EditorUI.CreateLayout()),indention);
		}
		public static Color Draw(this Color current,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw(()=>EditorGUILayout.ColorField(label,current,EditorUI.CreateLayout()),indention);
		}
		public static void DrawLabel(this string current,GUIStyle style=null,bool indention=true){
			current.ToLabel().DrawLabel(style,indention);
		}
		public static void DrawLabel(this UnityLabel current,UnityLabel label,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.label;
			var layout = style.CreateLayout() ?? EditorUI.CreateLayout();
			EditorUI.Draw(()=>EditorGUILayout.LabelField(label,current,style,layout),indention);
		}
		public static void DrawLabel(this UnityLabel current,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.label;
			var layout = style.CreateLayout() ?? EditorUI.CreateLayout();
			if(indention){
				EditorUI.Draw(()=>EditorGUILayout.LabelField(current,style,layout),indention);
				return;
			}
			EditorUI.Draw(()=>GUILayout.Label(current,style,layout),indention);
		}
		public static void DrawPrefix(this UnityLabel current,GUIStyle style=null,GUIStyle followStyle=null,bool indention=true){
			style = style ?? EditorStyles.label;
			followStyle = followStyle ?? GUI.skin.button;
			EditorUI.Draw(()=>EditorGUILayout.PrefixLabel(current,followStyle,style),indention);
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
			var layout = style.CreateLayout() ?? EditorUI.CreateLayout();
			return EditorUI.Draw(()=>EditorGUILayout.TextField(label,current,style,layout),indention);
		}
		public static bool DrawButton(this UnityLabel current,GUIStyle style=null,bool indention=true){
			style = style ?? GUI.skin.button;
			var layout = style.CreateLayout() ?? EditorUI.CreateLayout();
			return EditorUI.Draw(()=>GUILayout.Button(current,style,layout),indention);
		}
		public static int DrawInt(this int current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.numberField;
			var layout = style.CreateLayout() ?? EditorUI.CreateLayout();
			return EditorUI.Draw(()=>EditorGUILayout.IntField(label,current,style,layout),indention);
		}
		public static int DrawSlider(this int current,int min,int max,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw(()=>EditorGUILayout.IntSlider(label,current,min,max,EditorUI.CreateLayout()),indention);
		}
		public static float DrawSlider(this float current,float min,float max,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw(()=>EditorGUILayout.Slider(label,current,min,max,EditorUI.CreateLayout()),indention);
		}
		public static Type Draw<Type>(this UnityObject current,UnityLabel label=null,bool allowScene=true,bool indention=true) where Type : UnityObject{
			return (Type)EditorUI.Draw(()=>EditorGUILayout.ObjectField(label,current,typeof(Type),allowScene,EditorUI.CreateLayout()),indention);
		}
		public static IEnumerable<bool> DrawFlags(this IEnumerable<string> current,IEnumerable<bool> active,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.popup;
			var layout = style.CreateLayout() ?? EditorUI.CreateLayout();
			var mask = EditorUI.Draw(()=>EditorGUILayout.MaskField(label,active.ToBitFlags(),current.ToArray(),style,layout),indention);
			return mask.ToFlags(active.Count());
		}
		public static Enum DrawFlags(this Enum current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.popup;
			var layout = style.CreateLayout() ?? EditorUI.CreateLayout();
			#if UNITY_2017_3_OR_NEWER
			return EditorUI.Draw(()=>EditorGUILayout.EnumFlagsField(label,current,style,layout),indention);
			#else
			return EditorUI.Draw(()=>EditorGUILayout.EnumMaskField(label,current,style,layout),indention);
			#endif
		}
		public static Vector2 DrawVector2(this Vector2 current,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw(()=>EditorGUILayout.Vector2Field(label,current,EditorUI.CreateLayout()),indention);
		}
		public static Vector3 DrawVector3(this Vector3 current,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw(()=>EditorGUILayout.Vector3Field(label,current,EditorUI.CreateLayout()),indention);
		}
		public static Vector4 DrawVector4(this Vector4 current,UnityLabel label=null,bool indention=true){
			return EditorUI.Draw(()=>EditorGUILayout.Vector4Field(label.ToString(),current,EditorUI.CreateLayout()),indention);
		}
	}
	//============================
	// Delayed
	//============================
	public static partial class EditorUI{
		public static string DrawDelayed(this string current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.textField;
			var layout = style.CreateLayout() ?? EditorUI.CreateLayout();
			#if UNITY_5_5_OR_NEWER
			return EditorUI.Draw(()=>EditorGUILayout.DelayedTextField(label,current,style,layout),indention);
			#else
			return EditorUI.Draw(()=>EditorGUILayout.TextField(label,current,style,layout),indention);
			#endif
		}
		public static float DrawDelayed(this float current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.numberField;
			var layout = style.CreateLayout() ?? EditorUI.CreateLayout();
			#if UNITY_5_5_OR_NEWER
			return EditorUI.Draw(()=>EditorGUILayout.DelayedFloatField(label,current,style,layout),indention);
			#else
			return EditorUI.Draw(()=>EditorGUILayout.FloatField(label,current,style,layout),indention);
			#endif
		}
		public static int DrawIntDelayed(this int current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			style = style ?? EditorStyles.numberField;
			var layout = style.CreateLayout() ?? EditorUI.CreateLayout();
			#if UNITY_5_5_OR_NEWER
			return EditorUI.Draw(()=>EditorGUILayout.DelayedIntField(label,current,style,layout),indention);
			#else
			return EditorUI.Draw(()=>EditorGUILayout.IntField(label,current,style,layout),indention);
			#endif
		}
	}
	//============================
	// Special
	//============================
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
		public static void DrawFields(this object current,string header="Fields",BindingFlags flags=Reflection.declaredFlags){
			if(header.IsEmpty() || EditorUI.DrawFoldout(header,current)){
				if(!header.IsEmpty()){EditorGUI.indentLevel += 1;}
				foreach(var item in current.GetVariables(flags)){
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
			var open = EditorUI.DrawFoldout(label.ToString().ToTitleCase(),EditorUI.CreateLayout());
			if(!open){return;}
			EditorGUI.indentLevel += 1;
			foreach(DictionaryEntry item in current){
				item.Value.DrawAuto(item.Key.ToString(),style,true);
			}
			EditorGUI.indentLevel -= 1;
		}
		public static void PadStart(int space=0){
			EditorUI.SetFieldSize(-1,EditorGUIUtility.labelWidth-(space*2));
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(space);
		}
		public static void PadEnd(){
			EditorGUILayout.EndHorizontal();
		}
		public static void LabelStyle(GUIStyle style){
			if(EditorUI.label.IsNull()){
				EditorUI.label = GUI.skin.GetStyle("ControlLabel").Copy();
			}
			GUI.skin.GetStyle("ControlLabel").Use(style);
			EditorUI.resetLabel = true;
		}
		public static void LabelToggle(string key){
			var expanded = EditorPrefs.GetBool(key);
			var style = expanded ? EditorStyles.foldout.UseState("onNormal","*") : EditorStyles.foldout;
			EditorUI.PadStart(-5);
			EditorUI.LabelStyle(style);
		}
		public static bool LabelToggleEnd(string key){
			var state = EditorPrefs.GetBool(key);
			EditorUI.PadEnd();
			if(GUILayoutUtility.GetLastRect().SetWidth(EditorGUIUtility.labelWidth).Clicked()){
				EditorPrefs.SetBool(key+"-Buffer",!state);
			}
			if(Proxy.IsRepainting() && EditorPrefs.HasKey(key+"-Buffer")){
				state = EditorPrefs.GetBool(key+"-Buffer");
				EditorPrefs.DeleteKey(key+"-Buffer");
				EditorPrefs.SetBool(key,state);
				ProxyEditor.RepaintAll();
				return !state;
			}
			EditorPrefs.SetBool(key,state);
			return state;
		}
		public static bool DrawFoldout(this UnityLabel current,object key=null,GUIStyle style=null,bool indention=true){
			var lastState = GUI.changed;
			style = style ?? EditorStyles.foldout;
			string name = key.IsNull() ? current + "Header" : key.GetHashCode().ToString() + "Header";
			if(key is string){name = (string)key;}
			bool previous = PlayerPref.Get<bool>(name);
			#if UNITY_5_5_OR_NEWER
			bool state = EditorUI.Draw(()=>EditorGUILayout.Foldout(previous,current,true,style),indention);
			#else
			bool state = EditorUI.Draw(()=>EditorGUILayout.Foldout(previous,current,style),indention);
			#endif
			if(previous != state){
				PlayerPref.Set<bool>(name,state);
				EditorUI.foldoutChanged = true;
				//GUI.FocusControl("");
			}
			GUI.changed = lastState;
			return state;
		}
		public static bool DrawHeader(this UnityLabel current,object key=null,GUIStyle style=null,bool editable=false,Action callback=null,bool indention=true){
			string stateName = key.IsNull() ? current + "Header" : key.GetHashCode().ToString() + "Header";
			if(key is string){stateName = (string)key;}
			bool state = PlayerPref.Get<bool>(stateName);
			//current = state ? "▼ " + current : "▶ " + current;
			var fallback = editable ? EditorStyles.textField : EditorStyles.label;
			var currentStyle = style.IsNull() ? fallback.Copy() : new GUIStyle(style);
			if(state){currentStyle.normal = currentStyle.onNormal;}
			if(!editable){
				if(current.DrawButton(currentStyle,indention)){
					state = !state;
					PlayerPref.Set<bool>(stateName,state);
					if(!callback.IsNull()){callback();}
				}
			}
			else{
				current.value.text = current.value.text.Draw(null,currentStyle,indention);
			}
			return state;
		}
		public static Enum DrawMask(this Enum current,UnityLabel label=null,GUIStyle style=null,bool indention=true){
			var control = EditorGUILayout.GetControlRect();
			return current.DrawMask(control,label,style,indention);
		}
		public static int DrawPrompt(this UnityLabel current,ref string field,GUIStyle titleStyle=null,GUIStyle inputStyle=null){
			int result = 0;
			if(Button.EventKeyDown("KeypadEnter") || Button.EventKeyDown("Return")){result = 1;}
			if(Button.EventKeyDown("Escape")){result = -1;}
			if(titleStyle == null){titleStyle = Style.Get("Prompt","DialogQuestion");}
			if(inputStyle == null){inputStyle = Style.Get("Prompt","DialogInput");}
			float width = (Screen.width/2).Max(150);
			Rect full = new Rect(0,0,Screen.width,Screen.height);
			Rect center = new Rect(Screen.width/2,Screen.height/2,0,0);
			Rect input = center.AddY(-15).SetSize(width,40).AddX(-width/2);
			current.DrawLabel(full,titleStyle);
			GUI.SetNextControlName("PromptField");
			field = field.Draw(input,null,inputStyle);
			EditorGUI.FocusTextInControl("PromptField");
			return result;
		}
		public static int DrawButtonPrompt(this UnityLabel current,GUIStyle titleStyle=null,GUIStyle buttonStyle=null){
			int result = 0;
			if(Button.EventKeyDown("Escape")){result = -1;}
			if(titleStyle == null){titleStyle = Style.Get("Prompt","DialogQuestion");}
			if(buttonStyle == null){buttonStyle = Style.Get("Prompt","DialogButton");}
			Rect full = new Rect(0,0,Screen.width,Screen.height);
			Rect button = new Rect(Screen.width/2,Screen.height/2,100,40).AddY(-10);
			current.DrawLabel(full,titleStyle);
			if("Yes".ToLabel().DrawButton(button.AddX(-105),buttonStyle)){result = 1;}
			if("No".ToLabel().DrawButton(button.AddX(5),buttonStyle)){result = 2;}
			return result;
		}
	}
	//============================
	// Layout
	//============================
	public enum EditorUILayout{Auto,Global,Style,None}
	public static partial class EditorUI{
		public static float width = 0;
		public static float height = 0;
		public static float minWidth = 0;
		public static float minHeight = 0;
		public static float maxWidth = 0;
		public static float maxHeight = 0;
		public static bool? autoWidth = null;
		public static bool? autoHeight = null;
		public static bool resetLayout;
		public static bool resetField;
		public static Vector2 resetFieldSize;
		public static EditorUILayout layoutType = EditorUILayout.Global;
		public static void SetFieldSize(Vector2 size,bool nextOnly=true){EditorUI.SetFieldSize(size.x,size.y,nextOnly);}
		public static void SetFieldSize(float valueWidth=-1,float labelWidth=-1,bool nextOnly=true){
			if(nextOnly && !EditorUI.resetField){
				EditorUI.resetFieldSize = new Vector2(EditorGUIUtility.fieldWidth,EditorGUIUtility.labelWidth);
			}
			EditorUI.resetField = nextOnly;
			if(valueWidth != -1){EditorGUIUtility.fieldWidth = valueWidth;}
			if(labelWidth != -1){EditorGUIUtility.labelWidth = labelWidth;}
		}
		public static void SetLayoutOnce(float width=-1,float height=-1,float maxWidth=-1,float maxHeight=-1,float minWidth=-1,float minHeight=-1,bool? autoWidth=null,bool? autoHeight=null){
			EditorUI.SetLayout(width,height,maxWidth,maxHeight,minWidth,minHeight,autoWidth,autoHeight,true);
		}
		public static void SetLayout(float width=-1,float height=-1,float maxWidth=-1,float maxHeight=-1,float minWidth=-1,float minHeight=-1,bool? autoWidth=null,bool? autoHeight=null,bool nextOnly=false){
			if(width != -1){EditorUI.width = width;}
			if(height != -1){EditorUI.height = height;}
			if(maxWidth != -1){EditorUI.maxWidth = maxWidth;}
			if(maxHeight != -1){EditorUI.maxHeight = maxHeight;}
			if(minWidth != -1){EditorUI.minWidth = minWidth;}
			if(minHeight != -1){EditorUI.minHeight = minHeight;}
			EditorUI.autoWidth = autoWidth;
			EditorUI.autoHeight = autoHeight;
			EditorUI.resetLayout = nextOnly;
		}
		public static void SetSpace(float space){
			EditorUI.space = space;
		}
		public static void SetSpace<T>(this T current,float space){
			EditorUI.space = space;
		}
		public static T Layout<T>(this T current,float width=-1,float height=-1,float maxWidth=-1,float maxHeight=-1,float minWidth=-1,float minHeight=-1,bool? autoWidth=null,bool? autoHeight=null,bool nextOnly=true){
			EditorUI.SetLayout(width,height,maxWidth,maxHeight,minWidth,minHeight,autoWidth,autoHeight,nextOnly);
			return current;
		}
		public static GUILayoutOption[] CreateLayout(this GUIStyle current){
			var options = new GUILayoutOption[0];
			if(!current.IsNull() && EditorUI.layoutType.MatchesAny("Style","Auto")){
				options = EditorUI.GenerateLayout(current.fixedWidth,current.fixedHeight,0,0,0,0,current.stretchWidth,current.stretchHeight);
			}
			return options.Length < 1 && EditorUI.layoutType.Matches("Style") ? options.ToArray() : null;
		}
		public static GUILayoutOption[] CreateLayout(){
			var options = new GUILayoutOption[0];
			if(EditorUI.layoutType.MatchesAny("Global","Auto")){
				options = EditorUI.GenerateLayout(EditorUI.width,EditorUI.height,EditorUI.maxWidth,EditorUI.maxHeight,EditorUI.minWidth,EditorUI.minHeight,EditorUI.autoWidth,EditorUI.autoHeight);
			}
			return options.Length < 1 ? null : options.ToArray();
		}
		public static GUILayoutOption[] GenerateLayout(float width=0,float height=0,float maxWidth=0,float maxHeight=0,float minWidth=0,float minHeight=0,bool? autoWidth=null,bool? autoHeight=null){
			var options = new List<GUILayoutOption>();
			if(width != 0){options.Add(GUILayout.Width(width));}
			if(height != 0){options.Add(GUILayout.Height(height));}
			if(maxWidth != 0){options.Add(GUILayout.MaxWidth(maxWidth));}
			if(maxHeight != 0){options.Add(GUILayout.MaxHeight(maxHeight));}
			if(minWidth != 0){options.Add(GUILayout.MinWidth(minWidth));}
			if(minHeight != 0){options.Add(GUILayout.MinHeight(minHeight));}
			if(autoWidth != null){options.Add(GUILayout.ExpandWidth(autoWidth.As<bool>()));}
			if(autoHeight != null){options.Add(GUILayout.ExpandHeight(autoHeight.As<bool>()));}
			return options.ToArray();
		}
		public static void Status(){
			Log.Show("------------------------------");
			Log.Show("Width      : " + EditorUI.width);
			Log.Show("Height     : " + EditorUI.height);
			Log.Show("MaxWidth   : " + EditorUI.maxWidth);
			Log.Show("MaxHeight  : " + EditorUI.maxHeight);
			Log.Show("MinWidth   : " + EditorUI.minWidth);
			Log.Show("MinHeight  : " + EditorUI.minHeight);
			Log.Show("AutoWidth  : " + EditorUI.autoWidth);
			Log.Show("AutoHeight : " + EditorUI.autoHeight);
			Log.Show("Reset      : " + EditorUI.resetLayout);
		}
		public static void Reset(){
			EditorUI.space = 0;
			EditorUI.allowIndention = true;
			EditorUI.anyChanged = false;
			EditorUI.lastChanged = false;
			EditorUI.foldoutChanged = false;
			EditorUI.ResetFieldSize();
			EditorUI.ResetLayout();
			GUI.changed = false;
		}
		public static void ResetFieldSize(){
			EditorUI.SetFieldSize(0,0,false);
		}
		public static void ResetLayout(){
			EditorUI.SetLayout(0,0,0,0,0,0,null,null,false);
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
		public static string Draw(this string current,UnityLabel label=null,GUIStyle style=null,bool indention=true){return current;}
		public static float Draw(this float current,UnityLabel label=null,GUIStyle style=null,bool indention=true){return current;}
		public static bool Draw(this bool current,UnityLabel label=null,GUIStyle style=null,bool indention=true){return current;}
		public static Enum Draw(this Enum current,UnityLabel label=null,GUIStyle style=null,bool indention=true){return current;}
		public static int Draw(this IEnumerable<string> current,int index,UnityLabel label=null,GUIStyle style=null,bool indention=true){return current;}
		public static void Draw(this SerializedProperty current,UnityLabel label=null,bool allowScene=true,bool indention=true){}
		public static Rect Draw(this Rect current,UnityLabel label=null,bool indention=true){return current;}
		public static AnimationCurve Draw(this AnimationCurve current,UnityLabel label=null,bool indention=true){return current;}
		public static Color Draw(this Color current,UnityLabel label=null,bool indention=true){return current;}
		public static void DrawLabel(this string current,GUIStyle style=null,bool indention=true){}
		public static void DrawLabel(this UnityLabel current,UnityLabel label,GUIStyle style=null,bool indention=true){}
		public static void DrawLabel(this UnityLabel current,GUIStyle style=null,bool indention=true){}
		public static void DrawPrefix(this UnityLabel current,GUIStyle style=null,GUIStyle followStyle=null,bool indention=true){}
		public static void DrawHelp(this string current,string textType="Info",bool indention=true){}
		public static string DrawTextArea(this string current,UnityLabel label=null,GUIStyle style=null,bool indention=true){return current;}
		public static bool DrawButton(this UnityLabel current,GUIStyle style=null,bool indention=true){return false;}
		public static int DrawInt(this int current,UnityLabel label=null,GUIStyle style=null,bool indention=true){return current;}
		public static int DrawSlider(this int current,int min,int max,UnityLabel label=null,bool indention=true){return current;}
		public static float DrawSlider(this float current,float min,float max,UnityLabel label=null,bool indention=true){return current;}
		public static Type Draw<Type>(this UnityObject current,UnityLabel label=null,bool allowScene=true,bool indention=true) where Type : UnityObject{return default(Type);}
		public static IEnumerable<bool> DrawFlags(this IEnumerable<string> current,IEnumerable<bool> active,UnityLabel label=null,GUIStyle style=null,bool indention=true){return default(IEnumerable<bool>);}
		public static Enum DrawFlags(this Enum current,UnityLabel label=null,GUIStyle style=null,bool indention=true){return current;}
		public static Vector2 DrawVector2(this Vector2 current,UnityLabel label=null,bool indention=true){return current;}
		public static Vector3 DrawVector3(this Vector3 current,UnityLabel label=null,bool indention=true){return current;}
		public static Vector4 DrawVector4(this Vector4 current,UnityLabel label=null,bool indention=true){return current;}
	}
	//============================
	// Delayed
	//============================
	public static partial class EditorUI{
		public static string DrawDelayed(this string current,UnityLabel label=null,GUIStyle style=null,bool indention=true){return current;}
		public static float DrawDelayed(this float current,UnityLabel label=null,GUIStyle style=null,bool indention=true){return current;}
		public static int DrawIntDelayed(this int current,UnityLabel label=null,GUIStyle style=null,bool indention=true){return current;}
	}
	//============================
	// Special
	//============================
	public static partial class EditorUI{
		public static void DrawAuto(this object current,UnityLabel label=null,GUIStyle style=null,bool indention=true){}
		public static void DrawFields(this object current,string header="Fields",BindingFlags flags=Reflection.declaredFlags){}
		public static void Draw<T>(this IList<T> current,string header="List"){}
		public static void Draw(this IDictionary current,UnityLabel label=null,GUIStyle style=null,bool indention=true){}
		public static void PadStart(int space=0){}
		public static void PadEnd(){}
		public static void LabelStyle(GUIStyle style){}
		public static void LabelToggle(string key){}
		public static bool LabelToggleOpen(string key){return false;}
		public static bool DrawFoldout(this UnityLabel current,object key=null,GUIStyle style=null,bool indention=true){return false;}
		public static bool DrawHeader(this UnityLabel current,object key=null,GUIStyle style=null,bool editable=false,Action callback=null,bool indention=true){return false;}
		public static Enum DrawMask(this Enum current,UnityLabel label=null,GUIStyle style=null,bool indention=true){return current;}
		public static int DrawPrompt(this UnityLabel current,ref string field,GUIStyle titleStyle=null,GUIStyle inputStyle=null){return 0;}
		public static int DrawButtonPrompt(this UnityLabel current,GUIStyle titleStyle=null,GUIStyle buttonStyle=null){return 0;}
	}
	//============================
	// Layout
	//============================
	public static partial class EditorUI{
		public static float width;
		public static float height;
		public static float minWidth;
		public static float minHeight;
		public static float maxWidth;
		public static float maxHeight;
		public static bool? autoWidth = null;
		public static bool? autoHeight = null;
		public static bool resetLayout;
		public static bool resetField;
		public static Vector2 resetFieldSize;
		public static EditorUILayout layoutType = EditorUILayout.Global;
		public static void SetFieldSize(Vector2 size,bool nextOnly=true){}
		public static void SetFieldSize(float valueWidth=-1,float labelWidth=-1,bool nextOnly=true){}
		public static void SetLayoutOnce(float width=-1,float height=-1,float maxWidth=-1,float maxHeight=-1,float minWidth=-1,float minHeight=-1,bool? autoWidth=null,bool? autoHeight=null){}
		public static void SetLayout(float width=-1,float height=-1,float maxWidth=-1,float maxHeight=-1,float minWidth=-1,float minHeight=-1,bool? autoWidth=null,bool? autoHeight=null,bool nextOnly=false){}
		public static void SetSpace(float space){}
		public static void SetSpace<T>(this T current,float space){}
		public static T Layout<T>(this T current,float width=-1,float height=-1,float maxWidth=-1,float maxHeight=-1,float minWidth=-1,float minHeight=-1,bool? autoWidth=null,bool? autoHeight=null,bool nextOnly=true){return current;}
		public static GUILayoutOption[] CreateLayout(this GUIStyle current){return new GUILayoutOption[0];}
		public static GUILayoutOption[] CreateLayout(){return new GUILayoutOption[0];}
		public static GUILayoutOption[] GenerateLayout(float width=0,float height=0,float maxWidth=0,float maxHeight=0,float minWidth=0,float minHeight=0,bool? autoWidth=null,bool? autoHeight=null){return new GUILayoutOption[0];}
		public static void Status(){}
		public static void Reset(){}
		public static void ResetFieldSize(){}
		public static void ResetLayout(){}
	}
}
#endif