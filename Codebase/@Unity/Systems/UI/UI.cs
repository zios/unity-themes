#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Zios.Unity.EditorUI{
	using Zios.Extensions;
	using Zios.Unity.Extensions;
	using Zios.Unity.Extensions.Convert;
	[InitializeOnLoad]
	public static partial class EditorUI{
		public static float space = 0;
		public static bool allowIndention = true;
		public static bool render = true;
		public static bool resetLabel;
		public static bool foldoutChanged;
		public static bool lastChanged;
		public static bool anyChanged;
		public static GUIStyle label;
		public static Type Draw<Type>(Func<Type> method,bool indention=true){
			int indentValue = EditorGUI.indentLevel;
			indention = EditorUI.allowIndention && indention;
			if(EditorUI.space!=0){GUILayout.Space(EditorUI.space);}
			if(!indention){EditorGUI.indentLevel = 0;}
			bool wasChanged = GUI.changed;
			GUI.changed = false;
			Type value = (Type)method();
			EditorUI.lastChanged = GUI.changed;
			EditorUI.anyChanged = GUI.changed = GUI.changed || wasChanged;
			EditorGUI.indentLevel = indentValue;
			if(EditorUI.resetLabel){GUI.skin.GetStyle("ControlLabel").Use(EditorUI.label);}
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
		public static bool DrawDialog(this string title,string prompt,string confirm,string cancel){
			return EditorUtility.DisplayDialog(title,prompt,confirm,cancel);
		}
		public static bool DrawProgressBar(this string title,string message,float percent,bool inline=false){
			if(inline){
				EditorGUI.ProgressBar(EditorGUILayout.GetControlRect().SetWidth(Screen.width-20),percent,title+" - "+message);
				return false;
			}
			return EditorUtility.DisplayCancelableProgressBar(title,message,percent);
		}
		public static void ClearProgressBar(){
			EditorUtility.ClearProgressBar();
		}
	}
	public class EditorMenu : Dictionary<string,EditorAction>{
		public void AddSeparator(){
			base.Add(Path.GetRandomFileName(),null);
		}
		public void Add(string key,bool active,Action value){
			base.Add(key,new EditorAction(value,active));
		}
		public void Add(string key,Action value,bool active=false){
			base.Add(key,new EditorAction(value,active));
		}
		public void Draw(){
			var menu = new GenericMenu();
			foreach(var item in this){
				var name = item.Key;
				if(name.StartsWith("!")){continue;}
				if(name.StartsWith("/") || item.Value == null){
					menu.AddSeparator("");
					continue;
				}
				GenericMenu.MenuFunction method = new GenericMenu.MenuFunction(item.Value.action);
				menu.AddItem(new GUIContent(name),item.Value.active,method);
			}
			menu.ShowAsContext();
			Event.current.Use();
		}
	}
}
#else
using UnityEngine;
using System;
using System.Collections.Generic;
namespace Zios.Unity.EditorUI{
	public delegate void MenuFunction2(object userData);
	public static partial class EditorUI{
		public static bool allowIndention;
		public static float space = 0;
		public static bool render = true;
		public static bool foldoutChanged;
		public static bool lastChanged;
		public static bool anyChanged;
		public static Type Draw<Type>(Func<Type> method,bool indention=true){return default(Type);}
		public static void Draw(Action method,bool indention=true){}
		public static bool DrawDialog(this string title,string prompt,string confirm,string cancel){return false;}
		public static bool DrawProgressBar(this string title,string message,float percent,bool inline=false){return false;}
		public static void ClearProgressBar(){}
	}
	public class EditorMenu : Dictionary<string,EditorAction>{
		public void AddSeparator(){}
		public void Add(string key,bool active,Action value){}
		public void Add(string key,Action value,bool active=false){}
		public void Draw(){}
	}
}
#endif