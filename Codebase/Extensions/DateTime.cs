using System;
namespace Zios{
	public static class DateTimeExtension{
		public static string ToQuickDate(this DateTime current){
			return current.ToShortDateString().Replace("/","-");
		}
	}
}