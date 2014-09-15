using UnityEngine;
using Zios;
using System;
using System.Collections.Generic;
public enum InputRange{Any,Zero,Negative,Positive}
public static class InputState{
	public static Dictionary<string,int> owner = new Dictionary<string,int>();
	public static bool CheckOwner(string key,int id,bool released){
		if(InputState.owner.ContainsKey(key)){
			int owner = InputState.owner[key];
			bool isOwner = owner == id;
			if(isOwner && released){
				InputState.owner[key] = -1;
				return true;
			}
			if(!isOwner && owner != -1){
				return true;
			}
		}
		return false;
	}
	public static bool CheckRequirement(InputRange requirement,float intensity){
		bool none = requirement == InputRange.Zero && intensity == 0; 
		bool any = requirement == InputRange.Any && intensity != 0;
		bool less = requirement == InputRange.Negative && intensity < 0;
		bool more = requirement == InputRange.Positive && intensity > 0;
		return any || less || more || none;
	}
}