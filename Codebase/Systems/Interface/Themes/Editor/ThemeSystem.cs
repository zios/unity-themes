using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace Zios.Interface{
	using UnityEditor;
	#if UNITY_EDITOR_WIN
	using Microsoft.Win32;
	#endif
	[InitializeOnLoad]
	public partial class Theme{
		public static Theme active;
		[NonSerialized] public static string revision = "2 [r487]";
		[NonSerialized] public static int themeIndex;
		[NonSerialized] public static int paletteIndex;
		[NonSerialized] public static int fontsetIndex;
		[NonSerialized] public static string storagePath;
		[NonSerialized] public static bool liveEdit;
		[NonSerialized] public static bool responsive = true;
		[NonSerialized] public static bool setup;
		[NonSerialized] public static bool disabled;
		[NonSerialized] public static bool debug;
		[NonSerialized] public static ThemeWindow window;
		[NonSerialized] private static bool needsRefresh;
		[NonSerialized] private static bool needsRebuild;
		[NonSerialized] private static float nextStep;
		[NonSerialized] private static float nextSync;
		[NonSerialized] private static Vector2 scroll = Vector2.zero;
		[NonSerialized] private static float paletteChangeTime;
		[NonSerialized] private static int paletteChangeCount;
		//[NonSerialized] public static float verticalSpacing = 2.0f;
		static Theme(){
			EditorApplication.playmodeStateChanged += ()=>Theme.nextStep = 0;
			Theme.Step();
			EditorApplication.update += Theme.ShowWindow;
		}
		public static void ShowWindow(){
			Theme.window = Resources.FindObjectsOfTypeAll<ThemeWindow>().FirstOrDefault();
			if(Theme.window.IsNull()){
				Theme.window = ScriptableObject.CreateInstance<ThemeWindow>();
				Theme.window.position = new Rect(9001,9001,1,1);
				Theme.window.minSize = new Vector2(1,1);
				Theme.window.ShowPopup();
			}
			Theme.window.position = new Rect(9001,9001,1,1);
			Theme.window.minSize = new Vector2(1,1);
		}
		public static void Step(){
			if(Time.realtimeSinceStartup < Theme.nextStep || !Theme.setup){return;}
			Theme.ShowWindow();
			//Themes.CheckPaste();
			Theme.UpdateColors();
			Utility.RepaintAll();
			Theme.nextStep = Time.realtimeSinceStartup + (Theme.responsive ? 0.05f : 1f);
		}
		public static void Update(){
			if(EditorApplication.isCompiling || EditorApplication.isUpdating){return;}
			if(Theme.needsRefresh){
				Theme.UpdateSettings();
				Utility.DelayCall(ThemeContent.Apply,0.5f);
				if(Theme.needsRebuild){
					Theme.needsRebuild = false;
					return;
				}
				Utility.CallEditorPref("EditorTheme-Refresh");
				Theme.needsRefresh = false;
			}
			if(Theme.needsRebuild){
				Theme.RebuildStyles();
				Theme.Refresh();
				Utility.CallEditorPref("EditorTheme-Rebuild");
			}
			if(!Theme.setup){
				var themes = FileManager.Find("*.unitytheme",Theme.debug);
				if(themes.IsNull()){
					Debug.LogWarning("[Themes] No .unityTheme files found. Disabling until refreshed.");
					Theme.setup = true;
					Theme.disabled = true;
					return;
				}
				Theme.storagePath = themes.GetFolderPath().Trim("/","\\").GetDirectory()+"/";
				Theme.Load();
				Theme.UpdateSettings();
				Theme.UpdateColors();
				if(!Theme.setup){
					Utility.CallEditorPref("EditorTheme-Setup");
					Theme.setup = true;
					Theme.Rebuild();
				}
			}
		}
		public static void Load(){
			Theme.all.Clear();
			ThemeFontset.all.Clear();
			ThemePalette.all.Clear();
			ThemePalette.Import();
			Theme.Import();
			var activeThemeName = EditorPrefs.GetString("EditorTheme","@Default");
			var activePaletteName = EditorPrefs.GetString("EditorPalette","Slate");
			var activeFontsetName = EditorPrefs.GetString("EditorFontset-"+activeThemeName,activeThemeName);
			Theme.themeIndex = Theme.all.FindIndex(x=>x.name==activeThemeName).Max(0);
			Theme.paletteIndex = ThemePalette.all.FindIndex(x=>x.name==activePaletteName).Max(0);
			Theme.fontsetIndex = ThemeFontset.all.AddNew(activeThemeName).FindIndex(x=>x.name==activeFontsetName).Max(0);
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
			if(theme.allowSystemColor && theme.useSystemColor && theme.palette.colors.Count > 0){
				var mainColor = theme.palette.colors.First().Value;
				var parsedColor = mainColor.value;
				object key = null;
				#if UNITY_EDITOR_WIN
				key = Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\DWM\\","AccentColor",null);
				key = key ?? Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Accent","AccentColor",null);
				if(!key.IsNull()){
					int value = key.As<int>();
					parsedColor = value  != -1 ? value.ToHex().ToColor(true) : mainColor.value;
				}
				else{
					key = Registry.GetValue("HKEY_LOCAL_MACHINE\\Software\\Policies\\Microsoft\\Windows\\Personalization","PersonalColor_Accent",null);
					key = key ?? Registry.GetValue("HKEY_CURRENT_USER\\Control Panel\\Colors","WindowFrame",null);
					if(!key.IsNull()){
						string value = key.As<string>();
						parsedColor = value != "" ? value.ToColor(" ",false,false) : mainColor.value;
					}
				}
				#endif
				if(parsedColor != mainColor.value){
					mainColor.ApplyColor(parsedColor);
					foreach(var color in theme.palette.colors){
						color.Value.ApplyOffset(mainColor);
					}
					Theme.Refresh();
				}
				return;
			}
			foreach(var color in theme.palette.colors){
				color.Value.ApplyOffset();
			}
			EditorPrefs.SetBool("EditorTheme-Dark",Theme.active.palette.Get("Window").GetIntensity() < 0.4f);
		}
		public static void UpdateSettings(){
			if(Theme.all.Count < 1){return;}
			var baseTheme = Theme.all[Theme.themeIndex];
			var theme = Theme.active = new Theme().Use(baseTheme);
			theme.useSystemColor = EditorPrefs.GetBool("EditorTheme-"+theme.name+"-UseSystemColor",false);
			Theme.responsive = EditorPrefs.GetBool("EditorTheme-ResponsiveUI",true);
			if(theme.allowColorCustomization && ThemePalette.all.Count > 0){
				var basePalette = ThemePalette.all[Theme.paletteIndex];
				theme.palette = new ThemePalette().Use(basePalette);
				Theme.LoadColors();
				Theme.UpdateColors();
			}
			if(theme.allowFontsetCustomization && ThemeFontset.all.AddNew(baseTheme.name).Count > 0){
				var baseFontset = ThemeFontset.all[baseTheme.name][Theme.fontsetIndex];
				theme.fontset = new ThemeFontset().Use(baseFontset);
				Theme.LoadFontset();
			}
			foreach(Theme option in theme.options.Where(x=>x.name.StartsWith("+"))){
				var optionName = "EditorTheme-"+theme.name+option.name.Remove("+");
				if(EditorPrefs.GetBool(optionName)){
					theme.Use(option);
				}
			}
			if(theme.useColorAssets){
				foreach(var color in theme.palette.colors){
					if(color.Value.skipTexture){continue;}
					color.Value.UpdateTexture(Theme.active.path);
				}
			}
			Theme.Apply();
		}
		public static void Apply(string themeName=""){
			if(Theme.active.IsNull()){return;}
			var theme = Theme.active;
			if(themeName.IsEmpty()){themeName = theme.name;}
			var isDefault = themeName == "@Default";
			var loadPath = Theme.storagePath+themeName;
			var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
			var main = Style.GetSkin(themeName,false);
			if(Theme.debug && !main.IsNull() && main.customStyles.Length != skin.customStyles.Length){
				Debug.LogWarning("[Themes] Mismatched style count [" + skin.customStyles.Length + "] for main editor skin [" + main.customStyles.Length+"]. Possible version conflict.");
			}
			if(!main.IsNull()){
				var palette = theme.palette;
				if(!isDefault && !Theme.liveEdit){main = theme.fontset.Apply(main);}
				skin.Use(main,!Theme.liveEdit);
				Utility.GetUnityType("AppStatusBar").ClearVariable("background");
				Utility.GetUnityType("RenameOverlay").SetVariable("s_DefaultTextFieldStyle",skin.textField);
				Utility.GetUnityType("SceneRenderModeWindow.Styles").SetVariable("sMenuItem",skin.Get("MenuItem"));
				Utility.GetUnityType("SceneRenderModeWindow.Styles").SetVariable("sSeparator",skin.Get("sv_iconselector_sep"));
				Utility.GetUnityType("GameView.Styles").SetVariable("gizmoButtonStyle",skin.Get("GV Gizmo DropDown"));
				typeof(SceneView).SetVariable<GUIStyle>("s_DropDownStyle",skin.Get("GV Gizmo DropDown"));
				var console = Utility.GetUnityType("ConsoleWindow.Constants");
				console.SetVariable("ms_Loaded",false);
				console.CallMethod("Init");
				var hostView = Utility.GetUnityType("HostView");
				if(!theme.windowBackgroundOverride.IsNull()){skin.GetStyle("TabWindowBackground").normal.background = theme.windowBackgroundOverride;}
				if(palette.Has("Cursor")){skin.settings.cursorColor = palette.Get("Cursor");}
				if(palette.Has("Selection")){skin.settings.selectionColor = palette.Get("Selection");}
				if(palette.Has("Curve")){typeof(EditorGUI).SetVariable("kCurveColor",palette.Get("Curve"));}
				if(palette.Has("CurveBackground")){typeof(EditorGUI).SetVariable("kCurveBGColor",palette.Get("CurveBackground"));}
				if(palette.Has("Window")){
					typeof(EditorGUIUtility).SetVariable("kDarkViewBackground",palette.Get("Window"));
					hostView.SetVariable<Color>("kViewColor",palette.Get("Window"));
				}
				foreach(var view in Resources.FindObjectsOfTypeAll(hostView)){
					//view.SetVariable<GUIStyle>("background",skin.Get("hostview"));
					view.ClearVariable("background");
				}
				foreach(var window in Locate.GetAssets<EditorWindow>()){
					window.antiAlias = 1;
					window.minSize = window.GetType().Name.Contains("Preferences") ? window.minSize : new Vector2(100,20);
					window.wantsMouseMove = Theme.responsive;
					window.autoRepaintOnSceneChange = Theme.responsive;
				}
				if(!isDefault){
					var preferences = Utility.GetUnityType("PreferencesWindow");
					var constants = preferences.GetVariable("constants");
					if(constants.IsNull()){
						var instance = Activator.CreateInstance(preferences.GetVariableType("constants"));
						preferences.SetVariable("constants",instance);
					}
					preferences.GetVariable("constants").SetVariable("sectionHeader",skin.Get("LargeLabel"));
				}
			}
			foreach(var skinFile in FileManager.FindAll(loadPath+"/*.guiskin",true,false)){
				if(skinFile.name == themeName){continue;}
				var field = skinFile.name.Split(".").Last();
				var parent = skinFile.name.Replace("."+field,"");
				var typeDirect = Utility.GetUnityType(skinFile.name);
				var typeParent = Utility.GetUnityType(parent);
				skin = isDefault || Theme.liveEdit ? skinFile.GetAsset<GUISkin>() : theme.fontset.Apply(skinFile.GetAsset<GUISkin>());
				var styles = skin.GetNamedStyles(false,true,true);
				var flags = field.Contains("s_Current") ? ObjectExtension.privateFlags : ObjectExtension.staticFlags;
				if(Theme.debug && typeDirect.IsNull() && (typeParent.IsNull() || !typeParent.HasVariable(field))){
					Debug.LogWarning("[Themes] No matching class/field found for GUISkin -- " + skinFile.name + ". Possible version conflict.");
					continue;
				}
				Action<Dictionary<string,GUIStyle>,object> SetStyles = (current,scope)=>{
					if(Theme.debug && current.Count != 0 && current.Count != styles.Count){
						Debug.LogWarning("[Themes] Mismatched style count [" + current.Count + "] for -- " + skinFile.name + "[" + styles.Count+"]. Possible version conflict.");
					}
					foreach(var item in current){
						if(styles.ContainsKey(item.Key)){
							var baseName = styles[item.Key].name.Parse("[","]");
							var replacement = styles[item.Key];
							var newStyle = Theme.liveEdit ? replacement : new GUIStyle(replacement).Rename(baseName);
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
						try{
							target = Activator.CreateInstance(typeParent.GetVariableType(field));
							typeParent.SetVariable(field,target);
						}
						catch{continue;}
					}
					SetStyles(target.GetVariables<GUIStyle>(),target);
				}
			}
		}
		//=================================
		// Preferences
		//=================================
		[PreferenceItem("Themes")]
		public static void ShowPreferences(){
			if(Theme.active.IsNull()){return;}
			var theme = Theme.active;
			var isDefault = theme.name == "@Default";
			var current = Theme.themeIndex;
			var window = EditorWindow.focusedWindow;
			if(!isDefault && !window.IsNull() && window.GetType().Name.Contains("Preferences")){
				window.minSize = new Vector2(600,100);
			}
			Theme.scroll = EditorGUILayout.BeginScrollView(Theme.scroll);
			EditorGUIUtility.labelWidth = 140;
			Theme.themeIndex = Theme.all.Select(x=>x.name).Draw(Theme.themeIndex,"Theme");
			GUILayout.Space(3);
			if(theme.allowCustomization){
				bool hasPalettes = ThemePalette.all.Count > 0;
				bool hasFontsets = ThemeFontset.all.Count > 0 && ThemeFontset.all[theme.name].Count > 0;
				bool paletteAltered = !theme.palette.Matches(ThemePalette.all[Theme.paletteIndex]);
				bool fontsetAltered = !theme.fontset.Matches(ThemeFontset.all[theme.name][Theme.fontsetIndex]);
				if(theme.allowColorCustomization && hasPalettes){
					var paletteNames = ThemePalette.all.Select(x=>x.name).ToList();
					if(paletteAltered){paletteNames[Theme.paletteIndex] = "[Custom]";}
					if(theme.useSystemColor){paletteNames[Theme.paletteIndex] = "[System]";}
					GUI.enabled = !theme.useSystemColor;
					Theme.paletteIndex = paletteNames.Draw(Theme.paletteIndex,"Palette");
					GUI.enabled = true;
					GUILayout.Space(3);
					if(EditorGUIExtension.lastChanged){
						var selectedPalette = ThemePalette.all[Theme.paletteIndex];
						theme.palette = new ThemePalette().Use(selectedPalette);
						EditorPrefs.SetString("EditorPalette",selectedPalette.name);
						Theme.SaveColors();
					}
				}
				if(theme.allowFontsetCustomization && hasFontsets){
					var fontsetNames = ThemeFontset.all[theme.name].Select(x=>x.name).ToList();
					if(fontsetAltered){fontsetNames[Theme.fontsetIndex] = "[Custom]";}
					Theme.fontsetIndex = fontsetNames.Draw(Theme.fontsetIndex,"Fontset");
					GUILayout.Space(3);
					if(EditorGUIExtension.lastChanged){
						var selectedFontset = ThemeFontset.all[theme.name][Theme.fontsetIndex];
						theme.fontset = new ThemeFontset().Use(selectedFontset);
						EditorPrefs.SetString("EditorFontset-"+theme.name,selectedFontset.name);
						Theme.SaveFontset();
						Theme.Rebuild();
					}
				}
				bool open = "Options".ToLabel().DrawFoldout("Theme.Options");
				if(EditorGUIExtension.lastChanged){GUI.changed = false;}
				if(open){
					EditorGUI.indentLevel += 1;
					//Theme.verticalSpacing = Theme.verticalSpacing.Draw("Vertical Spacing");
					Theme.responsive = Theme.responsive.Draw("Responsive UI");
					EditorPrefs.SetBool("EditorTheme-ResponsiveUI",Theme.responsive);
					GUILayout.Space(2);
					foreach(var toggle in theme.options.Where(x=>x.name.StartsWith("+"))){
						var toggleName = "EditorTheme-"+theme.name+toggle.name.Remove("+");
						EditorPrefs.SetBool(toggleName,EditorPrefs.GetBool(toggleName).Draw(toggle.name.ToTitleCase().Remove("+")));
						GUILayout.Space(2);
					}
					EditorGUI.indentLevel -= 1;
				}
				if(theme.allowColorCustomization){
					if(hasPalettes){
						open = "Colors".ToLabel().DrawFoldout("Theme.Colors");
						if(EditorGUIExtension.lastChanged){GUI.changed = false;}
						if(open){
							EditorGUI.indentLevel += 1;
							if(theme.allowSystemColor){
								theme.useSystemColor = theme.useSystemColor.Draw("Use System Color");
								EditorPrefs.SetBool("EditorTheme-"+theme.name+"-UseSystemColor",theme.useSystemColor);
								if(EditorGUIExtension.lastChanged && !theme.useSystemColor){Theme.LoadColors();}
							}
							if(!theme.useSystemColor){
								var names = theme.palette.colors.Keys.ToList();
								foreach(var item in theme.palette.colors){
									var color = item.Value;
									if(!color.sourceName.IsEmpty()){
										EditorGUILayout.BeginHorizontal();
										var index = names.IndexOf(color.sourceName);
										var offsetStyle = EditorStyles.numberField.FixedWidth(35).Margin(0,0,2,0);
										color.sourceName = names[names.Draw(index,color.name)];
										EditorGUIUtility.labelWidth = 5;
										color.offset = color.offset.Draw(" ",offsetStyle,false);
										EditorGUIUtility.labelWidth = 140;
										color.Assign(theme.palette.colors[color.sourceName]);
										GUILayout.Space(15);
										EditorGUILayout.EndHorizontal();
										GUILayout.Space(2);
										continue;
									}
									theme.palette.colors[color.name].value = color.value.Draw(color.name);
								}
								if(paletteAltered){
									EditorGUILayout.BeginHorizontal();
									GUILayout.Space(15);
									if(GUILayout.Button("Save",GUILayout.Width(100))){theme.palette.Export();}
									if(GUILayout.Button("Reset",GUILayout.Width(100))){Theme.LoadColors(true);}
									EditorGUILayout.EndHorizontal();
								}
								Theme.SaveColors();
							}
							EditorGUI.indentLevel -=1;
						}
					}
				}
				if(hasFontsets && theme.allowFontsetCustomization){
					open = "Fonts".ToLabel().DrawFoldout("Theme.Fonts");
					if(EditorGUIExtension.lastChanged){GUI.changed = false;}
					if(open){
						EditorGUI.indentLevel += 1;
						var ttf = FileManager.GetAssets<Font>("@Fonts/*.ttf");
						var otf = FileManager.GetAssets<Font>("@Fonts/*.otf");
						var fonts = ttf.Concat(otf);
						var fontNames = fonts.Select(x=>x.name).ToArray();
						if(fontNames.Length < 1){fontNames = fontNames.Add("No fonts found.");}
						GUIStyleExtension.autoLayout = false;
						bool existingChanges = GUI.changed;
						foreach(var item in theme.fontset.fonts){
							if(item.Value.font.IsNull()){continue;}
							var themeFont = item.Value;
							EditorGUILayout.BeginHorizontal();
							var index = fonts.IndexOf(themeFont.font);
							if(index == -1){
								EditorGUILayout.EndHorizontal();
								var message = "[" + themeFont.name + " not found]";
								index = fontNames.Unshift(message).Draw(0,item.Key);
								if(index != 0){themeFont.font = fonts[index-1];}
								continue;
							}
							var offsetStyle = EditorStyles.numberField.FixedWidth(35).Margin(0,0,2,0);
							themeFont.font = fonts[fontNames.Draw(index,item.Key)];
							EditorGUIUtility.labelWidth = 38;
							EditorGUIUtility.fieldWidth = 25;
							themeFont.sizeOffset = themeFont.sizeOffset.DrawInt("Size",offsetStyle.FixedWidth(25),false);
							EditorGUIUtility.labelWidth = 20;
							EditorGUIUtility.fieldWidth = 35;
							themeFont.offsetX = themeFont.offsetX.Draw("X",offsetStyle,false);
							themeFont.offsetY = themeFont.offsetY.Draw("Y",offsetStyle,false);
							EditorGUIUtility.labelWidth = 140;
							EditorGUILayout.EndHorizontal();
						}
						GUIStyleExtension.autoLayout = true;
						if(fontsetAltered){
							EditorGUILayout.BeginHorizontal();
							GUILayout.Space(15);
							if(GUILayout.Button("Save",GUILayout.Width(100))){theme.fontset.Export();}
							if(GUILayout.Button("Reset",GUILayout.Width(100))){Theme.LoadFontset(true);}
							EditorGUILayout.EndHorizontal();
						}
						if(GUI.changed && !existingChanges){
							Utility.DelayCall(Theme.Rebuild,0.5f);
						}
						EditorGUI.indentLevel -=1;
						Theme.SaveFontset();
						GUILayout.Space(10);
					}
				}
			}
			if(current != Theme.themeIndex){
				EditorPrefs.SetString("EditorTheme",Theme.all[Theme.themeIndex].name);
				Theme.UpdateSettings();
				Theme.RebuildStyles();
				Utility.RebuildInspectors();
			}
			if(!Theme.needsRebuild && GUI.changed){
				if(!Theme.responsive){Utility.DelayCall(Theme.Refresh,0.5f);}
				else{Theme.Refresh();}
			}
			EditorGUILayout.EndScrollView();
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
				var original = ThemeFontset.all[theme.name][Theme.fontsetIndex];
				theme.fontset = new ThemeFontset().Use(original);
			}
			else if(theme.allowFontsetCustomization){
				var value = EditorPrefs.GetString("EditorTheme-"+theme.name+"-Fontset",null);
				theme.fontset.Deserialize(value);
			}
		}
		//=================================
		// Colors
		//=================================
		public static void SaveColors(){
			var theme = Theme.active;
			foreach(var color in theme.palette.colors){
				EditorPrefs.SetString("EditorTheme-"+theme.name+"-Color-"+color.Key,color.Value.Serialize());
			}
		}
		public static void LoadColors(bool reset=false){
			var theme = Theme.active;
			if(!theme.useSystemColor){
				if(reset){
					var original = ThemePalette.all[Theme.paletteIndex];
					theme.palette = new ThemePalette().Use(original);
				}
				else if(theme.allowColorCustomization){
					var colors = theme.palette.colors;
					foreach(var color in colors){
						var name = color.Key;
						var value = EditorPrefs.GetString("EditorTheme-"+theme.name+"-Color-"+name,color.Value.Serialize());
						colors[name].Deserialize(value);
					}
					foreach(var color in colors){
						if(color.Value.sourceName.IsEmpty()){continue;}
						var source = colors[color.Value.sourceName];
						colors[color.Key].Assign(source);
					}
				}
			}
		}
		public static void CheckPaste(){
			if(Time.realtimeSinceStartup > Theme.nextSync){
				Theme.nextSync = Time.realtimeSinceStartup + 0.5f;
				var clipboard = EditorGUIUtility.systemCopyBuffer;
				if(!clipboard.IsEmpty()){
					try{
						/*bool changes = false;
						if(clipboard.Contains("Color #")){
							Themes.active.palette.background = clipboard.Parse("Color","\n").Trim().ToColor();
						}
						if(clipboard.Contains("DarkColor #")){
							Themes.active.palette.backgroundDark = clipboard.Parse("DarkColor","\n").Trim().ToColor();
						}
						if(clipboard.Contains("LightColor #")){
							Themes.active.palette.backgroundLight = clipboard.Parse("LightColor","\n").Trim().ToColor();
						}*/
					}
					catch{}
				}
			}
		}
		//=================================
		// Shortcuts
		//=================================
		[MenuItem("Zios/Theme/Refresh #F1")]
		public static void ForceRefresh(){
			Debug.Log("[Themes] Forced Refresh.");
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
		public static void AdjustPalette(int adjust){
			var theme = Theme.active;
			if(!theme.IsNull() && theme.allowCustomization && theme.allowColorCustomization){
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
			}
		}
		[MenuItem("Zios/Theme/Previous Fontset %F1")]
		public static void PreviousFontset(){Theme.AdjustFontset(-1);}
		[MenuItem("Zios/Theme/Next Fontset %F2")]
		public static void NextFontset(){Theme.AdjustFontset(1);}
		public static void AdjustFontset(int adjust){
			var theme = Theme.active;
			if(!theme.IsNull() && theme.allowCustomization && theme.allowFontsetCustomization){
				Theme.fontsetIndex = (Theme.fontsetIndex + adjust) % ThemeFontset.all[theme.name].Count;
				if(Theme.fontsetIndex < 0){Theme.fontsetIndex = ThemeFontset.all[theme.name].Count-1;}
				var fontset = ThemeFontset.all[theme.name][Theme.fontsetIndex];
				theme.fontset = new ThemeFontset().Use(fontset);
				EditorPrefs.SetString("EditorFontset-"+theme.name,fontset.name);
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
		}
		public void OnPreprocessTexture(){
			TextureImporter importer = (TextureImporter)this.assetImporter;
			if(importer.assetPath.Contains("Color")){
				importer.isReadable = true;
				if(importer.assetPath.Contains("Border")){
					importer.filterMode = FilterMode.Point;
				}
			}
		}
	}
}