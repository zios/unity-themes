using System;
using System.Collections.Generic;
using System.Linq;
namespace Zios{ 
    public static class IntExtension{
		//=====================
		// General
		//=====================
	    public static int Modulus(this int current,int max){
		    return (((current % max) + max) % max);
	    }
		//=====================
		// Bitwise
		//=====================
	    public static bool Contains(this int current,Enum mask){
		    return (current & mask.ToInt()) != 0;
		}
	    public static bool Contains(this int current,int mask){
		    return (current & mask) != 0;
		}
		//=====================
		// Numeric
		//=====================
		public static int MoveTowards(this int current,int end,int speed){
			if(current > end){speed *= -1;}
			current += speed;
			current = end < current ? Math.Max(current,end) : Math.Min(current,end);
			if((speed > 0 && current > end) || (speed < 0 && current < end)){current = end;}
			return current;
		}
		public static int Distance(this int current,int end){
			return Math.Abs(current-end);
		}
	    public static bool Between(this int current,int start,int end){
		    return current >= start && current <= end;
	    }
	    public static bool InRange(this int current,int start,int end){
		    return current.Between(start,end);
	    }
	    public static bool ToBool(this int current){
		    return current != 0;
	    }
		public static int Closest(this int current,params int[] values){
			int match = int.MaxValue;
			foreach(int value in values){
				if(current.Distance(value) < match){
					match = value;
				}
			}
			return match;
		}
	    public static int RoundClosestDown(this int current,params int[] values){
		    int highest = -1;
		    foreach(int value in values){
			    if(current >= value){
				    highest = value;
				    break;
			    }
		    }
		    foreach(int value in values){
			    if(current >= value && value > highest){
				    highest = value;
			    }
		    }
		    return highest;
	    }
	    public static int RoundClosestUp(this int current,params int[] values){
		    int lowest = -1;
		    foreach(int value in values){
			    if(current >= value){
				    lowest = value;
				    break;
			    }
		    }
		    foreach(int value in values){
			    if(current <= value && value < lowest){
				    lowest = value;
			    }
		    }
		    return lowest;
	    }
		public static int Mean(this IEnumerable<int> current){return (int)current.Average();}
		public static int Median(this IEnumerable<int> current){
			int count = current.Cast<object>().Count(); 
			var sorted = current.OrderBy(n=>n); 
			int midValue = sorted.ElementAt(count/2);
			int median = midValue;
			if(count%2==0){
				median = (midValue + sorted.ElementAt((count/2)-1))/2;
			}
			return median;
		}
		public static int Mode(this IEnumerable<int> current){
			return current.GroupBy(x=>x).OrderByDescending(x=>x.Count()).Select(x=>x.Key).FirstOrDefault();
		}
		public static int Min(this int current,int value){return Math.Min(current,value);}
		public static int Max(this int current,int value){return Math.Max(current,value);}
    }
}