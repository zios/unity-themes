using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
namespace Zios.Interface{
	using Events;
	using UnityEngine;
	using UnityEditor;
	[Serializable]
	public class ThemeSkinset{
		public static string dumpPath;
		public static Dictionary<string,object> dumpBuffer = new Dictionary<string,object>();
		public static List<ThemeSkinset> all = new List<ThemeSkinset>();
		public bool active = true;
		public string name;
		public string path;
		public List<ThemeSkin> main = new List<ThemeSkin>();
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
				var filter = skinFile.name.Contains("#") ? skinFile.name.Parse("#",".") : "";
				var skinName = skinFile.name.Remove("#"+filter);
				if(skinName == skinset.name){
					skinset.skins.Remove(active);
					active = skinset.main.AddNew();
				}
				active.name = skinName;
				active.path = skinFile.path;
				active.skin = skinFile.GetAsset<GUISkin>();
				if(active.skin.IsNull()){
					Debug.LogWarning("[Themes] GUISkin could not be loaded. This usually occurs when the guiSkin was saved as binary in a newer version.");
					skinset.skins.Remove(active);
					continue;
				}
				active.skinset = skinset;
				var field = skinName.Split(".").Last();
				var parent = skinName.Replace("."+field,"");
				var typeDirect = Utility.GetUnityType(skinName);
				var typeParent = Utility.GetUnityType(parent);
				var flags = field.Contains("s_Current") ? ObjectExtension.privateFlags : ObjectExtension.staticFlags;
				if(typeDirect.IsNull() && (typeParent.IsNull() || !typeParent.HasVariable(field))){
					if(Theme.debug){Debug.LogWarning("[Themes] No matching class/field found for GUISkin -- " + skinFile.name + ". Possible version conflict.");}
					continue;
				}
				active.GetScope = ()=>{return !typeDirect.IsNull() ? typeDirect : typeParent.InstanceVariable(field);};
				active.scopedStyles = !typeDirect.IsNull() ? active.GetScope().GetVariables<GUIStyle>(null,flags) : active.GetScope().GetVariables<GUIStyle>();
			}
			foreach(var variantPath in Directory.GetDirectories(path).Where(x=>x.GetPathTerm().Contains("+"))){
				var variant = ThemeSkinset.Import(variantPath);
				variant.active = false;
				skinset.variants.Add(variant);
			}
			return skinset;
		}
		public void Export(string path=null){}
		public string Serialize(){return "";}
		public void Deserialize(string data){}
		//=================================
		// Utilities
		//=================================
		public void Apply(Theme theme){
			foreach(var skin in this.main){skin.ApplyMain(theme);}
			foreach(var skin in this.skins){skin.Apply(theme);}
			foreach(var variant in this.variants.Where(x=>x.active)){variant.Apply(theme);}
		}
		//=================================
		// Dump
		//=================================
		[MenuItem("Edit/Themes/Development/Dump/GUISkin")]
		public static void Dump(){
			var savePath = EditorUtility.SaveFilePanel("Dump GUISkin",Theme.storagePath,"Default","guiSkin").GetAssetPath();
			var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
			skin = ScriptableObject.CreateInstance<GUISkin>().Use(skin);
			AssetDatabase.CreateAsset(skin,savePath);
		}
		[MenuItem("Edit/Themes/Development/Dump/GUISkin [Extended]")]
		public static void DumpExtended(){
			var warning = "Dumping all GUISkin will deep scan the editor assembly for GUIStyles.  ";
			warning += "This will produce warnings/errors and could cause Unity to become unresponsible at end of operation.  Continue?";
			if(EditorUtility.DisplayDialog("Scan/Dump All GUIStyles?",warning,"Yes","Cancel")){
				ThemeSkinset.dumpPath = EditorUtility.SaveFolderPanel("Dump GUISkin [Extended]",Theme.storagePath,"Default");
				var allTypes = typeof(Editor).Assembly.GetTypes().Where(x=>!x.IsNull()).ToArray();
				var stepper = new EventStepper(ThemeSkinset.DumpExtendedStep,ThemeSkinset.DumpExtendedComplete,allTypes,50);
				EditorApplication.update += stepper.Step;
			}
		}
		public static void DumpExtendedStep(object collection,int itemIndex){
			var types = (Type[])collection;
			var type = types[itemIndex];
			if(type.IsGeneric()){return;}
			if(!type.Name.ContainsAny("$","__Anon","<","AudioMixerDraw")){
				EventStepper.title = "Scanning " + types.Length + " Types";
				EventStepper.message = "Analyzing : " + type.Name;
				var terms = new string[]{"Styles","styles","s_GOStyles","s_Current","s_Styles","m_Styles","ms_Styles","constants","s_Defaults"};
				foreach(var term in terms){
					if(!type.HasVariable(term,ObjectExtension.staticFlags)){continue;}
					if(!type.GetProperty(term).IsNull()){continue;}
					ThemeSkinset.dumpBuffer[type.FullName+"."+term] = type.InstanceVariable(term,-1,ObjectExtension.staticFlags);
				}
				var styles = type.GetVariables<GUIStyle>(null,ObjectExtension.staticFlags);
				if(styles.Count > 0){ThemeSkinset.dumpBuffer[type.FullName] = styles;}
			}
		}
		public static void DumpExtendedComplete(){
			var path = ThemeSkinset.dumpPath;
			var savePath = path.GetAssetPath();
			var themeName = savePath.Split("/").Last();
			AssetDatabase.StartAssetEditing();
			EditorUtility.ClearProgressBar();
			EditorApplication.update -= EventStepper.active.Step;
			foreach(var buffer in ThemeSkinset.dumpBuffer){
				var skinPath = savePath+"/"+buffer.Key+".guiskin";
				var customStyles = new List<GUIStyle>();
				var styles = buffer.Value is Dictionary<string,GUIStyle> ? (Dictionary<string,GUIStyle>)buffer.Value : buffer.Value.GetVariables<GUIStyle>().Distinct();
				foreach(var styleData in styles){
					var style = new GUIStyle(styleData.Value);
					if(buffer.Key.Contains(".")){style.Rename(styleData.Key + " ["+style.name+"]");}
					customStyles.Add(style);
				}
				if(customStyles.Count > 0){
					GUISkin newSkin = ScriptableObject.CreateInstance<GUISkin>();
					newSkin.name = buffer.Key;
					newSkin.customStyles = customStyles.ToArray();
					FileManager.Create(path);
					AssetDatabase.CreateAsset(newSkin,skinPath);
				}
			}
			var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
			skin = ScriptableObject.CreateInstance<GUISkin>().Use(skin);
			AssetDatabase.CreateAsset(skin,savePath+"/"+themeName+".guiskin");
			AssetDatabase.StopAssetEditing();
			ThemeSkinset.dumpBuffer.Clear();
		}
		[MenuItem("Edit/Themes/Development/Dump/Target/GUISkin Assets + Builtin")]
		public static void DumpAssetsAll(){ThemeSkinset.DumpAssets("",true);}
		[MenuItem("Edit/Themes/Development/Dump/Target/GUISkin Assets")]
		public static void DumpAssets(){ThemeSkinset.DumpAssets("");}
		public static void DumpAssets(string path,bool includeBuiltin=false){
			path = path.IsEmpty() ? EditorUtility.SaveFolderPanel("Dump GUISkin Assets",Theme.storagePath,"").GetAssetPath() : path;
			var files = FileManager.FindAll(path+"/*.guiSkin");
			FileManager.Create(path+"/Background");
			FileManager.Create(path+"/Font");
			AssetDatabase.StartAssetEditing();
			foreach(var file in files){
				var guiSkin = file.GetAsset<GUISkin>();
				guiSkin.SaveFonts(path+"/Font",includeBuiltin);
				guiSkin.SaveBackgrounds(path+"/Background",includeBuiltin);
			}
			AssetDatabase.StopAssetEditing();
		}
	}
	[Serializable]
	public class ThemeSkin{
		public Dictionary<string,GUIStyle> scopedStyles = new Dictionary<string,GUIStyle>();
		public Func<object> GetScope;
		public GUISkin skin;
		public string name;
		public string path;
		public ThemeSkinset skinset;
		public void Apply(Theme theme){
			var isDefault = this.skinset.name == "Default";
			var skin = this.skin;
			if(!isDefault){
				skin = theme.fontset.Apply(skin);
				theme.palette.Apply(skin);
				ThemeSkin.RemoveHover(skin);
			}
			var styles = skin.GetNamedStyles(false,true,true);
			foreach(var item in this.scopedStyles){
				if(styles.ContainsKey(item.Key)){
					var baseName = styles[item.Key].name.Parse("[","]");
					var replacement = styles[item.Key];
					var newStyle = new GUIStyle(replacement).Rename(baseName);
					this.GetScope().SetVariable(item.Key,newStyle);
				}
			}
		}
		public void ApplyMain(Theme theme){
			if(this.skin.IsNull()){return;}
			var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
			var isFragment = this.path.Contains("#");
			var isDefault = this.skinset.name == "Default";
			var main = this.skin;
			var palette = theme.palette;
			if(!isDefault){
				main = theme.fontset.Apply(main);
				theme.palette.Apply(main);
				ThemeSkin.RemoveHover(main);
			}
			skin.Use(main,!isFragment,true);
			EditorGUIUtility.GetBuiltinSkin(EditorSkin.Game).Use(skin,!isFragment,true);
			EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).Use(skin,!isFragment,true);
			var collabStyle = isDefault ? skin.Get("DropDown").Padding(24,14,2,3) : skin.Get("DropDown");
			Utility.GetUnityType("Toolbar.Styles").GetVariable<GUIStyle>("collabButtonStyle").Use(collabStyle);
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
				window.minSize = new Vector2(100,20);
			}
			if(!isDefault){
				Utility.GetUnityType("PreferencesWindow").InstanceVariable("constants").SetVariable("sectionHeader",skin.Get("HeaderLabel"));
			}
			Utility.GetUnityType("BuildPlayerWindow").InstanceVariable("styles").SetVariable("toggleSize",new Vector2(24,16));
		}
		public static void RemoveHover(GUISkin skin){
			if(Theme.hoverResponse != HoverResponse.None){return;}
			foreach(var style in skin.GetStyles()){
				style.hover = style.normal;
			}
		}
	}
}