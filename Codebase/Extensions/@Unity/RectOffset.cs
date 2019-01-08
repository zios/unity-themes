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
		public static bool IsEmpty(this RectOffset current){
			return current.Matches(new RectOffset(0,0,0,0));
		}
		public static bool Matches(this RectOffset current,RectOffset other){
			if(current.left != other.left){return false;}
			if(current.top != other.top){return false;}
			if(current.right != other.right){return false;}
			if(current.bottom != other.bottom){return false;}
			return true;
		}
		public static string Serialize(this RectOffset current,string separator=" "){
			return current.left+separator+current.right+separator+current.top+separator+current.bottom;
		}
	}
}