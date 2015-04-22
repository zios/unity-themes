using System;
namespace Zios{
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
    }
}