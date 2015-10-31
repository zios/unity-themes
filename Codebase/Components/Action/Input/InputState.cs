using UnityEngine;
using Zios;
using System;
using System.Collections.Generic;
namespace Zios{
	public enum InputRange{Any,Zero,Negative,Positive}
	public static class InputState{
		public static Dictionary<string,int> owner = new Dictionary<string,int>();
		public static void SetOwner(string key,int id){
			InputState.owner[key] = id;
		}
		public static void ResetOwner(string key){
			InputState.SetOwner(key,-1);
		}
		public static bool HasOwner(string key){
			if(InputState.owner.ContainsKey(key)){
				return InputState.owner[key] != -1;
			}
			return false;
		}
		public static bool IsOwner(string key,int id){
			if(InputState.HasOwner(key)){
				return InputState.owner[key] == id;
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
}