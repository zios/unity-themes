using System;
using System.Collections.Generic;
using UnityEngine;
namespace Zios{
	[AddComponentMenu("Zios/Singleton/Shader Settings (Global)")][ExecuteInEditMode]
	public class ShaderGlobalSettings : MonoBehaviour{
		public static ShaderGlobalSettings instance;
		[Header("Shading")]
		public ShadingBlend shadingBlend = ShadingBlend.Multiply;
		[Header("Shadows")]
		public ShadowType shadowType = ShadowType.Stepped;
		public ShadowMode shadowMode;
		public ShadowBlend shadowBlend;
		[Header("Visibility")]
		public FadeType fadeType;
		public FadeGrayscale fadeGrayscale;
		public FadeBlend fadeBlend;
		private bool dirty;
		private List<Material> materials = new List<Material>();
		private List<Material> materialsChanged = new List<Material>();
		public static ShaderGlobalSettings Get(){return ShaderGlobalSettings.instance;}
		public void OnEnable(){this.Setup();}
		public void Awake(){this.Setup();}
		public void OnValidate(){
			if(!this.CanValidate()){return;}
			this.Setup();
		}
		public void Setup(){
			ShaderGlobalSettings.instance = this;
			if(Application.isEditor){
				this.materials = VariableMaterial.GetAll();
				this.dirty = false;
				this.SetKeyword(shadingBlend);
				this.SetKeyword(shadowType);
				this.SetKeyword(shadowMode);
				this.SetKeyword(shadowBlend);
				this.SetKeyword(fadeType);
				this.SetKeyword(fadeBlend);
				this.SetKeyword(fadeGrayscale);
				if(this.dirty){
					Events.AddStepper("On Editor Update",ShaderGlobalSettings.RefreshStep,this.materialsChanged,50);
				}
			}
		}
		public static void RefreshStep(object collection,int index){
			var materials = (List<Material>)collection;
			Events.stepperTitle = "Updating " + materials.Count + " Materials";
			Events.stepperMessage = "Updating material : " + materials[index].name;
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
					if(!this.dirty){
						this.materialsChanged.Clear();
						this.dirty = true;
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
	public enum FadeGrayscale{Off,On};
	public enum FadeType{Smooth,Stepped};
	public enum FadeBlend{Multiply,Add,Lerp,Overlay,Screen,SoftLight,LinearLight};
}