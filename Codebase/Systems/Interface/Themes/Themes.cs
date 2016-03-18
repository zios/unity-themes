using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
namespace Zios{
	using Interface;
	#if UNITY_EDITOR
	using UnityEditor;
	#if UNITY_EDITOR_WIN
	using Microsoft.Win32;
	#endif
	public class Theme{
		[Internal] public string name;
		[Internal] public string path;
		[Internal] public List<Texture2D> colorImages = new List<Texture2D>();
		[Internal] public List<GUIStyle> colorStyles = new List<GUIStyle>();
		[Internal] public List<Theme> variants = new List<Theme>();
		[Internal] public bool useSystemColor;
		[Internal] public Color color = new Color(0.7f,0.7f,0.7f);
		[Internal] public RelativeColor colorDark = 0.8f;
		[Internal] public RelativeColor colorLight = 1.3f;
		public bool allowCustomization;
		public bool allowColorCustomization;
		public bool allowSystemColor;
		public bool useColorAssets = true;
		public Texture2D windowBackgroundOverride;
		public Font fontOverride;
		public float fontScale = 1;
		public float spacingScale = 1;
		public void Use(Theme other){
			this.UseVariables(other,typeof(InternalAttribute).AsList());
			//this.color = other.color;
			//this.colorDark = new RelativeColor(other.colorDark);
			//this.colorLight = new RelativeColor(other.colorLight);
		}
	}
	public class RelativeColor{
		public Color value;
		public float offset;
		public static implicit operator RelativeColor(string value){
			return value.IsNumber() ? new RelativeColor(value.ToFloat()) : new RelativeColor(value.ToColor());
		}
		public static implicit operator RelativeColor(float value){return new RelativeColor(value);}
		public static implicit operator RelativeColor(Color value){return new RelativeColor(value);}
		public static implicit operator Color(RelativeColor current){return current.value;}
		public static implicit operator string(RelativeColor current){return current.offset != 0 ? current.offset.Serialize() : current.value.Serialize();}
		public RelativeColor(float offset){this.offset = offset;}
		public RelativeColor(Color manual){this.value = manual;}
		public void Update(Color initial){this.value = this.offset != 0 ? initial.Multiply(this.offset) : this.value;}
	}
	public static class Themes{
		public static List<Theme> all = new List<Theme>();
		public static Theme active;
		[NonSerialized] public static int activeIndex;
		[NonSerialized] public static string storagePath = "Assets/@Zios/Interface/Skins/";
		[NonSerialized] public static bool setup;
		[NonSerialized] public static bool needsRefresh;
		[NonSerialized] public static bool needsRebuild;
		[NonSerialized] private static Color lastSystemColor = Color.clear;
		[NonSerialized] private static float nextUpdate;
		static Themes(){
			//SceneView.onSceneGUIDelegate += (a)=>Themes.Setup();
			EditorApplication.projectWindowItemOnGUI += (a,b)=>Themes.Setup();
			EditorApplication.hierarchyWindowItemOnGUI += (a,b)=>Themes.Setup();
			EditorApplication.update += ()=>{
				if(Time.realtimeSinceStartup < Themes.nextUpdate){return;}
				Themes.UpdateColors();
				Utility.RepaintAll();
				Themes.nextUpdate = Time.realtimeSinceStartup + 0.1f;
			};
			Themes.Load();
		}
		public static void Setup(){
			if(!Themes.setup && !EditorApplication.isCompiling){
				Themes.UpdateColors();
				Themes.Apply();
				Utility.RepaintAll();
				Themes.setup = true;
			}
			if(Themes.needsRefresh){
				Themes.UpdateSettings();
				Utility.RepaintAll();
				Themes.needsRefresh = false;
			}
			if(Themes.needsRebuild){
				Themes.UpdateSettings();
				Themes.RebuildStyles();
				Themes.needsRebuild = false;
				Themes.needsRefresh = true;
			}
		}
		public static void RebuildStyles(){
			foreach(var type in typeof(UnityEditor.Editor).Assembly.GetTypes()){
				if(type.IsNull()){continue;}
				type.ClearVariable("styles",ObjectExtension.staticFlags);
				type.ClearVariable("s_GOStyles",ObjectExtension.staticFlags);
				type.ClearVariable("s_Styles",ObjectExtension.staticFlags);
				type.ClearVariable("m_Styles",ObjectExtension.staticFlags);
				type.ClearVariable("ms_Styles",ObjectExtension.staticFlags);
				type.ClearVariable("constants",ObjectExtension.staticFlags);
			}
			Utility.GetUnityType("AppStatusBar").ClearVariable("background");
			var console = Utility.GetUnityType("ConsoleWindow.Constants");
			console.SetVariable("ms_Loaded",false);
			console.CallMethod("Init");
			typeof(EditorStyles).SetVariable<EditorStyles>("s_CachedStyles",null,0);
			typeof(EditorStyles).SetVariable<EditorStyles>("s_CachedStyles",null,1);
			typeof(EditorGUIUtility).CallMethod("SkinChanged");
			var buildWindow = Utility.GetUnityType("BuildPlayerWindow");
			var aboutWindow = Utility.GetUnityType("AboutWindow");
			foreach(var window in Resources.FindObjectsOfTypeAll<EditorWindow>()){
				if(window.GetType() == buildWindow || window.GetType() == aboutWindow){
					window.Close();
				}
			}
		}
		[MenuItem("Zios/Process/Theme/Refresh %2")]
		public static void Refresh(){
			Debug.Log("Force Theme Refresh");
			Themes.needsRebuild = true;
		}
		//=================================
		// Menu Preferences
		//=================================
		[PreferenceItem("Themes")]
		public static void ShowPreferences(){
			var active = Themes.active;
			var current = Themes.activeIndex;
			Themes.activeIndex = Themes.all.Select(x=>x.name).Draw(Themes.activeIndex,"Theme");
			GUILayout.Space(3);
			foreach(var toggle in active.variants.Where(x=>x.name.StartsWith("+"))){
				var toggleName = "EditorTheme-"+active.name+toggle.name.Remove("+");
				EditorPrefs.SetBool(toggleName,EditorPrefs.GetBool(toggleName).Draw(toggle.name.Remove("+")));
				GUILayout.Space(2);
			}
			if(active.allowColorCustomization){
				if(active.allowSystemColor){
					active.useSystemColor = active.useSystemColor.Draw("Use System Color");
					EditorPrefs.SetBool("EditorTheme-"+active.name+"-UseSystemColor",active.useSystemColor);
					if(GUI.changed && !active.useSystemColor){Themes.LoadColors();}
				}
				if(!active.useSystemColor){
					active.color = active.color.Draw("Color");
					active.colorDark.value = active.colorDark.value.Draw("Color Dark");
					active.colorLight.value = active.colorLight.value.Draw("Color Light");
					if(GUILayout.Button("Reset",GUILayout.Width(120))){Themes.LoadColors(true);}
					EditorPrefs.SetString("EditorTheme-"+active.name+"Color",active.color.Serialize());
					EditorPrefs.SetString("EditorTheme-"+active.name+"ColorDark",active.colorDark);
					EditorPrefs.SetString("EditorTheme-"+active.name+"ColorLight",active.colorLight);
				}
			}
			if(current != Themes.activeIndex){
				Themes.active = Themes.all[Themes.activeIndex].Clone();
				EditorPrefs.SetString("EditorTheme",Themes.active.name);
				Themes.UpdateSettings();
				Themes.RebuildStyles();
			}
			if(GUI.changed){
				//Themes.Create();
				Themes.lastSystemColor = Color.clear;
				Themes.needsRefresh = true;
			}
		}
		//=================================
		// Saving
		//=================================
		public static GUISkin[] Create(string name="@Default",bool save=true){
			GUISkin skin = ScriptableObject.CreateInstance<GUISkin>();
			var styles = new Dictionary<string,GUIStyle>();
			var sorted = new List<GUIStyle>();
			var skins = new List<GUISkin>();
			var savePath = Themes.storagePath+name;
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
				if(save && !File.Exists(savePath)){
					AssetDatabase.CreateAsset(skin,savePath+"/"+skin.name+".guiskin");
					//Styles.SaveCSS(savePath,skin);
				}
				skins.Add(skin);
				sorted.Clear();
				styles.Clear();
				skin = ScriptableObject.CreateInstance<GUISkin>();
			};
			if(save){Directory.CreateDirectory(savePath);}
			skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).Copy();
			SaveSkin("Inspector",false);
			styles = typeof(EditorStyles).GetVariable<EditorStyles>("s_Current").GetVariables<GUIStyle>(null,ObjectExtension.privateFlags);
			SaveSkin("EditorStyles",true);
			styles = Utility.GetUnityType("TreeViewGUI").GetVariable("s_Styles").GetVariables<GUIStyle>();
			SaveSkin("TreeView",true);
			styles = Utility.GetUnityType("GameObjectTreeViewGUI").GetVariable("s_GOStyles").GetVariables<GUIStyle>();
			SaveSkin("GameObjectTreeView",true);
			AssetDatabase.StopAssetEditing();
			return skins.ToArray();
		}
		//=================================
		// Loading
		//=================================
		public static void Load(){
			var configs = FileManager.FindAll("*.unitytheme");
			foreach(var file in configs){
				Theme theme = null;
				Theme root = null;
				foreach(var line in file.GetText().GetLines()){
					if(line.Trim().IsEmpty()){continue;}
					if(line.Contains("[")){
						string name = theme.IsNull() ? file.name : line.Parse("[","]");
						theme = root.IsNull() ? Themes.all.AddNew() : root.variants.AddNew();
						theme.name = name.ToPascalCase();
						theme.path = file.GetAssetPath().GetDirectory();
						if(root.IsNull()){root = theme;}
						if(root.variants.Count > 0){
							root.variants.Last().Use(root);
						}
						continue;
					}
					if(theme.IsNull()){continue;}
					var term = line.Parse(""," ").Trim();
					var value = line.Parse(" ").Trim();
					if(value.Matches("None",true)){continue;}
					else if(term.Matches("AllowSystemColor",true)){theme.allowSystemColor = value.ToBool();}
					else if(term.Matches("AllowCustomization",true)){theme.allowCustomization = value.ToBool();}
					else if(term.Matches("AllowColorCustomization",true)){theme.allowColorCustomization = value.ToBool();}
					else if(term.Matches("UseSystemColor",true)){theme.useSystemColor = value.ToBool();}
					else if(term.Matches("UseColorAssets",true)){theme.useColorAssets = value.ToBool();}
					else if(term.Matches("FontOverride",true)){theme.fontOverride = FileManager.GetAsset<Font>(value);}
					else if(term.Matches("WindowBackgroundOverride",true)){theme.windowBackgroundOverride = FileManager.GetAsset<Texture2D>(value);}
					else if(term.Matches("FontScale",true)){theme.fontScale = value.ToFloat();}
					else if(term.Matches("Color",true)){theme.color = value.ToColor();}
					else if(term.Matches("DarkColor",true)){theme.colorDark = value;}
					else if(term.Matches("LightColor",true)){theme.colorLight = value;}
				}
			}
			var activeThemeName = EditorPrefs.GetString("EditorTheme","@Default");
			Themes.activeIndex = Themes.all.FindIndex(x=>x.name==activeThemeName).Max(0);
			Themes.active = Themes.all[Themes.activeIndex].Clone();
			Themes.UpdateSettings();
		}
		public static void LoadColors(bool reset=false){
			if(!active.useSystemColor){
				if(!reset && active.allowColorCustomization){
					active.color = EditorPrefs.GetString("EditorTheme-"+active.name+"Color",active.color.Serialize()).Deserialize<Color>();
					active.colorDark = EditorPrefs.GetString("EditorTheme-"+active.name+"ColorDark",active.colorDark);
					active.colorLight = EditorPrefs.GetString("EditorTheme-"+active.name+"ColorLight",active.colorLight);
				}
				else{
					var original = Themes.all[Themes.activeIndex];
					active.color = original.color;
					active.colorDark = original.colorDark.Clone();
					active.colorLight = original.colorLight.Clone();
				}
			}
		}
		//=================================
		// Updating
		//=================================
		public static void UpdateColors(){
			if(Themes.active.allowSystemColor && Themes.active.useSystemColor){
				Themes.active.colorDark.offset = 0.8f;
				Themes.active.colorLight.offset = 1.3f;
				Color color;
				int systemColor = -1;
				#if UNITY_EDITOR_WIN
				systemColor = Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\DWM\\","AccentColor","-1").As<int>();
				#endif
				color = systemColor != -1 ? systemColor.ToHex().ToColor(true) : Themes.active.color;
				if(color != Themes.lastSystemColor){
					Themes.lastSystemColor = color;
					Themes.active.color = color;
					Themes.active.colorDark.Update(Themes.active.color);
					Themes.active.colorLight.Update(Themes.active.color);
					Themes.UpdateSettings();
				}
				return;
			}
			Themes.active.colorDark.Update(Themes.active.color);
			Themes.active.colorLight.Update(Themes.active.color);
		}
		public static void UpdateSettings(){
			var active = Themes.active;
			active.Use(Themes.all[Themes.activeIndex]);
			active.useSystemColor = EditorPrefs.GetBool("EditorTheme-"+active.name+"-UseSystemColor",false);
			Themes.LoadColors();
			Themes.UpdateColors();
			active.colorStyles.Clear();
			active.colorImages.Clear();
			foreach(var toggle in active.variants.Where(x=>x.name.StartsWith("+"))){
				var toggleName = "EditorTheme-"+active.name+toggle.name.Remove("+");
				if(EditorPrefs.GetBool(toggleName)){
					active.Use(toggle);
				}
			}			
			Themes.UpdateAssets("ThemeColor",Themes.active.color);
			Themes.UpdateAssets("ThemeColorDark",Themes.active.colorDark);
			Themes.UpdateAssets("ThemeColorLight",Themes.active.colorLight);
			Themes.Apply();
		}
		public static void UpdateAssets(string colorName,Color color){
			if(!Themes.active.useColorAssets){return;}
			var imagePath = Themes.active.path+"/"+Themes.active.name+colorName+".png";
			var borderPath = Themes.active.path+"/"+Themes.active.name+colorName+"Border.png";
			var image = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
			var border = AssetDatabase.LoadAssetAtPath<Texture2D>(borderPath);
			if(image.IsNull()){
				image = new Texture2D(1,1);
				image.SaveAs(imagePath);
				AssetDatabase.ImportAsset(imagePath);
			}
			if(border.IsNull()){
				border = new Texture2D(3,3);
				border.SaveAs(borderPath);
				AssetDatabase.ImportAsset(borderPath);
			}
			Themes.active.colorStyles.AddNew().GetStates().ForEach(x=>x.background = image);
			Themes.active.colorImages.AddNew(image);
			image.SetPixel(0,0,color);
			image.Apply();
			border.SetPixels(new Color[]{color,color,color,color,Color.clear,color,color,color,color});
			border.Apply();
			image.SaveAs(imagePath);
			border.SaveAs(borderPath);
		}
		public static void Apply(string name=""){
			if(name.IsEmpty()){name = Themes.active.name;}
			GUISkin skin;
			object styles;
			skin = Style.GetSkin(name+"-EditorStyles",false);
			if(!skin.IsNull()){
				styles = typeof(EditorStyles).GetVariable("s_Current");
				styles.SetValuesByType<GUIStyle>(skin.customStyles,null,ObjectExtension.privateFlags);
				if(name != "@Default"){
					Utility.GetUnityType("PreferencesWindow").GetVariable("constants").SetVariable("sectionHeader",skin.GetStyle("m_LargeLabel"));
				}
			}
			skin = Style.GetSkin(name+"-TreeView",false);
			if(!skin.IsNull()){
				styles = Utility.GetUnityType("TreeViewGUI").GetVariable("s_Styles");
				styles.SetValuesByType<GUIStyle>(skin.customStyles);
			}
			skin = Style.GetSkin(name+"-GameObjectTreeView",false);
			if(!skin.IsNull()){
				styles = Utility.GetUnityType("GameObjectTreeViewGUI").GetVariable("s_GOStyles");
				styles.SetValuesByType<GUIStyle>(skin.customStyles);
			}
			skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
			skin.Use(Style.GetSkin(name+"-Inspector",false));
			skin.settings.selectionColor = skin.settings.cursorColor = Themes.active.colorDark;
			skin.GetStyle("TabWindowBackground").normal.background = Themes.active.windowBackgroundOverride;
			typeof(EditorGUIUtility).SetVariable("kDarkViewBackground",Themes.active.color);
			typeof(EditorGUI).SetVariable("kCurveColor",Themes.active.colorDark.value);
			typeof(EditorGUI).SetVariable("kCurveBGColor",Themes.active.colorLight.value);
			var hostView = Utility.GetUnityType("HostView");
			hostView.SetVariable<Color>("kViewColor",Themes.active.color);
			foreach(var view in Resources.FindObjectsOfTypeAll(hostView)){
				view.ClearVariable("background");
			}
			foreach(var window in Locate.GetAssets<EditorWindow>()){
				window.antiAlias = 1;
				window.minSize = new Vector2(100,20);
				window.wantsMouseMove = true;
				window.autoRepaintOnSceneChange = true;
			}
		}
	}
	public class ColorImportSettings : AssetPostprocessor{
		public void OnPreprocessTexture(){
			TextureImporter importer = (TextureImporter)this.assetImporter;
			if(importer.assetPath.Contains("ThemeColor")){
				importer.isReadable = true;
				if(importer.assetPath.Contains("Border")){
					importer.filterMode = FilterMode.Point;
				}
			}
		}
	}
	#endif
}