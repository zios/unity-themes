using System;
public static class EnumExtension{
	public static String ToString(this Enum current){
		return Enum.GetName(current.GetType(),current);
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
}