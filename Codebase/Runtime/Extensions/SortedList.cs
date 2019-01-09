using System.Collections.Generic;
namespace Zios.Extensions{
	public static class SortedListExtension{
		public static SortedList<T,V> Copy<T,V>(this SortedList<T,V> current){
			return new SortedList<T,V>(current);
		}
	}
}
