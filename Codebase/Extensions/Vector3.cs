using UnityEngine;
public static class Vector3Extension{
	public static string Print(this Vector3 current){
		return "("+current.x+","+current.y+","+current.z+")";
	}
	public static float[] ToFloat(this Vector3 current){
		return new float[3]{current.x,current.y,current.z};
	}
	public static Vector3 Scale(this Vector3 current,Vector3 other){
		return Vector3.Scale(current,other);
	}
	public static Vector3 Sign(this Vector3 vector,bool allowZero=true){
		Vector3 signed = new Vector3(0,0,0);
		if(!allowZero || vector.x != 0){
			signed.x = vector.x > 0 ? 1 : -1;
		}
		if(!allowZero || vector.y != 0){
			signed.y = vector.y > 0 ? 1 : -1;
		}
		if(!allowZero || vector.z != 0){
			signed.z = vector.z > 0 ? 1 : -1;
		}
		return signed;
	}
	public static Vector3 LerpAngle(this Vector3 vector,Vector3 start,Vector3 end,float percent){
		Vector3 copy = vector;
		copy.x = Mathf.LerpAngle(start.x,end.x,percent);
		copy.y = Mathf.LerpAngle(start.y,end.y,percent);
		copy.z = Mathf.LerpAngle(start.z,end.z,percent);
		return copy;
	}
	public static Vector3 Clamp(this Vector3 vector,Vector3 min,Vector3 max){
		Vector3 clamp = vector;
		clamp.x = Mathf.Clamp(clamp.x,min.x,max.x);
		clamp.y = Mathf.Clamp(clamp.y,min.y,max.y);
		clamp.z = Mathf.Clamp(clamp.z,min.z,max.z);
		return clamp;
	}
	public static Vector3 Clamp(this Vector3 vector,float[] min,float[] max){
		Vector3 clamp = vector;
		clamp.x = Mathf.Clamp(clamp.x,min[0],max[0]);
		clamp.y = Mathf.Clamp(clamp.y,min[1],max[1]);
		clamp.z = Mathf.Clamp(clamp.z,min[2],max[2]);
		return clamp;
	}
	public static Vector3 ToRadian(this Vector3 vector){
		Vector3 copy = vector;
		copy.x = vector.x / 360.0f;
		copy.y = vector.y / 360.0f;
		copy.z = vector.z / 360.0f;
		return copy;
	}
	public static Vector3 Abs(this Vector3 vector){
		Vector3 copy = vector;
		copy.x = Mathf.Abs(copy.x);
		copy.y = Mathf.Abs(copy.y);
		copy.z = Mathf.Abs(copy.z);
		return copy;
	}
}