using UnityEngine;
using System;
using System.Collections.Generic;
public static class DelegateExtension{
	public static bool ContainsMethod(this Delegate current,Delegate value){
		foreach(Delegate item in current.GetInvocationList()){
			if(item == value){return true;}
		}
		return false;
	}
	public static bool Contains(this Delegate current,Delegate value){
		return current.ContainsMethod(value);
	}
}