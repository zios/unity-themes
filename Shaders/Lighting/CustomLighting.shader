Shader "Hidden/Zios/Lighting/CustomLighting"{
	Properties{
	}
		SubShader{
		Pass{
			CGPROGRAM
			#include "../Utility/Unity-CG.cginc"
			#include "../Utility/Unity-Light.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma multi_compile_fwdbase
			#pragma fragmentoption ARB_precision_hint_fastest
			fixed4 lightDirection;
			fixed shadingSpread;
			fixed shadingContrast;
			struct vertexInput{
				float4 vertex        : POSITION;
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
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				input = setupCustomLighting(input);
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				return output;
			}
			ENDCG
		}
	}
}