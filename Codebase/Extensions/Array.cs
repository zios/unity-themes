using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Zios{
	public static class ArrayExtension{
		//=======================
		// Default
		//=======================
		public static List<T> ToList<T>(this T[] current){
			return new List<T>(current);
		}
		public static int IndexOf<T>(this Array current,T value){
			return Array.IndexOf(current,value);
		}
		public static int IndexOf<T>(this Array current,Enum enumerable){
			string name = enumerable.ToString();
			return current.IndexOf(name);
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
		public static bool Exists<T>(this T[] current,Predicate<T> predicate){
			return Array.Exists(current,predicate);
		}
		public static T Find<T>(this T[] current,Predicate<T> predicate){
			return Array.Find(current,predicate);
		}
		public static void Clear<T>(this T[] current){
			Array.Clear(current,0,current.Length);
		}
		public static T[] Add<T>(this T[] current,T element){
			T[] extra = new T[]{element};
			return current.Concat(extra);
		}
		public static T[] Remove<T>(this T[] current,T value){
			List<T> copy = new List<T>(current);
			copy.Remove(value);
			return copy.ToArray();
		}
		public static T[] RemoveAt<T>(this T[] current,int index){
			List<T> copy = new List<T>(current);
			copy.RemoveAt(index);
			return copy.ToArray();
		}
		public static T[] RemoveAll<T>(this T[] current,T value){
			List<T> copy = new List<T>(current);
			copy.RemoveAll(x=>x.Equals(value));
			return copy.ToArray();
		}
		public static T[] Resize<T>(this T[] current,int newSize){
			while(current.Length > newSize){
				current = current.RemoveAt(current.Length-1);
			}
			while(current.Length < newSize){
				current = current.Add(default(T));
			}
			return current;
		}
		public static T[] Order<T>(this T[] current){
			var copy = current.Copy().ToList();
			copy.Sort();
			return copy.ToArray();
		}
		public static bool HasAny<T>(this T[] current,params T[] values){return current.ContainsAny(values);}
		public static bool HasAll<T>(this T[] current,params T[] values){return current.ContainsAll(values);}
		public static bool ContainsAny<T>(this T[] current,params T[] values){
			for(int index=0;index<values.Length;++index){
				if(current.Contains(values[index])){return true;}
			}
			return false;
		}
		public static bool ContainsAll<T>(this T[] current,params T[] values){
			for(int index=0;index<values.Length;++index){
				if(!current.Contains(values[index])){return false;}
			}
			return true;
		}
		public static void ForEach<T>(this T[] current,Action<T> method){
			for(int index=0;index<current.Length;++index){
				method(current[index]);
			}
		}
		//=======================
		// Float
		//=======================
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
			float x = current.Length >= 1 ? current[0] : 0;
			float y = current.Length >= 2 ? current[1] : 0;
			return new Vector2(x,y);
		}
		public static Vector3 ToVector3(this float[] current){
			float x = current.Length >= 1 ? current[0] : 0;
			float y = current.Length >= 2 ? current[1] : 0;
			float z = current.Length >= 3 ? current[2] : 0;
			return new Vector3(x,y,z);
		}
		public static Vector4 ToVector4(this float[] current){
			float x = current.Length >= 1 ? current[0] : 0;
			float y = current.Length >= 2 ? current[1] : 0;
			float z = current.Length >= 3 ? current[2] : 0;
			float w = current.Length >= 4 ? current[3] : 0;
			return new Vector4(x,y,z,w);
		}
		public static Rect ToRect(this float[] current){
			Rect result = new Rect();
			result.x = current.Length >= 1 ? current[0] : 0;
			result.y = current.Length >= 2 ? current[1] : 0;
			result.width = current.Length >= 3 ? current[2] : 0;
			result.height = current.Length >= 4 ? current[3] : 0;
			return result;
		}
	}
}