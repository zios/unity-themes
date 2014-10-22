Shader "Zios/Dori/Vertex Color + Outline + Shading"{
	Properties{
		outlineSize("Outline Size",Range(0.002,0.01)) = 0.005
		outlineIntensity("Outline Intensity",Range(0,1)) = 0.8
		ambientIntensity("Ambient Intensity",Range(0,1)) = 0.2
		//shadeIntensity("Shade Intensity",Range(-0.5,0.5)) = 0.5
		//shades("Shades",float) = 3.0
		shadingRamp("Shading Ramp",2D) = "white"{}
	}
	SubShader{
		Pass{
			Cull Front
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			fixed outlineIntensity;
			fixed outlineSize;
			struct vertexInput{
				float4 vertex        : POSITION;
				float3 normal        : NORMAL;
				float4 color         : COLOR;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float4 color    	 : COLOR;
			};
			struct pixelOutput{
				float4 color         : COLOR0;
			};
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				float3 normal = mul((float3x3)UNITY_MATRIX_IT_MV,input.normal);
				float2 offset = TransformViewToProjection(normal.xy);
				output.pos.xy += offset * output.pos.z * outlineSize;
				output.color = input.color * outlineIntensity;
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output;
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output.color = input.color;
				return output;
			}
			ENDCG
		}
		Pass{
			Cull Off
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			fixed shadeIntensity;
			fixed shades;
			fixed ambientIntensity;
			sampler2D shadingRamp;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 texcoord      : TEXCOORD0;
				float3 normal        : NORMAL;
				float4 color         : COLOR;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float3 color    	 : COLOR;
				float3 lightNormal	 : TEXCOORD0;
				float4 normal        : TEXCOORD1;
				float4 UV            : TEXCOORD2;
				float  lighting      : TEXCOORD3;
			};
			struct pixelOutput{
				float4 color         : COLOR0;
			};
			pixelOutput setupPixel(vertexOutput input){
				pixelOutput output;
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output.color = float4(0,0,0,0);
				return output;
			}
			vertexOutput setupHalfLighting(vertexOutput input){
				//shades = 1/shades;
				input.lighting = dot(input.normal,input.lightNormal) * 0.5 + 0.5;
				//input.lighting = shades < 1 ? round(input.lighting / shades) * shades : 1;
				return input;
			}
			pixelOutput applyVertexColor(vertexOutput input,pixelOutput output){
				output.color.rgb = input.color;
				return output;
			}
			pixelOutput applyDiffuseLerpShading(vertexOutput input,pixelOutput output){
				//float4 shadingColor = float4(input.color * shadeIntensity,1);
				float3 shadingColor = lerp(input.color.rgb,input.color.rgb*2,ambientIntensity);
				//output.color.rgb = lerp(shadingColor.rgb,output.color.rgb,input.lighting);
				output.color.rgb = shadingColor * tex2D(shadingRamp,input.lighting);
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.UV = float4(input.texcoord.xy,0,0);
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.color = input.color;
				output.normal = float4(input.normal,0);
				output.lightNormal = ObjSpaceLightDir(input.vertex);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				input = setupHalfLighting(input);
				output = applyVertexColor(input,output);
				output = applyDiffuseLerpShading(input,output);
				return output;
			}
			ENDCG
		}
	}
}