using System.Text;
namespace Zios.Extensions{
	public static class StringBuilderExtension{
		public static void Clear(this StringBuilder current){
			current.Length = 0;
			current.Capacity = 0;
		}
		public static StringBuilder Append(this StringBuilder current,params string[] values){
			foreach(var value in values){
				current.Append(value);
			}
			return current;
		}
		public static StringBuilder Prepend(this StringBuilder current,params string[] values){
			var index = 0;
			foreach(var value in values){
				current.Insert(index,value);
				index += value.Length;
			}
			return current;
		}
	}
}