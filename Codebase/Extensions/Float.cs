using System;
using System.Collections.Generic;
using System.Linq;
namespace Zios{ 
    public static class FloatExtension{
		//=====================
		// Numeric
		//=====================
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
	    public static bool ToBool(this float current){
		    return current != 0;
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
			int count = current.Cast<object>().Count(); 
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
		public static float LerpRelative(this float current,float start,float end){
			return ((current-start)/(end-start)).Saturate();
		}
    }
}