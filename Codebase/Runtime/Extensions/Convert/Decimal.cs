namespace Zios.Extensions.Convert{
	public static class ConvertDecimal{
		public static bool ToBool(this decimal current){
			return current != 0;
		}
	}
}