Shader "Zios/(Components)/Utility/Billboard"{
	Properties{
		diffuseMap("Diffuse Map",2D) = "white"{}
	}
	SubShader{
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 texcoord      : TEXCOORD0;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float4 UV            : COLOR0;
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
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = mul(UNITY_MATRIX_P, UnityObjectToViewPos(float4(0.0f,0.0f,0.0f,1.0f))+float4(input.vertex.x,input.vertex.y,0.0f,0.0f));
				output.UV = float4(input.texcoord.xy,0.0f,0.0f);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				return output;
			}
			ENDCG
		}
	}
}