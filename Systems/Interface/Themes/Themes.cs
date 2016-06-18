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
		[NonSerialized] public static string revision = "1 [r485]";
		[NonSerialized] public static int themeIndex;
		[NonSerialized] public static int paletteIndex;
		[NonSerialized] public static string storagePath;
		[NonSerialized] public static bool liveEdit;
		[NonSerialized] public static bool responsive = true;
		[NonSerialized] public static bool setup;
		[NonSerialized] public static bool disabled;
		[NonSerialized] public static bool debug;
		[NonSerialized] public static bool needsRefresh;
		[NonSerialized] public static bool needsRebuild;
		[NonSerialized] private static float nextUpdate;
		//[NonSerialized] public static float verticalSpacing = 2.0f;
		static Themes(){
			EditorApplication.projectWindowItemOnGUI += (a,b)=>Themes.Setup();
			EditorApplication.hierarchyWindowItemOnGUI += (a,b)=>Themes.Setup();
			EditorApplication.playmodeStateChanged += ()=>Themes.nextUpdate = 0;
			EditorApplication.update += ()=>{
				if(Time.realtimeSinceStartup < Themes.nextUpdate || !Themes.setup || Themes.disabled){return;}
				Themes.UpdateColors();
				Utility.RepaintAll();
				Themes.nextUpdate = Time.realtimeSinceStartup + (Themes.responsive ? 0.01f : 0.5f);
			};
		}
		public static void Setup(){
			if(EditorApplication.isCompiling || EditorApplication.isUpdating){return;}
			if(Themes.needsRefresh){
				Themes.UpdateSettings();
				Themes.ApplyContents();
				Utility.RepaintAll();
				Themes.needsRefresh = false;
			}
			if(Themes.needsRebuild){
				Themes.UpdateSettings();
				Themes.RebuildStyles(true);
				Utility.RepaintAll();
				Themes.needsRebuild = false;
				Themes.needsRefresh = true;
			}
			if(!Themes.setup){
				var themes = FileManager.Find("*.unitytheme",Themes.debug);
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
		}
		[MenuItem("Zios/Theme/Refresh _`1")]
		public static void Refresh(){
			Debug.Log("[Themes] Forced Refresh.");
			Themes.setup = false;
			Themes.disabled = false;
			Utility.RepaintAll();
		}
		[MenuItem("Zios/Theme/Development/Toggle Live Edit _`2")]
		public static void ToggleEdit(){
			Themes.liveEdit = !Themes.liveEdit;
			Debug.Log("[Themes] Live editing : " + Themes.liveEdit);
			Themes.Refresh();
		}
		[MenuItem("Zios/Theme/Development/Toggle Debug _`3")]
		public static void ToggleDebug(){
			Themes.debug = !Themes.debug;
			Debug.Log("[Themes] Debug messages : " + Themes.debug);
		}
		[MenuItem("Zios/Theme/Next Palette &F2")]
		public static void NextPalette(){Themes.AdjustPalette(1);}
		[MenuItem("Zios/Theme/Previous Palette &F1")]
		public static void PreviousPalette(){Themes.AdjustPalette(-1);}
		public static void AdjustPalette(int adjust){
			if(Themes.active.allowCustomization && Themes.active.allowColorCustomization){
				Themes.paletteIndex = (Themes.paletteIndex + adjust) % Themes.palettes.Count;
				if(Themes.paletteIndex < 0){Themes.paletteIndex = Themes.palettes.Count-1;}
				var palette = Themes.palettes[Themes.paletteIndex];
				Themes.active.palette = new ThemePalette().Use(palette);
				EditorPrefs.SetString("EditorPalette",palette.name);
				Themes.SaveColors();
				Utility.RepaintAll();
				Themes.needsRefresh = true;
			}
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
				EditorPrefs.SetBool("EditorTheme-ResponsiveUI",Themes.responsive);
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
				foreach(var content in Themes.active.contents){content.Revert();}
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
				Color color = theme.palette.background;
				object key = null;
				#if UNITY_EDITOR_WIN
				key = Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\DWM\\","AccentColor",null);
				key = key ?? Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Accent","AccentColor",null);
				if(!key.IsNull()){
					int value = key.As<int>();
					color = value  != -1 ? value.ToHex().ToColor(true) : color;
				}
				else{
					key = Registry.GetValue("HKEY_LOCAL_MACHINE\\Software\\Policies\\Microsoft\\Windows\\Personalization","PersonalColor_Accent",null);
					key = key ?? Registry.GetValue("HKEY_CURRENT_USER\\Control Panel\\Colors","WindowFrame",null);
					if(!key.IsNull()){
						string value = key.As<string>();
						color = value != "" ? value.ToColor(" ",false,false) : color;
					}
				}
				#endif
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
			Themes.responsive = EditorPrefs.GetBool("EditorTheme-ResponsiveUI",true);
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
				Utility.GetUnityType("SceneRenderModeWindow.Styles").SetVariable("sMenuItem",skin.GetStyle("MenuItem"));
				Utility.GetUnityType("SceneRenderModeWindow.Styles").SetVariable("sSeparator",skin.GetStyle("sv_iconselector_sep"));
				Utility.GetUnityType("GameView.Styles").SetVariable("gizmoButtonStyle",skin.GetStyle("GV Gizmo DropDown"));
				typeof(SceneView).SetVariable<GUIStyle>("s_DropDownStyle",skin.GetStyle("GV Gizmo DropDown"));
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
						preferences.GetVariable("constants").SetVariable("sectionHeader",skin.GetStyle("m_LargeLabel [LargeLabel]"));
					}
				}
			}
			foreach(var skinFile in FileManager.FindAll(loadPath+"/*.guiskin",true,false)){
				if(skinFile.name == themeName){continue;}
				var field = skinFile.name.Split(".").Last();
				var parent = skinFile.name.Replace("."+field,"");
				var typeDirect = Utility.GetUnityType(skinFile.name);
				var typeParent = Utility.GetUnityType(parent);
				var styles = skinFile.GetAsset<GUISkin>().GetNamedStyles(false,true,true);
				var flags = field.Contains("s_Current") ? ObjectExtension.privateFlags : ObjectExtension.staticFlags;
				if(Themes.debug && typeDirect.IsNull() && (typeParent.IsNull() || !typeParent.HasVariable(field))){
					Debug.LogWarning("[Themes] No matching class/field found for GUISkin -- " + skinFile.name + ". Possible version conflict.");
					continue;
				}
				Action<Dictionary<string,GUIStyle>,object> SetStyles = (current,scope)=>{
					if(Themes.debug && current.Count != 0 && current.Count != styles.Count){
						Debug.LogWarning("[Themes] Mismatched style count [" + current.Count + "] for -- " + skinFile.name + "[" +styles.Count+"]. Possible version conflict.");
					}
					foreach(var item in current){
						if(styles.ContainsKey(item.Key)){
							var baseName = styles[item.Key].name.Parse("[","]");
							var replacement = styles[item.Key];
							var newStyle = Themes.liveEdit ? replacement : new GUIStyle(replacement).Rename(baseName);
							scope.SetVariable(item.Key,newStyle);
						}
					}
				};
				if(!typeDirect.IsNull()){
					SetStyles(typeDirect.GetVariables<GUIStyle>(null,flags),typeDirect);
				}
				if(!typeParent.IsNull()){
					var target = typeParent.GetVariable(field);
					if(target.IsNull()){
						try{target = Activator.CreateInstance(typeParent.GetVariableType(field));}
						catch{continue;}
					}
					SetStyles(target.GetVariables<GUIStyle>(),target);
				}
			}
		}
		public static void ApplyContents(){
			Utility.DelayCall(()=>{foreach(var content in Themes.active.contents){content.Apply();}},0.5f);
		}
	}
	public class ThemesAbout :EditorWindow{
		[MenuItem("Zios/Theme/About",false,1)]
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
			string buildText = "Build <b><color=#FFFFFF>"+Themes.revision+"</color></b>";
			EditorGUILayout.BeginVertical(box.Background(""));
			buildText.ToLabel().DrawLabel(EditorStyles.miniLabel.RichText(true).Clipping("Overflow").FontSize(15).Alignment("UpperCenter"));
			"Part of the <i>Zios</i> framework. Developed by Brad Smithee.".ToLabel().DrawLabel(EditorStyles.wordWrappedLabel.FontSize(12).RichText(true));
			if("Source Repository".ToLabel().DrawButton(GUI.skin.button.FixedWidth(150).Margin(12,0,5,0))){
				Application.OpenURL("https://github.com/zios/unity-themes");
			}
			EditorGUILayout.EndVertical();
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