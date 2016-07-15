using System;
using UnityEngine;
namespace Zios{
	using System.Collections.Generic;
	public enum ColorBlend{
		Normal,
		Darken,
		Multiply,
		ColorBurn,
		LinearBurn,
		DarkerColor,
		Lighten,
		Screen,
		ColorDodge,
		LinearDodge,
		LighterColor,
		Overlay,
		SoftLight,
		HardLight,
		VividLight,
		LinearLight,
		PinLight,
		HardMix,
		Difference,
		Exclusion,
		Subtract,
		Divide,
		Hue,
		Saturation,
		Color,
		Luminosity
	}
	public static class ColorExtension{
		public static bool Matches(this Color current,Color other,bool matchAlpha=true){
			return matchAlpha ? current == other : (current.r == other.r && current.g == other.g && current.b == other.b);
		}
		public static Color Blend(this Color current,string name,float amount){
			var mode = Enum.Parse(typeof(ColorBlend),name).As<ColorBlend>();
			return current.Blend(current,mode,amount);
		}
		public static Color Blend(this Color current,Color target,string name,float amount=1){
			var mode = Enum.Parse(typeof(ColorBlend),name).As<ColorBlend>();
			return current.Blend(target,mode,amount);
		}
		public static Color Blend(this Color current,Color target,ColorBlend mode,float amount=1){
			var white = Color.white;
			var black = Color.black;
			var value = Color.cyan;
			Func<float,float,float> formula = null;
			if(mode.Has("Darken")){value = current.Min(target);}
			else if(mode.Has("Multiply")){value = current * target;}
			else if(mode.Has("ColorBurn")){
				var blend = current.Invert().Divide(target);
				value = blend.Invert();
			}
			else if(mode.Has("LinearBurn")){value = current+target-white;}
			else if(mode.Has("DarkerColor")){}
			else if(mode.Has("Lighten")){value = current.Max(target);}
			else if(mode.Has("Screen")){value = white-current.Invert()*target.Invert();}
			else if(mode.Has("ColorDodge")){value = current.Divide(target.Invert());}
			else if(mode.Has("LinearDodge")){value = (current + target);}
			else if(mode.Has("LighterColor")){}
			else if(mode.Has("Overlay")){formula = (a,b)=>{return a < 0.5f ? 2*a*b : 1 - (2 * (1-a) * (1-b));};}
			else if(mode.Has("SoftLight")){formula = (a,b)=>{return b < 0.5f ? 2*a*b+a+a*(1-2*b) : Math.Sqrt(a).ToFloat() * (2*b-1) + 2*a*(1-b);};}
			else if(mode.Has("HardLight")){}
			else if(mode.HasAny("VividLight","LinearLight","PinLight")){
				ColorBlend modeA = ColorBlend.ColorBurn;
				ColorBlend modeB = ColorBlend.ColorDodge;
				if(mode.Has("LinearLight")){
					modeA = ColorBlend.LinearBurn;
					modeB = ColorBlend.LinearDodge;
				}
				if(mode.Has("PinLight")){
					modeA = ColorBlend.Darken;
					modeB = ColorBlend.Lighten;
				}
				var blendA = current.Blend(2*target,modeA);
				var blendB = current.Blend(2*(target-(white*0.5f)),modeB);
				value.r = target.r < 0.5f ? blendA.r : blendB.r;
				value.g = target.g < 0.5f ? blendA.g : blendB.g;
				value.b = target.b < 0.5f ? blendA.b : blendB.b;
			}
			else if(mode.Has("HardMix")){
				var blend = current.Blend(target,ColorBlend.VividLight);
				value.r = blend.r < 0.5f ? 0 : 1;
				value.g = blend.g < 0.5f ? 0 : 1;
				value.b = blend.b < 0.5f ? 0 : 1;
			}
			else if(mode.Has("Difference")){value = (current-target).Abs();}
			else if(mode.Has("Exclusion")){value = (current + target - 2.0f * current * target);}
			else if(mode.Has("Subtract")){value = target-current;}
			else if(mode.Has("Divide")){value = target.Divide(current);}
			else if(mode.Has("Hue")){}
			else if(mode.Has("Saturation")){}
			else if(mode.Has("Color")){}
			else if(mode.Has("Luminosity")){}
			if(!formula.IsNull()){
				value.r = formula(current.r,target.r);
				value.g = formula(current.g,target.g);
				value.b = formula(current.b,target.b);
			}
			return value.Max(black).Min(white);
		}
		public static Vector4 ToVector4(this Color current){
			return new Vector4(current.r,current.g,current.b,current.a);
		}
		public static Vector3 ToVector3(this Color current){
			return new Vector3(current.r,current.g,current.b);
		}
		public static Color Abs(this Color current){
			current.r = Mathf.Abs(current.r);
			current.g = Mathf.Abs(current.g);
			current.b = Mathf.Abs(current.b);
			return current;
		}
		public static Color Min(this Color current,Color other){
			current.r = Mathf.Min(current.r,other.r);
			current.g = Mathf.Min(current.g,other.g);
			current.b = Mathf.Min(current.b,other.b);
			return current;
		}
		public static Color Max(this Color current,Color other){
			current.r = Mathf.Max(current.r,other.r);
			current.g = Mathf.Max(current.g,other.g);
			current.b = Mathf.Max(current.b,other.b);
			return current;
		}
		public static Color Add(this Color current,Color amount){
			current.r = Mathf.Clamp(current.r+amount.r,0,1);
			current.g = Mathf.Clamp(current.g+amount.g,0,1);
			current.b = Mathf.Clamp(current.b+amount.b,0,1);
			return current;
		}
		public static Color Add(this Color current,float amount){
			return current.Add(new Color(amount,amount,amount));
		}
		public static Color Subtract(this Color current,Color amount){
			current.r = Mathf.Clamp(current.r-amount.r,0,1);
			current.g = Mathf.Clamp(current.g-amount.g,0,1);
			current.b = Mathf.Clamp(current.b-amount.b,0,1);
			return current;
		}
		public static Color Subtract(this Color current,float amount){
			return current.Subtract(new Color(amount,amount,amount));
		}
		public static Color Multiply(this Color current,Color amount){
			current.r = Mathf.Clamp(current.r*amount.r,0,1);
			current.g = Mathf.Clamp(current.g*amount.g,0,1);
			current.b = Mathf.Clamp(current.b*amount.b,0,1);
			return current;
		}
		public static Color Multiply(this Color current,float amount){
			return current.Multiply(new Color(amount,amount,amount));
		}
		public static Color Divide(this Color current,Color amount){
			current.r = Mathf.Clamp(current.r/amount.r,0,1);
			current.g = Mathf.Clamp(current.g/amount.g,0,1);
			current.b = Mathf.Clamp(current.b/amount.b,0,1);
			return current;
		}
		public static Color Divide(this Color current,float amount){
			return current.Divide(new Color(amount,amount,amount));
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
		public static float GetIntensity(this Color current){
			return (current.r+current.g+current.b)/3;
		}
		public static Color Invert(this Color current){
			return Color.white - current;
		}
	}
}