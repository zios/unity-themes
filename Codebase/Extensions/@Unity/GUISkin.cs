using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Zios{
	public static class GUISkinExtension{
		public static GUISkin Copy(this GUISkin current){
			var copy = ScriptableObject.CreateInstance<GUISkin>();
			copy.font = current.font;
			copy.name = current.name;
			copy.customStyles = current.customStyles.Copy();
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
			return copy;
		}
		public static GUISkin Use(this GUISkin current,GUISkin other){
			if(other.IsNull()){return current;}
			current.font = other.font;
			current.customStyles = other.customStyles;
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
			current.settings.doubleClickSelectsWord = other.settings.doubleClickSelectsWord;
			current.settings.tripleClickSelectsLine = other.settings.tripleClickSelectsLine;
			current.settings.cursorColor = other.settings.cursorColor;
			current.settings.cursorFlashSpeed = other.settings.cursorFlashSpeed;
			current.settings.selectionColor = other.settings.selectionColor;
			return current;
		}
		public static GUIStyle[] GetStyles(this GUISkin current){
			var styles = new List<GUIStyle>();
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
			styles.AddRange(current.customStyles);
			return styles.ToArray();
		}
		public static void SaveBackgrounds(this GUISkin current,string path,bool includeBuiltin=true){
			foreach(var style in current.GetStyles()){
				foreach(var state in style.GetStates()){
					if(!state.background.IsNull()){
						string savePath = path+"/"+state.background.name+".png";
						string assetPath = AssetDatabase.GetAssetPath(state.background);
						if(!includeBuiltin && assetPath.Contains("unity editor resources")){continue;}
						if(!FileManager.Exists(savePath)){
							if(!savePath.GetFileName().IsEmpty()){
								Debug.Log(current.name+"."+style.name + " = " + savePath.GetFileName());
							}
							state.background.SaveAs(savePath,true);
						}
					}
				}
			}
		}
		#if UNITY_EDITOR
		public static void SaveFonts(this GUISkin current,string path){
			foreach(var style in current.GetStyles()){
				if(!style.font.IsNull()){
					var fontPath = AssetDatabase.GetAssetPath(style.font);
					var savePath = path+"/"+fontPath.GetPathTerm();
					if(!FileManager.Exists(savePath)){
						if(!fontPath.GetFileName().IsEmpty()){
							Debug.Log(current.name+"."+style.name + " = " + fontPath.GetFileName());
						}
						AssetDatabase.CopyAsset(fontPath,savePath);
					}
				}
			}
		}
		#endif
	}
}