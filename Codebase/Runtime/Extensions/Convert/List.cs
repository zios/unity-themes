using System.Collections.Generic;
namespace Zios.Extensions.Convert{
	public static class ConvertList{
		public static Dictionary<string,Type> ToDictionary<Type>(this List<Type> current){
			var values = new Dictionary<string,Type>();
			for(int index=0;index<current.Count;++index){
				values["#"+index] = current[index];
			}
			return values;
		}
	}
}