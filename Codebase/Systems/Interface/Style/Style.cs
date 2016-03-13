using System.Collections.Generic;
using UnityEngine;
namespace Zios{
	[InitializeOnLoad]
	public static partial class Style{
		public static Dictionary<string,GUISkin> skins = new Dictionary<string,GUISkin>();
		public static Dictionary<GUISkin,Dictionary<string,GUIStyle>> styles = new Dictionary<GUISkin,Dictionary<string,GUIStyle>>();
		public static GUISkin defaultSkin;
		public static GUISkin GetSkin(string name="",bool defaultFallback=true){
			if(Style.skins.ContainsKey(name)){
				return Style.skins[name];
			}
			var cssFile = FileManager.Find(name+".css",true,false);
			if(!cssFile.IsNull()){
				//return Style.LoadCSS(name,cssFile.GetText());
			}
			Style.defaultSkin = Style.defaultSkin ?? typeof(GUI).GetVariable<GUISkin>("s_Skin");
			var defaultSkin = defaultFallback ? Style.defaultSkin : null;
			return FileManager.GetAsset<GUISkin>(name+".guiskin",defaultFallback) ?? defaultSkin;
		}
		public static GUIStyle Get(string skin,string name,bool copy=false){
			var guiSkin = Style.GetSkin(skin);
			return Style.Get(guiSkin,name,copy);
		}
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
		public static GUIStyle Get(string name,bool copy=false){
			return Style.Get(GUI.skin,name,copy);
		}
	}
}