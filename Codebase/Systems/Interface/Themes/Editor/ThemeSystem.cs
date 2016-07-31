using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using UnityEvent= UnityEngine.Event;
using CallbackFunction = UnityEditor.EditorApplication.CallbackFunction;
namespace Zios.Interface{
	using UnityEditor;
	using Events;
	#if UNITY_EDITOR_WIN
	using Microsoft.Win32;
	#endif
	public enum HoverResponse{None=1,Slow,Moderate,Instant};
	[InitializeOnLoad][NotSerialized]
	public partial class Theme{
		public static Theme active;
		public static string revision = "3 [r514]";
		public static int themeIndex;
		public static int paletteIndex;
		public static int fontsetIndex;
		public static int iconsetIndex;
		public static int skinsetIndex;
		public static string storagePath;
		public static bool liveEdit;
		public static HoverResponse hoverResponse = HoverResponse.Moderate;
		public static bool separatePlaymodeSettings = false;
		public static bool setup;
		public static bool loaded;
		public static bool disabled;
		public static bool debug;
		public static ThemeWindow window;
		public static string suffix;
		private static bool needsRefresh;
		private static bool needsRebuild;
		private static Vector2 scroll = Vector2.zero;
		private static float paletteChangeTime;
		private static int paletteChangeCount;
		private static List<string> paletteNames = new List<string>();
		private static List<string> fontsetNames = new List<string>();
		private static List<string> fontNames = new List<string>();
		//public static float verticalSpacing = 2.0f;
		static Theme(){
			EditorApplication.playmodeStateChanged += Theme.Reset;
			EditorApplication.update += Theme.ShowWindow;
			Event.Add("On Window Reordered",Theme.ResetWindow);
		}
		public static void ShowWindow(){
			if(Theme.window.IsNull()){
				Theme.window = Resources.FindObjectsOfTypeAll<ThemeWindow>().FirstOrDefault();
				if(Theme.window.IsNull()){
					Theme.window = ScriptableObject.CreateInstance<ThemeWindow>();
					Theme.window.position = new Rect(9001,9001,1,1);
					Theme.window.minSize = new Vector2(1,1);
					Theme.window.wantsMouseMove = Theme.hoverResponse != HoverResponse.None;
					Theme.window.ShowPopup();
				}
			}
			Theme.window.position = new Rect(9001,9001,1,1);
			Theme.window.minSize = new Vector2(1,1);
		}
		public static void Step(){
			if(!Theme.setup){return;}
			Theme.ShowWindow();
		}
		public static void Update(){
			if(EditorApplication.isCompiling || EditorApplication.isUpdating){return;}
			if(Theme.needsRefresh){
				Theme.UpdateSettings();
				if(Theme.needsRebuild){
					Utility.RebuildInspectors();
					Utility.RepaintAll();
					Theme.needsRebuild = false;
					return;
				}
				Utility.CallEditorPref("EditorTheme-Refresh",Theme.debug);
				Utility.RepaintAll();
				Theme.Cleanup();
				Theme.needsRefresh = false;
			}
			if(Theme.needsRebuild){
				Theme.RebuildStyles();
				Theme.Refresh();
				Utility.RepaintAll();
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
				Utility.RepaintAll();
				Utility.CallEditorPref("EditorTheme-Setup",Theme.debug);
				Theme.setup = true;
			}
		}
		public static void Load(){
			if(Theme.loaded){return;}
			Theme.loaded = true;
			ThemeFontset.all = ThemeFontset.Import();
			ThemePalette.all = ThemePalette.Import();
			ThemeSkinset.all = ThemeSkinset.Import();
			ThemeIconset.all = ThemeIconset.Import();
			Theme.all = Theme.Import().OrderBy(x=>x.name!="Default").ToList();
			Theme.hoverResponse = EditorPrefs.GetInt("EditorTheme-HoverResponse",2).As<HoverResponse>();
			Theme.separatePlaymodeSettings = EditorPrefs.GetBool("EditorTheme-SeparatePlaymodeSettings",false);
			Theme.suffix = EditorApplication.isPlayingOrWillChangePlaymode && Theme.separatePlaymodeSettings ? "-Playmode" : "";
			Theme.themeIndex = Theme.all.FindIndex(x=>x.name==EditorPrefs.GetString("EditorTheme"+Theme.suffix,"Default")).Max(0);
			var theme = Theme.all[themeIndex];
			Theme.suffix = "-"+theme.name+suffix;
			Theme.fontsetIndex = ThemeFontset.all.FindIndex(x=>x.name==EditorPrefs.GetString("EditorFontset"+Theme.suffix,theme.fontset.name)).Max(0);
			Theme.paletteIndex = ThemePalette.all.FindIndex(x=>x.name==EditorPrefs.GetString("EditorPalette"+Theme.suffix,theme.palette.name)).Max(0);
			Theme.skinsetIndex = ThemeSkinset.all.FindIndex(x=>x.name==EditorPrefs.GetString("EditorSkinset"+Theme.suffix,theme.skinset.name)).Max(0);
			Theme.iconsetIndex = ThemeIconset.all.FindIndex(x=>x.name==EditorPrefs.GetString("EditorIconset"+Theme.suffix,theme.iconset.name)).Max(0);
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
				variant.active = EditorPrefs.GetBool("EditorTheme-Skinset-"+variant.name+Theme.suffix,false);
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
				CallbackFunction UpdateDynamic = ()=>{
					if(theme.palette.swap.Count < 1){return;}
					foreach(var file in FileManager.FindAll("#*.png")){
						theme.palette.ApplyTexture(file.path,file.GetAsset<Texture2D>());
					}
				};
				if(Application.isPlaying){UpdateDynamic();}
				else{Utility.DelayCall("UpdateDynamicTextures",UpdateDynamic,0.1f);}
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
		public static void DrawPreferences(){
			if(Theme.active.IsNull()){return;}
			var isDefault =  Theme.active.name == "Default";
			var current = Theme.themeIndex;
			var window = EditorWindow.focusedWindow;
			if(!isDefault && !window.IsNull() && window.GetType().Name.Contains("Preferences")){
				window.minSize = new Vector2(600,100);
			}
			Theme.scroll = EditorGUILayout.BeginScrollView(Theme.scroll);
			Theme.DrawThemes();
			Theme.DrawIconsets();
			Theme.DrawPalettes();
			Theme.DrawFontsets();
			if(!isDefault){Theme.DrawOptions();}
			Theme.DrawColors();
			Theme.DrawFonts();
			if(current != Theme.themeIndex){
				var suffix = Theme.suffix.Remove("-"+Theme.active.name);
				EditorPrefs.SetString("EditorTheme"+suffix,Theme.all[Theme.themeIndex].name);
				Theme.InstantRefresh();
				Theme.ApplyIconset();
			}
			else if(!Theme.needsRebuild && GUI.changed){
				Utility.DelayCall(Theme.Rebuild,0.25f);
			}
			EditorGUILayout.EndScrollView();
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
				if(EditorGUIExtension.lastChanged){
					Theme.ApplyIconset();
				}
			}
		}
		public static void DrawPalettes(){
			var theme = Theme.active;
			bool hasPalettes = ThemePalette.all.Count > 0;
			bool paletteAltered = !theme.palette.Matches(ThemePalette.all[Theme.paletteIndex]);
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
					EditorPrefs.SetString("EditorPalette"+Theme.suffix,selectedPalette.name);
					Theme.AdjustPalette();
				}
			}
		}
		public static void DrawFontsets(){
			var theme = Theme.active;
			bool hasFontsets = ThemeFontset.all.Count > 0;
			bool fontsetAltered = !theme.fontset.Matches(ThemeFontset.all[Theme.fontsetIndex]);
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
					EditorPrefs.SetString("EditorFontset"+Theme.suffix,selectedFontset.name);
					Theme.SaveFontset();
					Theme.Rebuild();
				}
			}
		}
		public static void DrawOptions(){
			var theme = Theme.active;
			bool open = "Options".ToLabel().DrawFoldout("Theme.Options");
			if(EditorGUIExtension.lastChanged){GUI.changed = false;}
			if(open){
				EditorGUI.indentLevel += 1;
				//Theme.verticalSpacing = Theme.verticalSpacing.Draw("Vertical Spacing");
				Theme.hoverResponse = Theme.hoverResponse.Draw("Hover Response").As<HoverResponse>();
				Theme.separatePlaymodeSettings = Theme.separatePlaymodeSettings.Draw("Separate Playmode Settings");
				if(EditorGUIExtension.lastChanged && Application.isPlaying){
					EditorPrefs.SetBool("EditorTheme-SeparatePlaymodeSettings",Theme.separatePlaymodeSettings);
					Theme.Reset(true);
					return;
				}
				foreach(var variant in theme.skinset.variants){
					variant.active = variant.active.Draw(variant.name);
					if(EditorGUIExtension.lastChanged){
						Theme.Refresh();
						EditorPrefs.SetBool("EditorTheme-Skinset-"+variant.name+Theme.suffix,variant.active);
					}
				}
				Theme.window.wantsMouseMove = Theme.hoverResponse != HoverResponse.None;
				EditorPrefs.SetInt("EditorTheme-HoverResponse",Theme.hoverResponse.ToInt());
				EditorPrefs.SetBool("EditorTheme-SeparatePlaymodeSettings",Theme.separatePlaymodeSettings);
				GUILayout.Space(2);
				EditorGUI.indentLevel -= 1;
			}
		}
		public static void DrawColors(){
			var theme = Theme.active;
			bool hasPalettes = ThemePalette.all.Count > 0;
			bool paletteAltered = !theme.palette.Matches(ThemePalette.all[Theme.paletteIndex]);
			if(theme.customizablePalette && hasPalettes){
				bool open = "Colors".ToLabel().DrawFoldout("Theme.Colors");
				if(EditorGUIExtension.lastChanged){GUI.changed = false;}
				if(!open){return;}
				EditorGUI.indentLevel += 1;
				foreach(var group in theme.palette.colors.Where(x=>x.Key!="*")){
					var groupName = group.Key;
					var isGroup = groupName != "Default";
					var drawFoldout = groupName.ToLabel().DrawFoldout("Theme.Colors."+groupName);
					if(EditorGUIExtension.lastChanged){GUI.changed = false;}
					if(isGroup && !drawFoldout){continue;}
					if(isGroup){EditorGUI.indentLevel += 1;}
					var names = theme.palette.colors["*"].Keys.ToList();
					foreach(var item in theme.palette.colors[groupName]){
						var color = item.Value;
						Rect area = new Rect(1,1,1,1);
						if(!color.sourceName.IsEmpty()){
							var index = names.IndexOf(color.sourceName);
							var offsetStyle = EditorStyles.numberField.FixedWidth(35).Margin(0,0,2,0);
							EditorGUILayout.BeginHorizontal();
							if(index == -1){
								var message = "[" + color.sourceName + " not found]";
								index = names.Unshift(message).Draw(0,item.Key.ToTitleCase());
								if(index != 0){color.sourceName = names[index];}
							}
							else{
								color.sourceName = names[names.Draw(index,color.name.ToTitleCase())];
								EditorGUIUtility.labelWidth = 5;
								color.offset = color.offset.Draw(" ",offsetStyle,false);
								area = GUILayoutUtility.GetLastRect();
								EditorGUIUtility.labelWidth = 200;
								color.Assign(theme.palette.colors["*"][color.sourceName]);
							}
							GUILayout.Space(15);
							EditorGUILayout.EndHorizontal();
							area = GUILayoutUtility.GetLastRect();
							GUILayout.Space(2);
						}
						else{
							theme.palette.colors["*"][color.name].value = color.value.Draw(color.name.ToTitleCase());
							area = GUILayoutUtility.GetLastRect();
						}
						if(area.Clicked(1)){
							GenericMenu menu = new GenericMenu();
							menu.AddItem(new GUIContent("Normal"),color.sourceName.IsEmpty(),()=>color.sourceName="");
							menu.AddItem(new GUIContent("Inherited"),!color.sourceName.IsEmpty(),()=>color.sourceName=names[0]);
							menu.ShowAsContext();
							UnityEvent.current.Use();
						}
					}
					if(isGroup){EditorGUI.indentLevel -= 1;}
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
		}
		public static void DrawFonts(){
			var theme = Theme.active;
			bool hasFontsets = ThemeFontset.all.Count > 0;
			bool fontsetAltered = !theme.fontset.Matches(ThemeFontset.all[Theme.fontsetIndex]);
			if(theme.customizableFontset && hasFontsets){
				bool open = "Fonts".ToLabel().DrawFoldout("Theme.Fonts");
				if(EditorGUIExtension.lastChanged){GUI.changed = false;}
				if(!open){return;}
				EditorGUI.indentLevel += 1;
				var builtin = Resources.FindObjectsOfTypeAll<Font>().Where(x=>AssetDatabase.GetAssetPath(x).Contains("Library/unity")).ToArray();
				var ttf = FileManager.FindAll("Themes/Fonts/*.ttf");
				var otf = FileManager.FindAll("Themes/Fonts/*.otf");
				var fontFiles = ttf.Concat(otf);
				var fonts = builtin.Concat(fontFiles.Select(x=>x.GetAsset<Font>())).ToArray();
				if(Theme.fontNames.Count < 1){
					var fontPath = Theme.storagePath+"Fonts/";
					Theme.fontNames = builtin.Select(x=>"@Builtin/"+x.name).Concat(fontFiles.Select(x=>x.path)).ToList();
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
					Theme.fontNames = Theme.fontNames.Select(x=>FixFontNames(x)).ToList();
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
		//=================================
		// Iconset
		//=================================
		public static void ApplyIconset(){
			if(Theme.active.customizableIconset){
				var iconset = ThemeIconset.all[Theme.iconsetIndex];
				Theme.active.iconset = iconset;
				EditorPrefs.SetString("EditorIconset"+Theme.suffix,iconset.name);
			}
			Theme.active.iconset.Apply();
		}
		//=================================
		// Fonts
		//=================================
		public static void SaveFontset(){
			var theme = Theme.active;
			EditorPrefs.SetString("EditorTheme-"+theme.name+"-Fontset"+Theme.suffix,theme.fontset.Serialize());
		}
		public static void LoadFontset(bool reset=false){
			var theme = Theme.active;
			if(reset){
				var original = ThemeFontset.all[Theme.fontsetIndex];
				theme.fontset = new ThemeFontset(original).UseBuffer(theme.fontset);
				return;
			}
			var value = EditorPrefs.GetString("EditorTheme-"+theme.name+"-Fontset"+Theme.suffix,null);
			theme.fontset.Deserialize(value);
		}
		//=================================
		// Colors
		//=================================
		public static void SaveColors(){
			var theme = Theme.active;
			foreach(var group in theme.palette.colors.Where(x=>x.Key!="*")){
				foreach(var color in group.Value){
					EditorPrefs.SetString("EditorTheme-"+theme.name+"-Color-"+group.Key+"-"+color.Key+Theme.suffix,color.Value.Serialize());
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
					var value = EditorPrefs.GetString("EditorTheme-"+theme.name+"-Color-"+group.Key+"-"+color.Key+Theme.suffix,color.Value.Serialize());
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
		public static void Reset(){Theme.Reset(false);}
		public static void Reset(bool force){
			if(force || Application.isPlaying){
				Theme.loaded = false;
				Theme.setup = false;
			}
		}
		public static void ResetWindow(){
			Theme.window = null;
			Theme.ShowWindow();
		}
		[MenuItem("Zios/Theme/Development/Refresh #F1")]
		public static void DebugRefresh(){
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
				EditorPrefs.SetString("EditorPalette"+Theme.suffix,palette.name);
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
				EditorPrefs.SetString("EditorFontset"+Theme.suffix,defaultFontset.name);
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