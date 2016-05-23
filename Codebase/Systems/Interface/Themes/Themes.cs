using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace Zios{
	using Events;
	using Interface;
	using Containers;
	#if UNITY_EDITOR
	using UnityEditor;
	#if UNITY_EDITOR_WIN
	using Microsoft.Win32;
	#endif
	public static class Themes{
		public static List<ThemePalette> palettes = new List<ThemePalette>();
		public static List<Theme> all = new List<Theme>();
		public static Theme active;
		[NonSerialized] public static int themeIndex;
		[NonSerialized] public static int paletteIndex;
		[NonSerialized] public static string storagePath = "Assets/@Zios/Interface/Skins/";
		[NonSerialized] public static bool setup;
		[NonSerialized] public static bool needsRefresh;
		[NonSerialized] public static bool needsRebuild;
		[NonSerialized] private static Color lastSystemColor = Color.clear;
		[NonSerialized] private static float nextUpdate;
		[NonSerialized] public static string createName;
		[NonSerialized] public static Dictionary<string,object> styleGroupBuffer = new Dictionary<string,object>();
		[NonSerialized] public static Hierarchy<string,string,GUIContent> contentBuffer = new Hierarchy<string,string,GUIContent>();
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
			}
			if(GUI.changed){
				//Themes.Create();
				Themes.lastSystemColor = Color.clear;
				Themes.needsRefresh = true;
				Utility.RepaintAll();
			}
		}
		//=================================
		// Saving
		//=================================
		public static void Create(string name="@Default"){
			Themes.createName = name;
			Themes.styleGroupBuffer.Clear();
			var allTypes = typeof(UnityEditor.Editor).Assembly.GetTypes().Where(x=>!x.IsNull()).ToArray();
			Event.AddStepper("On Editor Update",Themes.CreateStep,allTypes,50);
		}
		public static void CreateStep(object collection,int itemIndex){
			var types = (Type[])collection;
			var type = types[itemIndex];
			if(!type.Name.ContainsAny("$","__Anon","<")){
				Event.stepperTitle = "Scanning " + types.Length + " Types";
				Event.stepperMessage = "Analyzing : " + type.Name;
				var terms = new string[]{"Styles","styles","s_GOStyles","s_Current","s_Styles","m_Styles","ms_Styles","constants"};
				foreach(var term in terms){
					if(!type.HasVariable(term,ObjectExtension.staticFlags)){continue;}
					try{
						var styleGroup = type.GetVariable(term,-1,ObjectExtension.staticFlags) ?? Activator.CreateInstance(type.GetVariableType(term));
						Themes.styleGroupBuffer[type.FullName+"."+term] = styleGroup;
					}
					catch{}
				}
				try{
					var styles = type.GetVariables<GUIStyle>(null,ObjectExtension.staticFlags);
					var content = type.GetVariables<GUIContent>(null,ObjectExtension.staticFlags);
					var contentGroups = type.GetVariables<GUIContent[]>(null,ObjectExtension.staticFlags);
					if(styles.Count > 0){Themes.styleGroupBuffer[type.FullName] = styles;}
					if(content.Count > 0){Themes.contentBuffer[type.FullName] = content;}
					foreach(var contentSet in contentGroups){
						if(contentSet.Value.IsNull() || contentSet.Value.Length < 1){continue;}
						var contents = Themes.contentBuffer[type.FullName+"."+contentSet.Key] = new Dictionary<string,GUIContent>();
						for(int index=0;index<contentSet.Value.Length;++index){
							contents[index.ToString()] = contentSet.Value[index];
						}
					}
				}
				catch{}
			}
			if(itemIndex >= types.Length-1){
				var savePath = Themes.storagePath+Themes.createName;
				EditorUtility.ClearProgressBar();
				Directory.CreateDirectory(savePath);
				Directory.CreateDirectory(savePath+"/GUIContent");
				Directory.CreateDirectory(savePath+"/Background");
				foreach(var buffer in Themes.styleGroupBuffer){
					var customStyles = new List<GUIStyle>();
					var skinPath = savePath+"/"+buffer.Key+".guiskin";
					var contentPath = savePath+"/"+buffer.Key+".guicontent";
					var styles = buffer.Value is Dictionary<string,GUIStyle> ? (Dictionary<string,GUIStyle>)buffer.Value : buffer.Value.GetVariables<GUIStyle>().Distinct();
					foreach(var styleData in styles){
						var style = new GUIStyle(styleData.Value);
						if(!buffer.Key.Contains("s_Current")){style.Rename(styleData.Key);}
						customStyles.Add(style);
					}
					if(customStyles.Count > 0){
						GUISkin newSkin = ScriptableObject.CreateInstance<GUISkin>();
						newSkin.name = buffer.Key;
						newSkin.customStyles = customStyles.ToArray();
						AssetDatabase.CreateAsset(newSkin,skinPath);
						newSkin.SaveBackgrounds(savePath+"/Background/");
					}
					Themes.SaveGUIContent(contentPath,buffer.Value.GetVariables<GUIContent>());
				}
				foreach(var buffer in Themes.contentBuffer){
					var contentPath = savePath+"/"+buffer.Key+".guicontent";
					Themes.SaveGUIContent(contentPath,buffer.Value);
				}
				var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
				skin = ScriptableObject.CreateInstance<GUISkin>().Use(skin);
				skin.SaveBackgrounds(savePath+"/Background/");
				AssetDatabase.CreateAsset(skin,savePath+"/"+Themes.createName+".guiskin");
			}
		}
		public static void SaveGUIContent(string path,Dictionary<string,GUIContent> data){
			if(data.Count < 1){return;}
			var contents = "";
			var keys = data.Keys.ToList();
			keys.Sort();
			foreach(var key in keys){
				if(key.ContainsAny("<",">")){continue;}
				GUIContent value = data[key];
				contents = contents.AddLine("["+key+"]");
				if(!value.text.IsEmpty()){contents = contents.AddLine("text = "+value.text);}
				if(!value.image.IsNull()){
					var image = value.image;
					var imagePath = path.GetDirectory()+"/GUIContent/"+image.name+".png";
					contents = contents.AddLine("image = "+image.name);
					if(!File.Exists(imagePath)){
						image.SaveAs(imagePath,true);
					}
				}
				if(!value.tooltip.IsEmpty()){contents = contents.AddLine("tooltip = "+value.tooltip);}
				contents = contents.AddLine("");
			}
			FileManager.CreateFile(path).WriteText(contents.Trim());
		}
		public static void SavePalette(){
			var path = EditorUtility.SaveFilePanel("Save Palette",Themes.storagePath+"@Palettes","TheColorsDuke","unitypalette");
			if(path.Length > 0){
				Themes.LoadColors();
				var palette = Themes.active.palette;
				var file = FileManager.CreateFile(path);
				var contents = "";
				contents = contents.AddLine("Color "+palette.background.ToHex(false));
				contents = contents.AddLine("DarkColor "+palette.backgroundDark.value.Serialize());
				contents = contents.AddLine("LightColor "+palette.backgroundLight.value.Serialize());
				file.WriteText(contents);
				EditorPrefs.SetString("EditorPalette",path.GetFileName());
				FileManager.Refresh();
				Themes.setup = false;
			}
		}
		//=================================
		// Loading
		//=================================
		public static void Load(){
			Themes.all.Clear();
			Themes.palettes.Clear();
			foreach(var file in FileManager.FindAll("*.unitypalette")){
				var palette = Themes.palettes.AddNew();
				palette.name = file.name;
				foreach(var line in file.GetText().GetLines()){
					if(line.Trim().IsEmpty()){continue;}
					var term = line.Parse(""," ").Trim();
					var value = line.Parse(" ").Trim().Trim("=").Trim();
					if(term.Matches("Color",true)){palette.background = value.ToColor();}
					else if(term.Matches("DarkColor",true)){palette.backgroundDark = value;}
					else if(term.Matches("LightColor",true)){palette.backgroundLight = value;}
				}

			}
			var themes = FileManager.FindAll("*.unitytheme");
			foreach(var file in themes){
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
					var value = line.Parse(" ").Trim().Trim("=").Trim();
					if(value.Matches("None",true)){continue;}
					else if(term.Matches("AllowSystemColor",true)){theme.allowSystemColor = value.ToBool();}
					else if(term.Matches("AllowCustomization",true)){theme.allowCustomization = value.ToBool();}
					else if(term.Matches("AllowColorCustomization",true)){theme.allowColorCustomization = value.ToBool();}
					else if(term.Matches("UseSystemColor",true)){theme.useSystemColor = value.ToBool();}
					else if(term.Matches("UseColorAssets",true)){theme.useColorAssets = value.ToBool();}
					else if(term.Matches("FontOverride",true)){theme.fontOverride = FileManager.GetAsset<Font>(value);}
					else if(term.Matches("WindowBackgroundOverride",true)){theme.windowBackgroundOverride = FileManager.GetAsset<Texture2D>(value);}
					else if(term.Matches("FontScale",true)){theme.fontScale = value.ToFloat();}
					else if(term.Matches("Palette",true)){theme.palette = palettes.Find(x=>x.name==value) ?? new ThemePalette();}
				}
			}
			var activeThemeName = EditorPrefs.GetString("EditorTheme","@Default");
			var activePaletteName = EditorPrefs.GetString("EditorPalette","Slate");
			Themes.themeIndex = Themes.all.FindIndex(x=>x.name==activeThemeName).Max(0);
			Themes.paletteIndex = Themes.palettes.FindIndex(x=>x.name==activePaletteName).Max(0);
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
			var imagePath = theme.path+"/"+theme.name+colorName+".png";
			var borderPath = theme.path+"/"+theme.name+colorName+"Border.png";
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