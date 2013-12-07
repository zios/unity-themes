Shader "Zios/Lighting/Diffuse Shading (Lerp)"{
	Properties{
		shadingColor("Diffuse Shading Color",Color) = (0.5,0.5,0.5,1)
		shadingCutoff("Diffuse Shading Cutoff",Range(0.0,1.0)) = 0
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
			fixed4 shadingColor;
			float shadingCutoff;
			fixed shadingIgnoreCutoff;
			struct vertexInput{
				float4 vertex        : POSITION;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float  lighting      : TEXCOORD5;
			};
			struct pixelOutput{
				float4 color         : COLOR0;
			};
			pixelOutput setupPixel(vertexOutput input){
				pixelOutput output;
				output.color = float4(0,0,0,0);
				return output;
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
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				output = applyDiffuseLerpShading(input,output,shadingColor,shadingCutoff);
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				return output;
			}
			ENDCG
		}
	}
}
