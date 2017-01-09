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
		public bool active = true;
		public string name;
		public string path;
		public ThemeSkin main;
		public List<ThemeSkin> skins = new List<ThemeSkin>();
		public List<ThemeSkinset> variants = new List<ThemeSkinset>();
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
			var isVariant = path.GetPathTerm().Contains("+");
			skinset.name = path.GetPathTerm().Remove("+");
			skinset.path = path;
			foreach(var skinFile in FileManager.FindAll(path+"/*.guiskin",true,false)){
				if(!isVariant && skinFile.path.Contains("/+")){continue;}
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
				active.GetScope = ()=>{return !typeDirect.IsNull() ? typeDirect : typeParent.InstanceVariable(field);};
				active.scopedStyles = !typeDirect.IsNull() ? active.GetScope().GetVariables<GUIStyle>(null,flags) : active.GetScope().GetVariables<GUIStyle>();
			}
			foreach(var variantPath in Directory.GetDirectories(path).Where(x=>x.Contains("+"))){
				var variant = ThemeSkinset.Import(variantPath);
				variant.active = false;
				skinset.variants.Add(variant);
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
				EditorPrefs.SetString("EditorTheme"+Theme.suffix,theme.name);
				Theme.setup = false;
				Theme.loaded = false;
			}
		}
		public void Apply(Theme theme){
			this.ApplyMain(theme);
			foreach(var skin in this.skins){
				skin.Apply(theme);
			}
			foreach(var variant in this.variants.Where(x=>x.active)){
				variant.Apply(theme);
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
				ThemeSkin.RemoveHover(main);
			}
			skin.Use(main,!Theme.liveEdit);
			EditorGUIUtility.GetBuiltinSkin(EditorSkin.Game).Use(skin,!Theme.liveEdit);
			EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).Use(skin,!Theme.liveEdit);
			Utility.GetUnityType("AppStatusBar").ClearVariable("background");
			Utility.GetUnityType("SceneRenderModeWindow.Styles").SetVariable("sMenuItem",skin.Get("MenuItem"));
			Utility.GetUnityType("SceneRenderModeWindow.Styles").SetVariable("sSeparator",skin.Get("sv_iconselector_sep"));
			Utility.GetUnityType("GameView.Styles").SetVariable("gizmoButtonStyle",skin.Get("GV Gizmo DropDown"));
			typeof(SceneView).SetVariable<GUIStyle>("s_DropDownStyle",skin.Get("GV Gizmo DropDown"));
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
		public Func<object> GetScope;
		public GUISkin skin;
		public string name;
		public string path;
		public void Apply(Theme theme){
			var isDefault = this.name == "Default";
			var skin = this.skin;
			if(!isDefault && !Theme.liveEdit){
				skin = theme.fontset.Apply(skin);
				theme.palette.Apply(skin);
				ThemeSkin.RemoveHover(skin);
			}
			var styles = skin.GetNamedStyles(false,true,true);
			foreach(var item in this.scopedStyles){
				if(styles.ContainsKey(item.Key)){
					var baseName = styles[item.Key].name.Parse("[","]");
					var replacement = styles[item.Key];
					var newStyle = Theme.liveEdit ? replacement : new GUIStyle(replacement).Rename(baseName);
					this.GetScope().SetVariable(item.Key,newStyle);
				}
			}
		}
		public static void RemoveHover(GUISkin skin){
			if(Theme.hoverResponse != HoverResponse.None){return;}
			foreach(var style in skin.GetStyles()){
				style.hover = style.normal;
			}
		}
	}
}