using System;
namespace Zios{ 
    public static class IntExtension{
	    public static bool Between(this int current,int start,int end){
		    return current >= start && current <= end;
	    }
	    public static bool InRange(this int current,int start,int end){
		    return current.Between(start,end);
	    }
	    public static int Modulus(this int current,int max){
		    return (((current % max) + max) % max);
	    }
	    public static bool Contains(this int current,Enum mask){
		    return (current & mask.ToInt()) != 0;
		}
	    public static bool Contains(this int current,int mask){
		    return (current & mask) != 0;
		}
	    public static bool ToBool(this int current){
		    return current != 0;
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
    }
}