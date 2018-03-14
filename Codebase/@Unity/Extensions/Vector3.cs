using System.Collections.Generic;
using UnityEngine;
namespace Zios.Unity.Extensions{
	using Zios.Extensions;
	using Zios.Unity.Extensions.Convert;
	public static class Vector3Extension{
		//=====================
		// General
		//=====================
		public static Vector3 Set(this Vector3 current,Vector3 target){return current.Use(target);}
		public static Vector3 Use(this Vector3 current,Vector3 target){
			current.x = target.x;
			current.y = target.y;
			current.z = target.z;
			return current;
		}
		public static Vector3 MoveTowards(this Vector3 current,Vector3 end,Vector3 speed){
			current.x = current.x.MoveTowards(end.x,speed.x);
			current.y = current.y.MoveTowards(end.y,speed.y);
			current.z = current.z.MoveTowards(end.z,speed.z);
			return current;
		}
		public static Vector3 MoveTowards(this Vector3 current,Vector3 end,float speed){
			return Vector3.MoveTowards(current,end,speed);
		}
		public static float Distance(this Vector3 current,Vector3 end){
			return Vector3.Distance(current,end);
		}
		public static Vector3 ScaleBy(this Vector3 current,Vector3 other){
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
		public static Vector3 Abs(this Vector3 vector){
			Vector3 copy = vector;
			copy.x = Mathf.Abs(copy.x);
			copy.y = Mathf.Abs(copy.y);
			copy.z = Mathf.Abs(copy.z);
			return copy;
		}
		public static Vector3 RotateAround(this Vector3 current,Vector3 point,Vector3 euler){
			return euler.ToRotation() * (current - point) + point;
		}
		public static bool Approximately(this Vector3 current,Vector3 value){
			bool x = Mathf.Approximately(current.x,value.x);
			bool y = Mathf.Approximately(current.y,value.y);
			bool z = Mathf.Approximately(current.z,value.z);
			return x && y && z;
		}
		public static Vector3 Divide(this Vector3 current,Vector3 value){
			Vector3 copy = current;
			copy[0] /= value[0];
			copy[1] /= value[1];
			copy[2] /= value[2];
			return copy;
		}
		public static float[] Distance(this IList<Vector3> current,IList<Vector3> other){
			var result = new float[current.Count];
			for(int index=0;index<current.Count;++index){
				if(index >= other.Count){continue;}
				result[index] = Vector3.Distance(current[index],other[index]);
			}
			return result;
		}
		public static Vector3[] Subtract(this IList<Vector3> current,IList<Vector3> other){
			var result = new Vector3[current.Count];
			for(int index=0;index<current.Count;++index){
				result[index] = current[index];
				if(index >= other.Count){continue;}
				result[index] -= other[index];
			}
			return result;
		}
		public static Vector3[] Add(this IList<Vector3> current,IList<Vector3> other){
			var result = new Vector3[current.Count];
			for(int index=0;index<current.Count;++index){
				result[index] = current[index];
				if(index >= other.Count){continue;}
				result[index] += other[index];
			}
			return result;
		}
		public static Vector3[] Multiply(this IList<Vector3> current,IList<Vector3> other){
			var result = new Vector3[current.Count];
			for(int index=0;index<current.Count;++index){
				result[index] = current[index];
				if(index >= other.Count){continue;}
				result[index].Scale(other[index]);
			}
			return result;
		}
		public static Vector3[] Divide(this IList<Vector3> current,IList<Vector3> other){
			var result = new Vector3[current.Count];
			for(int index=0;index<current.Count;++index){
				result[index] = current[index];
				if(index >= other.Count){continue;}
				result[index] = current[index].Divide(other[index]);
			}
			return result;
		}
	}
}