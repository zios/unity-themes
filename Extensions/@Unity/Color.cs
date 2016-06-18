using System.Linq;
using UnityEngine;
namespace Zios{
	using System.Collections.Generic;
	public static class ColorExtension{
		public static Color Add(this Color current,Color amount){
			current.r = Mathf.Clamp(current.r+amount.r,0,1);
			current.g = Mathf.Clamp(current.g+amount.g,0,1);
			current.b = Mathf.Clamp(current.b+amount.b,0,1);
			return current;
		}
		public static Color Add(this Color current,float amount){
			current.r = Mathf.Clamp(current.r+amount,0,1);
			current.g = Mathf.Clamp(current.g+amount,0,1);
			current.b = Mathf.Clamp(current.b+amount,0,1);
			return current;
		}
		public static Color Multiply(this Color current,Color amount){
			current.r = Mathf.Clamp(current.r*amount.r,0,1);
			current.g = Mathf.Clamp(current.g*amount.g,0,1);
			current.b = Mathf.Clamp(current.b*amount.b,0,1);
			return current;
		}
		public static Color Multiply(this Color current,float amount){
			current.r = Mathf.Clamp(current.r*amount,0,1);
			current.g = Mathf.Clamp(current.g*amount,0,1);
			current.b = Mathf.Clamp(current.b*amount,0,1);
			return current;
		}
		public static Color Random(this Color current,float intensity=1.0f){
			int[] order = (new List<int>(){0,1,2}).Shuffle().ToArray();
			float[] color = new float[3];
			color[order[0]] = UnityEngine.Random.Range(intensity,1.0f);
			color[order[1]] = UnityEngine.Random.Range(0,1.0f - intensity);
			color[order[2]] = UnityEngine.Random.Range(0,1.0f);
			return new Color(color[0],color[1],color[2]);
		}
		public static string ToHex(this Color current,bool alwaysAlpha=true){
			var red = (current.r*255).ToInt().ToString("X2");
			var green = (current.g*255).ToInt().ToString("X2");
			var blue = (current.b*255).ToInt().ToString("X2");
			var alpha = (current.a*255).ToInt().ToString("X2");
			if(alpha == "FF" && !alwaysAlpha){alpha = "";}
			return "#"+red+green+blue+alpha;
		}
		public static string Serialize(this Color current){
			return current.ToHex(false);
		}
		public static Color Deserialize(this Color current,string value){
			return value.ToColor("-");
		}
	}
}