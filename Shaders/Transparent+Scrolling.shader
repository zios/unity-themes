Shader "Zios/Transparent + Scrolling"{
	Properties{
		diffuseMap("Diffuse Map",2D) = "white"{}
		lerpColor("Lerp Color",Color) = (0,0,0,0)
		lerpCutoff("Lerp Cutoff",Range(0,1)) = 0.8
		UVScrollX("UV Scroll X",Float) = 0
		UVScrollY("UV Scroll Y",Float) = 0
	}
	SubShader{
		Tags{"LightMode"="ForwardBase" "Queue"="Transparent+2"}
		ZWrite Off
		Pass{
			AlphaTest Greater 0
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#include "./Utility/Unity-CG.cginc"
			#include "./Utility/Unity-Light.cginc"
			#include "./Utility/Shared.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma multi_compile_fwdbase
			#pragma fragmentoption ARB_precision_hint_fastest
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				input = setupInput(input);
				input = setupUVScroll(input,timeConstant);
				input = setupLighting(input);
				output = applyDiffuseMap(input,output);
				output = applyLerpColor(input,output);
				return output;
			}
			ENDCG
		}
	}
}
