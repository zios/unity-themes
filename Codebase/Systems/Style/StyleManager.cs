using UnityEngine;
using System;
using System.Collections.Generic;
namespace Zios{
    public static class Style{
		public static Dictionary<GUISkin,Dictionary<string,GUIStyle>> styles = new Dictionary<GUISkin,Dictionary<string,GUIStyle>>();
		public static GUIStyle Get(GUISkin skin,string name,bool copy=false){
			GUIStyle style;
			if(Style.styles.AddNew(skin).ContainsKey(name)){
				style = Style.styles[skin][name];
				if(copy){return new GUIStyle(style);}
				return style;
			}
			style = skin.GetStyle(name);
			if(style != null){Style.styles[skin][name] = style;}
			if(copy){return new GUIStyle(style);}
			return style;
		}
		public static GUIStyle Get(string name,bool copy=false){return Style.Get(GUI.skin,name,copy);}
    }
}