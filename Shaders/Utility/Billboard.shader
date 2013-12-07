Shader "Zios/Utility/Billboard"{
	Properties{
		diffuseMap("Diffuse Map",2D) = "white"{}
	}
	SubShader{
		Pass{
			CGPROGRAM
			#include "../Utility/Unity-CG.cginc"
			#include "../Utility/Unity-Light.cginc"
			#pragma vertex vertexPassBillboard
			#pragma fragment pixelPass
			#pragma multi_compile_fwdbase
			#pragma fragmentoption ARB_precision_hint_fastest
			float scale;
			sampler2D diffuseMap;
			fixed4 diffuseMap_ST;
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
				output.color = float4(0,0,0,0);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				output.color = tex2D(diffuseMap,input.UV.xy);
				return output;
			}
			vertexOutput vertexPassBillboard(vertexInput input){
				vertexOutput output;
				output.UV = float4(input.texcoord.xy,0.0f,0.0f);
				output.pos = mul(UNITY_MATRIX_P,mul(UNITY_MATRIX_MV, float4(0.0f, 0.0f, 0.0f, 1.0f)) + float4(input.vertex.x, input.vertex.y, 0.0f, 0.0f));
				return output;
			}
			ENDCG
		}
	}
}
