using System;
using UnityEngine;
using UnityEditor;
namespace Zios{
	public static class GenericMenuExtension{
		public static void AddItem(this GenericMenu current,string label,bool state,GenericMenu.MenuFunction method){
			current.AddItem(new GUIContent(label),state,method);
		}
		public static void AddItem(this GenericMenu current,string label,bool state,GenericMenu.MenuFunction2 method,object data){
			current.AddItem(new GUIContent(label),state,method,data);
		}
	}
}