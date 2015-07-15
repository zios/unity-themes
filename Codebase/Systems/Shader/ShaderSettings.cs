using UnityEngine;
using System;
namespace Zios{
	[AddComponentMenu("Zios/Singleton/Shader")][ExecuteInEditMode]
	public class ShaderSettings : MonoBehaviour{
		public static ShaderSettings instance;
		[Header("General")]
		public float globalAlphaCutoff = 0.3f;
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
		private Material[] materials = new Material[0];
		private bool keywordChanged;
		public static ShaderSettings Get(){return ShaderSettings.instance;}
		public void Awake(){this.Setup();}
		public void Setup(){
			ShaderSettings.instance = this;
			Shader.SetGlobalFloat("alphaCutoffGlobal",this.globalAlphaCutoff);
			Shader.SetGlobalFloat("cullDistance",this.cullDistance);
			Shader.SetGlobalFloat("fadeSteps",this.fadeSteps);
			Shader.SetGlobalFloat("fadeStartDistance",this.fadeStartDistance);
			Shader.SetGlobalFloat("fadeEndDistance",this.fadeEndDistance);
			Shader.SetGlobalColor("fadeStartColor",this.fadeStartColor);
			Shader.SetGlobalColor("fadeEndColor",this.fadeEndColor);
			this.materials = Locate.GetAssets<Material>();
			this.SetKeyword(fadeType);
			this.SetKeyword(fadeBlend);
			this.SetKeyword(fadeGrayscale);
			if(this.keywordChanged){
				this.keywordChanged = false;
				VariableMaterial.Refresh(this.materials);
			}
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
				string editorName = material.shader.GetVariable<string>("customEditor");
				if(editorName == "VariableMaterialEditor"){
					foreach(var name in target.GetNames()){
						string keyword = typeName+name.ToUpper();
						if(keyword != targetKeyword && material.IsKeywordEnabled(keyword)){
							material.DisableKeyword(keyword);
						}
					}
					if(!material.IsKeywordEnabled(targetKeyword)){
						this.keywordChanged = true;
						material.EnableKeyword(targetKeyword);
					}
				}
			}
		}
	}
	public enum FadeGrayscale{Off,On};
	public enum FadeType{Smooth,Stepped};
	public enum FadeBlend{Multiply,Add,Lerp,Overlay,Screen,SoftLight,LinearLight};
}