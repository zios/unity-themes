using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
namespace Zios.Interface{
	[Serializable]
	public partial class Theme{
		public static List<Theme> all = new List<Theme>();
		[Internal] public string name;
		[Internal] public string path;
		[Internal] public bool useSystemColor;
		[Internal] public ThemePalette palette = new ThemePalette();
		[Internal] public ThemeFontset fontset = new ThemeFontset();
		[Internal] public List<ThemeContent> contents = new List<ThemeContent>();
		[Internal] public List<Theme> options = new List<Theme>();
		public bool allowCustomization;
		public bool allowColorCustomization;
		public bool allowFontsetCustomization;
		public bool allowSystemColor;
		public bool useColorAssets = true;
		public Texture2D windowBackgroundOverride;
		public float spacingScale = 1;
		public static void Parse(){
			var themes = FileManager.FindAll("*.unitytheme");
			foreach(var themeFile in themes){
				Theme theme = null;
				Theme root = null;
				string name = "";
				foreach(var line in themeFile.GetText().GetLines()){
					if(line.Trim().IsEmpty()){continue;}
					if(line.Contains("[")){
						name = theme.IsNull() ? themeFile.name : line.Parse("[","]");
						theme = root.IsNull() ? Theme.all.AddNew() : root.options.AddNew();
						theme.name = name.ToPascalCase();
						theme.path = themeFile.GetAssetPath().GetDirectory();
						if(root.IsNull()){
							root = theme;
							ThemeContent.Parse(theme,themeFile.directory);
							ThemeFontset.Parse(root.name,themeFile.directory);
						}
						if(root.options.Count > 0){
							root.options.Last().Use(root);
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
					else if(term.Matches("AllowFontsetCustomization",true)){theme.allowFontsetCustomization = value.ToBool();}
					else if(term.Matches("UseSystemColor",true)){theme.useSystemColor = value.ToBool();}
					else if(term.Matches("UseColorAssets",true)){theme.useColorAssets = value.ToBool();}
					else if(term.Matches("WindowBackgroundOverride",true)){theme.windowBackgroundOverride = FileManager.GetAsset<Texture2D>(value);}
					else if(term.Matches("Palette",true)){theme.palette = ThemePalette.all.Find(x=>x.name==value) ?? new ThemePalette();}
					else if(term.Matches("Fontset",true)){theme.fontset = ThemeFontset.all.AddNew(root.name).Find(x=>x.name==value) ?? new ThemeFontset();}
				}
			}
		}
		public Theme Use(Theme other){
			this.UseVariables(other,typeof(InternalAttribute).AsList());
			if(this.name.IsEmpty()){this.name = other.name;}
			if(this.path.IsEmpty()){this.path = other.path;}
			this.options = other.options;
			this.contents = other.contents;
			return this;
		}
	}
}