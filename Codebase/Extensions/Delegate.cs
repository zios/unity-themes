using System;
namespace Zios.Extensions{
	public static class DelegateExtension{
		public static bool ContainsMethod(this Delegate current,Delegate value){
			if(current.IsNull()){return false;}
			foreach(var item in current.GetInvocationList()){
				if(item == value){return true;}
			}
			return false;
		}
		public static bool Contains(this Delegate current,Delegate value){
			return current.ContainsMethod(value);
		}
		public static Delegate AddNew(this Delegate current,Delegate value){
			if(!current.ContainsMethod(value)){
				return Delegate.Combine(current,value);
			}
			return current;
		}
	}
}
