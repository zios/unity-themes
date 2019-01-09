using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Zios.Unity.Editor.Themes{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.File;
	using Zios.Reflection;
	using Zios.Unity.Editor.Pref;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.Extensions;
	using Zios.Unity.Locate;
	[Serializable]
	public class ThemeFontset{
		public static List<ThemeFontset> all = new List<ThemeFontset>();
		public string name;
		public string path;
		public Dictionary<string,ThemeFont> fonts = new Dictionary<string,ThemeFont>();
		public GUISkin buffer;
		public ThemeFontset(){}
		public ThemeFontset(ThemeFontset other){this.Use(other);}
		//=================================
		// Files
		//=================================
		public static List<ThemeFontset> Import(string path=null){
			path = path ?? "*.unityfontset";
			var imported = new List<ThemeFontset>();
			foreach(var file in File.FindAll(path,Theme.debug)){
				var active = imported.AddNew();
				active.name = file.name;
				active.path = file.path;
				active.Deserialize(file.ReadText());
			}
			return imported;
		}
		public void Export(string path=null){
			var theme = Theme.active;
			var savePath = path ?? Theme.storagePath+"Fontsets";
			var saveName = theme.fontset.name+"-Variant";
			path = path.IsEmpty() ? ProxyEditor.SaveFilePanel("Save Theme [Fonts]",savePath.GetAssetPath(),saveName,"unityfontset") : path;
			if(path.Length > 0){
				var file = File.Create(path);
				file.Write(this.Serialize());
				ProxyEditor.ImportAsset(path.GetAssetPath());
				EditorPref.Set<string>("Zios.Theme.Fontset"+Theme.suffix,path.GetFileName());
				Theme.Reset();
			}
		}
		//=================================
		// Data
		//=================================
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
					themeFont.path = name+".ttf";
					themeFont.proxy = File.GetAsset<Font>(themeFont.path);
					continue;
				}
				if(themeFont.IsNull()){continue;}
				var current = line.Remove("\"","'");
				var term = current.Parse("","=").Trim();
				var value = current.Parse("=").Trim();
				if(term.Matches("Font",true)){
					themeFont.font = File.GetAsset<Font>(value+".ttf",false);
					themeFont.font = themeFont.font ?? File.GetAsset<Font>(value+".otf",false);
					themeFont.font = themeFont.font ?? Locate.GetAssets<Font>().Where(x=>x.name==value).FirstOrDefault();
				}
				else if(term.Matches("SizeOffset",true)){themeFont.sizeOffset = value.ToInt();}
				else if(term.Matches("OffsetX",true)){themeFont.offsetX = value.ToFloat();}
				else if(term.Matches("OffsetY",true)){themeFont.offsetY = value.ToFloat();}
			}
		}
		//=================================
		// Utilities
		//=================================
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
		public ThemeFontset UseBuffer(ThemeFontset other){
			this.buffer = other.buffer;
			return this;
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
			if(skin.IsNull()){return skin;}
			if(!this.buffer.IsNull()){
				ScriptableObject.DestroyImmediate(this.buffer);
			}
			this.buffer = skin.Copy();
			foreach(var style in this.buffer.GetStyles()){
				foreach(var item in this.fonts){
					var themeFont = item.Value;
					if(style.font == themeFont.proxy){
						style.font = themeFont.font;
						style.fontSize += themeFont.sizeOffset;
						style.contentOffset += new Vector2(themeFont.offsetX,themeFont.offsetY);
					}
				}
			}
			return this.buffer;
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