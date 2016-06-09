using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace Zios{
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
		[NonSerialized] public static string buildID = "1 (beta)";
		[NonSerialized] public static int themeIndex;
		[NonSerialized] public static int paletteIndex;
		[NonSerialized] public static string storagePath;
		[NonSerialized] public static bool liveEdit;
		[NonSerialized] public static bool responsive;
		[NonSerialized] public static bool setup;
		[NonSerialized] public static bool disabled;
		[NonSerialized] public static bool debug;
		[NonSerialized] public static bool needsRefresh;
		[NonSerialized] public static bool needsRebuild;
		[NonSerialized] private static float nextUpdate;
		//[NonSerialized] public static float verticalSpacing = 2.0f;
		static Themes(){
			//SceneView.onSceneGUIDelegate += (a)=>Themes.Setup();
			EditorApplication.projectWindowItemOnGUI += (a,b)=>Themes.Setup();
			EditorApplication.hierarchyWindowItemOnGUI += (a,b)=>Themes.Setup();
			EditorApplication.update += ()=>{
				if(Time.realtimeSinceStartup < Themes.nextUpdate || !Themes.setup || Themes.disabled){return;}
				Themes.UpdateColors();
				Utility.RepaintAll();
				Themes.nextUpdate = Time.realtimeSinceStartup + (Themes.responsive ? 0.01f : 0.5f);
			};
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
				FileManager.Refresh();
				var themes = FileManager.Find("*.unitytheme",true,Themes.debug);
				if(themes.IsNull()){
					Debug.LogWarning("[Themes] No .unityTheme files found. Disabling until refreshed.");
					Themes.setup = true;
					Themes.disabled = true;
					return;
				}
				Themes.storagePath = themes.GetFolderPath().Trim("/","\\").GetDirectory()+"/";
				Themes.Load();
				Themes.UpdateSettings();
				Themes.UpdateColors();
				Utility.RepaintAll();
				if(!Themes.setup){
					Themes.setup = true;
					Themes.needsRebuild = true;
				}
			}
		}
		public static void RebuildStyles(bool skipTree=false){
			var terms = new string[]{"Styles","styles","s_GOStyles","s_Current","s_Styles","m_Styles","ms_Styles","constants"};
			foreach(var type in typeof(UnityEditor.Editor).Assembly.GetTypes()){
				if(skipTree && type.Name == "TreeViewGUI"){continue;}
				if(type.IsNull()){continue;}
				foreach(var term in terms){
					type.ClearVariable(term,ObjectExtension.staticFlags);
				}
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
			Debug.Log("[Themes] Forced Refresh.");
			Themes.setup = false;
			Themes.disabled = false;
			Utility.RepaintAll();
		}
		[MenuItem("Zios/Process/Theme/Development/Toggle Live Edit %3")]
		public static void ToggleEdit(){
			Themes.liveEdit = !Themes.liveEdit;
			Debug.Log("[Themes] Live editing : " + Themes.liveEdit);
			Themes.Refresh();
		}
		[MenuItem("Zios/Process/Theme/Development/Toggle Debug %4")]
		public static void ToggleDebug(){
			Themes.debug= !Themes.debug;
			Debug.Log("[Themes] Debug messages : " + Themes.debug);
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
				Themes.responsive = Themes.responsive.Draw("Responsive UI");
				EditorPrefs.SetBool("EditorTheme-"+theme.name+"-ResponsiveUI",Themes.responsive);
				GUILayout.Space(2);
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
				Utility.RebuildInspectors();
			}
			if(GUI.changed){
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
				if(color != theme.palette.background){
					theme.palette.background = color;
					theme.palette.backgroundDark.Update(theme.palette.background);
					theme.palette.backgroundLight.Update(theme.palette.background);
					Themes.needsRefresh = true;
					Utility.RepaintAll();
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
			Themes.responsive = EditorPrefs.GetBool("EditorTheme-"+baseTheme.name+"-ResponsiveUI",true);
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
			var image = (Texture2D)AssetDatabase.LoadAssetAtPath(imagePath,typeof(Texture2D));
			var border = (Texture2D)AssetDatabase.LoadAssetAtPath(borderPath,typeof(Texture2D));
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
		public static void Apply(string themeName=""){
			var theme = Themes.active;
			if(themeName.IsEmpty()){themeName = theme.name;}
			var loadPath = Themes.storagePath+themeName;
			var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
			var main = Style.GetSkin(themeName,false);
			if(Themes.debug && !main.IsNull() && main.customStyles.Length != skin.customStyles.Length){
				Debug.LogWarning("[Themes] Mismatched style count [" + skin.customStyles.Length + "] for main editor skin [" +main.customStyles.Length+"]. Possible version conflict.");
			}
			if(!main.IsNull()){
				skin.Use(main,!Themes.liveEdit);
				skin.settings.cursorColor = theme.palette.backgroundLight;
				skin.settings.selectionColor = theme.palette.backgroundDark;
				skin.GetStyle("TabWindowBackground").normal.background = theme.windowBackgroundOverride;
				Utility.GetUnityType("AppStatusBar").ClearVariable("background");
				Utility.GetUnityType("Toolbar").CallMethod("RepaintToolbar");
				Utility.GetUnityType("SceneRenderModeWindow.Styles").SetVariable("sMenuItem",skin.GetStyle("MenuItem"));
				Utility.GetUnityType("SceneRenderModeWindow.Styles").SetVariable("sSeparator",skin.GetStyle("sv_iconselector_sep"));
				typeof(EditorGUIUtility).SetVariable("kDarkViewBackground",theme.palette.background);
				typeof(EditorGUI).SetVariable("kCurveColor",theme.palette.backgroundDark.value);
				typeof(EditorGUI).SetVariable("kCurveBGColor",theme.palette.backgroundLight.value);
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
				if(themeName != "@Default"){
					skin = Style.GetSkin(themeName+"/UnityEditor.EditorStyles.s_Current",false);
					if(!skin.IsNull()){
						var preferences = Utility.GetUnityType("PreferencesWindow");
						var constants = preferences.GetVariable("constants");
						if(constants.IsNull()){
							var instance = Activator.CreateInstance(preferences.GetVariableType("constants"));
							preferences.SetVariable("constants",instance);
						}
						preferences.GetVariable("constants").SetVariable("sectionHeader",skin.GetStyle("LargeLabel"));
					}
				}
			}
			foreach(var skinFile in FileManager.FindAll(loadPath+"/*.guiskin",true,false)){
				if(skinFile.name == themeName){continue;}
				var field = skinFile.name.Split(".").Last();
				var parent = skinFile.name.Replace("."+field,"");
				var whole = Utility.GetUnityType(skinFile.name);
				var partial = Utility.GetUnityType(parent);
				var styles = skinFile.GetAsset<GUISkin>().GetNamedStyles(false);
				var flags = field.Contains("s_Current") ? ObjectExtension.privateFlags : ObjectExtension.staticFlags;
				if(Themes.debug && whole.IsNull() && (partial.IsNull() || !partial.HasVariable(field))){
					Debug.LogWarning("[Themes] No matching class/field found for GUISkin -- " + skinFile.name + ". Possible version conflict.");
					continue;
				}
				Action<Dictionary<string,GUIStyle>,object> SetStyles = (current,scope)=>{
					if(Themes.debug && current.Count != 0 && current.Count != styles.Count){
						Debug.LogWarning("[Themes] Mismatched style count [" + current.Count + "] for -- " + skinFile.name + "[" +styles.Count+"]. Possible version conflict.");
					}
					foreach(var item in current){
						var name = item.Key;
						var style = item.Value;
						var styleName = style.name.IsEmpty() ? name : style.name;
						if(styles.ContainsKey(styleName)){
							scope.SetVariable(name,styles[styleName]);
						}
					}
				};
				if(!whole.IsNull()){
					SetStyles(whole.GetVariables<GUIStyle>(null,flags),whole);
				}
				if(!partial.IsNull()){
					var target = partial.GetVariable(field);
					if(target.IsNull()){
						try{target = Activator.CreateInstance(partial.GetVariableType(field));}
						catch{continue;}
					}
					SetStyles(target.GetVariables<GUIStyle>(),target);
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
				object target = partial.IsNull() ? whole : partial.GetVariable(field);
				if(target.IsNull()){
					try{target = Activator.CreateInstance(partial.GetVariableType(field));}
					catch{continue;}
				}
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
		}
	}
	public class ThemesAbout :EditorWindow{
		[MenuItem("Zios/Process/Theme/About",false,1)]
		public static void Init(){
			var window = ScriptableObject.CreateInstance<ThemesAbout>();
			window.position = new Rect(100,100,1,1);
			window.minSize = window.maxSize = new Vector2(190,100);
			window.ShowAuxWindow();
		}
		public void OnGUI(){
			var box = EditorStyles.label;
			#if UNITY_5
			this.titleContent = "About Themes".ToContent();
			box = EditorStyles.helpBox;
			#endif
			string buildText = "Build <b><color=#FFFFFF>"+Themes.buildID+"</color></b>";
			EditorGUILayout.BeginVertical(box.Background(""));
			buildText.ToLabel().DrawLabel(EditorStyles.miniLabel.RichText(true).Clipping("Overflow").FontSize(15).Alignment("UpperCenter"));
			"Part of the <i>Zios</i> framework. Developed by Brad Smithee.".ToLabel().DrawLabel(EditorStyles.wordWrappedLabel.FontSize(12).RichText(true));
			if("Source Repository".ToLabel().DrawButton(GUI.skin.button.FixedWidth(150).Margin(12,0,5,0))){
				Application.OpenURL("http://ziosproject.com/Shared/Codebase");
			}
			EditorGUILayout.EndVertical();
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
		public static void OnPostprocessAllAssets(string[] imported,string[] deleted,string[] movedTo, string[] movedFrom){
			Themes.setup = false;
			Utility.RepaintAll();
		}
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