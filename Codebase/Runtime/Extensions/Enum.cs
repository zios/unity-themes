using System;
using System.Linq;
namespace Zios.Extensions{
	public static class EnumExtension{
		public static Enum Get(this Enum current,string value,int fallback=-1){
			Type type = current.GetType();
			string[] items = Enum.GetNames(type);
			bool found = false;
			for(int index=0;index<items.Length;++index){
				string name = items[index];
				if(name.Matches(value,true)){
					value = name;
					found = true;
					break;
				}
			}
			if(!found && fallback != -1){
				value = fallback.ToString();
			}
			return (Enum)Enum.Parse(type,value);
		}
		public static string[] GetNames(this Enum current){
			return Enum.GetNames(current.GetType());
		}
		public static Array GetValues(this Enum current){
			return Enum.GetValues(current.GetType());
		}
		public static T[] GetValues<T>(this Enum current){
			return (T[])Enum.GetValues(typeof(T));
		}
		public static int GetMaskFull(this Enum current){
			return current.GetValues().Cast<int>().Sum();
		}
		public static T Parse<T>(string value){
			return (T)Enum.Parse(typeof(T),value);
		}
		public static T ParseEnum<T>(this T current,string value){
			return (T)Enum.Parse(current.GetType(),value);
		}
	}
}
