using UnityEngine;
namespace Zios.Unity.Extensions{
	public static class GUIStyleStateExtension{
		public static GUIStyleState InvertTextColor(this GUIStyleState current,float intensityCompare,float difference=0.6f){
			var comparison = Mathf.Abs(intensityCompare - current.textColor.GetIntensity());
			if(comparison < difference){
				current.textColor = current.textColor.Invert();
			}
			return current;
		}
	}
}