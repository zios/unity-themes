Shader "Zios/Component/Stepped Shading [Blend]"{
	Properties{
		shadingSteps("Shading Steps",float) = 3.0
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
			fixed shadingSteps;
			fixed4 shadingColor;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 color         : COLOR;
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
			vertexOutput setupSteppedLighting(vertexOutput input,float shadingSteps){
				input = setupLighting(input);
				float stepSize = shadingSteps;
				input.lighting = ceil((input.lighting / stepSize)-0.5) * stepSize;
				return input;
			}
			vertexOutput setupSteppedLighting(vertexOutput input){
				return setupSteppedLighting(input,1.0 / (shadingSteps-1));
			}
			pixelOutput applyDiffuseBlendShading(vertexOutput input,pixelOutput output){
				output.color.rgb *= input.lighting * (shadingColor.rgb * shadingColor.a);
				output.color.a = 1;
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				input = setupSteppedLighting(input);
				output = applyDiffuseBlendShading(input,output);
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
	Fallback "Zios/Component/Shadow Pass/Basic"
}
