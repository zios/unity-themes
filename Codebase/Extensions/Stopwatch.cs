using System.Diagnostics;
namespace Zios{
	public static class StopWatchExtension{
		public static string Passed(this Stopwatch current){
			current.Stop();
			var value = current.Elapsed.TotalMilliseconds.ToFloat()/1000.0f + " seconds";
			current.Start();
			return value;
		}
	}
}