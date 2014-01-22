public static class IntExtension{
	public static bool Contains(this int current,int mask){
		int bits = 1<<current;
		return (mask & bits) == bits;
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