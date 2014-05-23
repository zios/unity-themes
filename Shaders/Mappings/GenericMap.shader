Shader "Zios/Mappings/Generic Map"{
	Properties{
		genericMap("Texture",2D) = "white"{}
	}
	SubShader{
		Pass{
			CGPROGRAM
			#include "../Utility/Unity-CG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D genericMap;
			fixed4 genericMap_ST;
			fixed4 unity_LightmapST;
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
			pixelOutput applyTexture(sampler2D genericMap,vertexOutput input,pixelOutput output){
				output.color = tex2D(genericMap,TRANSFORM_TEX(input.UV.xy,genericMap));
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				output = applyTexture(genericMap,input,output);
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.UV = float4(input.texcoord.xy,0,0);
				return output;
			}
			ENDCG
		}
	}
}
