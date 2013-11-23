Shader "Zios/Mesh"{
	Properties{
		alpha("Alpha",Range(0.0,1.0)) = 1.0
		diffuseMap("Diffuse Map",2D) = "white"{}
		normalMap("Normal Map",2D) = "white"{}
		lerpColor("Lerp Color",Color) = (0,0,0,0)
		lerpCutoff("Lerp Cutoff",Range(0,1)) = 0.8
		shadingColor("Shading Color",Color) = (0.11,0,0.11,0)
		shadingSteps("Shading Steps",float) = 3.0
		shadingIgnoreCutoff("Shading Ignore Cutoff",Range(0,1)) = 0.3
		atlasUV("Atlas UV",Vector) = (0,0,1,1)
		atlasUVScale("Atlas UV Scale",Vector) = (1,1,0,0)
		lightOffset("Light Offset",Vector) = (0,0,0,0)
	}
	SubShader{
		Tags{"LightMode"="ForwardBase" "Queue"="Transparent-1"}
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
				input = setupAtlas(input);
				output = applyDiffuseMap(input,output);
				output = applyLerpColor(input,output);
				output = applyDiffuseLerpShading(input,output);
				output = applyAlpha(input,output);
				return output;
			}
			ENDCG
		}
	}
}
