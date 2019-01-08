using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using UnityEvent= UnityEngine.Event;
namespace Zios.Interface{
	using UnityEditor;
	using SystemAttributes;
	public enum HoverResponse{None=1,Slow,Moderate,Instant};
	[InitializeOnLoad][NotSerialized]
	public partial class Theme{
		public static string revision = "[r{revision}]";
		public static string storagePath = "Assets/@Themes/";
		public static int themeIndex;
		public static int paletteIndex;
		public static int fontsetIndex;
		public static int iconsetIndex;
		public static int skinsetIndex;
		public static HoverResponse hoverResponse = HoverResponse.None;
		public static bool delayUpdate;
		public static bool singleUpdate;
		public static bool separatePlaymodeSettings;
		public static bool showColorsAdvanced;
		public static bool showFontsAdvanced;
		public static bool changed;
		public static bool initialized;
		public static bool setup;
		public static bool loaded;
		public static bool lazyLoaded;
		public static bool disabled;
		public static bool debug;
		public static ThemeWindow window;
		public static string suffix;
		private static bool liveEdit;
		private static bool needsRefresh;
		private static bool needsRebuild;
		private static bool needsInstantRefresh;
		private static bool setupPreferences;
		private static Vector2 scroll = Vector2.zero;
		private static float colorChangeTime;
		private static int colorChangeCount;
		private static Action undoCallback;
		private static List<string> paletteNames = new List<string>();
		private static List<string> fontsetNames = new List<string>();
		private static List<string> fontNames = new List<string>();
		private static Font[] fonts = new Font[0];
		private static Font[] builtinFonts = new Font[0];
		static Theme(){
			EditorApplication.playmodeStateChanged += Theme.CheckUpdate;
			EditorApplication.update += ThemeWindow.ShowWindow;
			AppDomain.CurrentDomain.DomainUnload += ThemeWindow.CloseWindow;
			Events.Add("On Window Reordered",ThemeWindow.CloseWindow);
			Events.Add("On GUISkin Changed",()=>{
				if(Theme.liveEdit){
					Utility.DelayCall(Theme.DelayedInstantRefresh,0.5f);
				}
			});
		}
		public static void Update(){
			if(Theme.disabled){return;}
			if(!Theme.needsRefresh && !Theme.needsRebuild && Theme.needsInstantRefresh){
				Theme.needsInstantRefresh = false;
				Theme.InstantRefresh();
			}
			if(Theme.needsRebuild){
				Theme.RebuildStyles();
				Theme.Refresh();
				if(!Theme.active.IsNull() && !Theme.active.iconset.IsNull()){
					Theme.active.iconset.Apply(false);
				}
				Utility.CallEditorPref("EditorTheme-Rebuild",Theme.debug);
				Theme.needsRebuild = false;
				Theme.Update();
			}
			else if(Theme.needsRefresh){
				Theme.ApplySettings();
				Utility.CallEditorPref("EditorTheme-Refresh",Theme.debug);
				Utility.RepaintAll();
				Utility.DelayCall(Utility.RepaintAll,0.25f);
				Theme.Cleanup();
				Theme.needsRefresh = false;
			}
			else if(!Theme.setup){
				var themes = FileManager.Find("*.unitytheme",Theme.debug);
				if(themes.IsNull()){
					Debug.LogWarning("[Themes] No .unityTheme files found. Disabling until refreshed.");
					Theme.setup = true;
					Theme.disabled = true;
					return;
				}
				Theme.storagePath = themes.path.GetDirectory()+"/";
				Theme.Load(!Theme.initialized);
				Theme.LoadSettings();
				Theme.Rebuild();
				Theme.fontNames.Clear();
				Theme.fontsetNames.Clear();
				Theme.paletteNames.Clear();
				Theme.setupPreferences = false;
				if(Theme.separatePlaymodeSettings){Utility.DelayCall(Theme.Rebuild,0.25f);}
				Utility.CallEditorPref("EditorTheme-Setup",Theme.debug);
				Theme.setup = true;
				Theme.initialized = true;
				Theme.Update();
			}
		}
		public static void LoadCheck(){
			if(Theme.lazyLoaded){
				Theme.Load(false);
				Theme.LoadSettings();
				Theme.ApplySettings();
			}
		}
		public static void Load(bool lazy=false){
			Theme.separatePlaymodeSettings = Utility.GetPref("EditorTheme-SeparatePlaymodeSettings",false);
			Theme.suffix = EditorApplication.isPlayingOrWillChangePlaymode && Theme.separatePlaymodeSettings ? "-Playmode" : "";
			var theme = Utility.GetPref("EditorTheme"+Theme.suffix,"Default").ToPascalCase();
			var suffix = "-"+theme+Theme.suffix;
			if(!Utility.HasPref("EditorSkinset"+suffix)){lazy = false;}
			var fontset = lazy ? Utility.GetPref("EditorFontset"+suffix,"Classic") + ".unityFontset" : null;
			var palette = lazy ? Utility.GetPref("EditorPalette"+suffix,"Classic") + ".unityPalette" : null;
			var iconset = lazy ? FileManager.Find("Iconsets/"+Utility.GetPref("EditorIconset"+suffix,"Default")) : null;
			var skinset = lazy ? FileManager.Find("Skinsets/"+Utility.GetPref("EditorSkinset"+suffix,"Default")) : null;
			var unityTheme = lazy ? theme + ".unitytheme" : null;
			ThemeFontset.all = ThemeFontset.Import(fontset);
			ThemePalette.all = ThemePalette.Import(palette);
			ThemeSkinset.all = skinset.IsNull() ? ThemeSkinset.Import() : ThemeSkinset.Import(skinset.path).AsList();
			ThemeIconset.all = iconset.IsNull() ? ThemeIconset.Import() : ThemeIconset.Import(iconset.path).AsList();
			Theme.all = Theme.Import(unityTheme).OrderBy(x=>x.name!="Default").ToList();
			Theme.loaded = true;
			Theme.lazyLoaded = lazy;
		}
		public static void LoadSettings(){
			FileManager.monitor = false;
			RelativeColor.autoBalance = Utility.GetPref("EditorTheme-AutobalanceColors",1).As<AutoBalance>();
			Theme.showColorsAdvanced = Utility.GetPref("EditorTheme-ShowAdvancedColors",false);
			Theme.showFontsAdvanced = Utility.GetPref("EditorTheme-ShowAdvancedFonts",false);
			Theme.hoverResponse = Utility.GetPref("EditorTheme-HoverResponse",1).As<HoverResponse>();
			Theme.themeIndex = Theme.all.FindIndex(x=>x.name==Utility.GetPref("EditorTheme"+Theme.suffix,"Default")).Max(0);
			var theme = Theme.all[Theme.themeIndex];
			Theme.suffix = "-"+theme.name+Theme.suffix;
			if(!Theme.lazyLoaded){
				Theme.fontsetIndex = ThemeFontset.all.FindIndex(x=>x.name==Utility.GetPref<string>("EditorFontset"+Theme.suffix,theme.fontset.name)).Max(0);
				Theme.paletteIndex = ThemePalette.all.FindIndex(x=>x.name==Utility.GetPref<string>("EditorPalette"+Theme.suffix,theme.palette.name)).Max(0);
				Theme.skinsetIndex = ThemeSkinset.all.FindIndex(x=>x.name==Utility.GetPref<string>("EditorSkinset"+Theme.suffix,theme.skinset.name)).Max(0);
				Theme.iconsetIndex = ThemeIconset.all.FindIndex(x=>x.name==Utility.GetPref<string>("EditorIconset"+Theme.suffix,theme.iconset.name)).Max(0);
			}
		}
		public static void Refresh(){Theme.needsRefresh = true;}
		public static void Rebuild(){Theme.needsRebuild = true;}
		public static void RebuildStyles(){
			var terms = new string[]{"Styles","styles","s_GOStyles","s_Current","s_Styles","m_Styles","ms_Styles","constants","s_Defaults"};
			foreach(var type in typeof(Editor).Assembly.GetTypes()){
				if(type.Name.Contains("LookDev")){continue;}
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
		public static void CheckUpdate(){
			if(Theme.separatePlaymodeSettings && !EditorApplication.isPlayingOrWillChangePlaymode){
				Theme.Reset(true);
			}
		}
		public static void UpdateColors(){
			if(Theme.active.IsNull()){return;}
			RelativeColor.UpdateSystem();
			foreach(var color in Theme.active.palette.colors["*"]){
				color.Value.ApplyOffset();
				Utility.SetPref<bool>("EditorTheme-Dark-"+color.Key,color.Value.value.GetIntensity() < 0.4f);
			}
			Utility.SetPref<bool>("EditorTheme-Dark",Theme.active.palette.Get("Window").GetIntensity() < 0.4f);
		}
		public static void ApplySettings(){
			if(Theme.all.Count < 1){return;}
			var baseTheme = Theme.all[Theme.themeIndex];
			var theme = Theme.active = new Theme().Use(baseTheme);
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
			if(Theme.changed){
				foreach(var variant in Theme.active.skinset.variants){Undo.RecordPref<bool>("EditorVariant"+Theme.suffix+"-"+variant.name,false);}
				foreach(var variant in Theme.active.defaultVariants){Undo.RecordPref<bool>("EditorVariant"+Theme.suffix+"-"+variant,true);}
				Theme.changed = false;
			}
			foreach(var variant in theme.skinset.variants){
				variant.active = Utility.GetPref<bool>("EditorVariant"+Theme.suffix+"-"+variant.name,false);
			}
			Utility.SetPref<string>("EditorSkinset"+Theme.suffix,theme.skinset.name);
			if(!Utility.HasPref("EditorFontset"+Theme.suffix)){Utility.SetPref<string>("EditorFontset"+Theme.suffix,theme.fontset.name);}
			if(!Utility.HasPref("EditorPalette"+Theme.suffix)){Utility.SetPref<string>("EditorPalette"+Theme.suffix,theme.palette.name);}
			if(!Utility.HasPref("EditorIconset"+Theme.suffix)){Utility.SetPref<string>("EditorIconset"+Theme.suffix,theme.iconset.name);}
			Theme.Apply();
		}
		public static void Apply(string themeName="",bool forceWrite=false){
			if(Theme.active.IsNull()){return;}
			var theme = Theme.active;
			theme.skinset.Apply(theme);
			forceWrite = !Utility.IsPlaying() && (forceWrite || Theme.singleUpdate);
			var shouldUpdate = !Utility.IsPlaying() || Theme.singleUpdate || Theme.separatePlaymodeSettings;
			if(theme.name != "Default" && shouldUpdate){
				foreach(var color in theme.palette.colors["*"]){
					if(color.Value.skipTexture){continue;}
					color.Value.UpdateTexture(Theme.storagePath);
				}
				Action UpdateDynamic = ()=>{
					if(theme.palette.swap.Count < 1){return;}
					var variants = Theme.active.skinset.variants.Where(x=>x.active).Select(x=>x.name).ToArray();
					foreach(var file in FileManager.FindAll("#*.png")){
						 if(file.path.Contains("+") && !variants.Contains(file.path.Parse("+","/"))){
							continue;
						}
						theme.palette.ApplyTexture(file.path,file.GetAsset<Texture2D>(),forceWrite);
					}
				};
				Utility.DelayCall("UpdateDynamic",UpdateDynamic,Theme.delayUpdate ? 0.25f : 0);
			}
			Theme.delayUpdate = false;
			Theme.singleUpdate = false;
		}
		public static void Cleanup(){
			foreach(var guiSkin in Resources.FindObjectsOfTypeAll<GUISkin>()){
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
		public static void DrawPreferences(){
			EditorUI.Reset();
			Theme.LoadCheck();
			if(!Theme.separatePlaymodeSettings && EditorApplication.isPlayingOrWillChangePlaymode){
				"Theme Settings are not available while in play mode unless \"Separate play mode\" active.".DrawHelp();
				return;
			}
			if(Theme.disabled){
				Theme.disabled = Theme.disabled.Draw("Disable System");
				Undo.RecordPref<bool>("EditorTheme-Disabled",Theme.disabled);
				"Disabling existing themes requires Unity to be restarted.".DrawHelp("Info");
			}
			if(Theme.disabled){return;}
			if(Theme.active.IsNull()){
				ThemeWindow.ShowWindow();
				Theme.Reset(true);
				Theme.InstantRefresh();
				if(Theme.active.IsNull()){
					Theme.disabled = true;
					return;
				}
			}
			var current = Theme.themeIndex;
			var window = EditorWindow.focusedWindow;
			if(!Theme.setupPreferences){
				Theme.PrepareFonts();
				Theme.setupPreferences = true;
			}
			if(Theme.active.name != "Default" && !window.IsNull() && window.GetType().Name.Contains("Preferences")){
				window.maxSize = new Vector2(9999999,9999999);
			}
			Undo.RecordStart(typeof(Theme));
			Theme.undoCallback = Theme.Refresh;
			Theme.scroll = EditorGUILayout.BeginScrollView(Theme.scroll,false,false,GUI.skin.horizontalScrollbar,GUI.skin.verticalScrollbar,new GUIStyle().Padding(0,16,0,0));
			Theme.UpdateColors();
			Theme.DrawThemes();
			Theme.DrawIconsets();
			Theme.DrawPalettes();
			Theme.DrawFontsets();
			Theme.DrawOptions();
			Theme.DrawVariants();
			Theme.DrawColors();
			Theme.DrawFonts();
			if(current != Theme.themeIndex){
				var suffix = Theme.suffix.Remove("-"+Theme.active.name);
				Undo.RecordPref<string>("EditorTheme"+suffix,Theme.all[Theme.themeIndex].name);
				Theme.changed = true;
				Theme.InstantRefresh();
				Utility.DelayCall(Theme.Rebuild,0.25f);
				Theme.undoCallback = ()=>{
					Theme.DelayedInstantRefresh();
					Utility.DelayCall(Theme.Rebuild,0.25f);
				};
			}
			else if(!Theme.needsRebuild && GUI.changed){
				Theme.Rebuild();
				Theme.undoCallback += Theme.Rebuild;
			}
			EditorGUILayout.EndScrollView();
			Undo.RecordEnd("Theme Changes",typeof(Theme),Theme.undoCallback);
		}
		public static void DrawThemes(){
			EditorGUIUtility.labelWidth = 200;
			var themeNames = Theme.all.Select(x=>x.name).ToList();
			var themeIndex = Theme.themeIndex + 1 < 2 ? 0 : Theme.themeIndex + 1;
			themeNames.Insert(1,"/");
			Theme.themeIndex = (themeNames.Draw(themeIndex,"Theme")-1).Max(0);
			GUILayout.Space(3);
		}
		public static void DrawIconsets(){
			var theme = Theme.active;
			if(theme.customizableIconset){
				Theme.iconsetIndex = ThemeIconset.all.Select(x=>x.name).Draw(Theme.iconsetIndex,"Iconset");
				GUILayout.Space(3);
				if(EditorUI.lastChanged){
					Theme.ApplyIconset();
					Theme.undoCallback = Theme.ApplyIconset;
				}
			}
		}
		public static void DrawPalettes(){
			var theme = Theme.active;
			int index = Theme.paletteIndex;
			bool hasPalettes = ThemePalette.all.Count > 0;
			bool paletteAltered = !theme.palette.Matches(ThemePalette.all[index]);
			if(theme.customizablePalette && hasPalettes){
				if(Theme.paletteNames.Count < 1){
					var palettePath = Theme.storagePath+"Palettes/";
					Theme.paletteNames = ThemePalette.all.Select(x=>{
						var path = x.path.Remove(palettePath,".unitypalette");
						if(x.usesSystem && RelativeColor.system == Color.clear){
							return path.Replace(path.GetPathTerm(),"/").Trim("/");
						}
						return path;
					}).ToList();
				}
				var paletteNames = Theme.paletteNames.Copy();
				var popupStyle = EditorStyles.popup;
				if(paletteAltered){
					var name = paletteNames[index];
					popupStyle = EditorStyles.popup.FontStyle("boldanditalic");
					paletteNames[index] = name + " *";
				}
				Theme.paletteIndex = paletteNames.Draw(index,"Palette",popupStyle);
				Theme.DrawPaletteMenu(true);
				GUILayout.Space(3);
				if(EditorUI.lastChanged){
					Theme.AdjustPalette();
				}
			}
		}
		public static void DrawPaletteMenu(bool showAdjusters=false){
			var theme = Theme.active;
			if(GUILayoutUtility.GetLastRect().Clicked(1)){
				var menu = new EditorMenu();
				var clipboard = EditorGUIUtility.systemCopyBuffer;
				menu.Add("Copy Palette",()=>EditorGUIUtility.systemCopyBuffer=theme.palette.Serialize());
				if(clipboard.Contains("[Textured]")){
					menu.Add("Paste Palette",()=>{
						Theme.RecordAction(()=>{
							theme.palette.Deserialize(clipboard);
							Theme.SaveColors();
							Theme.UpdateColors();
							Theme.Rebuild();
						});
					});
				}
				menu.AddSeparator();
				if(showAdjusters){
					menu.Add("Previous Palette &F1",Theme.PreviousPalette);
					menu.Add("Next Palette &F2",Theme.NextPalette);
				}
				else{
					menu.Add("Randomize &F3",Theme.RandomizeColors);
				}
				menu.Draw();
			}
		}
		public static void DrawFontsets(){
			var theme = Theme.active;
			bool hasFontsets = ThemeFontset.all.Count > 0;
			bool fontsetAltered = !theme.fontset.Matches(ThemeFontset.all[Theme.fontsetIndex]);
			if(theme.customizableFontset && hasFontsets){
				if(Theme.fontsetNames.Count < 1){
					var fontsetsPath = Theme.storagePath+"Fontsets/";
					Theme.fontsetNames = ThemeFontset.all.Select(x=>x.path.Remove(fontsetsPath,".unityfontset").GetAssetPath()).ToList();
				}
				var fontsetNames = Theme.fontsetNames.Copy();
				var popupStyle = EditorStyles.popup;
				if(fontsetAltered){
					var name = fontsetNames[Theme.fontsetIndex];
					popupStyle = EditorStyles.popup.FontStyle("boldanditalic");
					fontsetNames[Theme.fontsetIndex] = name + " *";
				}
				Theme.fontsetIndex = fontsetNames.Draw(Theme.fontsetIndex,"Fontset",popupStyle);
				Theme.DrawFontsetMenu(true);
				GUILayout.Space(3);
				if(EditorUI.lastChanged){
					var selectedFontset = ThemeFontset.all[Theme.fontsetIndex];
					theme.fontset = new ThemeFontset(selectedFontset).UseBuffer(theme.fontset);
					Undo.RecordPref<string>("EditorFontset"+Theme.suffix,selectedFontset.name);
					Theme.SaveFontset();
					Theme.Rebuild();
				}
			}
		}
		public static void DrawFontsetMenu(bool showAdjusters=false){
			var theme = Theme.active;
			if(GUILayoutUtility.GetLastRect().Clicked(1)){
				var menu = new EditorMenu();
				var clipboard = EditorGUIUtility.systemCopyBuffer;
				menu.Add("Copy Fontset",()=>EditorGUIUtility.systemCopyBuffer=theme.fontset.Serialize());
				if(clipboard.Contains("Font = ")){
					menu.Add("Paste Fontset",()=>{
						Theme.RecordAction(()=>{
							theme.fontset.Deserialize(clipboard);
							Theme.SaveFontset();
							Theme.Rebuild();
						});
					});
				}
				if(showAdjusters){
					menu.AddSeparator();
					menu.Add("Previous Fontset %F1",Theme.PreviousFontset);
					menu.Add("Next Fontset %F2",Theme.NextFontset);
				}
				menu.Draw();
			}
		}
		public static void DrawVariants(){
			if(Theme.active.name == "Default" || Theme.active.skinset.variants.Count < 1){return;}
			var theme = Theme.active;
			bool open = "Variants".ToLabel().DrawFoldout("Theme.Variants");
			if(EditorUI.lastChanged){GUI.changed=false;}
			if(open){
				EditorGUI.indentLevel += 1;
				foreach(var variant in theme.skinset.variants){
					variant.active = variant.active.Draw(variant.name.ToTitleCase());
					if(EditorUI.lastChanged){
						Theme.Refresh();
						Undo.RecordPref<bool>("EditorVariant"+Theme.suffix+"-"+variant.name,variant.active);
					}
				}
				EditorGUI.indentLevel -= 1;
			}
		}
		public static void DrawOptions(){
			bool open = "Options".ToLabel().DrawFoldout("Theme.Options");
			if(EditorUI.lastChanged){GUI.changed=false;}
			if(open){
				EditorGUI.indentLevel += 1;
				//Theme.verticalSpacing = Theme.verticalSpacing.Draw("Vertical Spacing");
				Theme.hoverResponse = Theme.hoverResponse.Draw("Hover Response").As<HoverResponse>();
				Theme.separatePlaymodeSettings = Theme.separatePlaymodeSettings.Draw("Separate Playmode Settings");
				if(EditorUI.lastChanged){
					Undo.RecordPref<bool>("EditorTheme-SeparatePlaymodeSettings",Theme.separatePlaymodeSettings);
					Theme.Reset(true);
					return;
				}
				Theme.disabled = Theme.disabled.Draw("Disable System");
				if(!Theme.window.IsNull()){
					Theme.window.wantsMouseMove = Theme.hoverResponse != HoverResponse.None;
				}
				Undo.RecordPref<int>("EditorTheme-HoverResponse",Theme.hoverResponse.ToInt());
				Undo.RecordPref<bool>("EditorTheme-Disabled",Theme.disabled);
				GUILayout.Space(2);
				EditorGUI.indentLevel -= 1;
			}
		}
		public static void DrawColors(){
			var theme = Theme.active;
			bool hasPalettes = ThemePalette.all.Count > 0;
			bool paletteAltered = !theme.palette.Matches(ThemePalette.all[Theme.paletteIndex]);
			var existingChanges = GUI.changed;
			if(theme.customizablePalette && hasPalettes){
				bool open = "Colors".ToLabel().DrawFoldout("Theme.Colors");
				if(EditorUI.lastChanged){GUI.changed=false;}
				Theme.DrawPaletteMenu();
				if(!open){return;}
				EditorGUI.indentLevel += 1;
				Theme.showColorsAdvanced = Theme.showColorsAdvanced.Draw("Advanced");
				if(Theme.showColorsAdvanced){RelativeColor.autoBalance = RelativeColor.autoBalance.Draw("Autobalance").As<AutoBalance>();}
				foreach(var group in theme.palette.colors.Where(x=>x.Key!="*")){
					var groupName = group.Key;
					var isGroup = groupName != "Default";
					var colorCount = theme.palette.colors[groupName].Count(x=>x.Value.source.IsNull());
					var canExpand = Theme.showColorsAdvanced || colorCount > 3;
					if(!Theme.showColorsAdvanced && colorCount < 1){continue;}
					if(canExpand){
						var drawFoldout = groupName.ToLabel().DrawFoldout("Theme.Colors."+groupName);
						if(EditorUI.lastChanged){GUI.changed=false;}
						if(isGroup && !drawFoldout){continue;}
						if(isGroup){
							EditorGUI.indentLevel += 1;
						}
					}
					var names = theme.palette.colors["*"].Keys.ToList();
					if(Application.platform == RuntimePlatform.WindowsEditor){
						names = "@System".AsArray().Concat(names).ToList();
					}
					foreach(var item in theme.palette.colors[groupName]){
						var color = item.Value;
						Rect area = new Rect(1,1,1,1);
						if(!color.sourceName.IsEmpty()){
							if(!Theme.showColorsAdvanced){continue;}
							var index = names.IndexOf(color.sourceName);
							EditorGUILayout.BeginHorizontal();
							if(index == -1){
								var message = "[" + color.sourceName + " not found]";
								index = names.Unshift(message).Draw(0,item.Key.ToTitleCase());
								if(index != 0){color.sourceName = names[index];}
							}
							else{
								color.sourceName = names[names.Draw(index,color.name.ToTitleCase())];
								EditorUI.SetLayoutOnce(35);
								if(color.blendMode == ColorBlend.Normal){color.offset = color.offset.Draw(null,null,false);}
								color.Assign(theme.palette,color.sourceName);
								if(color.blendMode != ColorBlend.Normal){
									EditorUI.SetLayoutOnce(100);
									color.blendMode = color.blendMode.Draw(null,null,false).As<ColorBlend>();
									EditorUI.SetLayoutOnce(35);
									color.offset = color.offset.Draw("",null,false).Clamp(0,1);
									EditorUI.SetLayoutOnce(80);
									color.blend = color.blend.Draw("",false);
								}
							}
							EditorGUILayout.EndHorizontal();
							area = GUILayoutUtility.GetLastRect();
							GUILayout.Space(2);
						}
						else{
							color.value = color.value.Draw(color.name.ToTitleCase());
							area = GUILayoutUtility.GetLastRect();
						}
						if(area.Clicked(1)){
							GenericMenu menu = new GenericMenu();
							menu.AddItem(new GUIContent("Normal"),color.sourceName.IsEmpty(),()=>{
								color.blendMode = ColorBlend.Normal;
								color.sourceName = "";
							});
							menu.AddItem(new GUIContent("Inherited"),!color.sourceName.IsEmpty()&&color.blendMode==ColorBlend.Normal,()=>{
								color.blendMode = ColorBlend.Normal;
								if(color.sourceName.IsEmpty()){color.sourceName = names[0];}
							});
							menu.AddItem(new GUIContent("Blended"),color.blendMode!=ColorBlend.Normal,()=>{
								color.blendMode = ColorBlend.Lighten;
								if(color.sourceName.IsEmpty()){color.sourceName = names[0];}
							});
							menu.ShowAsContext();
							UnityEvent.current.Use();
						}
					}
					if(canExpand && isGroup){
						EditorGUI.indentLevel -= 1;
					}
				}
				if(paletteAltered){
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(15);
					if(GUILayout.Button("Save As",GUILayout.Width(100))){theme.palette.Export();}
					if(GUILayout.Button("Reset",GUILayout.Width(100))){Theme.LoadColors(true);}
					if(GUILayout.Button("Apply",GUILayout.Width(100))){theme.palette.Export(theme.palette.path);}
					EditorGUILayout.EndHorizontal();
				}
				if(!existingChanges && GUI.changed){
					Theme.SaveColors();
					Undo.RecordPref<int>("EditorTheme-AutobalanceColors",RelativeColor.autoBalance.ToInt());
					Undo.RecordPref<bool>("EditorTheme-ShowAdvancedColors",Theme.showColorsAdvanced);
					Utility.DelayCall(Theme.Refresh,0.1f);
					GUI.changed = false;
				}
				EditorGUI.indentLevel -=1;
			}
		}
		public static void PrepareFonts(){
			var fontPath = Theme.storagePath+"Fonts/";
			var fontFiles = FileManager.FindAll("*.*tf").Where(x=>!x.path.Contains("Fontsets")).ToArray();
			Theme.builtinFonts = Locate.GetAssets<Font>().Where(x=>FileManager.GetPath(x).Contains("Library/unity")).ToArray();
			Theme.fontNames = Theme.builtinFonts.Select(x=>"@Builtin/"+x.name).Concat(fontFiles.Select(x=>x.path)).ToList();
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
				return data.GetAssetPath().Trim("/");
			};
			Theme.fontNames = Theme.fontNames.Select(x=>FixFontNames(x)).ToList();
			Theme.fonts = Theme.builtinFonts.Concat(fontFiles.Select(x=>x.GetAsset<Font>())).ToArray();
		}
		public static void DrawFonts(){
			var theme = Theme.active;
			bool hasFontsets = ThemeFontset.all.Count > 0;
			bool fontsetAltered = !theme.fontset.Matches(ThemeFontset.all[Theme.fontsetIndex]);
			var existingChanges = GUI.changed;
			if(theme.customizableFontset && hasFontsets){
				bool open = "Fonts".ToLabel().DrawFoldout("Theme.Fonts");
				if(EditorUI.lastChanged){GUI.changed=false;}
				Theme.DrawFontsetMenu();
				if(!open){return;}
				EditorGUI.indentLevel += 1;
				var fonts = Theme.fonts;
				var fontNames = Theme.fontNames.Copy();
				if(fontNames.Count < 1){fontNames.Add("No fonts found.");}
				Theme.showFontsAdvanced = Theme.showFontsAdvanced.Draw("Advanced");
				if(EditorUI.lastChanged){
					Theme.SaveFontset();
					GUI.changed = false;
				}
				foreach(var item in theme.fontset.fonts){
					if(item.Value.font.IsNull()){continue;}
					var themeFont = item.Value;
					var fontName = item.Key.ToTitleCase();
					var showRenderMode = Theme.showFontsAdvanced && !Theme.builtinFonts.Contains(themeFont.font);
					EditorGUILayout.BeginHorizontal();
					var index = fonts.IndexOf(themeFont.font);
					if(index == -1){
						EditorGUILayout.EndHorizontal();
						var message = "[" + themeFont.name + " not found]";
						index = fontNames.Unshift(message).Draw(0,item.Key.ToTitleCase());
						if(index != 0){themeFont.font = fonts[index-1];}
						continue;
					}
					if(showRenderMode){
						var fontPath = FileManager.GetPath(themeFont.font);
						var importer = Locate.GetImporter<TrueTypeFontImporter>(fontPath);
						EditorUI.SetLayoutOnce(310);
						var mode = importer.fontRenderingMode.Draw(fontName).As<FontRenderingMode>();
						if(EditorUI.lastChanged){
							Utility.RecordObject(importer,"Font Render Mode");
							importer.fontRenderingMode = mode;
							AssetDatabase.WriteImportSettingsIfDirty(fontPath);
							AssetDatabase.Refresh();
						}
						fontName = null;
						EditorUI.SetFieldSize(-1,1);
					}
					themeFont.font = fonts[fontNames.Draw(index,fontName,null,!showRenderMode)];
					if(Theme.showFontsAdvanced){
						EditorUI.SetFieldSize(0,35,false);
						EditorUI.SetLayout(70);
						themeFont.sizeOffset = themeFont.sizeOffset.DrawInt("Size",null,false);
						EditorUI.SetFieldSize(0,20,false);
						EditorUI.SetLayout(55);
						themeFont.offsetX = themeFont.offsetX.Draw("X",null,false);
						themeFont.offsetY = themeFont.offsetY.Draw("Y",null,false);
						EditorUI.SetLayout(0);
						EditorUI.SetFieldSize(0,200,false);
					}
					EditorGUILayout.EndHorizontal();
				}
				if(fontsetAltered){
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(15);
					if(GUILayout.Button("Save As",GUILayout.Width(100))){theme.fontset.Export();}
					if(GUILayout.Button("Reset",GUILayout.Width(100))){Theme.LoadFontset(true);}
					if(GUILayout.Button("Apply",GUILayout.Width(100))){theme.fontset.Export(theme.fontset.path);}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUI.indentLevel -=1;
				if(!existingChanges && GUI.changed){Theme.SaveFontset();}
				GUILayout.Space(10);
			}
		}
		//=================================
		// Iconset
		//=================================
		public static void ApplyIconset(){
			if(Theme.active.IsNull()){return;}
			Theme.active.iconset = ThemeIconset.all[Theme.iconsetIndex];
			if(!Theme.lazyLoaded && Theme.active.customizableIconset){
				Undo.RecordPref<string>("EditorIconset"+Theme.suffix,Theme.active.iconset.name);
			}
			Theme.active.iconset.Apply();
		}
		//=================================
		// Fonts
		//=================================
		public static void SaveFontset(){
			var theme = Theme.active;
			Undo.RecordPref<string>("EditorFontset-Settings"+Theme.suffix,theme.fontset.Serialize());
			Undo.RecordPref<bool>("EditorTheme-ShowAdvancedFonts",Theme.showFontsAdvanced);
		}
		public static void LoadFontset(bool reset=false){
			var theme = Theme.active;
			if(reset){
				var original = ThemeFontset.all[Theme.fontsetIndex];
				theme.fontset = new ThemeFontset(original).UseBuffer(theme.fontset);
				return;
			}
			var value = Utility.GetPref<string>("EditorFontset-Settings"+Theme.suffix,"");
			theme.fontset.Deserialize(value);
		}
		[MenuItem("Edit/Themes/Development/Export/Fontset")]
		public static void ExportFontset(){Theme.active.fontset.Export();}
		//=================================
		// Colors
		//=================================
		public static void SaveColors(){
			var theme = Theme.active;
			foreach(var group in theme.palette.colors.Where(x=>x.Key!="*")){
				foreach(var color in group.Value){
					Undo.RecordPref<string>("EditorTheme"+Theme.suffix+"-Color-"+group.Key+"-"+color.Key,color.Value.Serialize());
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
					var value = Utility.GetPref<string>("EditorTheme"+Theme.suffix+"-Color-"+group.Key+"-"+color.Key,color.Value.Serialize());
					theme.palette.colors["*"][color.Key] = theme.palette.colors[group.Key][color.Key].Deserialize(value);
				}
			}
			foreach(var color in theme.palette.colors["*"].Copy()){
				var name = color.Value.sourceName;
				if(name.IsEmpty()){continue;}
				var source = name == "@System" ? RelativeColor.system : theme.palette.colors["*"][name];
				theme.palette.colors["*"][color.Key].Assign(source);
			}
		}
		[MenuItem("Edit/Themes/Development/Export/Palette")]
		public static void ExportPalette(){Theme.active.palette.Export();}
		//=================================
		// Shortcuts
		//=================================
		public static void DelayedInstantRefresh(){
			Theme.needsInstantRefresh = true;
		}
		public static void InstantRefresh(){
			Theme.setup = false;
			Theme.Update();
			Utility.DelayCall(Utility.RepaintAll,0.25f);
			Theme.ApplyIconset();
		}
		public static void Reset(){Theme.Reset(false);}
		public static void Reset(bool force){
			if(force || Utility.IsPlaying()){
				Theme.loaded = false;
				Theme.setup = false;
			}
		}
		[MenuItem("Edit/Themes/Development/Refresh #F1")]
		public static void DebugRefresh(){
			Theme.LoadCheck();
			Debug.Log("[Themes] Example Info message.");
			Debug.LogError("[Themes] Example Error message.");
			Debug.LogWarning("[Themes] Example Warning message.");
			Theme.Reset(true);
			Theme.disabled = false;
		}
		[MenuItem("Edit/Themes/Development/Toggle Debug #F2")]
		public static void ToggleDebug(){
			Theme.debug = !Theme.debug;
			Debug.Log("[Themes] Debug messages : " + Theme.debug);
		}
		[MenuItem("Edit/Themes/Development/Toggle Live Edit #F3")]
		public static void ToggleLiveEdit(){
			Theme.liveEdit = !Theme.liveEdit;
			Debug.Log("[Themes] Live edit : " + Theme.liveEdit);
		}
		[MenuItem("Edit/Themes/Previous Palette &F1")]
		public static void PreviousPalette(){Theme.RecordAction(()=>Theme.AdjustPalette(-1));}
		[MenuItem("Edit/Themes/Next Palette &F2")]
		public static void NextPalette(){Theme.RecordAction(()=>Theme.AdjustPalette(1));}
		public static void AdjustPalette(){Theme.AdjustPalette(0);}
		public static void AdjustPalette(int adjust){
			Theme.LoadCheck();
			var theme = Theme.active;
			if(!theme.IsNull() && theme.customizablePalette){
				var usable = false;
				ThemePalette palette = null;
				while(!usable){
					Theme.paletteIndex = (Theme.paletteIndex + adjust) % ThemePalette.all.Count;
					if(Theme.paletteIndex < 0){Theme.paletteIndex = ThemePalette.all.Count-1;}
					palette = ThemePalette.all[Theme.paletteIndex];
					usable = !palette.usesSystem || (RelativeColor.system != Color.clear);
				}
				theme.palette = new ThemePalette().Use(palette);
				Undo.RecordPref<string>("EditorPalette"+Theme.suffix,palette.name);
				Theme.SaveColors();
				Theme.singleUpdate = true;
				Theme.UpdateColors();
				Theme.Refresh();
				Utility.DelayCall(Theme.Rebuild,0.5f);
			}
		}
		[MenuItem("Edit/Themes/Development/Randomize Colors &F3")]
		public static void RandomizeColors(){
			foreach(var color in Theme.active.palette.colors["*"]){
				if(color.Value.skipTexture || !color.Value.sourceName.IsEmpty()){continue;}
				color.Value.value = Color.white.Random(0);
			}
			Theme.SaveColors();
			Theme.Refresh();
			Theme.delayUpdate = true;
			Theme.singleUpdate = true;
			var time = Time.realtimeSinceStartup;
			if(Theme.colorChangeCount > 35){
				Application.OpenURL("https://goo.gl/gg9609");
				Theme.colorChangeCount = -9609;
			}
			if(time < Theme.colorChangeTime){Theme.colorChangeCount += 1;}
			else if(Theme.colorChangeCount > 0){Theme.colorChangeCount = 0;}
			Theme.colorChangeTime = time + 1;
		}
		[MenuItem("Edit/Themes/Previous Fontset %F1")]
		public static void PreviousFontset(){Theme.RecordAction(()=>Theme.AdjustFontset(-1));}
		[MenuItem("Edit/Themes/Next Fontset %F2")]
		public static void NextFontset(){Theme.RecordAction(()=>Theme.AdjustFontset(1));}
		public static void AdjustFontset(int adjust){
			Theme.LoadCheck();
			var theme = Theme.active;
			if(!theme.IsNull() && theme.customizableFontset){
				Theme.fontsetIndex = (Theme.fontsetIndex + adjust) % ThemeFontset.all.Count;
				if(Theme.fontsetIndex < 0){Theme.fontsetIndex = ThemeFontset.all.Count-1;}
				var defaultFontset = ThemeFontset.all[Theme.fontsetIndex];
				theme.fontset = new ThemeFontset(defaultFontset).UseBuffer(theme.fontset);
				Undo.RecordPref("EditorFontset"+Theme.suffix,defaultFontset.name);
				Theme.SaveFontset();
				Theme.Rebuild();
			}
		}
		public static void RecordAction(Action method){
			Undo.RecordStart(typeof(Theme));
			Theme.undoCallback = Theme.Rebuild;
			method();
			Undo.RecordEnd("Theme Changes",typeof(Theme),Theme.undoCallback);
		}
	}
	public class ThemesAbout : EditorWindow{
		[MenuItem("Edit/Themes/About",false,1)]
		public static void Init(){
			var window = ScriptableObject.CreateInstance<ThemesAbout>();
			window.position = new Rect(100,100,1,1);
			window.minSize = window.maxSize = new Vector2(190,120);
			window.ShowAuxWindow();
		}
		public void OnGUI(){
			this.SetTitle("About Zios Themes");
			string buildText = "Build <b>"+ Theme.revision+"</b>";
			EditorGUILayout.BeginVertical(new GUIStyle().Padding(15,15,15,0));
			buildText.ToLabel().DrawLabel(EditorStyles.label.RichText(true).Clipping("Overflow").FontSize(15).Alignment("UpperCenter"));
			"Part of the <i>Zios</i> framework. Developed by Brad Smithee.".ToLabel().DrawLabel(EditorStyles.wordWrappedLabel.FontSize(12).RichText(true));
			if("Source Repository".ToLabel().DrawButton(GUI.skin.button.FixedWidth(150).Margin(12,0,5,0))){
				Application.OpenURL("https://github.com/zios/unity-themes");
			}
			EditorGUILayout.EndVertical();
		}
	}
}