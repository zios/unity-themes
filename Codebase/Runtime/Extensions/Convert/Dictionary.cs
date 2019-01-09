using System.Collections.Generic;
namespace Zios.Extensions.Convert{
	public static class ConvertDictionary{
		public static SortedList<TKey,TValue> ToSortedList<TKey,TValue>(this Dictionary<TKey,TValue> current){
			return new SortedList<TKey,TValue>(current);
		}
	}
}