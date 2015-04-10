using System;
using System.Collections.Generic;
namespace Zios{
    public static class EnumExtension{
	    public static String ToString(this Enum current){
		    return Enum.GetName(current.GetType(),current);
	    }
	    public static int ToInt(this Enum current){
		    return (int)Convert.ChangeType(current,current.GetType());
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
		    Array values = Enum.GetValues(current.GetType());	
		    List<string> result = new List<string>();
		    foreach(var value in values){
			    result.Add(value.ToString());
		    }
		    return result.ToArray();
	    }
	    public static bool Has(this Enum current,Enum mask){return current.Contains(mask);}
	    public static bool Contains(this Enum current,Enum mask){
		    return (current.ToInt() & mask.ToInt()) != 0;
		    //int bits = 1<<mask.ToInt();
		    //return (current.ToInt() & bits) == bits;
		    //return (current.ToInt() | (1<<mask.ToInt())) == current.ToInt();
	    }
	    public static bool Matches(this Enum current,Enum value){
		    return (current.ToInt() & value.ToInt()) == value.ToInt();
	    }
    }
}