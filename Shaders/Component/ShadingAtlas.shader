Shader "Zios/Component/Shading Atlas"{
	Properties{
		shadingID("Shading ID",Float) = 128
		shadingAtlas("Shading Atlas",2D) = "white"{}
		shadingSpread("Shading Spread",Float) = 0.5
		shadingIntensity("Shading Intensity",Float) = 0
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
			sampler2D shadingAtlas;
			fixed4 shadingAtlas_ST;
			fixed shadingIndex;
			sampler2D indexMap;
			fixed4 indexMap_ST;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 texcoord		 : TEXCOORD0;
				float4 color         : COLOR;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float4 UV            : COLOR0;
				float4 normal        : TEXCOORD1;
				float  lighting      : TEXCOORD5;
				LIGHTING_COORDS(6,7)
			};
			struct pixelOutput{
				float4 color         : COLOR0;
			};
			pixelOutput setupPixel(vertexOutput input){
				pixelOutput output;
				output.color = float4(0,0,0,0);
				return output;
			}
			pixelOutput applyShadingAtlas(float shadeRow,vertexOutput input,pixelOutput output){
				float2 shading = float2(input.lighting,shadeRow);
				fixed4 lookup = tex2D(shadingAtlas,shading);
				shadingIndex = shadeRow;
				if(shadeRow == 0){clip(-1);}
				output.color.rgb += lookup.rgb * lookup.a;
				output.color.a = lookup.a;
				return output;
			}
			pixelOutput applyShadingAtlas(sampler2D indexMap,vertexOutput input,pixelOutput output){
				float shadeRow = 1.0 - tex2D(indexMap,TRANSFORM_TEX(input.UV.xy,indexMap)).r;
				output = applyShadingAtlas(shadeRow,input,output);
				return output;
			}
			pixelOutput applyShadingAtlas(vertexOutput input,pixelOutput output){
				float shadeRow = 1.0 - input.normal.a;
				output = applyShadingAtlas(shadeRow,input,output);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				output = applyShadingAtlas(indexMap,input,output);
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
