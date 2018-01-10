#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Zios;
using UnityEvent = UnityEngine.Event;
namespace Zios.Interface{
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
	public class EditorAction{
		public Action action;
		public bool active;
		public static implicit operator EditorAction(Action current){return new EditorAction(current);}
		public static implicit operator EditorAction(Delegate current){return new EditorAction((Action)current);}
		public EditorAction(Action action){
			this.action = action;
		}
		public EditorAction(Action action,bool active){
			this.action = action;
			this.active = active;
		}
	}
	public class EditorMenu : Dictionary<string,EditorAction>{
		public void AddSeparator(){
			base.Add(System.IO.Path.GetRandomFileName(),null);
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
			UnityEvent.current.Use();
		}
	}
	public static class LabelExtensions{
		public static UnityLabel ToLabel(this GUIContent current){
			return new UnityLabel(current);
		}
		public static UnityLabel ToLabel(this string current){
			return new UnityLabel(current);
		}
	}
	public class UnityLabel{
		public GUIContent value = new GUIContent("");
		public UnityLabel(string value){this.value = new GUIContent(value);}
		public UnityLabel(GUIContent value){this.value = value;}
		public override string ToString(){return this.value.text;}
		public GUIContent ToContent(){return this.value;}
		//public static implicit operator string(UnityLabel current){return current.value.text;}
		public static implicit operator GUIContent(UnityLabel current){
			if(current == null){return GUIContent.none;}
			return current.value;
		}
		public static implicit operator UnityLabel(GUIContent current){return new UnityLabel(current);}
		public static implicit operator UnityLabel(string current){return new UnityLabel(current);}
	}
}
#endif