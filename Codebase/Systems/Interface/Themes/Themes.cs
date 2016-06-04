using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace Zios{
	using Events;
	using Interface;
	#if UNITY_EDITOR
	using UnityEditor;
	#if UNITY_EDITOR_WIN
	using Microsoft.Win32;
	#endif
	[InitializeOnLoad]
	public static partial class Themes{
		public static List<ThemePalette> palettes = new List<ThemePalette>();
		public static List<Theme> all = new List<Theme>();
		public static Theme active;
		[NonSerialized] public static int themeIndex;
		[NonSerialized] public static int paletteIndex;
		[NonSerialized] public static string storagePath;
		[NonSerialized] public static bool setup;
		[NonSerialized] public static bool needsRefresh;
		[NonSerialized] public static bool needsRebuild;
		[NonSerialized] private static Color lastSystemColor = Color.clear;
		[NonSerialized] private static float nextUpdate;
		//[NonSerialized] public static float verticalSpacing = 2.0f;
		static Themes(){
			//SceneView.onSceneGUIDelegate += (a)=>Themes.Setup();
			EditorApplication.projectWindowItemOnGUI += (a,b)=>Themes.Setup();
			EditorApplication.hierarchyWindowItemOnGUI += (a,b)=>Themes.Setup();
			EditorApplication.update += ()=>{
				if(Time.realtimeSinceStartup < Themes.nextUpdate || !Themes.setup){return;}
				Themes.UpdateColors();
				Utility.RepaintAll();
				Themes.nextUpdate = Time.realtimeSinceStartup + 0.1f;
			};
			Event.Add("On Asset Changed",()=>{Themes.setup = false;});
		}
		public static void Setup(){
			if(Themes.needsRefresh){
				Themes.UpdateSettings();
				Utility.RepaintAll();
				Themes.needsRefresh = false;
			}
			if(Themes.needsRebuild){
				Themes.UpdateSettings();
				Themes.RebuildStyles(true);
				Themes.needsRebuild = false;
				Themes.needsRefresh = true;
			}
			if(!Themes.setup && !EditorApplication.isCompiling){
				Themes.storagePath = FileManager.Find("*.unitytheme").GetFolderPath().Trim("/","\\").GetDirectory()+"/";
				Themes.Load();
				Themes.UpdateSettings();
				Themes.UpdateColors();
				Themes.Apply();
				Utility.RepaintAll();
				if(!Themes.setup){
					Themes.setup = true;
					Themes.needsRebuild = true;
				}
			}
		}
		public static void RebuildStyles(bool skipTree=false){
			foreach(var type in typeof(UnityEditor.Editor).Assembly.GetTypes()){
				if(skipTree && type.Name == "TreeViewGUI"){continue;}
				if(type.IsNull()){continue;}
				type.ClearVariable("Styles",ObjectExtension.staticFlags);
				type.ClearVariable("styles",ObjectExtension.staticFlags);
				type.ClearVariable("s_GOStyles",ObjectExtension.staticFlags);
				type.ClearVariable("s_Current",ObjectExtension.staticFlags);
				type.ClearVariable("s_Styles",ObjectExtension.staticFlags);
				type.ClearVariable("m_Styles",ObjectExtension.staticFlags);
				type.ClearVariable("ms_Styles",ObjectExtension.staticFlags);
				type.ClearVariable("constants",ObjectExtension.staticFlags);
			}
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
			Themes.setup = false;
		}
		//=================================
		// Menu Preferences
		//=================================
		[PreferenceItem("Themes")]
		public static void ShowPreferences(){
			var theme = Themes.active;
			var current = Themes.themeIndex;
			if(theme.IsNull()){return;}
			Themes.themeIndex = Themes.all.Select(x=>x.name).Draw(Themes.themeIndex,"Theme");
			GUILayout.Space(3);
			if(theme.allowCustomization){
				bool altered = !theme.palette.Matches(Themes.palettes[Themes.paletteIndex]);
				if(theme.allowColorCustomization){
					var paletteNames = Themes.palettes.Select(x=>x.name).ToList();
					if(altered){paletteNames[Themes.paletteIndex] = "[Custom]";}
					GUI.enabled = !theme.useSystemColor;
					Themes.paletteIndex = paletteNames.Draw(Themes.paletteIndex,"Palette");
					GUI.enabled = true;
					GUILayout.Space(3);
					if(EditorGUIExtension.lastChanged){
						var selectedPalette = Themes.palettes[Themes.paletteIndex];
						theme.palette = new ThemePalette().Use(selectedPalette);
						EditorPrefs.SetString("EditorPalette",selectedPalette.name);
						Themes.SaveColors();
					}
				}
				foreach(var toggle in theme.variants.Where(x=>x.name.StartsWith("+"))){
					var toggleName = "EditorTheme-"+theme.name+toggle.name.Remove("+");
					EditorPrefs.SetBool(toggleName,EditorPrefs.GetBool(toggleName).Draw(toggle.name.Remove("+")));
					GUILayout.Space(2);
				}
				if(theme.allowColorCustomization){
					if(theme.allowSystemColor){
						theme.useSystemColor = theme.useSystemColor.Draw("Use System Color");
						EditorPrefs.SetBool("EditorTheme-"+theme.name+"-UseSystemColor",theme.useSystemColor);
						if(EditorGUIExtension.lastChanged && !theme.useSystemColor){Themes.LoadColors();}
					}
					if(!theme.useSystemColor){
						theme.palette.background = theme.palette.background.Draw("Background");
						theme.palette.backgroundDark.value = theme.palette.backgroundDark.value.Draw("Background Dark");
						theme.palette.backgroundLight.value = theme.palette.backgroundLight.value.Draw("Background Light");
						if(altered){
							EditorGUILayout.BeginHorizontal();
							if(GUILayout.Button("Save",GUILayout.Width(100))){Themes.SavePalette();}
							if(GUILayout.Button("Reset",GUILayout.Width(100))){Themes.LoadColors(true);}
							EditorGUILayout.EndHorizontal();
						}
						Themes.SaveColors();
					}
					//Themes.verticalSpacing = Themes.verticalSpacing.Draw("Vertical Spacing");
				}
			}
			if(current != Themes.themeIndex){
				EditorPrefs.SetString("EditorTheme",Themes.all[Themes.themeIndex].name);
				Themes.UpdateSettings();
				Themes.RebuildStyles();
				Themes.Apply();
			}
			if(GUI.changed){
				//Themes.Create();
				Themes.lastSystemColor = Color.clear;
				Themes.needsRefresh = true;
				Utility.RepaintAll();
			}
		}
		public static void SaveColors(){
			var theme = Themes.active;
			EditorPrefs.SetString("EditorTheme-"+theme.name+"Color",theme.palette.background.Serialize());
			EditorPrefs.SetString("EditorTheme-"+theme.name+"ColorDark",theme.palette.backgroundDark);
			EditorPrefs.SetString("EditorTheme-"+theme.name+"ColorLight",theme.palette.backgroundLight);
		}
		public static void LoadColors(bool reset=false){
			var theme = Themes.active;
			if(!theme.useSystemColor){
				if(reset){
					var original = Themes.palettes[Themes.paletteIndex];
					theme.palette = new ThemePalette().Use(original);
				}
				else if(theme.allowColorCustomization){
					theme.palette.background = EditorPrefs.GetString("EditorTheme-"+theme.name+"Color",theme.palette.background.Serialize()).Deserialize<Color>();
					theme.palette.backgroundDark = EditorPrefs.GetString("EditorTheme-"+theme.name+"ColorDark",theme.palette.backgroundDark);
					theme.palette.backgroundLight = EditorPrefs.GetString("EditorTheme-"+theme.name+"ColorLight",theme.palette.backgroundLight);
				}
			}
		}
		//=================================
		// Updating
		//=================================
		public static void UpdateColors(){
			var theme = Themes.active;
			if(theme.allowSystemColor && theme.useSystemColor){
				theme.palette.backgroundDark.offset = 0.8f;
				theme.palette.backgroundLight.offset = 1.3f;
				Color color;
				object key;
				int systemColor = -1;
				#if UNITY_EDITOR_WIN
				key = Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\DWM\\","AccentColor",null);
				//if(key.IsNull()){key = Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\DWM\\","AccentColor",null);}
				if(!key.IsNull()){systemColor = key.As<int>();}
				#endif
				color = systemColor != -1 ? systemColor.ToHex().ToColor(true) : theme.palette.background;
				if(color != Themes.lastSystemColor){
					Themes.lastSystemColor = color;
					theme.palette.background = color;
					theme.palette.backgroundDark.Update(theme.palette.background);
					theme.palette.backgroundLight.Update(theme.palette.background);
					Themes.needsRefresh = true;
				}
				return;
			}
			theme.palette.backgroundDark.Update(theme.palette.background);
			theme.palette.backgroundLight.Update(theme.palette.background);
		}
		public static void UpdateSettings(){
			var baseTheme = Themes.all[Themes.themeIndex];
			var theme = Themes.active = new Theme().Use(baseTheme);
			theme.palette = new ThemePalette().Use(baseTheme.palette);
			theme.useSystemColor = EditorPrefs.GetBool("EditorTheme-"+theme.name+"-UseSystemColor",false);
			Themes.LoadColors();
			Themes.UpdateColors();
			foreach(Theme variant in theme.variants.Where(x=>x.name.StartsWith("+"))){
				var variantName = "EditorTheme-"+theme.name+variant.name.Remove("+");
				if(EditorPrefs.GetBool(variantName)){
					theme.Use(variant);
				}
			}
			if(Themes.active.useColorAssets){
				theme.colorStyles.Clear();
				theme.colorImages.Clear();
				Themes.UpdateAssets("ThemeColor",theme.palette.background);
				Themes.UpdateAssets("ThemeColorDark",theme.palette.backgroundDark);
				Themes.UpdateAssets("ThemeColorLight",theme.palette.backgroundLight);
			}
			Themes.Apply();
		}
		public static void UpdateAssets(string colorName,Color color){
			var theme = Themes.active;
			FileManager.Create(theme.path+"/Background");
			var imagePath = theme.path+"/Background/"+theme.name+colorName+".png";
			var borderPath = theme.path+"/Background/"+theme.name+colorName+"Border.png";
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
			theme.colorStyles.AddNew().GetStates().ForEach(x=>x.background = image);
			theme.colorImages.AddNew(image);
			image.SetPixel(0,0,color);
			image.Apply();
			border.SetPixels(new Color[]{color,color,color,color,Color.clear,color,color,color,color});
			border.Apply();
			image.SaveAs(imagePath);
			border.SaveAs(borderPath);
		}
		public static void Apply(string name=""){
			var theme = Themes.active;
			if(name.IsEmpty()){name = theme.name;}
			var loadPath = Themes.storagePath+name;
			foreach(var skinFile in FileManager.FindAll(loadPath+"/*.guiskin",true,false)){
				if(skinFile.name == name){continue;}
				var field = skinFile.name.Split(".").Last();
				var parent = skinFile.name.Replace("."+field,"");
				var whole = Utility.GetUnityType(skinFile.name);
				var partial = Utility.GetUnityType(parent);
				var styles = skinFile.GetAsset<GUISkin>().customStyles;
				var flags = field.Contains("s_Current") ? ObjectExtension.privateFlags : ObjectExtension.staticFlags;
				if(whole.IsNull() && (partial.IsNull() || !partial.HasVariable(field))){
					Debug.LogWarning("[Themes] No matching class/field found for GUISkin -- " + skinFile.name);
					continue;
				}
				if(!whole.IsNull()){
					whole.SetValuesByType<GUIStyle>(styles,null,flags);
				}
				if(!partial.IsNull()){
					partial.GetVariable(field).SetValuesByType<GUIStyle>(styles);
				}
			}
			/*foreach(var contentFile in FileManager.FindAll(loadPath+"/*.guicontent",true,false)){
				var field = contentFile.name.Split(".").Last();
				var parent = contentFile.name.Replace("."+field,"");
				var whole = Utility.GetUnityType(contentFile.name);
				var partial = Utility.GetUnityType(parent);
				if(whole.IsNull() && (partial.IsNull() || !partial.HasVariable(field))){
					Debug.LogWarning("[Themes] No matching class/field found for GUIContent -- " + contentFile.name);
					continue;
				}
				object target = partial.IsNull() ? whole : partial.GetVariable(field) ?? Activator.CreateInstance(partial.GetVariableType(field));
				var content = new GUIContent();
				var contentName = "";
				foreach(var line in contentFile.GetText().GetLines()){
					if(line.Trim().IsEmpty()){continue;}
					if(line.ContainsAll("[","]")){
						if(!contentName.IsEmpty() && target.HasVariable(contentName)){
							target.SetVariable<GUIContent>(contentName,content);
						}
						content = new GUIContent();
						contentName = line.Parse("[","]");
					}
					else{
						var term = line.Parse("","=").Trim();
						var value = line.Parse("=").Trim();
						if(term == "image"){content.image = FileManager.GetAsset<Texture2D>(name+"/GUIContent/"+value+".png");}
						else if(term == "text"){content.text = value;}
						else if(term == "tooltip"){content.tooltip = value;}
					}
				}
				if(!contentName.IsEmpty() && target.HasVariable(contentName)){
					target.SetVariable<GUIContent>(contentName,content);
				}
			}*/
			var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
			skin.Use(Style.GetSkin(name,false));
			skin.settings.cursorColor = theme.palette.backgroundLight;
			skin.settings.selectionColor = theme.palette.backgroundDark;
			skin.GetStyle("TabWindowBackground").normal.background = theme.windowBackgroundOverride;
			Utility.GetUnityType("Toolbar.Styles").GetVariable<GUIStyle>("appToolbar").Use(skin.GetStyle("AppToolbar"));
			Utility.GetUnityType("Toolbar.Styles").GetVariable<GUIStyle>("dropdown").Use(skin.GetStyle("Dropdown"));
			Utility.GetUnityType("SceneRenderModeWindow.Styles").SetVariable("sMenuItem",skin.GetStyle("MenuItem"));
			Utility.GetUnityType("SceneRenderModeWindow.Styles").SetVariable("sSeparator",skin.GetStyle("sv_iconselector_sep"));
			typeof(EditorGUIUtility).SetVariable("kDarkViewBackground",theme.palette.background);
			typeof(EditorGUI).SetVariable("kCurveColor",theme.palette.backgroundDark.value);
			typeof(EditorGUI).SetVariable("kCurveBGColor",theme.palette.backgroundLight.value);
			Utility.GetUnityType("AppStatusBar").ClearVariable("background");
			var console = Utility.GetUnityType("ConsoleWindow.Constants");
			console.SetVariable("ms_Loaded",false);
			console.CallMethod("Init");
			var hostView = Utility.GetUnityType("HostView");
			hostView.SetVariable<Color>("kViewColor",theme.palette.background);
			foreach(var view in Resources.FindObjectsOfTypeAll(hostView)){
				view.ClearVariable("background");
			}
			foreach(var window in Locate.GetAssets<EditorWindow>()){
				window.antiAlias = 1;
				window.minSize = new Vector2(100,20);
				window.wantsMouseMove = true;
				window.autoRepaintOnSceneChange = true;
			}
			if(name != "@Default"){
				skin = Style.GetSkin(name+"/UnityEditor.EditorStyles.s_Current");
				if(!skin.IsNull()){
					Utility.GetUnityType("PreferencesWindow").GetVariable("constants").SetVariable("sectionHeader",skin.GetStyle("m_LargeLabel"));
				}
			}
		}
	}
	public class Theme{
		[Internal] public string name;
		[Internal] public string path;
		[Internal] public List<Texture2D> colorImages = new List<Texture2D>();
		[Internal] public List<GUIStyle> colorStyles = new List<GUIStyle>();
		[Internal] public bool useSystemColor;
		[Internal] public ThemePalette palette = new ThemePalette();
		public List<Theme> variants = new List<Theme>();
		public bool allowCustomization;
		public bool allowColorCustomization;
		public bool allowSystemColor;
		public bool useColorAssets = true;
		public Texture2D windowBackgroundOverride;
		public Font fontOverride;
		public float fontScale = 1;
		public float spacingScale = 1;
		public Theme Use(Theme other){
			this.UseVariables(other,typeof(InternalAttribute).AsList());
			if(this.name.IsEmpty()){this.name = other.name;}
			if(this.path.IsEmpty()){this.path = other.path;}
			this.variants = other.variants;
			return this;
		}
	}
	public class ThemePalette{
		public string name;
		public Color background = new Color(0.7f,0.7f,0.7f);
		public RelativeColor backgroundDark = 0.8f;
		public RelativeColor backgroundLight = 1.3f;
		public ThemePalette Use(ThemePalette other){
			this.background = other.background;
			this.backgroundDark = other.backgroundDark.Serialize();
			this.backgroundLight = other.backgroundLight.Serialize();
			return this;
		}
		public bool Matches(ThemePalette other){
			var match = this.background == other.background;
			var matchDark = this.backgroundDark.value == other.backgroundDark.value;
			var matchLight = this.backgroundLight.value == other.backgroundLight.value;
			return match && matchDark && matchLight;
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
		public static implicit operator string(RelativeColor current){return current.Serialize();}
		public RelativeColor(float offset){this.offset = offset;}
		public RelativeColor(Color manual){this.value = manual;}
		public string Serialize(){return this.offset != 0 ? this.offset.Serialize() : this.value.Serialize();}
		public void Update(Color initial){this.value = this.offset != 0 ? initial.Multiply(this.offset) : this.value;}
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