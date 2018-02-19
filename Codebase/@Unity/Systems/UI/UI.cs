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
	public static partial class EditorUI{
		public static float space = 0;
		public static bool render = true;
		public static bool foldoutChanged;
		public static bool lastChanged;
		public static bool anyChanged;
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
				if(item.Value == null){
					menu.AddSeparator("/");
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
		public static float space = 0;
		public static bool render = true;
		public static bool foldoutChanged;
		public static bool lastChanged;
		public static bool anyChanged;
		public static bool DrawDialog(this string title,string prompt,string confirm,string cancel){return false;}
		public static bool DrawProgressBar(this string title,string message,float percent,bool inline=false){return false;}
		public static void ClearProgressBar(){}
		public static void DrawMenu(this IEnumerable<string> current,MenuFunction2 callback,IEnumerable<string> selected=null,IEnumerable<string> disabled=null){}
		public static void DrawMenu(this IEnumerable<string> current,Rect area,MenuFunction2 callback,IEnumerable<string> selected=null,IEnumerable<string> disabled=null){}
	}
	public class EditorMenu : Dictionary<string,EditorAction>{
		public void AddSeparator(){}
		public void Add(string key,bool active,Action value){}
		public void Add(string key,Action value,bool active=false){}
		public void Draw(){}
	}
}
#endif