public static class IntExtension{
	public static bool Contains(this int current,int mask){
		int bits = 1<<current;
		return (mask & bits) == bits;
	}
	public static bool ToBool(this int current){
		return current != 0;
	}
}