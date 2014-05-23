Shader "Zios/Mappings/Shadow Lightmap"{
	Properties{
	}
	SubShader{
		Pass{
			CGPROGRAM
			#include "../Utility/Unity-CG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			fixed4 shadowColor;
			fixed4 unity_LightmapST;
			sampler2D unity_Lightmap;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 texcoord1     : TEXCOORD1;
				float4 color         : COLOR;
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
			pixelOutput applyShadowLightMap(vertexOutput input,pixelOutput output){
				output.color.rgb *= saturate(DecodeLightmap(tex2D(unity_Lightmap,input.UV.zw))) + shadowColor;
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				output = applyShadowLightMap(input,output);
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				float2 lightmapUV = input.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				return output;
			}
			ENDCG
		}
	}
}
