Shader "Zios/(Components)/Lighting/Diffuse Shading [Lerp]"{
	Properties{
		shadingColor("Diffuse Shading Color",Color) = (0.5,0.5,0.5,1)
		shadingCutoff("Diffuse Shading Cutoff",Range(0.0,1.0)) = 0
	}
	SubShader{
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			fixed4 shadingColor;
			float shadingCutoff;
			fixed shadingIgnoreCutoff;
			struct vertexInput{
				float4 vertex        : POSITION;
				float3 normal        : NORMAL;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float3 lightNormal	 : TEXCOORD0;
				float4 normal        : TEXCOORD1;
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
			vertexOutput setupLighting(vertexOutput input){
				input.lighting = saturate(dot(input.normal.xyz,input.lightNormal));
				return input;
			}
			vertexOutput setupLighting(float3 lightDirection,vertexOutput input){
				input.lighting = saturate(dot(lightDirection,input.lightNormal));
				return input;
			}
			pixelOutput applyDiffuseLerpShading(vertexOutput input,pixelOutput output,fixed4 shadingColor,float shadingCutoff){
				if(length(output.color.rgb) > shadingCutoff){
					float shadeValue = saturate(input.lighting+(1-shadingColor.a));
					output.color.rgb = lerp(shadingColor.rgb,output.color.rgb,shadeValue);
				}
				return output;
			}
			pixelOutput applyDiffuseLerpShading(vertexOutput input,pixelOutput output){
				return applyDiffuseLerpShading(input,output,shadingColor,shadingIgnoreCutoff);
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = UnityObjectToClipPos(input.vertex);
				output.lightNormal = ObjSpaceLightDir(input.vertex);
				output.normal = float4(input.normal,0);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				input = setupLighting(input);
				output = applyDiffuseLerpShading(input,output,shadingColor,shadingCutoff);
				return output;
			}
			ENDCG
		}
	}
}
