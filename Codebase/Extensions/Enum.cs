using UnityEngine;
using System;
using System.Collections.Generic;
namespace Zios{
    public static class EnumExtension{
	    public static string ToString(this Enum current){
		    return Enum.GetName(current.GetType(),current);
	    }
	    public static int ToInt(this Enum current){
			return Convert.ToInt32(current);
			//return (int)current;
	    }
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
	    public static T Parse<T>(this T current,string value){
			return (T)Enum.Parse(current.GetType(),value);
		}
	    public static bool Contains(this Enum current,Enum mask){
		    return (current.ToInt() & mask.ToInt()) != 0;
		    //int bits = 1<<mask.ToInt();
		    //return (current.ToInt() & bits) == bits;
		    //return (current.ToInt() | (1<<mask.ToInt())) == current.ToInt();
	    }
		public static bool Contains(this Enum current,params string[] values){return current.Has(values);}
	    public static bool Has(this Enum current,Enum mask){return current.Contains(mask);}
		public static bool Has(this Enum current,params string[] values){return current.HasAny(values);}
	    public static bool HasAny(this Enum current,params string[] values){return current.ToString().ContainsAny(values);}
	    public static bool HasAll(this Enum current,params string[] values){return current.ToString().ContainsAll(values);}
	    public static bool Matches(this Enum current,Enum value){return (current.ToInt() & value.ToInt()) == value.ToInt();}
	    public static bool Matches(this Enum current,params string[] values){return current.MatchesAny(values);}
	    public static bool MatchesAny(this Enum current,params string[] values){return current.ToString().MatchesAny(values);}
	    public static bool MatchesAll(this Enum current,params string[] values){return current.ToString().MatchesAll(values);}
    }
}