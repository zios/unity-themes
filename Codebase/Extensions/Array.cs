using UnityEngine;
using System;
using System.Collections.Generic;
public static class ArrayExtension{
	public static T[] Convert<T>(this Array current){
		List<T> casted = new List<T>();
		Type type = typeof(T);
		foreach(var item in current){
			if(type == typeof(Single)){
				float value = System.Convert.ToSingle(item);
				casted.Add((T)System.Convert.ChangeType(value,type));
			}
			else{
				casted.Add((T)item);
			}
		}
		return casted.ToArray();
	}
	public static float[] Scale(this float[] current,float scalar){
		float[] result = current;
		for(int index=0;index<current.Length;++index){
			result[index] = current[index] * scalar;
		}
		return result;
	}
	public static Color ToColor(this float[] current){
		if(current.Length >= 3){
			float r = current[0];
			float g = current[1];
			float b = current[2];
			if(current.Length > 3){
				return new Color(r,g,b,current[3]);
			}
			return new Color(r,g,b);
		}
		return Color.white;
	}
	public static Vector2 ToVector2(this float[] current){
		if(current.Length >= 2){
			float a = current[0];
			float b = current[1];
			return new Vector2(a,b);
		}
		return Vector2.zero;
	}
	public static Vector3 ToVector3(this float[] current){
		if(current.Length >= 3){
			float a = current[0];
			float b = current[1];
			float c = current[2];
			return new Vector3(a,b,c);
		}
		return Vector3.zero;
	}
	public static Vector4 ToVector4(this float[] current){
		if(current.Length >= 4){
			float a = current[0];
			float b = current[1];
			float c = current[2];
			float d = current[3];
			return new Vector4(a,b,c,d);
		}
		return Vector4.zero;
	}
	public static T[] Copy<T>(this T[] current){
		T[] result = new T[current.Length];
		current.CopyTo(result,0);
		return result;
	}
	public static T[] Concat<T>(this T[] current,T[] list){
		T[] result = new T[current.Length + list.Length];
		current.CopyTo(result,0);
		list.CopyTo(result,current.Length);
		return result;
	}
}