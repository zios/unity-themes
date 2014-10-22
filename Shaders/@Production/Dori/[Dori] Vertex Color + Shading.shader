Shader "Zios/Dori/Vertex Color + Shading"{
	Properties{
		shadeIntensity("Shade Intensity",Range(0,1)) = 0.5
		shades("Shades",float) = 3.0
		//shadingRamp("Shading Ramp",2D) = "white"{}
	}
	SubShader{
		Pass{
			Cull Off
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			fixed shadeIntensity;
			fixed shades;
			//sampler2D shadingRamp;
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
				shades = 1/shades;
				input.lighting = dot(input.normal,input.lightNormal) * 0.5 + 0.5;
				input.lighting = shades < 1 ? round(input.lighting / shades) * shades : 1;
				return input;
			}
			pixelOutput applyVertexColor(vertexOutput input,pixelOutput output){
				output.color.rgb = input.color;
				return output;
			}
			pixelOutput applyDiffuseLerpShading(vertexOutput input,pixelOutput output){
				float4 shadingColor = float4(input.color * shadeIntensity,1);
				output.color.rgb = lerp(shadingColor.rgb,output.color.rgb,input.lighting);
				//output.color.rgb = input.color * tex2D(shadingRamp,input.lighting);
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