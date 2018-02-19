using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Zios.Unity.Extensions.Convert{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Unity.SystemAttributes;
	[InitializeOnLoad]
	public static class ConvertColor{
		static ConvertColor(){Setup();}
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void Setup(){
			ConvertObject.serializeMethods.Add((current)=>{
				return current is Color ? current.As<Color>().Serialize() : null;
			});
			ConvertString.deserializeMethods.Add((type,current)=>{
				return type == typeof(Color) ? Color.white.Deserialize(current).Box() : null;
			});
		}
		//============================
		// From
		//============================
		public static string Serialize(this Color current){
			return current.ToHex(false);
		}
		public static string ToHex(this Color current,bool alwaysAlpha=true){
			var red = (current.r*255).ToInt().ToString("X2");
			var green = (current.g*255).ToInt().ToString("X2");
			var blue = (current.b*255).ToInt().ToString("X2");
			var alpha = (current.a*255).ToInt().ToString("X2");
			if(alpha == "FF" && !alwaysAlpha){alpha = "";}
			return "#"+red+green+blue+alpha;
		}
		public static string ToDecimal(this Color current,bool alwaysAlpha=true){
			var red = (current.r*255).ToInt();
			var green = (current.g*255).ToInt();
			var blue = (current.b*255).ToInt();
			var alpha = " " + (current.a*255).ToInt();
			if(alpha==" 255" && !alwaysAlpha){alpha = "";}
			return red+" "+green+" "+blue+alpha;
		}
		public static Vector4 ToVector4(this Color current){
			return new Vector4(current.r,current.g,current.b,current.a);
		}
		public static Vector3 ToVector3(this Color current){
			return new Vector3(current.r,current.g,current.b);
		}
		//============================
		// To
		//============================
		public static Color Deserialize(this Color current,string value){
			return value.ToColor("-");
		}
		public static bool IsColor(this string current,string separator=",",bool? normalized=null){
			try{
				current.ToColor(separator,normalized);
				return true;
			}
			catch{
				return false;
			}
		}
		public static Color[] ToColor(this IEnumerable<string> current){return current.Select(x=>x.ToColor()).ToArray();}
		public static Color ToColor(this float[] current){
			if(current.Length >= 3){
				float r = current[0];
				float g = current[1];
				float b = current[2];
				if(current.Length > 3){
					return new Color(r,g,b,current[3]);
				}
				return new Color(r,g,b);
			}
			return Color.white;
		}
		public static Color ToColor(this string current,string separator=",",bool? normalized=null){
			current = current.Remove("#").Remove("0x").Trim();
			if(current.Contains(separator)){
				var parts = current.Split(separator).ConvertAll<float>();
				normalized = normalized.IsNull() ? current.Contains(".") : normalized;
				if(!normalized.As<bool>()){
					parts = parts.Select(x=>x/255.0f).ToArray();
				}
				float r = parts[0];
				float g = parts[1];
				float b = parts[2];
				float a = parts.Length > 3 ? parts[3] : 1;
				return new Color(r,g,b,a);
			}
			else if(current.Length == 8 || current.Length == 6 || current.Length == 3){
				if(current.Length == 3){
					current += current;
				}
				float r = (float)System.Convert.ToInt32(current.Substring(0,2),16) / 255.0f;
				float g = (float)System.Convert.ToInt32(current.Substring(2,2),16) / 255.0f;
				float b = (float)System.Convert.ToInt32(current.Substring(4,2),16) / 255.0f;
				float a = current.Length == 8 ? (float)System.Convert.ToInt32(current.Substring(6,2),16) / 255.0f : 1;
				return new Color(r,g,b,a);
			}
			else{
				var message = "[StringExtension] Color strings can only be converted from Hexidecimal or comma/space separated Decimal -- " + current;
				throw new Exception(message);
			}
		}
	}
}