using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
namespace Zios.Interface{
	using UnityEngine;
	using UnityEditor;
	[Serializable]
	public class ThemeSkinset{
		public static List<ThemeSkinset> all = new List<ThemeSkinset>();
		public string name;
		public string path;
		public ThemeSkin main;
		public List<ThemeSkin> skins = new List<ThemeSkin>();
		//=================================
		// Files
		//=================================
		public static List<ThemeSkinset> Import(){
			var imported = new List<ThemeSkinset>();
			foreach(var path in Directory.GetDirectories(Theme.storagePath+"Skinsets")){
				imported.Add(ThemeSkinset.Import(path));
			}
			return imported;
		}
		public static ThemeSkinset Import(string path){
			path = path.Replace("\\","/");
			var skinset = new ThemeSkinset();
			skinset.name = path.GetPathTerm();
			skinset.path = path;
			foreach(var skinFile in FileManager.FindAll(path+"/*.guiskin",true,false)){
				var active = skinset.skins.AddNew();
				if(skinFile.name == skinset.name){
					skinset.skins.Remove(active);
					active = skinset.main = new ThemeSkin();
				}
				active.name = skinFile.name;
				active.path = skinFile.path;
				active.skin = skinFile.GetAsset<GUISkin>();
				var field = skinFile.name.Split(".").Last();
				var parent = skinFile.name.Replace("."+field,"");
				var typeDirect = Utility.GetUnityType(skinFile.name);
				var typeParent = Utility.GetUnityType(parent);
				var flags = field.Contains("s_Current") ? ObjectExtension.privateFlags : ObjectExtension.staticFlags;
				if(typeDirect.IsNull() && (typeParent.IsNull() || !typeParent.HasVariable(field))){
					if(Theme.debug){Debug.LogWarning("[Themes] No matching class/field found for GUISkin -- " + skinFile.name + ". Possible version conflict.");}
					continue;
				}
				active.scope = !typeDirect.IsNull() ? typeDirect : typeParent.InstanceVariable(field);
				active.scopedStyles = !typeDirect.IsNull() ? active.scope.GetVariables<GUIStyle>(null,flags) : active.scope.GetVariables<GUIStyle>();
				var styles = new List<GUIStyle>();
				foreach(var style in active.scopedStyles){styles.Add(style.Value.Copy());}
			}
			return skinset;
		}
		public void Export(string path=null){
			var theme = Theme.active;
			var targetPath = path ?? Theme.storagePath;
			var targetName = theme.name+"-Variant";
			path = path.IsEmpty() ? EditorUtility.SaveFilePanel("Save Theme",targetPath,targetName,"unitytheme") : path;
			if(path.Length > 0){
				var file = FileManager.Create(path);
				file.WriteText(this.Serialize());
				EditorPrefs.SetString("EditorTheme",theme.name);
				Theme.setup = false;
			}
		}
		public void Apply(Theme theme){
			this.ApplyMain(theme);
			foreach(var skin in this.skins){
				skin.Apply(theme);
			}
		}
		public void ApplyMain(Theme theme){
			if(this.main.IsNull()){return;}
			var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
			var isDefault = this.name == "Default";
			var main = this.main.skin;
			var palette = theme.palette;
			if(!isDefault && !Theme.liveEdit){
				main = theme.fontset.Apply(main);
				theme.palette.Apply(main);
			}
			skin.Use(main,!Theme.liveEdit);
			EditorGUIUtility.GetBuiltinSkin(EditorSkin.Game).Use(main,!Theme.liveEdit);
			EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).Use(main,!Theme.liveEdit);
			Utility.GetUnityType("AppStatusBar").ClearVariable("background");
			Utility.GetUnityType("SceneRenderModeWindow.Styles").SetVariable("sMenuItem",skin.Get("MenuItem"));
			Utility.GetUnityType("SceneRenderModeWindow.Styles").SetVariable("sSeparator",skin.Get("sv_iconselector_sep"));
			Utility.GetUnityType("GameView.Styles").SetVariable("gizmoButtonStyle",skin.Get("GV Gizmo DropDown"));
			typeof(SceneView).SetVariable<GUIStyle>("s_DropDownStyle",skin.Get("GV Gizmo DropDown"));
			var console = Utility.GetUnityType("ConsoleWindow.Constants");
			console.SetVariable("ms_Loaded",false);
			console.CallMethod("Init");
			var hostView = Utility.GetUnityType("HostView");
			if(palette.Has("Cursor")){skin.settings.cursorColor = palette.Get("Cursor");}
			if(palette.Has("Selection")){skin.settings.selectionColor = palette.Get("Selection");}
			if(palette.Has("Curve")){typeof(EditorGUI).SetVariable("kCurveColor",palette.Get("Curve"));}
			if(palette.Has("CurveBackground")){typeof(EditorGUI).SetVariable("kCurveBGColor",palette.Get("CurveBackground"));}
			if(palette.Has("Window")){
				typeof(EditorGUIUtility).SetVariable("kDarkViewBackground",palette.Get("Window"));
				hostView.SetVariable("kViewColor",palette.Get("Window"));
			}
			foreach(var view in Resources.FindObjectsOfTypeAll(hostView)){
				view.ClearVariable("background");
			}
			foreach(var window in Locate.GetAssets<EditorWindow>()){
				window.antiAlias = 1;
				window.minSize = window.GetType().Name.Contains("Preferences") ? window.minSize : new Vector2(100,20);
				window.wantsMouseMove = Theme.responsive;
				window.autoRepaintOnSceneChange = Theme.responsive;
			}
			if(!isDefault){
				Utility.GetUnityType("PreferencesWindow").InstanceVariable("constants").SetVariable("sectionHeader",skin.Get("HeaderLabel"));
			}
			Utility.GetUnityType("BuildPlayerWindow").InstanceVariable("styles").SetVariable("toggleSize",new Vector2(24,16));
		}
	}
	[Serializable]
	public class ThemeSkin{
		public Dictionary<string,GUIStyle> scopedStyles;
		public object scope;
		public GUISkin skin;
		public string name;
		public string path;
		public void Apply(Theme theme){
			var isDefault = this.name == "Default";
			var styles = this.skin.GetNamedStyles(false,true,true);
			var skin = this.skin;
			if(isDefault && !Theme.liveEdit){
				skin = theme.fontset.Apply(skin);
				theme.palette.Apply(skin);
			}
			foreach(var item in this.scopedStyles){
				if(styles.ContainsKey(item.Key)){
					var baseName = styles[item.Key].name.Parse("[","]");
					var replacement = styles[item.Key];
					var newStyle = Theme.liveEdit ? replacement : new GUIStyle(replacement).Rename(baseName);
					scope.SetVariable(item.Key,newStyle);
				}
			}
		}
	}
}
