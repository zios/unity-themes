Shader "Zios/Lighting/Diffuse Shading (Additive)"{
	Properties{
		shadingColor("Diffuse Shading Color",Color) = (0.5,0.5,0.5,1)
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
				pixelOutput applyDiffuseAddShading(vertexOutput input,pixelOutput output){
				output.color.rgb += input.lighting * (shadingColor.rgb * shadingColor.a);
				output.color.a = 1;
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				output = applyDiffuseAddShading(input,output);
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
