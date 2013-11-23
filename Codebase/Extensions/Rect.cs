using UnityEngine;
public static class RectExtension{
	public static bool ContainsPoint(this Rect area,Vector3 position){
		return (position.x > area.xMin) && (position.x < area.xMax) && (position.y > area.yMin) && (position.y < area.yMax);
	}
}