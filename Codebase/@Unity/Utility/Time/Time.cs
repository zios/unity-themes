using System.Diagnostics;
using UnityTime = UnityEngine.Time;
namespace Zios.Unity.Time{
	public static class Time{
		public static Stopwatch clock = new Stopwatch();
		public static float Get(){return UnityTime.realtimeSinceStartup;}
		public static float GetDelta(){return UnityTime.deltaTime;}
		public static float GetFixed(){return UnityTime.fixedTime;}
		public static float GetFixedDelta(){return UnityTime.fixedDeltaTime;}
		public static void Start(){
			Time.clock.Reset();
			Time.clock.Start();
		}
		public static float Check(){return (float)Time.clock.Elapsed.TotalMilliseconds/1000f;}
	}
	public static class FloatExtensions{
		public static bool Elapsed(this float current,bool unity=true){return Time.Get()>=current;}
		public static string Passed(this float current,bool unity=true){return Time.Get()-current+" seconds";}
		public static float AddTime(this float current,bool unity=true){return current+Time.Get();}
	}
	public static class IntExtensions{
		public static bool Elapsed(this int current,bool unity=true){return Time.Get()>=current;}
		public static string Passed(this int current,bool unity=true){return Time.Get()-current+" seconds";}
		public static float AddTime(this int current,bool unity=true){return current+Time.Get();}
	}
}