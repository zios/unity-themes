Shader "Zios/Standalone/Billboard"{
	Properties{
		shadowColor("Shadow Color", Color) = (0.0,0.0,0.0,1.0)
		diffuseColor("Diffuse Color", Color) = (0.5,0.5,0.5,1)
		diffuseMap("Diffuse Map",2D) = "white"{}
		alphaCutoff("Alpha Cutoff",Range(0.001,0.999)) = 0.0
		diffuseCutoff("Diffuse Cut Off",Range(0,1)) = 0
	}
	SubShader{
		LOD 200
		Pass{
			Tags{"LightMode"="ForwardBase"}
			Alphatest Greater [alphaCutoff]
			CGPROGRAM
			#include "./Utility/Unity-CG.cginc"
			#include "./Utility/Unity-Light.cginc"
			#include "./Utility/Shared.cginc"
			#pragma vertex vertexStep
			#pragma fragment pixelPass
			#pragma multi_compile_fwdbase
			#pragma fragmentoption ARB_precision_hint_fastest
			vertexOutput vertexStep(vertexInput input){
				vertexOutput output = vertexPass(input);
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				TRANSFER_VERTEX_TO_FRAGMENT(output);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				output = applyDiffuseMap(input,output);
				output = applyDiffuseColor(input,output,diffuseCutoff);
				output = applyShadows(input,output);
				return output;
			}
			ENDCG
		}
	}
	Fallback "Zios/Fallback/CutoutVertexLit"
}
