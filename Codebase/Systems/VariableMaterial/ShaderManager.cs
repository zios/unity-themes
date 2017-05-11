using System;
using System.Collections.Generic;
using UnityEngine;
namespace Zios.Shaders{
	using Event;
	public class ShaderManager : Singleton{
		public static ShaderManager instance;
		[Header("Shading")]
		public ShadingBlend shadingBlend = ShadingBlend.Multiply;
		[Range(0,1)] public float alphaCutoff = 0.3f;
		[Header("Shadows")]
		public ShadowType shadowType = ShadowType.Stepped;
		public ShadowMode shadowMode;
		public ShadowBlend shadowBlend;
		public Color shadowColor = new Color(0,0,0,0.25f);
		[Range(1,32)] public int shadowSteps = 3;
		[Header("Lightmap")]
		public LightmapType lightmapType = LightmapType.Stepped;
		public LightmapMode lightmapMode;
		public LightmapBlend lightmapBlend;
		public Color lightmapColor = new Color(0,0,0,0.25f);
		[Range(1,32)] public int lightmapSteps = 3;
		[Header("Visibility")]
		public FadeType fadeType;
		public FadeGrayscale fadeGrayscale;
		public FadeBlend fadeBlend;
		public int cullDistance = 150;
		[Range(1,32)] public int fadeSteps = 3;
		public int fadeStartDistance = 80;
		public int fadeEndDistance = 100;
		public Color fadeStartColor = new Color(0,0,0,1);
		public Color fadeEndColor = new Color(0,0,0,0);
		private bool keywordsChanged;
		private List<Material> materials = new List<Material>();
		private List<Material> materialsChanged = new List<Material>();
		public static ShaderManager Get(){return ShaderManager.instance;}
		public void OnEnable(){this.Setup();}
		public void Setup(){
			ShaderManager.instance = this;
			Events.Add("On Update",this.Update);
			if(Application.isEditor){
				this.materials = VariableMaterial.GetAll();
				this.keywordsChanged = false;
				this.SetKeyword(shadingBlend);
				this.SetKeyword(shadowType);
				this.SetKeyword(shadowMode);
				this.SetKeyword(shadowBlend);
				this.SetKeyword(lightmapType);
				this.SetKeyword(lightmapMode);
				this.SetKeyword(lightmapBlend);
				this.SetKeyword(fadeType);
				this.SetKeyword(fadeBlend);
				this.SetKeyword(fadeGrayscale);
				if(this.keywordsChanged){
					Events.AddStepper("On Editor Update",ShaderManager.RefreshStep,this.materialsChanged,50);
				}
			}
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
			Shader.SetGlobalFloat("timeConstant",Time.realtimeSinceStartup);
		}
		public static void RefreshStep(object collection,int index){
			var materials = (List<Material>)collection;
			EventStepper.title = "Updating " + materials.Count + " Materials";
			EventStepper.message = "Updating material : " + materials[index].name;
			VariableMaterial.Refresh(true,materials[index]);
		}
		public void SetKeyword(Enum target){
			string typeName = target.GetType().Name.ToUpper()+"_";
			string targetKeyword = typeName+target.ToString().ToUpper();
			foreach(var material in this.materials){
				foreach(var name in target.GetNames()){
					string keyword = typeName+name.ToUpper();
					if(keyword != targetKeyword && material.IsKeywordEnabled(keyword)){
						material.DisableKeyword(keyword);
					}
				}
				if(!material.IsKeywordEnabled(targetKeyword)){
					if(!this.keywordsChanged){
						this.materialsChanged.Clear();
						this.keywordsChanged = true;
					}
					this.materialsChanged.Add(material);
					material.EnableKeyword(targetKeyword);
				}
			}
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