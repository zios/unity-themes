using System;
using UnityEngine;
namespace Zios.Unity.ShaderManager{
	using Zios.Events;
	using Zios.Unity.Supports.Singleton;
	using Zios.Unity.Time;
	//asm Zios.Shortcuts;
	//asm Zios.Unity.Shortcuts;
	public class ShaderManager : Singleton{
		public static ShaderManager singleton;
		[Header("Shading")]
		public ShadingBlend shadingBlend = ShadingBlend.Multiply;
		[Range(0,1)] public float alphaCutoff = 0.3f;
		[Header("Shadows")]
		public ShadowType shadowType = ShadowType.Smooth;
		public ShadowMode shadowMode;
		public ShadowBlend shadowBlend;
		public Color shadowColor = new Color(0,0,0,0.25f);
		[Range(1,32)] public int shadowSteps = 3;
		[Header("Lightmap")]
		public LightmapType lightmapType = LightmapType.Smooth;
		public LightmapMode lightmapMode;
		public LightmapBlend lightmapBlend;
		public Color lightmapColor = new Color(0,0,0,0.25f);
		[Range(1,32)] public int lightmapSteps = 3;
		[Header("Visibility")]
		public FadeType fadeType = FadeType.Smooth;
		public FadeGrayscale fadeGrayscale;
		public FadeBlend fadeBlend = FadeBlend.Screen;
		public int cullDistance = 150;
		[Range(1,32)] public int fadeSteps = 3;
		public int fadeStartDistance = 80;
		public int fadeEndDistance = 100;
		public Color fadeStartColor = new Color(0,0,0,1);
		public Color fadeEndColor = new Color(0,0,0,0);
		public static ShaderManager Get(){
			ShaderManager.singleton = ShaderManager.singleton ?? Singleton.Get<ShaderManager>();
			return ShaderManager.singleton;
		}
		public void OnEnable(){this.Setup();}
		public void Setup(){
			ShaderManager.singleton = this;
			Events.Add("On Update",this.Update);
			this.cullDistance = Math.Max(0,this.cullDistance);
			this.fadeStartDistance = Math.Max(0,Math.Min(this.fadeStartDistance,this.fadeEndDistance));
			this.fadeEndDistance = Math.Max(this.fadeStartDistance,this.fadeEndDistance);
			Shader.SetGlobalFloat("globalAlphaCutoff",this.alphaCutoff);
			Shader.SetGlobalColor("globalShadowColor",this.shadowColor);
			Shader.SetGlobalFloat("globalShadowSteps",this.shadowSteps);
			Shader.SetGlobalColor("globalLightmapColor",this.lightmapColor);
			Shader.SetGlobalFloat("globalLightmapSteps",this.lightmapSteps);
			Shader.SetGlobalFloat("cullDistance",this.cullDistance);
			Shader.SetGlobalFloat("fadeSteps",this.fadeSteps);
			Shader.SetGlobalFloat("fadeStartDistance",this.fadeStartDistance);
			Shader.SetGlobalFloat("fadeEndDistance",this.fadeEndDistance);
			Shader.SetGlobalColor("fadeStartColor",this.fadeStartColor);
			Shader.SetGlobalColor("fadeEndColor",this.fadeEndColor);
		}
		public void Update(){
			Shader.SetGlobalFloat("timeConstant",Time.Get());
		}
	}
	public enum ShadingBlend{Multiply,Add,Lerp,Overlay,Screen,SoftLight,LinearLight};
	public enum ShadowType{Smooth,Stepped};
	public enum ShadowMode{Shaded,Blended};
	public enum ShadowBlend{Lerp,Multiply,Subtract};
	public enum LightmapType{Smooth,Stepped};
	public enum LightmapMode{Shaded,Blended};
	public enum LightmapBlend{Lerp,Multiply,Subtract};
	public enum FadeGrayscale{Off,On};
	public enum FadeType{Smooth,Stepped};
	public enum FadeBlend{Multiply,Add,Lerp,Overlay,Screen,SoftLight,LinearLight};
}