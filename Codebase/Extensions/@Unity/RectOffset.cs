using UnityEngine;
namespace Zios{
	public static class RectOffsetExtension{
		public static RectOffset Rotate90(RectOffset current){
			int left = current.left;
			int right = current.right;
			int top = current.top;
			int bottom = current.bottom;
			current.left = top;
			current.right = bottom;
			current.top = left;
			current.bottom = right;
			return current;
		}
	}
}