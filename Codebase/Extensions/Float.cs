using System;
using System.Collections.Generic;
using System.Linq;
namespace Zios.Extensions{
	public static class FloatExtension{
		public static float MoveTowards(this float current,float end,float speed){
			if(current > end){speed *= -1;}
			current += speed;
			current = end < current ? Math.Max(current,end) : Math.Min(current,end);
			if((speed > 0 && current > end) || (speed < 0 && current < end)){current = end;}
			return current;
		}
		public static float Distance(this float current,float end){
			return Math.Abs(current-end);
		}
		public static bool Between(this float current,float start,float end){
			return current >= start && current <= end;
		}
		public static bool InRange(this float current,float start,float end){
			return current.Between(start,end);
		}
		public static float Closest(this float current,params float[] values){
			float match = float.MaxValue;
			foreach(float value in values){
				if(current.Distance(value) < match){
					match = value;
				}
			}
			return match;
		}
		public static float RoundClosestDown(this float current,params float[] values){
			float highest = -1;
			foreach(float value in values){
				if(current >= value){
					highest = value;
					break;
				}
			}
			foreach(float value in values){
				if(current >= value && value > highest){
					highest = value;
				}
			}
			return highest;
		}
		public static float RoundClosestUp(this float current,params float[] values){
			float lowest = -1;
			foreach(float value in values){
				if(current >= value){
					lowest = value;
					break;
				}
			}
			foreach(float value in values){
				if(current <= value && value < lowest){
					lowest = value;
				}
			}
			return lowest;
		}
		public static float Mean(this IEnumerable<float> current){return (float)current.Average();}
		public static float Median(this IEnumerable<float> current){
			int count = current.Count();
			var sorted = current.OrderBy(n=>n);
			float midValue = sorted.ElementAt(count/2);
			float median = midValue;
			if(count%2==0){
				median = (midValue + sorted.ElementAt((count/2)-1))/2;
			}
			return median;
		}
		public static float Mode(this IEnumerable<float> current){
			return current.GroupBy(x=>x).OrderByDescending(x=>x.Count()).Select(x=>x.Key).FirstOrDefault();
		}
		public static float Saturate(this float current){
			return current.Clamp(0,1);
		}
		public static float Clamp(this float current,float min,float max){
			if(current < min){return min;}
			if(current > max){return max;}
			return current;
		}
		public static double ClampStep(this float current,float stepSize){
			return (Math.Round(current / stepSize) * stepSize);
		}
		public static float Lerp(this float current,float start,float end){
			return ((end-start)*current)+start;
		}
		public static float InverseLerp(this float current,float start,float end){
			return ((current-start)/(end-start)).Saturate();
		}
		public static float Min(this float current,float value){return Math.Min(current,value);}
		public static float Max(this float current,float value){return Math.Max(current,value);}
		public static float Abs(this float current){return Math.Abs(current);}
		public static float Ciel(this float current){return (float)Math.Ceiling(current);}
		public static float Floor(this float current){return (float)Math.Floor(current);}
		public static float[] Subtract(this IList<float> current,IList<float> other){
			var result = new float[current.Count];
			for(int index=0;index<current.Count;++index){
				result[index] = current[index];
				if(index >= other.Count){continue;}
				result[index] -= other[index];
			}
			return result;
		}
		public static float[] Add(this IList<float> current,IList<float> other){
			var result = new float[current.Count];
			for(int index=0;index<current.Count;++index){
				result[index] = current[index];
				if(index >= other.Count){continue;}
				result[index] += other[index];
			}
			return result;
		}
		public static float[] Multiply(this IList<float> current,IList<float> other){
			var result = new float[current.Count];
			for(int index=0;index<current.Count;++index){
				result[index] = current[index];
				if(index >= other.Count){continue;}
				result[index] *= other[index];
			}
			return result;
		}
		public static float[] Divide(this IList<float> current,IList<float> other){
			var result = new float[current.Count];
			for(int index=0;index<current.Count;++index){
				result[index] = current[index];
				if(index >= other.Count){continue;}
				result[index] /= other[index];
			}
			return result;
		}
	}
}