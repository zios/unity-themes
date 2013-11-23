Shader "Zios/Vertex Outlines"{
	Properties{
		outlineColor("Outline Color",Color) = (0.0,0.0,0.0,1.0)
		outlineLength("Outline Length",float) = 0.09
		outlineShrinkStart("Outline Shrink Start",float) = 50.0
		outlineShrinkEnd("Outline Shrink End",float) = 1.0
	}
	SubShader{
		Tags{"LightMode"="ForwardBase"}
		Pass{
			Name "Normal"
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Front
			CGPROGRAM
			#include "./Unity-CG.cginc"
			#include "./Unity-Light.cginc"
			#include "./Shared.cginc"
			#pragma vertex vertexPassOutline
			#pragma fragment pixelPassOutline
			#pragma multi_compile_fwdbase
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG
		}
		Pass{
			Name "DiffuseMap"
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Front
			CGPROGRAM
			#include "./Unity-CG.cginc"
			#include "./Unity-Light.cginc"
			#include "./Shared.cginc"
			#pragma vertex vertexPassOutline
			#pragma fragment pixelPassBlendOutline
			#pragma multi_compile_fwdbase
			#pragma fragmentoption ARB_precision_hint_fastest
			pixelOutput pixelPassBlendOutline(vertexOutput input){
				pixelOutput output;
				output.color = lerp(tex2D(diffuseMap,TRANSFORM_TEX(input.UV.xy,diffuseMap)),outlineColor,outlineColor.a);
				return output;
			}
			ENDCG
		}
	}
}

