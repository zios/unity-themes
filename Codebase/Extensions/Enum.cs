using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
namespace Zios{
    public static class EnumExtension{
	    public static string ToName(this Enum current){
			Type type = current.GetType();
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
					Enum value = current.Parse(currentName);
					if(current.Contains(value)){
						names.Append(currentName + " ");
					}
				}
				name = names.ToString();
			}
		    return name;
	    }
	    public static int ToInt(this Enum current){
			return Convert.ToInt32(current);
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
		public static bool Within(this Enum current,params string[] values){
			for(int index=0;index<values.Length;++index){
				Enum value = current.Parse(values[index]);
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
		public static bool Contains(this Enum current,params string[] values){return current.Has(values);}
	    public static bool Has(this Enum current,Enum mask){return current.Contains(mask);}
		public static bool Has(this Enum current,params string[] values){return current.HasAny(values);}
	    public static bool HasAny(this Enum current,params string[] values){return current.ToName().HasAny(values);}
	    public static bool HasAll(this Enum current,params string[] values){return current.ToName().HasAll(values);}
	    public static bool Matches(this Enum current,Enum value){return (current.ToInt() & value.ToInt()) == value.ToInt();}
	    public static bool Matches(this Enum current,params string[] values){return current.MatchesAny(values);}
	    public static bool MatchesAny(this Enum current,params string[] values){return current.ToName().MatchesAny(values);}
    }
}