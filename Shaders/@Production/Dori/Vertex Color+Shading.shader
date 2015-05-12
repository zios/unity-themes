Shader "Zios/Dori/Vertex Color + Shading"{
	Properties{
		contrast("Contrast",Range(-0.5,0.5)) = 0.2
		shadingRamp("Shading Ramp",2D) = "white"{}
		shadeIntensity("Shade Intensity",Range(0,1)) = 1
	}
	SubShader{
		Tags{"LightMode"="ForwardBase"}
		Pass{
			Cull Off
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			fixed shadeIntensity;
			fixed contrast;
			fixed4 _LightColor0;
			sampler2D shadingRamp;
			struct vertexInput{
				float4 vertex          : POSITION;
				float4 texcoord        : TEXCOORD0;
				float3 normal          : NORMAL;
				float4 color           : COLOR;
			};
			struct vertexOutput{
				float4 pos             : POSITION;
				float3 color    	   : COLOR;
				float4 normal          : NORMAL;
				float3 lightNormal	   : TEXCOORD0;
				float4 UV              : TEXCOORD2;
				float  lighting        : TEXCOORD3;
			};
			struct pixelOutput{
				float4 color           : COLOR0;
			};
			pixelOutput setupPixel(vertexOutput input){
				pixelOutput output;
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output.color = float4(0,0,0,0);
				return output;
			}
			vertexOutput setupHalfLighting(vertexOutput input){
				input.lighting = dot(input.normal,input.lightNormal) * 0.5f + 0.5f;
				return input;
			}
			pixelOutput applyVertexColor(vertexOutput input,pixelOutput output){
				output.color.rgb += input.color.rgb;
				return output;
			}
			pixelOutput applySceneAmbient(vertexOutput input,pixelOutput output){
				output.color.rgb += UNITY_LIGHTMODEL_AMBIENT.rgb;
				output.color.rgb *= _LightColor0.rgb;
				return output;
			}
			pixelOutput applyDiffuseLerpShading(vertexOutput input,pixelOutput output){
				output.color.rgb *= tex2D(shadingRamp,input.lighting) + (1-shadeIntensity);
				//output.color.rgb *= tex2D(shadingRamp,input.lighting);
				return output;
			}
			pixelOutput applyContrast(vertexOutput input,pixelOutput output){
				output.color.rgb = (output.color.rgb - 0.5f) * max(contrast + 1.0,0) + 0.5f;
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
				input.normal = normalize(input.normal);
				input.lightNormal = normalize(input.lightNormal);
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				input = setupHalfLighting(input);
				output = applyVertexColor(input,output);
				output = applyDiffuseLerpShading(input,output);
				output = applySceneAmbient(input,output);
				output = applyContrast(input,output);
				output.color.rgb *= 2;
				return output;
			}
			ENDCG
		}
	}
	//CustomEditor "ExtendedMaterialEditor"
}
