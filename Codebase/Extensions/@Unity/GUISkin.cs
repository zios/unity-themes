using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
	}
}