using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Zios{
	using Containers;
	public static class GUISkinExtension{
		public static Hierarchy<GUISkin,string,GUIStyle> cachedStyles = new Hierarchy<GUISkin,string,GUIStyle>();
		public static GUISkin Copy(this GUISkin current){
			var copy = ScriptableObject.CreateInstance<GUISkin>();
			copy.font = current.font;
			copy.name = current.name;
			copy.box = new GUIStyle(current.box);
			copy.button = new GUIStyle(current.button);
			copy.toggle = new GUIStyle(current.toggle);
			copy.label = new GUIStyle(current.label);
			copy.textField = new GUIStyle(current.textField);
			copy.textArea = new GUIStyle(current.textArea);
			copy.window = new GUIStyle(current.window);
			copy.horizontalSlider = new GUIStyle(current.horizontalSlider);
			copy.horizontalSliderThumb = new GUIStyle(current.horizontalSliderThumb);
			copy.verticalSlider = new GUIStyle(current.verticalSlider);
			copy.verticalSliderThumb = new GUIStyle(current.verticalScrollbarThumb);
			copy.horizontalScrollbar = new GUIStyle(current.horizontalScrollbar);
			copy.horizontalScrollbarThumb = new GUIStyle(current.horizontalScrollbarThumb);
			copy.horizontalScrollbarLeftButton = new GUIStyle(current.horizontalScrollbarLeftButton);
			copy.horizontalScrollbarRightButton = new GUIStyle(current.horizontalScrollbarRightButton);
			copy.verticalScrollbar = new GUIStyle(current.verticalScrollbar);
			copy.verticalScrollbarThumb = new GUIStyle(current.verticalScrollbarThumb);
			copy.verticalScrollbarUpButton = new GUIStyle(current.verticalScrollbarUpButton);
			copy.verticalScrollbarDownButton = new GUIStyle(current.verticalScrollbarDownButton);
			copy.scrollView = new GUIStyle(current.scrollView);
			copy.settings.doubleClickSelectsWord = current.settings.doubleClickSelectsWord;
			copy.settings.tripleClickSelectsLine = current.settings.tripleClickSelectsLine;
			copy.settings.cursorColor = current.settings.cursorColor;
			copy.settings.cursorFlashSpeed = current.settings.cursorFlashSpeed;
			copy.settings.selectionColor = current.settings.selectionColor;
			var styles = new List<GUIStyle>();
			foreach(var style in current.customStyles){
				styles.Add(new GUIStyle(style));
			}
			copy.customStyles = styles.ToArray();
			return copy;
		}
		public static GUIStyle Get(this GUISkin current,string name){
			if(GUISkinExtension.cachedStyles.AddNew(current).ContainsKey(name)){
				return GUISkinExtension.cachedStyles[current][name];
			}
			foreach(var style in current.GetStyles()){
				if(style.name == name){
					GUISkinExtension.cachedStyles[current][name] = style;
					return style;
				}
			}
			return null;
		}
		public static GUISkin Use(this GUISkin current,GUISkin other,bool inline=false){
			if(other.IsNull()){return current;}
			current.font = other.font;
			current.settings.doubleClickSelectsWord = other.settings.doubleClickSelectsWord;
			current.settings.tripleClickSelectsLine = other.settings.tripleClickSelectsLine;
			current.settings.cursorColor = other.settings.cursorColor;
			current.settings.cursorFlashSpeed = other.settings.cursorFlashSpeed;
			current.settings.selectionColor = other.settings.selectionColor;
			if(inline){
				var currentStyles = current.GetNamedStyles();
				var otherStyles = other.GetNamedStyles();
				foreach(var style in currentStyles){
					if(otherStyles.ContainsKey(style.Key)){
						style.Value.Use(otherStyles[style.Key]);
					}
				}
			}
			else{
				current.box = other.box;
				current.button = other.button;
				current.toggle = other.toggle;
				current.label = other.label;
				current.textField = other.textField;
				current.textArea = other.textArea;
				current.window = other.window;
				current.horizontalSlider = other.horizontalSlider;
				current.horizontalSliderThumb = other.horizontalSliderThumb;
				current.verticalSlider = other.verticalSlider;
				current.verticalSliderThumb = other.verticalScrollbarThumb;
				current.horizontalScrollbar = other.horizontalScrollbar;
				current.horizontalScrollbarThumb = other.horizontalScrollbarThumb;
				current.horizontalScrollbarLeftButton = other.horizontalScrollbarLeftButton;
				current.horizontalScrollbarRightButton = other.horizontalScrollbarRightButton;
				current.verticalScrollbar = other.verticalScrollbar;
				current.verticalScrollbarThumb = other.verticalScrollbarThumb;
				current.verticalScrollbarUpButton = other.verticalScrollbarUpButton;
				current.verticalScrollbarDownButton = other.verticalScrollbarDownButton;
				current.scrollView = other.scrollView;
				current.customStyles = other.customStyles;
			}
			return current;
		}
		public static Dictionary<string,GUIStyle> GetNamedStyles(this GUISkin current,bool includeStandard=true,bool includeCustom=true,bool trimBaseName=false){
			var data = new Dictionary<string,GUIStyle>();
			var styles = current.GetStyles(includeStandard,includeCustom);
			for(int index=0;index<styles.Length;++index){
				var style = styles[index];
				var name = trimBaseName ? style.name.Split("[")[0].Trim() : style.name;
				if(name.IsEmpty()){
					data["Element "+index] = style;
					continue;
				}
				while(data.ContainsKey(name)){
					name = name.ToLetterSequence();
					style.name = name;
				}
				data[name] = style;
			}
			return data;
		}
		public static GUIStyle[] GetStyles(this GUISkin current,bool includeStandard=true,bool includeCustom=true){
			var styles = new List<GUIStyle>();
			if(includeStandard){
				styles.Add(current.box);
				styles.Add(current.button);
				styles.Add(current.toggle);
				styles.Add(current.label);
				styles.Add(current.textField);
				styles.Add(current.textArea);
				styles.Add(current.window);
				styles.Add(current.horizontalSlider);
				styles.Add(current.horizontalSliderThumb);
				styles.Add(current.verticalSlider);
				styles.Add(current.verticalSliderThumb);
				styles.Add(current.horizontalScrollbar);
				styles.Add(current.horizontalScrollbarThumb);
				styles.Add(current.horizontalScrollbarLeftButton);
				styles.Add(current.horizontalScrollbarRightButton);
				styles.Add(current.verticalScrollbar);
				styles.Add(current.verticalScrollbarThumb);
				styles.Add(current.verticalScrollbarUpButton);
				styles.Add(current.verticalScrollbarDownButton);
				styles.Add(current.scrollView);
			}
			if(includeCustom){styles.AddRange(current.customStyles);}
			return styles.ToArray();
		}
		public static void InvertTextColors(this GUISkin current,float intensityCompare,float difference=1.0f){
			foreach(var style in current.GetStyles()){
				foreach(var state in style.GetStates()){
					state.InvertTextColor(intensityCompare,difference);
				}
			}
		}
		public static void SaveBackgrounds(this GUISkin current,string path,bool includeBuiltin=true){
			foreach(var style in current.GetStyles()){
				foreach(var state in style.GetStates()){
					if(!state.background.IsNull()){
						string assetPath = FileManager.GetPath(state.background);
						string savePath = path+"/"+state.background.name+".png";
						if(!includeBuiltin && assetPath.Contains("unity editor resources")){continue;}
						if(!FileManager.Exists(savePath)){
							state.background.SaveAs(savePath,true);
						}
					}
				}
			}
		}
		public static GUIStyle AddStyle(this GUISkin current,GUIStyle style){
			if(style.IsNull()){return null;}
			return current.AddStyle(style.name,style);
		}
		public static GUIStyle AddStyle(this GUISkin current,string name,GUIStyle style){
			if(!style.IsNull() && !current.customStyles.Exists(x=>x.name==name)){
				current.customStyles = current.customStyles.Add(style);
			}
			return style;
		}
		#if UNITY_EDITOR
		public static void SaveFonts(this GUISkin current,string path,bool includeBuiltin=true){
			foreach(var style in current.GetStyles()){
				if(!style.font.IsNull()){
					string assetPath = FileManager.GetPath(style.font);
					string savePath = path+"/"+assetPath.GetPathTerm();
					if(!includeBuiltin && assetPath.Contains("unity editor resources")){continue;}
					if(!FileManager.Exists(savePath)){
						AssetDatabase.CopyAsset(assetPath,savePath);
					}
				}
			}
		}
		#endif
	}
}