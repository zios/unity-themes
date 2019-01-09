using System;
using System.Collections.Generic;
using System.Linq;
namespace Zios.Extensions{
	public static class DecimalExtension{
		//=====================
		// Numeric
		//=====================
		public static decimal MoveTowards(this decimal current,decimal end,decimal speed){
			if(current > end){speed *= -1;}
			current += speed;
			current = end < current ? Math.Max(current,end) : Math.Min(current,end);
			if((speed > 0 && current > end) || (speed < 0 && current < end)){current = end;}
			return current;
		}
		public static decimal Distance(this decimal current,decimal end){
			return Math.Abs(current-end);
		}
		public static bool Between(this decimal current,decimal start,decimal end){
			return current >= start && current <= end;
		}
		public static bool InRange(this decimal current,decimal start,decimal end){
			return current.Between(start,end);
		}
		public static decimal Closest(this decimal current,params decimal[] values){
			decimal match = decimal.MaxValue;
			foreach(decimal value in values){
				if(current.Distance(value) < match){
					match = value;
				}
			}
			return match;
		}
		public static decimal RoundClosestDown(this decimal current,params decimal[] values){
			decimal highest = -1;
			foreach(decimal value in values){
				if(current >= value){
					highest = value;
					break;
				}
			}
			foreach(decimal value in values){
				if(current >= value && value > highest){
					highest = value;
				}
			}
			return highest;
		}
		public static decimal RoundClosestUp(this decimal current,params decimal[] values){
			decimal lowest = -1;
			foreach(decimal value in values){
				if(current >= value){
					lowest = value;
					break;
				}
			}
			foreach(decimal value in values){
				if(current <= value && value < lowest){
					lowest = value;
				}
			}
			return lowest;
		}
		public static decimal Mean(this IEnumerable<decimal> current){return (decimal)current.Average();}
		public static decimal Median(this IEnumerable<decimal> current){
			int count = current.Count();
			var sorted = current.OrderBy(n=>n);
			decimal midValue = sorted.ElementAt(count/2);
			decimal median = midValue;
			if(count%2==0){
				median = (midValue + sorted.ElementAt((count/2)-1))/2;
			}
			return median;
		}
		public static decimal Mode(this IEnumerable<decimal> current){
			return current.GroupBy(x=>x).OrderByDescending(x=>x.Count()).Select(x=>x.Key).FirstOrDefault();
		}
		public static decimal Min(this decimal current,decimal value){return Math.Min(current,value);}
		public static decimal Max(this decimal current,decimal value){return Math.Max(current,value);}
	}
}
