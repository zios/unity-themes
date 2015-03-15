using System;
using UnityEngine;
namespace Zios{
    public static class GUIStyleExtension{
		public static GUIStyle Rotate90(GUIStyle current){
			float width = current.fixedWidth;
			float height = current.fixedHeight;
			current.fixedWidth = height;
			current.fixedHeight = width;
			current.margin = RectOffsetExtension.Rotate90(current.margin);
			current.padding =RectOffsetExtension.Rotate90(current.padding);
			return current;
		}
	}
}