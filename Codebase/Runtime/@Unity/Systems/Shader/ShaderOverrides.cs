using System;
using UnityEngine;
namespace Zios.Unity.ShaderManager{
	using Zios.Unity.Proxy;
	using Zios.Unity.Time;
	[AddComponentMenu("Zios/Component/Rendering/Shader Overrides")][ExecuteInEditMode]
	public class ShaderOverrides : MonoBehaviour{
		public static ShaderOverrides instance;
		[Header("General")]
		[Range(0,1)] public float alphaCutoff = 0.3f;
		[Header("Shadows")]
		public Color shadowColor = new Color(0,0,0,0.25f);
		[Range(1,32)] public int shadowSteps = 3;
		[Header("Lightmap")]
		public Color lightmapColor = new Color(0,0,0,0.25f);
		[Range(1,32)] public int lightmapSteps = 3;
		[Header("Visibility")]
		public int cullDistance = 150;
		[Range(1,32)] public int fadeSteps = 3;
		public int fadeStartDistance = 80;
		public int fadeEndDistance = 100;
		public Color fadeStartColor = new Color(0,0,0,1);
		public Color fadeEndColor = new Color(0,0,0,0);
		public static ShaderOverrides Get(){return ShaderOverrides.instance;}
		public void OnEnable(){this.Setup();}
		public void Awake(){this.Setup();}
		public void Setup(){
			ShaderOverrides.instance = this;
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
		public void OnValidate(){
			if(!this.CanValidate()){return;}
			this.Setup();
			this.Update();
		}
	}
}