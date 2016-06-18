using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
namespace Zios{
	#if UNITY_EDITOR
	using UnityEditor;
	public static partial class Themes{
		public static void Load(){
			Themes.all.Clear();
			Themes.palettes.Clear();
			Themes.ParsePalettes();
			Themes.ParseThemes();
			var activeThemeName = EditorPrefs.GetString("EditorTheme","@Default");
			var activePaletteName = EditorPrefs.GetString("EditorPalette","Slate");
			Themes.themeIndex = Themes.all.FindIndex(x=>x.name==activeThemeName).Max(0);
			Themes.paletteIndex = Themes.palettes.FindIndex(x=>x.name==activePaletteName).Max(0);
		}
		public static void ParsePalettes(){
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
		}
		public static void ParseThemes(){
			var themes = FileManager.FindAll("*.unitytheme");
			foreach(var themeFile in themes){
				Theme theme = null;
				Theme root = null;
				foreach(var line in themeFile.GetText().GetLines()){
					if(line.Trim().IsEmpty()){continue;}
					if(line.Contains("[")){
						string name = theme.IsNull() ? themeFile.name : line.Parse("[","]");
						theme = root.IsNull() ? Themes.all.AddNew() : root.variants.AddNew();
						theme.name = name.ToPascalCase();
						theme.path = themeFile.GetAssetPath().GetDirectory();
						if(root.IsNull()){
							root = theme;
							Themes.ParseGUIContent(theme,themeFile.directory);
						}
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
		}
		public static void ParseGUIContent(Theme theme,string path){
			if(theme.name == "@Default"){return;}
			var contents = FileManager.FindAll(path+"/*.guiContent",true,false);
			foreach(var contentFile in contents){
				var content = new ThemeContent();
				var contentName = "";
				foreach(var line in contentFile.GetText().GetLines()){
					if(line.Trim().IsEmpty()){continue;}
					if(line.ContainsAll("[","]")){
						if(!contentName.IsEmpty()){
							content.Setup(contentFile.name,contentName);
							theme.contents.Add(content);
						}
						content = new ThemeContent();
						contentName = line.Parse("[","]");
					}
					else{
						var term = line.Parse("","=").Trim();
						var value = line.Parse("=").Trim();
						if(term == "image"){content.value.image = FileManager.GetAsset<Texture2D>(path+"/GUIContent/"+value+".png");}
						else if(term == "text"){content.value.text = value;}
						else if(term == "tooltip"){content.value.tooltip = value;}
					}
				}
				if(!contentName.IsEmpty()){
					content.Setup(contentFile.name,contentName);
					theme.contents.Add(content);
				}
			}
		}
	}
	public class Theme{
		[Internal] public string name;
		[Internal] public string path;
		[Internal] public bool useSystemColor;
		[Internal] public List<ThemeContent> contents = new List<ThemeContent>();
		[Internal] public List<Texture2D> colorImages = new List<Texture2D>();
		[Internal] public List<GUIStyle> colorStyles = new List<GUIStyle>();
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
			this.contents = other.contents;
			return this;
		}
	}
	public class ThemeContent{
		public static Dictionary<string,GUIContent> original = new Dictionary<string,GUIContent>();
		public string name;
		public string path;
		public object scope;
		public GUIContent target = new GUIContent();
		public GUIContent value = new GUIContent();
		public void Setup(string path,string contentName){
			this.path = path;
			this.name = contentName;
		}
		public void SyncScope(){
			string field = this.path.Split(".").Last();
			string parent =  this.path.Replace("."+field,"");
			var typeDirect = Utility.GetUnityType(this.path);
			var typeParent = Utility.GetUnityType(parent);
			if(typeDirect.IsNull() && (typeParent.IsNull() || !typeParent.HasVariable(field))){
				if(Themes.debug){Debug.LogWarning("[Themes] No matching class/field found for GUIContent -- " + path);}
				return;
			}
			this.scope = typeDirect ?? typeParent.GetVariable(field);
			if(this.scope.IsNull()){
				try{
					this.scope = Activator.CreateInstance(typeParent.GetVariableType(field));
					typeParent.SetVariable(field,this.scope);
				}
				catch{}
			}
		}
		public void SyncTarget(){
			if(this.scope.IsNull()){return;}
			bool isArray = this.scope.GetType() == typeof(GUIContent[]);
			if(isArray || this.scope.HasVariable(this.name)){
				this.target = this.scope.GetVariable<GUIContent>(this.name);
				if(this.target.IsNull()){
					this.target = new GUIContent();
					this.scope.SetVariable(this.name,this.target);
				}
				var path = this.path+"."+this.name;
				if(!ThemeContent.original.ContainsKey(path)){
					ThemeContent.original[path] = new GUIContent(this.target);
				}
			}
		}
		public void Apply(){
			this.SyncScope();
			this.SyncTarget();
			this.target.text = this.value.text;
			this.target.tooltip = this.value.tooltip;
			this.target.image = this.value.image;
		}
		public void Revert(){
			this.SyncScope();
			this.SyncTarget();
			var path = this.path+"."+this.name;
			if(!ThemeContent.original.ContainsKey(path)){return;}
			var original = ThemeContent.original[path];
			this.target.text = original.text;
			this.target.tooltip = original.tooltip;
			this.target.image = original.image;
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
	#endif
}