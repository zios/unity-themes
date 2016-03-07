using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Zios{
	using Interface;
	using Events;
	[InitializeOnLoad]
	public static class Style{
		public static Dictionary<GUISkin,Dictionary<string,GUIStyle>> styles = new Dictionary<GUISkin,Dictionary<string,GUIStyle>>();
		public static GUISkin defaultSkin;
		public static GUISkin activeTheme;
		[NonSerialized] public static bool setup;
		[NonSerialized] public static GUISkin[] themes;
		[NonSerialized] public static string themeName = "Default";
		[NonSerialized] public static int themeIndex = -1;
		#if UNITY_EDITOR
		static Style(){
			EditorApplication.projectWindowItemOnGUI += (a,b)=>Style.ValidateTheme();
			EditorApplication.hierarchyWindowItemOnGUI += (a,b)=>Style.ValidateTheme();
		}
		public static void Refresh(){
			var defaultTheme = Style.BuildTheme().AsArray();
			var userThemes = FileManager.GetAssets<GUISkin>("*.guiSkin").Where(x=>x.name.StartsWith("EditorStyles-")).ToArray();
			Style.themes = defaultTheme.Concat(userThemes);
			Style.themeName = EditorPrefs.GetString("EditorTheme","Default");
			Style.themeIndex = Style.themes.ToList().FindIndex(x=>x.name.Split("-")[1]==Style.themeName);
			Style.themeIndex = Math.Max(0,Style.themeIndex);
		}
		public static void ValidateTheme(){
			if(!Style.setup){
				Style.Refresh();
				Style.LoadTheme();
				Utility.RepaintAll();
				Style.setup = true;
			}
		}
		public static void SelectTheme(){
			if(Style.themes.IsNull() || Style.themes.Exists(x=>x.IsNull())){
				Style.Refresh();
			}
			var themeNames = Style.themes.Select(x=>x.name.Split("-")[1]).ToArray();
			Style.themeIndex = themeNames.Draw(Style.themeIndex,"Theme");
			Style.themeName = themeNames[Style.themeIndex];
			EditorPrefs.SetString("EditorTheme",Style.themeName);
			if(GUI.changed){
				Style.LoadTheme();
				Utility.RepaintAll();
			}
		}
		public static GUISkin BuildTheme(string name="Default"){
			var skin = ScriptableObject.CreateInstance<GUISkin>();
			skin.name = "EditorStyles-"+name;
			var defaultSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
			var defaultStyles = defaultSkin.GetVariables<GUIStyle>();
			var flexibleStyle = typeof(GUILayoutUtility).GetVariable<GUIStyle>("s_SpaceStyle");
			flexibleStyle.name = "s_SpaceStyle";
			skin.SetVariables<GUIStyle>(defaultStyles);
			var styles = new List<GUIStyle>(){flexibleStyle};
			//styles.AddRange(defaultSkin.customStyles);
			var editorStyles = typeof(EditorStyles).GetVariable<EditorStyles>("s_Current");
			foreach(var item in editorStyles.GetVariables<GUIStyle>(null,ObjectExtension.privateFlags)){
				var style = new GUIStyle(item.Value);
				style.name = item.Key;
				styles.Add(style);
			}
			skin.customStyles = styles.ToArray();
			return skin;
		}
		public static void CreateTheme(string name="Default"){
			var skin = Style.BuildTheme(name);
			AssetDatabase.CreateAsset(skin,"Assets/@Zios/Interface/Skins/"+name+"/EditorStyles-"+name+".guiskin");
		}
		public static void LoadTheme(string name=""){
			if(name.IsEmpty()){name = Style.themeName;}
			var baseSkin = name=="Default" ? Style.GetSkin() : Style.GetSkin("EditorSkin-"+name);
			var baseStyles = name=="Default" ? Style.themes[0] : Style.themes.Find(x=>x.name.Split("-")[1]==name);
			var editorStyles = typeof(EditorStyles).GetVariable<EditorStyles>("s_Current");
			typeof(GUILayoutUtility).SetVariable<GUIStyle>("s_SpaceStyle",baseStyles.GetStyle("s_SpaceStyle"));
			editorStyles.SetValuesByType<GUIStyle>(baseStyles.customStyles.Skip(1).ToArray(),null,ObjectExtension.privateFlags);
			/*if(Style.activeTheme.IsNull() || Style.activeTheme.name != name){
				var baseSkin = Style.GetSkin("EditorSkin-"+name);
				Style.activeTheme = ScriptableObject.CreateInstance<GUISkin>();
				Style.activeTheme.customStyles = baseStyles.customStyles.Concat(baseSkin.customStyles);
				Style.activeTheme.name = name;
			}*/
			GUI.skin = baseSkin;
			//Utility.GetInternalType("HostView").SetVariable<Color>("kViewColor",new Color(0,0,0,1));
		}
		#endif
		public static GUISkin GetSkin(string name=""){
			var skin = name.IsEmpty() ? null : FileManager.GetAsset<GUISkin>(name+".guiskin");
			if(skin.IsNull()){
				if(Style.defaultSkin.IsNull()){
					Style.defaultSkin = typeof(GUI).GetVariable<GUISkin>("s_Skin");
				}
				skin = Style.defaultSkin;
			}
			return skin;
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
		public static GUIStyle Get(string name,bool copy=false){return Style.Get(GUI.skin,name,copy);}
	}
}