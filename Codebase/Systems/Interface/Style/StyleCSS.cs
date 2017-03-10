using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace Zios{
	public static partial class Style{
		private static StringBuilder contents = new StringBuilder();
		public static void SaveCSS(string path,GUISkin skin){
			var output = new StringBuilder();
			var emptySkin = ScriptableObject.CreateInstance<GUISkin>();
			foreach(var item in skin.GetVariables<GUIStyle>(null,ObjectExtension.publicFlags)){
				var defaultStyle = emptySkin.GetVariable<GUIStyle>(item.Key);
				output.Append(Style.SaveCSS(item.Value,defaultStyle));
			}
			foreach(var style in skin.customStyles){
				if(style.IsNull()){continue;}
				output.Append(Style.SaveCSS(style,null,false));
			}
			if(output.Length > 0){
				var file = FileManager.Create(path+"/"+skin.name.Split("-")[0]+"/"+skin.name+".css");
				file.WriteText(output.ToString().Trim());
			}
		}
		public static string SaveCSS(GUIStyle style,GUIStyle empty=null,bool asTag=true){
			empty = empty ?? GUIStyle.none;
			var contents = Style.contents;
			var styleName = asTag ? style.name : "."+style.name;
			contents.Clear();
			if(!style.border.Matches(empty.border)){contents.AppendLine("\tborder : "+style.border.Serialize(" "));}
			if(!style.margin.Matches(empty.margin)){contents.AppendLine("\tmargin : "+style.margin.Serialize(" "));}
			if(!style.padding.Matches(empty.padding)){contents.AppendLine("\tpadding : "+style.padding.Serialize(" "));}
			if(!style.overflow.Matches(empty.overflow)){contents.AppendLine("\toverflow : "+style.overflow.Serialize(" "));}
			if(style.font != empty.font){contents.AppendLine("\tfont : "+style.font.name);}
			if(style.fontSize != empty.fontSize){contents.AppendLine("\tfont-size : "+style.fontSize);}
			if(style.fontStyle != empty.fontStyle){contents.AppendLine("\tfont-style : "+style.fontStyle.ToName().ToCamelCase());}
			if(style.alignment != empty.alignment){contents.AppendLine("\talignment : "+style.alignment.ToName().ToCamelCase());}
			if(style.wordWrap != empty.wordWrap){contents.AppendLine("\tword-wrap : "+style.wordWrap);}
			if(style.richText != empty.richText){contents.AppendLine("\trich-text : "+style.richText);}
			if(style.clipping != empty.clipping){contents.AppendLine("\ttext-clipping : "+style.clipping.ToName().ToCamelCase());}
			if(style.imagePosition != empty.imagePosition){contents.AppendLine("\timage-position : "+style.imagePosition.ToName().ToCamelCase());}
			if(!style.contentOffset.Equals(empty.contentOffset)){contents.AppendLine("\tcontent-offset : "+style.contentOffset.x+" "+style.contentOffset.y);}
			if(style.fixedWidth != empty.fixedWidth){contents.AppendLine("\tfixed-width : "+style.fixedWidth);}
			if(style.fixedHeight != empty.fixedHeight){contents.AppendLine("\tfixed-height : "+style.fixedHeight);}
			if(style.stretchWidth != empty.stretchWidth){contents.AppendLine("\tstretch-width : "+style.stretchWidth);}
			if(style.stretchHeight != empty.stretchHeight){contents.AppendLine("\tstretch-height : "+style.stretchHeight);}
			if(contents.Length > 0){
				contents.Insert(0,styleName+"{\n");
				contents.AppendLine("}");
			}
			foreach(var item in style.GetVariables<GUIStyleState>(null,ObjectExtension.publicFlags)){
				var state = item.Value;
				bool hasBackground = !state.background.IsNull();
				bool hasTextColor = state.textColor!=empty.normal.textColor;
				if(!hasBackground && !hasTextColor){continue;}
				contents.AppendLine(styleName+":"+item.Key+"{");
				if(hasBackground){contents.AppendLine("\tbackground : "+state.background.name);}
				if(hasTextColor){contents.AppendLine("\ttext-color : "+state.textColor.ToHex(false));}
				contents.AppendLine("}");
			}
			return contents.ToString().Replace("True","true").Replace("False","false");
		}
		public static GUISkin LoadCSS(string name,string contents){
			var skin = ScriptableObject.CreateInstance<GUISkin>();
			var styles = new Dictionary<string,GUIStyle>();
			GUIStyle active = null;
			GUIStyleState state = null;
			var stateName = "";
			//var textures = FileManager.GetAssets<Texture2D>().ToDictionary(x=>x.name,x=>x);
			//var fonts = FileManager.GetAssets<Texture2D>().ToDictionary(x=>x.name,x=>x);
			var textures = FileManager.GetNamedAssets<Texture2D>();
			var fonts = FileManager.GetNamedAssets<Font>();
			foreach(var current in contents.GetLines()){
				var line = current.TrimLeft(".").Trim();
				if(line.Contains("{")){
					state = null;
					stateName = "";
					bool builtin = !current.StartsWith(".");
					var styleName = line.Contains(":") ? line.Parse("",":") : line.Parse("","{");
					styleName = styleName.Trim();
					active = builtin ? skin.GetVariable<GUIStyle>(styleName) : styles.AddNew(styleName);
					active.name = styleName;
					if(line.Contains(":")){
						state = new GUIStyleState();
						stateName = line.Parse(":","{").Trim();
						active.SetVariable<GUIStyleState>(stateName,state);
					}
					continue;
				}
				if(line.StartsWith("}")){continue;}
				var term = line.Parse("",":").Trim();
				var value = line.Parse(":","").TrimRight(";").Trim();
				if(!state.IsNull()){
					if(term.Contains("background")){state.background = textures.ContainsKey(value) ? textures[value] : null;}
					if(term.Contains("text-color")){state.textColor = value.ToColor();}
					continue;
				}
				if(term.Contains("border")){active.border = value.ToRectOffset();}
				if(term.Contains("margin")){active.margin = value.ToRectOffset();}
				if(term.Contains("padding")){active.padding = value.ToRectOffset();}
				if(term.Contains("overflow")){active.overflow = value.ToRectOffset();}
				if(term.Contains("font")){active.font = fonts.ContainsKey(value) ? fonts[value] : null;}
				if(term.Contains("font-size")){active.fontSize = value.ToInt();}
				if(term.Contains("font-style")){active.fontStyle = (FontStyle)Enum.Parse(typeof(FontStyle),value);}
				if(term.Contains("alignment")){active.alignment = (TextAnchor)Enum.Parse(typeof(TextAnchor),value);}
				if(term.Contains("word-wrap")){active.wordWrap = value.ToBool();}
				if(term.Contains("rich-text")){active.wordWrap = value.ToBool();}
				if(term.Contains("text-clipping")){active.clipping = (TextClipping)Enum.Parse(typeof(TextClipping),value);}
				if(term.Contains("image-position")){active.imagePosition = (ImagePosition)Enum.Parse(typeof(ImagePosition),value);}
				if(term.Contains("content-offset")){active.imagePosition = (ImagePosition)Enum.Parse(typeof(ImagePosition),value);}
				if(term.Contains("fixed-width")){active.fixedWidth = value.ToFloat();}
				if(term.Contains("fixed-height")){active.fixedHeight = value.ToFloat();}
				if(term.Contains("stretch-width")){active.stretchWidth = value.ToBool();}
				if(term.Contains("stretch-height")){active.stretchHeight = value.ToBool();}
			}
			skin.customStyles = styles.Values.ToArray();
			return skin;
		}
	}
}