using UnityEngine;
using System;
using System.Collections.Generic;
namespace Zios{
	[AddComponentMenu("Zios/Singleton/Shader")][ExecuteInEditMode]
	public class ShaderSettings : MonoBehaviour{
		public static ShaderSettings instance;
		[Header("General")]
		public float alphaCutoff = 0.3f;
		[Header("Shadows")]
		public Color shadowColor = new Color(0,0,0,1);
		public ShadowMode shadowMode;
		public ShadowBlend shadowBlend;
		[Range(0,1)] public float shadowIntensity = 0.5f;
		[Header("Visibility")]
		public int cullDistance = 100;
		public FadeType fadeType;
		public FadeGrayscale fadeGrayscale;
		public FadeBlend fadeBlend;
		public int fadeSteps = 3;
		public int fadeStartDistance = 80;
		public int fadeEndDistance = 100;
		public Color fadeStartColor = new Color(1,1,1,1);
		public Color fadeEndColor = new Color(1,1,1,0);
		private bool dirty;
		private List<Material> materials = new List<Material>();
		private List<Material> materialsChanged = new List<Material>();
		public static ShaderSettings Get(){return ShaderSettings.instance;}
		public void Awake(){this.Setup();}
		public void Setup(){
			ShaderSettings.instance = this;
			Shader.SetGlobalFloat("globalAlphaCutoff",this.alphaCutoff);
			Shader.SetGlobalColor("globalShadowColor",this.shadowColor);
			Shader.SetGlobalFloat("globalShadowIntensity",1-this.shadowIntensity);
			Shader.SetGlobalFloat("cullDistance",this.cullDistance);
			Shader.SetGlobalFloat("fadeSteps",this.fadeSteps);
			Shader.SetGlobalFloat("fadeStartDistance",this.fadeStartDistance);
			Shader.SetGlobalFloat("fadeEndDistance",this.fadeEndDistance);
			Shader.SetGlobalColor("fadeStartColor",this.fadeStartColor);
			Shader.SetGlobalColor("fadeEndColor",this.fadeEndColor);
			if(Application.isEditor){
				this.materials = VariableMaterial.GetAll();
				this.dirty = false;
				this.SetKeyword(shadowMode);
				this.SetKeyword(shadowBlend);
				this.SetKeyword(fadeType);
				this.SetKeyword(fadeBlend);
				this.SetKeyword(fadeGrayscale);
				if(this.dirty){
					Events.AddStepper("On Editor Update",this.RefreshStep,this.materialsChanged,50);
				}
			}
		}
		public static void RefreshStep(object collection,int index){
			var materials = (List<Material>)collection;
			Events.stepperTitle = "Updating " + materials.Count + " Materials";
			Events.stepperMessage = "Updating material : " + materials[index].name;
			VariableMaterial.Refresh(true,materials[index]);
		}
		public void Update(){
			Shader.SetGlobalFloat("timeConstant",(Time.realtimeSinceStartup));
			Events.Add("On Editor Update",this.EditorUpdate);
		}
		public void OnValidate(){
			if(Application.isPlaying || Application.isLoadingLevel || !this.gameObject.activeInHierarchy){return;}
			this.Setup();
			this.Update();
		}
		public void EditorUpdate(){
			#if UNITY_EDITOR
			if(!Application.isPlaying && !Application.isLoadingLevel && UnityEditor.EditorPrefs.GetBool("ShaderSettings-AlwaysUpdate")){
				Shader.SetGlobalFloat("timeConstant",(Time.realtimeSinceStartup));
				Utility.RepaintSceneView();
			}
			#endif	
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
	public enum ShadowMode{Shaded,Blended};
	public enum ShadowBlend{Multiply,Subtract};
	public enum FadeGrayscale{Off,On};
	public enum FadeType{Smooth,Stepped};
	public enum FadeBlend{Multiply,Add,Lerp,Overlay,Screen,SoftLight,LinearLight};
}