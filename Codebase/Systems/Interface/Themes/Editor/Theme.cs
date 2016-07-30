using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
namespace Zios.Interface{
	[Serializable]
	public partial class Theme{
		public static List<Theme> all = new List<Theme>();
		[Internal] public string name;
		[Internal] public string path;
		[Internal] public ThemePalette palette = new ThemePalette();
		[Internal] public ThemeFontset fontset = new ThemeFontset();
		[Internal] public ThemeIconset iconset = new ThemeIconset();
		[Internal] public ThemeSkinset skinset = new ThemeSkinset();
		public bool customizablePalette;
		public bool customizableFontset;
		public bool customizableIconset;
		public static List<Theme> Import(string path=null){
			path = path ?? "*.unitytheme";
			var imported = new List<Theme>();
			foreach(var file in FileManager.FindAll(path,false)){
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
				var file = FileManager.Create(path);
				file.WriteText(this.Serialize());
				EditorPrefs.SetString("EditorTheme",theme.name);
				Theme.setup = false;
			}
		}
		public string Serialize(){return "";}
		public void Deserialize(string data){
			this.iconset = ThemeIconset.Import(this.path.GetDirectory());
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
				else if(term.Matches("Skinset",true)){this.skinset = ThemeSkinset.all.Find(x=>x.name==value) ?? new ThemeSkinset();}
			}
		}
		public Theme Use(Theme other){
			this.UseVariables(other,typeof(InternalAttribute).AsList());
			if(this.name.IsEmpty()){this.name = other.name;}
			if(this.path.IsEmpty()){this.path = other.path;}
			this.skinset = other.skinset;
			this.iconset = other.iconset;
			return this;
		}
	}
}