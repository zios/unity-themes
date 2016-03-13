using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEvent = UnityEngine.Event;
namespace Zios{
	using Interface;
	#if UNITY_EDITOR
	using UnityEditor;
	public static partial class Style{
		public static GUISkin[] defaultTheme;
		public static Color themeColor = new Color(0.7f,0.7f,0.7f,1);
		public static string[] themeNames;
		public static Font themeFont;
		public static string themeName = "@Default";
		public static string themePath = "Assets/@Zios/Interface/Skins/";
		public static float themeLineHeight = 16;
		public static Dictionary<Color,Texture2D> colorImages = new Dictionary<Color,Texture2D>();
		public static Dictionary<Color,GUIStyle> colorStyles = new Dictionary<Color,GUIStyle>();
		[NonSerialized] public static bool setup;
		[NonSerialized] public static int themeIndex = -1;
		[NonSerialized] public static int themeFontIndex = 0;
		static Style(){
			EditorApplication.update += ()=>{
				if(Style.setup){
					Utility.RepaintAll();
				}
			};
			EditorApplication.projectWindowItemOnGUI += (a,b)=>Style.ValidateTheme();
			EditorApplication.hierarchyWindowItemOnGUI += (a,b)=>Style.ValidateTheme();
		}
		public static void Refresh(){
			/*if(Style.defaultTheme.IsNull()){
				Style.defaultTheme = Style.BuildThemes("@Default",false);
			}*/
			//Style.themeNames = "Default".AsArray().Concat(FileManager.GetAssets<GUISkin>("*-EditorStyles.guiskin").Select(x=>x.name.Split("-")[0]).Skip(1).ToArray());
			Style.themeNames = FileManager.GetAssets<GUISkin>("*-EditorStyles.guiskin").Select(x=>x.name.Split("-")[0]).ToArray();
			Style.themeName = EditorPrefs.GetString("EditorTheme","@Default");
			Style.themeColor = EditorPrefs.GetString("EditorColor","0.7-0.7-0.7-1").Deserialize<Color>();
			Style.themeIndex = Style.themeNames.IndexOf(Style.themeName);
			Style.themeIndex = Math.Max(0,Style.themeIndex);
		}
		public static void ValidateTheme(){
			if(!Style.setup){
				Style.Refresh();
				Style.LoadTheme();
				Utility.RepaintAll();
				Style.setup = true;
			}
			if(UnityEvent.current.type == EventType.Repaint){
				//Style.LoadTheme();
			}
		}
		public static void SelectTheme(){
			var fonts = FileManager.GetAssets<Font>("*.ttf");
			var current = Style.themeName;
			Style.themeIndex = Style.themeNames.Draw(Style.themeIndex,"Theme");
			Style.themeName = Style.themeNames[Style.themeIndex];
			Style.themeColor = Style.themeColor.Draw("Theme Color");
			Style.themeFontIndex = fonts.Select(x=>x.name).ToArray().Draw(Style.themeFontIndex,"Theme Font").Max(0);
			Style.themeFont = fonts[Style.themeFontIndex];
			EditorPrefs.SetString("EditorTheme",Style.themeName);
			EditorPrefs.SetString("EditorColor",Style.themeColor.Serialize());
			if(current != Style.themeName){
				var path = Style.themePath+Style.themeName+"/"+Style.themeName+"ThemeColor.png";
				var image = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
				if(!image.IsNull()){Style.themeColor = image.GetPixel(0,0);}
			}
			if(GUI.changed){
				Style.setup = false;
				//Style.BuildThemes();
				Style.LoadTheme();
				Utility.RepaintAll();
			}
		}
		//=================================
		// Saving
		//=================================
		public static GUISkin[] BuildThemes(string name="@Default",bool save=true){
			GUISkin skin = ScriptableObject.CreateInstance<GUISkin>();
			var styles = new Dictionary<string,GUIStyle>();
			var sorted = new List<GUIStyle>();
			var skins = new List<GUISkin>();
			var savePath = Style.themePath+name;
			AssetDatabase.StartAssetEditing();
			Action<string,bool> SaveSkin = (type,sort)=>{
				skin.name = name+"-"+type;
				if(sort){
					foreach(var item in styles){
						var style = new GUIStyle(item.Value).Rename(item.Key);
						sorted.Add(style);
					}
				}
				if(sorted.Count > 0){skin.customStyles = sorted.ToArray();}
				if(save){
					AssetDatabase.CreateAsset(skin,savePath+"/"+skin.name+".guiskin");
					//Style.SaveCSS(savePath,skin);
				}
				skins.Add(skin);
				sorted.Clear();
				styles.Clear();
				skin = ScriptableObject.CreateInstance<GUISkin>();
			};
			if(save){Directory.CreateDirectory(savePath);}
			//==================
			// Inspector
			//==================
			skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).Copy();
			SaveSkin("Inspector",false);
			//==================
			// Game
			//==================
			skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Game).Copy();
			SaveSkin("Game",false);
			//==================
			// Scene
			//==================
			skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).Copy();
			SaveSkin("Scene",false);
			//==================
			// InspectorWindow
			//==================
			styles = Utility.GetInternalType("InspectorWindow").GetVariable("s_Styles").GetVariables<GUIStyle>();
			SaveSkin("InspectorWindow",true);
			//==================
			// Styles
			//==================
			styles = typeof(Editor).GetVariable("s_Styles").GetVariables<GUIStyle>();
			SaveSkin("Styles",true);
			//==================
			// Hierarchy
			//==================
			styles = Utility.GetInternalType("SceneHierarchySortingWindow").GetVariable("s_Styles").GetVariables<GUIStyle>();
			SaveSkin("Hierarchy",true);
			//==================
			// ProjectBrowser
			//==================
			styles = Utility.GetInternalType("ProjectBrowser").GetVariable("s_Styles").GetVariables<GUIStyle>();
			SaveSkin("ProjectBrowser",true);
			//==================
			// TreeView
			//==================
			styles = Utility.GetInternalType("TreewViewGUI").GetVariable("s_Styles").GetVariables<GUIStyle>();
			SaveSkin("TreeView",true);
			//==================
			// EditorStyles
			//==================
			styles = typeof(EditorStyles).GetVariable<EditorStyles>("s_Current").GetVariables<GUIStyle>(null,ObjectExtension.privateFlags);
			SaveSkin("EditorStyles",true);
			//==================
			// Assorted
			//==================
			sorted.Add(typeof(GUILayoutUtility).GetVariable<GUIStyle>("s_SpaceStyle").Rename("s_SpaceStyle"));
			SaveSkin("Assorted",false);
			AssetDatabase.StopAssetEditing();
			//==================
			// Textures
			//==================
			/*if(save){
				foreach(var image in Locate.GetAssets<Texture2D>()){
					image.SaveAs(savePath+"/Images/"+image.name+".png");
				}
			}*/
			return skins.ToArray();
		}
		//=================================
		// Loading
		//=================================
		public static void LoadColors(string name=""){
			var path = Style.themePath+name+"/";
			Directory.CreateDirectory(path);
			Action<string,Color> BuildColor = (colorName,color)=>{
				var imagePath = path+Style.themeName+colorName+".png";
				var image = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
				var style = Style.colorStyles.AddNew(color);
				if(image.IsNull()){
					image = new Texture2D(1,1);
					image.SaveAs(imagePath);
					AssetDatabase.ImportAsset(imagePath);
				}
				Style.colorImages[color] = image;
				image.SetPixel(0,0,color);
				image.Apply();
				style.GetStates().ForEach(x=>x.background = image);
			};
			BuildColor("ThemeColor",Style.themeColor);
			BuildColor("ThemeColorDark",Style.themeColor.Multiply(0.8f));
			BuildColor("ThemeColorLight",Style.themeColor.Multiply(1.3f));
		}
		[MenuItem("Zios/Process/Theme/Refresh %2")]
		public static void LoadDefaultTheme(){
			Style.skins.Clear();
			Style.LoadTheme();
		}
		public static void LoadTheme(string name=""){
			if(name.IsEmpty()){name = Style.themeName;}
			GUISkin skin;
			Style.LoadColors(name);
			var colorStyle = Style.colorStyles[Style.themeColor];
			var darkColorImage = Style.colorImages[Style.themeColor.Multiply(0.8f)];
			//==================
			// Inspector
			//==================
			skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
			skin.Use(Style.GetSkin(name+"-Inspector",false));
			skin.GetStyle("AppToolbar").normal.background = darkColorImage;
			skin.GetStyle("dockarea").normal.background = darkColorImage;
			/*GUI.skin.CallMethod("MakeCurrent");
			typeof(GUI).SetVariable("s_Skin",GUI.skin);
			typeof(GUISkin).SetVariable("current",GUI.skin);*/
			//==================
			// Game
			//==================
			skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Game);
			skin.Use(Style.GetSkin(name+"-Game",false));
			//==================
			// Scene
			//==================
			skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);
			skin.Use(Style.GetSkin(name+"-Scene",false));
			//==================
			// InspectorWindow
			//==================
			skin = Style.GetSkin(name+"-InspectorWindow",false);
			if(!skin.IsNull()){
				var inspectorStyles = Utility.GetInternalType("InspectorWindow").GetVariable("s_Styles");
				inspectorStyles.SetValuesByType<GUIStyle>(skin.customStyles);
			}
			//==================
			// Styles
			//==================
			skin = Style.GetSkin(name+"-Styles",false);
			if(!skin.IsNull()){
				var styles = typeof(Editor).GetVariable("s_Styles");
				styles.SetVariable("m_font",Style.themeFont);
				styles.SetValuesByType<GUIStyle>(skin.customStyles);
			}
			//==================
			// Hierarchy
			//==================
			skin = Style.GetSkin(name+"-Hierarchy",false);
			if(!skin.IsNull()){
				var hierarchyStyles = Utility.GetInternalType("SceneHierarchySortingWindow").GetVariable("s_Styles");
				hierarchyStyles.SetValuesByType<GUIStyle>(skin.customStyles);
			}
			//==================
			// ProjectBrowser
			//==================
			skin = Style.GetSkin(name+"-ProjectBrowser",false);
			if(!skin.IsNull()){
				var projectStyles = Utility.GetInternalType("ProjectBrowser").GetVariable("s_Styles");
				projectStyles.SetValuesByType<GUIStyle>(skin.customStyles);
			}
			//==================
			// TreeView
			//==================
			skin = Style.GetSkin(name+"-TreeView",false);
			if(!skin.IsNull()){
				var treeStyles = Utility.GetInternalType("TreewViewGUI").GetVariable("s_Styles");
				treeStyles.SetValuesByType<GUIStyle>(skin.customStyles);
			}
			//==================
			// EditorStyles
			//==================
			skin = Style.GetSkin(name+"-EditorStyles",false);
			if(!skin.IsNull()){
				var editorStyles = typeof(EditorStyles).GetVariable<EditorStyles>("s_Current");
				editorStyles.SetValuesByType<GUIStyle>(skin.customStyles,null,ObjectExtension.privateFlags);
			}
			//==================
			// Assorted
			//==================
			skin = Style.GetSkin(name+"-Assorted",false);
			if(!skin.IsNull()){
				typeof(GUILayoutUtility).SetVariable("s_SpaceStyle",skin.GetStyle("s_SpaceStyle"));
			}
			var hostView = Utility.GetInternalType("HostView");
			hostView.SetVariable<Color>("kViewColor",Style.themeColor);
			typeof(GUI).CallMethod("INTERNAL_set_backgroundColor",Style.themeColor);
			typeof(EditorGUIUtility).SetVariable("kDarkViewBackground",Style.themeColor.Multiply(0.5f));
			typeof(EditorGUI).SetVariable("kCurveColor",Color.black);
			typeof(EditorGUI).SetVariable("kCurveBGColor",Color.white);
			Utility.GetInternalType("ContainerWindow+Styles").SetValuesByType<GUIStyle>(colorStyle.AsArray(4));
			foreach(var view in Locate.GetAssets(hostView)){
				view.SetVariable<GUIStyle>("background",colorStyle);
			}
			foreach(var window in Locate.GetAssets<EditorWindow>()){
				window.antiAlias = 1;
				window.minSize = new Vector2(20,20);
				window.wantsMouseMove = true;
				window.autoRepaintOnSceneChange = true;
				//window.titleContent = new GUIContent("Peeep");
			}
			//Debug.Log("[Style] Theme loaded -- " + name);
		}
	}
	public class ColorImportSettings : AssetPostprocessor{
		public void OnPreprocessTexture(){
			TextureImporter importer = (TextureImporter)this.assetImporter;
			if(importer.assetPath.Contains("ThemeColor")){
				importer.isReadable = true;
			}
		}
	}
	#endif
}