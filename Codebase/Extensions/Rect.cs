using UnityEngine;
public static class RectExtension{
	/*public static bool ContainsPoint(this Rect area,Vector3 position){
		return (position.x > area.xMin) && (position.x < area.xMax) && (position.y > area.yMin) && (position.y < area.yMax);
	}*/
	public static Rect OverrideX(this Rect current,float value){
		current.x = value;
		return current;
	}
	public static Rect OverrideY(this Rect current,float value){
		current.y = value;
		return current;
	}
	public static Rect OverrideWidth(this Rect current,float value){
		current.width = value;
		return current;
	}
	public static Rect OverrideHeight(this Rect current,float value){
		current.height = value;
		return current;
	}
	public static Rect SetXY(this Rect current,float x,float y){
		return new Rect(current).OverrideX(x).OverrideY(y);
	}
	public static Rect SetSize(this Rect current,float width,float height){
		return new Rect(current).OverrideWidth(width).OverrideHeight(height);
	}
	public static Rect SetX(this Rect current,float value){
		return new Rect(current).OverrideX(value);
	}
	public static Rect SetY(this Rect current,float value){
		return new Rect(current).OverrideY(value);
	}
	public static Rect SetWidth(this Rect current,float value){
		return new Rect(current).OverrideWidth(value);
	}
	public static Rect SetHeight(this Rect current,float value){
		return new Rect(current).OverrideHeight(value);
	}
	public static Rect AddXY(this Rect current,float x,float y){return current.AddX(x).AddY(y);}
	public static Rect AddX(this Rect current,float value){return current.Add(value);}
	public static Rect AddY(this Rect current,float value){return current.Add(0,value);}
	public static Rect AddSize(this Rect current,float width,float height){return current.AddWidth(width).AddHeight(height);}
	public static Rect AddWidth(this Rect current,float value){return current.Add(0,0,value);}
	public static Rect AddHeight(this Rect current,float value){return current.Add(0,0,0,value);}
	public static Rect Add(this Rect current,params float[] other){
		return current.Add(other.ToRect());
	}
	public static Rect Add(this Rect current,Rect other){
		Rect result = new Rect(current);
		result.x += other.x;
		result.y += other.y;
		result.width += other.width;
		result.height += other.height;
		return result;
	}
}