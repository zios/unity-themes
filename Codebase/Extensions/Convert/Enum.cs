using System;
using System.Collections.Generic;
using System.Text;
namespace Zios.Extensions.Convert{
	using Zios.Extensions;
	public static class EnumConvert{
		public static Dictionary<Type,Dictionary<Enum,string>> nameCache = new Dictionary<Type,Dictionary<Enum,string>>();
		public static string ToName(this Enum current){
			Type type = current.GetType();
			var cache = EnumConvert.nameCache;
			if(cache.ContainsKey(type) && cache[type].ContainsKey(current)){
				return cache[type][current];
			}
			if(current.ToInt() == -1){
				string[] allNames = Enum.GetNames(type);
				return string.Join(" ",allNames);
			}
			string name = Enum.GetName(type,current);
			if(name.IsEmpty() || name.IsNull()){
				string[] allNames = Enum.GetNames(type);
				StringBuilder names = new StringBuilder();
				for(int index=0;index<allNames.Length;++index){
					string currentName = allNames[index];
					Enum value = current.ParseEnum(currentName);
					if(current.Contains(value)){
						names.Append(currentName + " ");
					}
				}
				name = names.ToString();
			}
			cache.AddNew(type)[current] = name;
			return name;
		}
		public static string Serialize(this Enum current,bool ignoreDefault=false,int defaultValue=0){
			return ignoreDefault && current.ToInt() == defaultValue ? null : current.ToName().Serialize();
		}
		public static int ToInt(this Enum current){
			return System.Convert.ToInt32(current);
		}
		public static int GetIndex(this Enum current){
			return current.GetNames().IndexOf(current.ToName());
		}
		public static bool Within(this Enum current,params string[] values){
			for(int index=0;index<values.Length;++index){
				Enum value = current.ParseEnum(values[index]);
				if(current.Contains(value)){return true;}
			}
			return false;
		}
		public static bool Contains(this Enum current,int mask){
			return (current.ToInt() & mask) != 0;
		}
		public static bool Contains(this Enum current,Enum mask){
			return (current.ToInt() & mask.ToInt()) != 0;
			//int bits = 1<<mask.ToInt();
			//return (current.ToInt() & bits) == bits;
			//return (current.ToInt() | (1<<mask.ToInt())) == current.ToInt();
		}
		public static bool ContainsAny(this Enum current,params string[] values){return current.HasAny(values);}
		public static bool Contains(this Enum current,params string[] values){return current.Has(values);}
		public static bool Has(this Enum current,Enum mask){return current.Contains(mask);}
		public static bool Has(this Enum current,params string[] values){return current.HasAny(values);}
		public static bool HasAny(this Enum current,params string[] values){return current.MatchesAny(values);}
		public static bool HasAll(this Enum current,params string[] values){return current.MatchesAll(values);}
		public static bool Matches(this Enum current,Enum value){return (current.ToInt() & value.ToInt()) == value.ToInt();}
		public static bool Matches(this Enum current,params string[] values){return current.MatchesAny(values);}
		public static bool MatchesAny(this Enum current,params string[] values){return current.ToName().Split(" ").ContainsAny(values);}
		public static bool MatchesAll(this Enum current,params string[] values){return current.ToName().Split(" ").ContainsAll(values);}
	}
}