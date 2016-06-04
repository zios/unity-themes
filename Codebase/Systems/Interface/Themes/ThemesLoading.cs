using System.Linq;
using UnityEngine;
namespace Zios{
	#if UNITY_EDITOR
	using UnityEditor;
	public static partial class Themes{
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
	}
	#endif
}