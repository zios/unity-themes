using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Zios{
	public static class GUISkinExtension{
		public static GUISkin Copy(this GUISkin current){
			var copy = ScriptableObject.CreateInstance<GUISkin>();
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
			copy.horizontalScrollbarLeftButton = new GUIStyle(current.horizontalScrollbarLeftButton);
			copy.horizontalScrollbarRightButton = new GUIStyle(current.horizontalScrollbarRightButton);
			copy.verticalScrollbar = new GUIStyle(current.verticalScrollbar);
			copy.verticalScrollbarThumb = new GUIStyle(current.verticalScrollbarThumb);
			copy.verticalScrollbarUpButton = new GUIStyle(current.verticalScrollbarUpButton);
			copy.verticalScrollbarDownButton = new GUIStyle(current.verticalScrollbarDownButton);
			copy.scrollView = new GUIStyle(current.scrollView);
			return copy;
		}
	}
}