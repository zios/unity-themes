Shader "Zios/Utility/Atlas Padding"{
	Properties{
		paddingUV("Padding UV",Vector) = (0,0,0,0)
	}
	SubShader{
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma multi_compile_fwdbase
			#pragma fragmentoption ARB_precision_hint_fastest
			fixed4 paddingUV;
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
			float2 clampRange(float2 min,float2 max,float2 value){return saturate((value-min)/(max-min));}
			vertexOutput setupPadding(vertexOutput input){
				input.UV.xy = clampRange(paddingUV.xy,paddingUV.zw,input.UV.xy);
				return input;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				input = setupPadding(input);
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.UV = float4(input.texcoord.x,input.texcoord.y,0,0);
				return output;
			}
			ENDCG
		}
	}
}
