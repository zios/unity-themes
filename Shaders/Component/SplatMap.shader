Shader "Zios/Component/Splat Map"{
	Properties{
		splatMap("Splat Map",2D) = "white"{}
		blendMapRed("Blend Texture Red",2D) = "white"{}
		blendMapGreen("Blend Texture Green",2D) = "white"{}
		blendMapBlue("Blend Texture Blue",2D) = "white"{}
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
			sampler2D splatMap;
			fixed4 splatMap_ST;
			sampler2D blendMapRed;
			sampler2D blendMapGreen;
			sampler2D blendMapBlue;
			fixed4 blendMapRed_ST;
			fixed4 blendMapGreen_ST;
			fixed4 blendMapBlue_ST;
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
			pixelOutput applySplatMap(vertexOutput input,pixelOutput output){
				fixed4 splat = tex2D(splatMap,TRANSFORM_TEX(input.UV.xy,splatMap));
				output.color += tex2D(blendMapRed,TRANSFORM_TEX(input.UV.xy,blendMapRed)) * splat.r;
				output.color += tex2D(blendMapGreen,TRANSFORM_TEX(input.UV.xy,blendMapGreen)) * splat.g;
				output.color += tex2D(blendMapBlue,TRANSFORM_TEX(input.UV.xy,blendMapBlue)) * splat.b;
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				output = applySplatMap(input,output);
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
