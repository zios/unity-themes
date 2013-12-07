Shader "Zios/Mappings/Light Map"{
	Properties{
		shadowColor("Shadow Color", Color) = (0.0,0.0,0.0,1.0)
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
			sampler2D unity_Lightmap;
			fixed4 unity_LightmapST;
			fixed4 shadowColor;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 texcoord      : TEXCOORD0;
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
			pixelOutput applyLightMap(vertexOutput input,pixelOutput output){
				output.color.rgb *= DecodeLightmap(tex2D(unity_Lightmap,input.UV.zw)) + shadowColor;
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				output = applyLightMap(input,output);
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				float2 lightmapUV = input.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.UV = float4(input.texcoord.x,input.texcoord.y,lightmapUV.x,lightmapUV.y);
				return output;
			}
			ENDCG
		}
	}
}
