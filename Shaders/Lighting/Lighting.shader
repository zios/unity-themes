Shader "Hidden/Zios/Lighting/Lighting"{
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
			vertexOutput setupLighting(vertexOutput input){
				input.lighting = saturate(dot(input.normal.xyz,input.lightNormal));
				return input;
			}
			vertexOutput setupLighting(float3 lightDirection,vertexOutput input){
				input.lighting = saturate(dot(lightDirection,input.lightNormal));
				return input;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				input = setupLighting(input);
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