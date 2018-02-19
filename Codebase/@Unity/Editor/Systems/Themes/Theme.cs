using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
namespace Zios.Unity.Editor.Themes{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.File;
	using Zios.Reflection;
	using Zios.SystemAttributes;
	using Zios.Unity.Editor.Pref;
	[Serializable]
	public partial class Theme{
		public static Theme active;
		public static List<Theme> all = new List<Theme>();
		[Internal] public string name;
		[Internal] public string path;
		[Internal] public ThemePalette palette = new ThemePalette();
		[Internal] public ThemeFontset fontset = new ThemeFontset();
		[Internal] public ThemeIconset iconset = new ThemeIconset();
		[Internal] public ThemeSkinset skinset = new ThemeSkinset();
		public string[] defaultVariants = new string[0];
		public bool customizablePalette;
		public bool customizableFontset;
		public bool customizableIconset;
		public static List<Theme> Import(string path=null){
			path = path ?? "*.unitytheme";
			var imported = new List<Theme>();
			foreach(var file in File.FindAll(path,Theme.debug)){
				var active = imported.AddNew();
				active.name = file.name.ToPascalCase();
				active.path = file.path;
				active.Deserialize(file.GetText());
			}
			return imported;
		}
		public void Export(string path=null){
			var theme = Theme.active;
			var targetPath = path ?? Theme.storagePath;
			var targetName = theme.name+"-Variant";
			path = path.IsEmpty() ? EditorUtility.SaveFilePanel("Save Theme",targetPath,targetName,"unitytheme") : path;
			if(path.Length > 0){
				var file = File.Create(path);
				file.WriteText(this.Serialize());
				EditorPref.Set<string>("EditorTheme"+Theme.suffix,theme.name);
				Theme.setup = false;
			}
		}
		public string Serialize(){return "";}
		public void Deserialize(string data){
			foreach(var line in data.GetLines()){
				if(line.Trim().IsEmpty()){continue;}
				var term = line.Parse(""," ").Trim();
				var value = line.Parse(" ").Trim().Trim("=").Trim();
				if(term.Matches("CustomizablePalette",true)){this.customizablePalette = value.ToBool();}
				else if(term.Matches("CustomizableFontset",true)){this.customizableFontset = value.ToBool();}
				else if(term.Matches("CustomizableIconset",true)){this.customizableIconset = value.ToBool();}
				else if(term.Matches("Palette",true)){this.palette = ThemePalette.all.Find(x=>x.name==value) ?? new ThemePalette();}
				else if(term.Matches("Fontset",true)){this.fontset = ThemeFontset.all.Find(x=>x.name==value) ?? new ThemeFontset();}
				else if(term.Matches("Iconset",true)){this.iconset = ThemeIconset.all.Find(x=>x.name==value) ?? new ThemeIconset();}
				else if(term.Matches("Skinset",true)){
					var variants = value.Split("+");
					this.defaultVariants = variants.Skip(1).ToArray();
					this.skinset = ThemeSkinset.all.Find(x=>x.name==variants[0]) ?? new ThemeSkinset();
				}
			}
		}
		public Theme Use(Theme other){
			this.UseVariables(other,typeof(InternalAttribute).AsList());
			if(this.name.IsEmpty()){this.name = other.name;}
			if(this.path.IsEmpty()){this.path = other.path;}
			this.skinset = other.skinset;
			this.iconset = other.iconset;
			this.palette = other.palette;
			this.fontset = other.fontset;
			return this;
		}
	}
}