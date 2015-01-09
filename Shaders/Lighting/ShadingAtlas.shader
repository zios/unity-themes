Shader "Zios/(Components)/Lighting/Shading Atlas"{
	Properties{
		shadingID("Shading ID",Float) = 128
		shadingAtlas("Shading Atlas",2D) = "white"{}
		shadingSpread("Shading Spread",Float) = 0.5
		shadingIntensity("Shading Intensity",Float) = 0
	}
	SubShader{
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D shadingAtlas;
			fixed4 shadingAtlas_ST;
			fixed shadingIndex;
			struct vertexInput{
				float4 vertex        : POSITION;
				float3 normal        : NORMAL;
				float4 color         : COLOR;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float4 UV            : COLOR0;
				float3 lightNormal	 : TEXCOORD0;
				float4 normal        : TEXCOORD1;
				float3 view	         : TEXCOORD4;
				float  lighting      : TEXCOORD5;
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
			vertexOutput setupInput(vertexOutput input){
				input.normal.xyz = normalize(input.normal.xyz);
				input.lightNormal = normalize(input.lightNormal);
				input.view = normalize(input.view);
				return input;
			}
			vertexOutput setupLighting(vertexOutput input){
				input.lighting = saturate(dot(input.normal.xyz,input.lightNormal));
				return input;
			}
			vertexOutput setupLighting(float3 lightDirection,vertexOutput input){
				input.lighting = saturate(dot(lightDirection,input.lightNormal));
				return input;
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
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.lightNormal = ObjSpaceLightDir(input.vertex);
				output.normal = float4(input.normal,0);
				output.view = ObjSpaceViewDir(input.vertex);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				input = setupInput(input);
				input = setupLighting(input);
				output = applyShadingAtlas(input,output);
				return output;
			}
			ENDCG
		}
	}
}