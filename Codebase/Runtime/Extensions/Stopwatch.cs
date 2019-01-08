using System.Diagnostics;
namespace Zios.Extensions{
	public static class StopWatchExtension{
		public static string Passed(this Stopwatch current){
			current.Stop();
			var value = ((float)current.Elapsed.TotalMilliseconds)/1000.0f + " seconds";
			current.Start();
			return value;
		}
	}
}