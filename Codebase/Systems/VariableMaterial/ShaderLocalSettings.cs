using UnityEngine;
using System;
using System.Collections.Generic;
namespace Zios{
	[AddComponentMenu("Zios/Components/Shader Settings (Local)")][ExecuteInEditMode]
	public class ShaderLocalSettings : MonoBehaviour{
		public static ShaderLocalSettings instance;
		[Header("General")]
		[Range(0,1)] public float alphaCutoff = 0.3f;
		[Header("Shadows")]
		public Color shadowColor = new Color(0,0,0,0.25f);
		[Range(1,32)] public int shadowSteps = 3;
		[Header("Visibility")]
		public int cullDistance = 150;
		[Range(1,32)] public int fadeSteps = 3;
		public int fadeStartDistance = 80;
		public int fadeEndDistance = 100;
		public Color fadeStartColor = new Color(0,0,0,1);
		public Color fadeEndColor = new Color(0,0,0,0);
		public static ShaderLocalSettings Get(){return ShaderLocalSettings.instance;}
		public void OnEnable(){this.Setup();}
		public void Awake(){this.Setup();}
		public void Setup(){
			ShaderLocalSettings.instance = this;
			this.cullDistance = Math.Max(0,this.cullDistance);
			this.fadeStartDistance = Math.Max(0,Math.Min(this.fadeStartDistance,this.fadeEndDistance));
			this.fadeEndDistance = Math.Max(this.fadeStartDistance,this.fadeEndDistance);
			Shader.SetGlobalFloat("globalAlphaCutoff",this.alphaCutoff);
			Shader.SetGlobalColor("globalShadowColor",this.shadowColor);
			Shader.SetGlobalFloat("globalShadowSteps",this.shadowSteps);
			Shader.SetGlobalFloat("cullDistance",this.cullDistance);
			Shader.SetGlobalFloat("fadeSteps",this.fadeSteps);
			Shader.SetGlobalFloat("fadeStartDistance",this.fadeStartDistance);
			Shader.SetGlobalFloat("fadeEndDistance",this.fadeEndDistance);
			Shader.SetGlobalColor("fadeStartColor",this.fadeStartColor);
			Shader.SetGlobalColor("fadeEndColor",this.fadeEndColor);
		}
		public void Update(){
			Shader.SetGlobalFloat("timeConstant",(Time.realtimeSinceStartup));
		}
		public void OnValidate(){
			if(!this.CanValidate()){return;}
			this.Setup();
			this.Update();
		}
	}
}