using System;
namespace Zios{
	public enum InputRange{Any,Zero,Negative,Positive}
	public static class InputState{
		[NonSerialized] public static bool disabled;
		public static bool CheckRequirement(InputRange requirement,float intensity){
			bool none = requirement == InputRange.Zero && intensity == 0;
			bool any = requirement == InputRange.Any && intensity != 0;
			bool less = requirement == InputRange.Negative && intensity < 0;
			bool more = requirement == InputRange.Positive && intensity > 0;
			return any || less || more || none;
		}
	}
}