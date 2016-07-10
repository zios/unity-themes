using System.Collections.Generic;
using System;
namespace Zios.Interface{
	using UnityEngine;
	using UnityEditor;
	[Serializable]
	public class ThemeFontset{
		public static Dictionary<string,List<ThemeFontset>> all = new Dictionary<string,List<ThemeFontset>>();
		public string name;
		public string path;
		public Dictionary<string,ThemeFont> fonts = new Dictionary<string,ThemeFont>();
		public static List<ThemeFontset> Import(string path){
			var imported = new List<ThemeFontset>();
			foreach(var file in FileManager.FindAll(path+"/Font/*.unityfontset",false)){
				var active = imported.AddNew();
				active.name = file.name;
				active.path = file.directory;
				active.Deserialize(file.GetText());
			}
			return imported;
		}
		public void Export(string path=""){
			var theme = Theme.active;
			var defaultPath = Theme.storagePath+theme.name+"/Fonts/";
			var defaultName = theme.fontset.name+"-Variant";
			path = path.IsEmpty() ? EditorUtility.SaveFilePanel("Save Theme [Fonts]",defaultPath,defaultName,"unityfontset") : path;
			if(path.Length > 0){
				var file = FileManager.Create(path);
				file.WriteText(this.Serialize());
				EditorPrefs.SetString("EditorFontset-"+theme.name,path.GetFileName());
				Theme.setup = false;
			}
		}
		public string Serialize(){
			var contents = "";
			foreach(var item in this.fonts){
				var themeFont = item.Value;
				if(themeFont.font.IsNull()){continue;}
				contents = contents.AddLine("["+themeFont.name+"]");
				contents = contents.AddLine("Font = "+themeFont.font.name);
				contents = contents.AddLine("SizeOffset = "+themeFont.sizeOffset);
				contents = contents.AddLine("OffsetX = "+themeFont.offsetX);
				contents = contents.AddLine("OffsetY = "+themeFont.offsetY);
				contents = contents.AddLine("");
			}
			return contents;
		}
		public void Deserialize(string data){
			if(data.IsEmpty()){return;}
			var name = "";
			ThemeFont themeFont = null;
			foreach(var line in data.GetLines()){
				if(line.Trim().IsEmpty()){continue;}
				if(line.Contains("[")){
					name = line.Parse("[","]");
					themeFont = this.fonts.AddNew(name);
					themeFont.name = name;
					themeFont.path = this.path+"/"+name+".ttf";
					themeFont.proxy = FileManager.GetAsset<Font>(themeFont.path);
					continue;
				}
				if(themeFont.IsNull()){continue;}
				var current = line.Remove("\"","'");
				var term = current.Parse("","=").Trim();
				var value = current.Parse("=").Trim();
				if(term.Matches("Font",true)){
					themeFont.font = FileManager.GetAsset<Font>(value+".ttf",false);
					themeFont.font = themeFont.font ?? FileManager.GetAsset<Font>(value+".otf",false);	
				}
				else if(term.Matches("SizeOffset",true)){themeFont.sizeOffset = value.ToInt();}
				else if(term.Matches("OffsetX",true)){themeFont.offsetX = value.ToFloat();}
				else if(term.Matches("OffsetY",true)){themeFont.offsetY = value.ToFloat();}
			}
		}
		public bool Matches(ThemeFontset other){
			foreach(var item in this.fonts){
				var name = item.Key;
				var themeFont = item.Value;
				if(!other.fonts.ContainsKey(name)){return false;}
				bool mismatchedFont = themeFont.font != other.fonts[name].font;
				bool mismatchedSizeOffset = themeFont.sizeOffset != other.fonts[name].sizeOffset;
				bool mismatchedOffsetX = themeFont.offsetX != other.fonts[name].offsetX;
				bool mismatchedOffsetY = themeFont.offsetY != other.fonts[name].offsetY;
				if(mismatchedFont || mismatchedSizeOffset || mismatchedOffsetX || mismatchedOffsetY){
					return false;
				}
			}
			return true;
		}
		public ThemeFontset Use(ThemeFontset other){
			this.name = other.name;
			this.path = other.path;
			foreach(var item in other.fonts){
				this.fonts[item.Key] = other.fonts[item.Key].Copy();
			}
			return this;
		}
		public GUISkin Apply(GUISkin skin){
			var modified = skin.Copy();
			foreach(var style in modified.GetStyles()){
				foreach(var item in this.fonts){
					var themeFont = item.Value;
					if(style.font == themeFont.proxy){
						style.font = themeFont.font;
						style.fontSize += themeFont.sizeOffset;
						style.contentOffset += new Vector2(themeFont.offsetX,themeFont.offsetY);
					}
				}
			}
			return modified;
		}
	}
	[Serializable]
	public class ThemeFont{
		public string name;
		public string path;
		public Font font;
		public Font proxy;
		public int sizeOffset;
		public float offsetX;
		public float offsetY;
		public ThemeFont(){}
		public ThemeFont(string name,int sizeOffset=0,float offsetX=0,float offsetY=0){
			this.name = name;
			this.sizeOffset = sizeOffset;
			this.offsetX = offsetX;
			this.offsetY = offsetY;
		}
	}
}
