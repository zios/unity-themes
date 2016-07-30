using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios.Interface{
	using UnityEditor;
	#if UNITY_EDITOR_WIN
	using Microsoft.Win32;
	#endif
	[InitializeOnLoad]
	public partial class Theme{
		public static Theme active;
		[NonSerialized] public static string revision = "3 [r506]";
		[NonSerialized] public static int themeIndex;
		[NonSerialized] public static int paletteIndex;
		[NonSerialized] public static int fontsetIndex;
		[NonSerialized] public static int iconsetIndex;
		[NonSerialized] public static int skinsetIndex;
		[NonSerialized] public static string storagePath;
		[NonSerialized] public static bool liveEdit;
		[NonSerialized] public static bool responsive = true;
		[NonSerialized] public static int loadPasses = 2;
		[NonSerialized] public static bool setup;
		[NonSerialized] public static bool loaded;
		[NonSerialized] public static bool disabled;
		[NonSerialized] public static bool debug;
		[NonSerialized] public static ThemeWindow window;
		[NonSerialized] private static bool needsRefresh;
		[NonSerialized] private static bool needsRebuild;
		[NonSerialized] private static float nextStep;
		[NonSerialized] private static float nextSync;
		[NonSerialized] private static float setupDelay;
		[NonSerialized] private static Vector2 scroll = Vector2.zero;
		[NonSerialized] private static float paletteChangeTime;
		[NonSerialized] private static int paletteChangeCount;
		[NonSerialized] private static List<string> paletteNames = new List<string>();
		[NonSerialized] private static List<string> fontsetNames = new List<string>();
		[NonSerialized] private static List<string> fontNames = new List<string>();
		//[NonSerialized] public static float verticalSpacing = 2.0f;
		static Theme(){
			EditorApplication.playmodeStateChanged += ()=>Theme.nextStep = 0;
			Theme.Step();
			EditorApplication.update += Theme.ShowWindow;
		}
		public static void ShowWindow(){
			if(Theme.window.IsNull()){
				Theme.window = Resources.FindObjectsOfTypeAll<ThemeWindow>().FirstOrDefault();
				if(Theme.window.IsNull()){
					Theme.window = ScriptableObject.CreateInstance<ThemeWindow>();
					Theme.window.position = new Rect(9001,9001,1,1);
					Theme.window.minSize = new Vector2(1,1);
					Theme.window.ShowPopup();
				}
			}
			Theme.window.position = new Rect(9001,9001,1,1);
			Theme.window.minSize = new Vector2(1,1);
		}
		public static void Step(){
			if(Time.realtimeSinceStartup < Theme.nextStep || !Theme.setup){return;}
			Theme.ShowWindow();
			if(Theme.active.name != "Default"){
				Theme.UpdateColors();
				Utility.RepaintAll();
			}
			Theme.nextStep = Time.realtimeSinceStartup + (Theme.responsive ? 0.05f : 1f);
		}
		public static void Update(){
			if(EditorApplication.isCompiling || EditorApplication.isUpdating){return;}
			if(Theme.needsRefresh){
				Theme.UpdateSettings();
				if(Theme.needsRebuild){
					Theme.needsRebuild = false;
					return;
				}
				Utility.CallEditorPref("EditorTheme-Refresh",Theme.debug);
				Theme.Cleanup();
				Theme.needsRefresh = false;
				if(Theme.loadPasses > 0){
					Theme.loadPasses -= 1;
					Theme.setup = false;
				}
			}
			if(Theme.needsRebuild){
				Theme.RebuildStyles();
				Theme.Refresh();
				Utility.DelayCall("ApplyIcons",()=>Theme.active.iconset.Apply(false),0.2f);
				Utility.CallEditorPref("EditorTheme-Rebuild",Theme.debug);
			}
			if(!Theme.setup){
				var themes = FileManager.Find("*.unitytheme",Theme.debug);
				if(themes.IsNull()){
					Debug.LogWarning("[Themes] No .unityTheme files found. Disabling until refreshed.");
					Theme.setup = true;
					Theme.disabled = true;
					return;
				}
				Theme.storagePath = themes.path.GetDirectory()+"/";
				Theme.Load();
				Theme.UpdateSettings();
				Theme.UpdateColors();
				Theme.Rebuild();
				Theme.ApplyIconset();
				Theme.fontNames.Clear();
				Theme.fontsetNames.Clear();
				Theme.paletteNames.Clear();
				Utility.CallEditorPref("EditorTheme-Setup",Theme.debug);
				Theme.setup = true;
			}
		}
		public static void Load(){
			if(Theme.loaded && Theme.loadPasses < 1){return;}
			Theme.loaded = true;
			ThemeFontset.all = ThemeFontset.Import();
			ThemePalette.all = ThemePalette.Import();
			ThemeSkinset.all = ThemeSkinset.Import();
			ThemeIconset.all = ThemeIconset.Import();
			Theme.all = Theme.Import().OrderBy(x=>x.name!="Default").ToList();
			Theme.themeIndex = Theme.all.FindIndex(x=>x.name==EditorPrefs.GetString("EditorTheme","Default")).Max(0);
			Theme.fontsetIndex = ThemeFontset.all.FindIndex(x=>x.name==EditorPrefs.GetString("EditorFontset","Default")).Max(0);
			Theme.paletteIndex = ThemePalette.all.FindIndex(x=>x.name==EditorPrefs.GetString("EditorPalette","Default")).Max(0);
			Theme.skinsetIndex = ThemeSkinset.all.FindIndex(x=>x.name==EditorPrefs.GetString("EditorSkinset","Default")).Max(0);
			Theme.iconsetIndex = ThemeIconset.all.FindIndex(x=>x.name==EditorPrefs.GetString("EditorIconset","Default")).Max(0);
		}
		public static void Refresh(){Theme.needsRefresh = true;}
		public static void Rebuild(){Theme.needsRebuild = true;}
		public static void RebuildStyles(){
			var terms = new string[]{"Styles","styles","s_GOStyles","s_Current","s_Styles","m_Styles","ms_Styles","constants","s_Defaults"};
			foreach(var type in typeof(Editor).Assembly.GetTypes()){
				foreach(var term in terms){
					type.ClearVariable(term,ObjectExtension.staticFlags);
				}
			}
			typeof(EditorStyles).SetVariable<EditorStyles>("s_CachedStyles",null,0);
			typeof(EditorStyles).SetVariable<EditorStyles>("s_CachedStyles",null,1);
			typeof(EditorGUIUtility).CallMethod("SkinChanged");
		}
		//=================================
		// Updating
		//=================================
		public static void UpdateColors(){
			if(Theme.active.IsNull()){return;}
			var theme = Theme.active;
			if(theme.palette.name == "[System]"){
				var mainColor = theme.palette.colors["*"].First().Value;
				object key = null;
				#if UNITY_EDITOR_WIN
				key = Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\DWM\\","AccentColor",null);
				key = key ?? Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Accent","AccentColor",null);
				if(!key.IsNull()){
					int value = key.As<int>();
					mainColor.value = value != -1 ? value.ToHex().ToColor(true) : mainColor.value;
				}
				else{
					key = Registry.GetValue("HKEY_LOCAL_MACHINE\\Software\\Policies\\Microsoft\\Windows\\Personalization","PersonalColor_Accent",null);
					key = key ?? Registry.GetValue("HKEY_CURRENT_USER\\Control Panel\\Colors","WindowFrame",null);
					if(!key.IsNull()){
						string value = key.As<string>();
						mainColor.value = value != "" ? value.ToColor(" ",false,false) : mainColor.value;
					}
				}
				#endif
			}
			foreach(var color in theme.palette.colors["*"]){
				color.Value.ApplyOffset();
			}
			EditorPrefs.SetBool("EditorTheme-Dark",Theme.active.palette.Get("Window").GetIntensity() < 0.4f);
		}
		public static void UpdateSettings(){
			if(Theme.all.Count < 1){return;}
			var baseTheme = Theme.all[Theme.themeIndex];
			var theme = Theme.active = new Theme().Use(baseTheme);
			Theme.responsive = EditorPrefs.GetBool("EditorTheme-ResponsiveUI",true);
			if(theme.customizablePalette && ThemePalette.all.Count > 0){
				var basePalette = ThemePalette.all[Theme.paletteIndex];
				theme.palette = new ThemePalette().Use(basePalette);
				Theme.LoadColors();
				Theme.UpdateColors();
			}
			if(theme.customizableFontset && ThemeFontset.all.Count > 0){
				var baseFontset = ThemeFontset.all[Theme.fontsetIndex];
				theme.fontset = new ThemeFontset(baseFontset).UseBuffer(theme.fontset);
				Theme.LoadFontset();
			}
			foreach(var variant in theme.skinset.variants){
				variant.active = EditorPrefs.GetBool("EditorTheme-Skinset-"+variant.name,false);
			}
			Theme.Apply();
		}
		public static void Apply(string themeName=""){
			if(Theme.active.IsNull()){return;}
			var theme = Theme.active;
			theme.skinset.Apply(theme);
			if(theme.name != "Default"){
				foreach(var color in theme.palette.colors["*"]){
					if(color.Value.skipTexture){continue;}
					color.Value.UpdateTexture(Theme.storagePath);
				}
				Utility.DelayCall("UpdateDynamicTextures",()=>{
					if(theme.palette.swap.Count < 1){return;}
					foreach(var file in FileManager.FindAll("#*.png")){
						theme.palette.ApplyTexture(file.path,file.GetAsset<Texture2D>());
					}
				},0.1f);
			}
		}
		public static void Cleanup(){
			foreach(var guiSkin in Resources.FindObjectsOfTypeAll<UnityObject>().Where(x=>x.name.Contains("EditorStyles"))){
				if(!Utility.IsAsset(guiSkin)){
					UnityObject.DestroyImmediate(guiSkin);
				}
			}
			//GC.Collect();
		}
		//=================================
		// Preferences
		//=================================
		[PreferenceItem("Themes")]
		public static void ShowPreferences(){
			if(Theme.active.IsNull()){return;}
			var theme = Theme.active;
			var isDefault = theme.name == "Default";
			var current = Theme.themeIndex;
			var window = EditorWindow.focusedWindow;
			if(!isDefault && !window.IsNull() && window.GetType().Name.Contains("Preferences")){
				window.minSize = new Vector2(600,100);
			}
			Theme.scroll = EditorGUILayout.BeginScrollView(Theme.scroll);
			EditorGUIUtility.labelWidth = 200;
			var themeNames = Theme.all.Select(x=>x.name).ToList();
			var themeIndex = Theme.themeIndex + 1 < 2 ? 0 : Theme.themeIndex + 1;
			themeNames.Insert(1,"/");
			Theme.themeIndex = (themeNames.Draw(themeIndex,"Theme")-1).Max(0);
			GUILayout.Space(3);
			bool hasPalettes = ThemePalette.all.Count > 0;
			bool hasFontsets = ThemeFontset.all.Count > 0;
			bool paletteAltered = !theme.palette.Matches(ThemePalette.all[Theme.paletteIndex]);
			bool fontsetAltered = !theme.fontset.Matches(ThemeFontset.all[Theme.fontsetIndex]);
			if(theme.customizableIconset){
				Theme.iconsetIndex = ThemeIconset.all.Select(x=>x.name).Draw(Theme.iconsetIndex,"Iconset");
				GUILayout.Space(3);
				if(EditorGUIExtension.lastChanged){
					Theme.ApplyIconset();
				}
			}
			if(theme.customizablePalette && hasPalettes){
				if(Theme.paletteNames.Count < 1){
					var palettePath = Theme.storagePath+"Palettes/";
					Theme.paletteNames = ThemePalette.all.Select(x=>x.path.Remove(palettePath,".unitypalette")).ToList();
				}
				var paletteNames = Theme.paletteNames.Copy();
				var popupStyle = EditorStyles.popup;
				if(paletteAltered){
					var name = paletteNames[Theme.paletteIndex];
					popupStyle = EditorStyles.popup.FontStyle("boldanditalic");
					paletteNames[Theme.paletteIndex] = name + " *";
				}
				Theme.paletteIndex = paletteNames.Draw(Theme.paletteIndex,"Palette",popupStyle);
				GUILayout.Space(3);
				if(EditorGUIExtension.lastChanged){
					var selectedPalette = ThemePalette.all[Theme.paletteIndex];
					theme.palette = new ThemePalette().Use(selectedPalette);
					EditorPrefs.SetString("EditorPalette",selectedPalette.name);
					Theme.AdjustPalette();
				}
			}
			if(theme.customizableFontset && hasFontsets){
				if(Theme.fontsetNames.Count < 1){
					var palettePath = Theme.storagePath+"Fontsets/";
					Theme.fontsetNames = ThemeFontset.all.Select(x=>x.path.Remove(palettePath,".unityfontset")).ToList();
				}
				var fontsetNames = Theme.fontsetNames.Copy();
				var popupStyle = EditorStyles.popup;
				if(fontsetAltered){
					var name = fontsetNames[Theme.fontsetIndex];
					popupStyle = EditorStyles.popup.FontStyle("boldanditalic");
					fontsetNames[Theme.fontsetIndex] = name + " *";
				}
				Theme.fontsetIndex = fontsetNames.Draw(Theme.fontsetIndex,"Fontset",popupStyle);
				GUILayout.Space(3);
				if(EditorGUIExtension.lastChanged){
					var selectedFontset = ThemeFontset.all[Theme.fontsetIndex];
					theme.fontset = new ThemeFontset(selectedFontset).UseBuffer(theme.fontset);
					EditorPrefs.SetString("EditorFontset",selectedFontset.name);
					Theme.SaveFontset();
					Theme.Rebuild();
				}
			}
			bool open = false;
			if(!isDefault){
				open = "Options".ToLabel().DrawFoldout("Theme.Options");
				if(EditorGUIExtension.lastChanged){GUI.changed = false;}
				if(open){
					EditorGUI.indentLevel += 1;
					//Theme.verticalSpacing = Theme.verticalSpacing.Draw("Vertical Spacing");
					Theme.responsive = Theme.responsive.Draw("Responsive Hover");
					foreach(var variant in theme.skinset.variants){
						variant.active = variant.active.Draw(variant.name);
						if(EditorGUIExtension.lastChanged){
							Theme.Refresh();
							EditorPrefs.SetBool("EditorTheme-Skinset-"+variant.name,variant.active);
						}
					}
					EditorPrefs.SetBool("EditorTheme-ResponsiveUI",Theme.responsive);
					GUILayout.Space(2);
					EditorGUI.indentLevel -= 1;
				}
			}
			if(theme.customizablePalette && hasPalettes){
				open = "Colors".ToLabel().DrawFoldout("Theme.Colors");
				if(EditorGUIExtension.lastChanged){GUI.changed = false;}
				if(open){
					EditorGUI.indentLevel += 1;
					foreach(var group in theme.palette.colors.Where(x=>x.Key!="*")){
						var groupName = group.Key;
						var isGroup = groupName != "Default";
						if(!isGroup || groupName.ToLabel().DrawFoldout("Theme.Colors."+groupName)){
							if(isGroup){EditorGUI.indentLevel += 1;}
							var names = theme.palette.colors["*"].Keys.ToList();
							foreach(var item in theme.palette.colors[groupName]){
								var color = item.Value;
								if(!color.sourceName.IsEmpty()){
									EditorGUILayout.BeginHorizontal();
									var index = names.IndexOf(color.sourceName);
									var offsetStyle = EditorStyles.numberField.FixedWidth(35).Margin(0,0,2,0);
									if(index == -1){
										EditorGUILayout.EndHorizontal();
										var message = "[" + color.sourceName + " not found]";
										index = names.Unshift(message).Draw(0,item.Key.ToTitleCase());
										if(index != 0){color.sourceName = names[index];}
										continue;
									}
									color.sourceName = names[names.Draw(index,color.name.ToTitleCase())];
									EditorGUIUtility.labelWidth = 5;
									color.offset = color.offset.Draw(" ",offsetStyle,false);
									EditorGUIUtility.labelWidth = 200;
									color.Assign(theme.palette.colors["*"][color.sourceName]);
									GUILayout.Space(15);
									EditorGUILayout.EndHorizontal();
									GUILayout.Space(2);
									continue;
								}
								theme.palette.colors["*"][color.name].value = color.value.Draw(color.name.ToTitleCase());
							}
							if(isGroup){EditorGUI.indentLevel -= 1;}
						}
					}
					if(paletteAltered){
						EditorGUILayout.BeginHorizontal();
						GUILayout.Space(15);
						if(GUILayout.Button("Save",GUILayout.Width(100))){theme.palette.Export();}
						if(GUILayout.Button("Reset",GUILayout.Width(100))){Theme.LoadColors(true);}
						if(GUILayout.Button("Apply",GUILayout.Width(100))){theme.palette.Export(theme.palette.path);}
						EditorGUILayout.EndHorizontal();
					}
					if(GUI.changed){Theme.SaveColors();}
					EditorGUI.indentLevel -=1;
				}
				if(theme.customizableFontset && hasFontsets){
					open = "Fonts".ToLabel().DrawFoldout("Theme.Fonts");
					if(EditorGUIExtension.lastChanged){GUI.changed = false;}
					if(open){
						EditorGUI.indentLevel += 1;
						var ttf = FileManager.FindAll("Themes/Fonts/*.ttf");
						var otf = FileManager.FindAll("Themes/Fonts/*.otf");
						var fontFiles = ttf.Concat(otf);
						var fonts = fontFiles.Select(x=>x.GetAsset<Font>()).ToArray();
						if(Theme.fontNames.Count < 1){
							var fontPath = Theme.storagePath+"Fonts/";
							Theme.fontNames = fontFiles.Select(x=>x.path).ToList();
							Func<string,string> FixFontNames = (data)=>{
								data = data.Remove(fontPath,".ttf",".otf");
								if(data.Contains("/")){
									var folder = data.GetDirectory();
									var folderPascal = folder.ToPascalCase();
									data = folder + "/" + data.Split("/").Last().Remove(folderPascal+"-",folderPascal);
									if(Theme.fontNames.Count(x=>x.Contains(folder+"/"))==1){
										data = folder;
									}
								}
								return data.Trim("/");
							};
							Theme.fontNames = Theme.fontNames.Select(x=>FixFontNames(x)).ToList().Order();
						}
						var fontNames = Theme.fontNames.Copy();
						if(fontNames.Count < 1){fontNames.Add("No fonts found.");}
						GUIStyleExtension.autoLayout = false;
						foreach(var item in theme.fontset.fonts){
							if(item.Value.font.IsNull()){continue;}
							var themeFont = item.Value;
							EditorGUILayout.BeginHorizontal();
							var index = fonts.IndexOf(themeFont.font);
							if(index == -1){
								EditorGUILayout.EndHorizontal();
								var message = "[" + themeFont.name + " not found]";
								index = fontNames.Unshift(message).Draw(0,item.Key.ToTitleCase());
								if(index != 0){themeFont.font = fonts[index-1];}
								continue;
							}
							var offsetStyle = EditorStyles.numberField.FixedWidth(35).Margin(0,0,2,0);
							themeFont.font = fonts[fontNames.Draw(index,item.Key.ToTitleCase())];
							EditorGUIUtility.labelWidth = 38;
							EditorGUIUtility.fieldWidth = 25;
							themeFont.sizeOffset = themeFont.sizeOffset.DrawInt("Size",offsetStyle.FixedWidth(25),false);
							EditorGUIUtility.labelWidth = 20;
							EditorGUIUtility.fieldWidth = 35;
							themeFont.offsetX = themeFont.offsetX.Draw("X",offsetStyle,false);
							themeFont.offsetY = themeFont.offsetY.Draw("Y",offsetStyle,false);
							EditorGUIUtility.labelWidth = 200;
							EditorGUILayout.EndHorizontal();
						}
						GUIStyleExtension.autoLayout = true;
						if(fontsetAltered){
							EditorGUILayout.BeginHorizontal();
							GUILayout.Space(15);
							if(GUILayout.Button("Save",GUILayout.Width(100))){theme.fontset.Export();}
							if(GUILayout.Button("Reset",GUILayout.Width(100))){Theme.LoadFontset(true);}
							if(GUILayout.Button("Apply",GUILayout.Width(100))){theme.fontset.Export(theme.fontset.path);}
							EditorGUILayout.EndHorizontal();
						}
						EditorGUI.indentLevel -=1;
						if(GUI.changed){Theme.SaveFontset();}
						GUILayout.Space(10);
					}
				}
			}
			if(current != Theme.themeIndex){
				EditorPrefs.SetString("EditorTheme",Theme.all[Theme.themeIndex].name);
				Theme.InstantRefresh();
				Theme.ApplyIconset();
				EditorGUILayout.EndScrollView();
				return;
			}
			if(!Theme.needsRebuild && GUI.changed){
				Utility.DelayCall(Theme.Rebuild,0.25f);
			}
			EditorGUILayout.EndScrollView();
		}
		//=================================
		// Iconset
		//=================================
		public static void ApplyIconset(){
			var iconset = ThemeIconset.all[Theme.iconsetIndex];
			Theme.active.iconset = iconset;
			Theme.active.iconset.Apply();
			EditorPrefs.SetString("EditorIconset",iconset.name);
		}
		//=================================
		// Fonts
		//=================================
		public static void SaveFontset(){
			var theme = Theme.active;
			EditorPrefs.SetString("EditorTheme-"+theme.name+"-Fontset",theme.fontset.Serialize());
		}
		public static void LoadFontset(bool reset=false){
			var theme = Theme.active;
			if(reset){
				var original = ThemeFontset.all[Theme.fontsetIndex];
				theme.fontset = new ThemeFontset(original).UseBuffer(theme.fontset);
				return;
			}
			var value = EditorPrefs.GetString("EditorTheme-"+theme.name+"-Fontset",null);
			theme.fontset.Deserialize(value);
		}
		//=================================
		// Colors
		//=================================
		public static void SaveColors(){
			var theme = Theme.active;
			foreach(var group in theme.palette.colors.Where(x=>x.Key!="*")){
				foreach(var color in group.Value){
					EditorPrefs.SetString("EditorTheme-"+theme.name+"-Color-"+group.Key+"-"+color.Key,color.Value.Serialize());
				}
			}
		}
		public static void LoadColors(bool reset=false){
			var theme = Theme.active;
			if(reset){
				var original = ThemePalette.all[Theme.paletteIndex];
				theme.palette = new ThemePalette().Use(original);
				return;
			}
			foreach(var group in theme.palette.colors.Where(x=>x.Key!="*")){
				foreach(var color in group.Value){
					var value = EditorPrefs.GetString("EditorTheme-"+theme.name+"-Color-"+group.Key+"-"+color.Key,color.Value.Serialize());
					theme.palette.colors["*"][color.Key] = theme.palette.colors[group.Key][color.Key].Deserialize(value);
				}
			}
			foreach(var color in theme.palette.colors["*"].Copy()){
				if(color.Value.sourceName.IsEmpty()){continue;}
				var source = theme.palette.colors["*"][color.Value.sourceName];
				theme.palette.colors["*"][color.Key].Assign(source);
			}
		}
		//=================================
		// Shortcuts
		//=================================
		public static void InstantRefresh(){
			Theme.setup = false;
			Utility.RepeatCall(Theme.Update,4);
			Utility.DelayCall(Utility.RepaintAll,0.1f);
		}
		[MenuItem("Zios/Theme/Refresh #F1")]
		public static void ForceRefresh(){
			Debug.Log("[Themes] Forced Refresh.");
			Debug.LogError("[Themes] Example Error message.");
			Debug.LogWarning("[Themes] Example Warning message.");
			Theme.setup = false;
			Theme.disabled = false;
		}
		[MenuItem("Zios/Theme/Development/Toggle Live Edit")]
		public static void ToggleEdit(){
			Theme.liveEdit = !Theme.liveEdit;
			Debug.Log("[Themes] Live editing : " + Theme.liveEdit);
			Theme.Refresh();
		}
		[MenuItem("Zios/Theme/Development/Toggle Debug #F2")]
		public static void ToggleDebug(){
			Theme.debug = !Theme.debug;
			Debug.Log("[Themes] Debug messages : " + Theme.debug);
		}
		[MenuItem("Zios/Theme/Previous Palette &F1")]
		public static void PreviousPalette(){ Theme.AdjustPalette(-1);}
		[MenuItem("Zios/Theme/Next Palette &F2")]
		public static void NextPalette(){Theme.AdjustPalette(1);}
		public static void AdjustPalette(int adjust=0){
			var theme = Theme.active;
			if(!theme.IsNull() && theme.customizablePalette){
				Theme.paletteIndex = (Theme.paletteIndex + adjust) % ThemePalette.all.Count;
				if(Theme.paletteIndex < 0){Theme.paletteIndex = ThemePalette.all.Count-1;}
				var palette = ThemePalette.all[Theme.paletteIndex];
				theme.palette = new ThemePalette().Use(palette);
				EditorPrefs.SetString("EditorPalette",palette.name);
				Theme.SaveColors();
				var time = Time.realtimeSinceStartup;
				if(Theme.paletteChangeCount > 35){
					Application.OpenURL("https://goo.gl/gg9609");
					Theme.paletteChangeCount = -9999;
				}
				if(time < Theme.paletteChangeTime){Theme.paletteChangeCount += 1;}
				else{Theme.paletteChangeCount = 0;}
				Theme.paletteChangeTime = time + 0.3f;
				Theme.Refresh();
				Theme.Update();
				Utility.DelayCall(Theme.Rebuild,0.25f);
			}
		}
		[MenuItem("Zios/Theme/Previous Fontset %F1")]
		public static void PreviousFontset(){Theme.AdjustFontset(-1);}
		[MenuItem("Zios/Theme/Next Fontset %F2")]
		public static void NextFontset(){Theme.AdjustFontset(1);}
		public static void AdjustFontset(int adjust){
			var theme = Theme.active;
			if(!theme.IsNull() && theme.customizableFontset){
				Theme.fontsetIndex = (Theme.fontsetIndex + adjust) % ThemeFontset.all.Count;
				if(Theme.fontsetIndex < 0){Theme.fontsetIndex = ThemeFontset.all.Count-1;}
				var defaultFontset = ThemeFontset.all[Theme.fontsetIndex];
				theme.fontset = new ThemeFontset(defaultFontset).UseBuffer(theme.fontset);
				EditorPrefs.SetString("EditorFontset",defaultFontset.name);
				Theme.SaveFontset();
				Theme.Refresh();
				Utility.DelayCall(Theme.Rebuild,0.25f);
			}
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
			string buildText = "Build <b><color=#FFFFFF>"+ Theme.revision+"</color></b>";
			EditorGUILayout.BeginVertical(box.Background(""));
			buildText.ToLabel().DrawLabel(EditorStyles.miniLabel.RichText(true).Clipping("Overflow").FontSize(15).Alignment("UpperCenter"));
			"Part of the <i>Zios</i> framework. Developed by Brad Smithee.".ToLabel().DrawLabel(EditorStyles.wordWrappedLabel.FontSize(12).RichText(true));
			if("Source Repository".ToLabel().DrawButton(GUI.skin.button.FixedWidth(150).Margin(12,0,5,0))){
				Application.OpenURL("https://github.com/zios/unity-themes");
			}
			EditorGUILayout.EndVertical();
		}
	}
	public class ColorImportSettings : AssetPostprocessor{
		public static void OnPostprocessAllAssets(string[] imported,string[] deleted,string[] movedTo, string[] movedFrom){
			Theme.setup = false;
			Theme.loaded = false;
		}
		public void OnPreprocessTexture(){
			TextureImporter importer = (TextureImporter)this.assetImporter;
			if(importer.assetPath.ContainsAny("Themes")){
				importer.isReadable = true;
				importer.textureFormat = TextureImporterFormat.RGBA32;
				if(importer.assetPath.Contains("Border")){
					importer.filterMode = FilterMode.Point;
				}
			}
		}
	}
}