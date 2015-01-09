Shader "Hidden/Zios/(Components)/Lighting/CustomLighting"{
	Properties{
	}
	SubShader{
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			fixed4 lightDirection;
			fixed shadingSpread;
			fixed shadingContrast;
			struct vertexInput{
				float4 vertex        : POSITION;
				float3 normal        : NORMAL;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float3 lightNormal	 : TEXCOORD0;
				float4 normal        : TEXCOORD1;
				float  lighting      : TEXCOORD5;
			};
			struct pixelOutput{
				fixed4 color         : COLOR0;
			};
			pixelOutput setupPixel(vertexOutput input){
				pixelOutput output;
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output.color = float4(0,0,0,0);
				return output;
			}
			vertexOutput setupCustomLighting(vertexOutput input){
				input.lighting = saturate(dot(input.normal.xyz,input.lightNormal)*(1.0+shadingContrast))+shadingSpread;
				return input;
			}
			vertexOutput setupCustomLighting(float3 lightDirection,vertexOutput input){
				input.lighting = saturate(dot(input.normal.xyz,lightDirection)*(1.0+shadingContrast))+shadingSpread;
				return input;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.lightNormal = ObjSpaceLightDir(input.vertex);
				output.normal = float4(input.normal,0);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				input = setupCustomLighting(input);
				return output;
			}
			ENDCG
		}
	}
}