Shader "Zios/(Components)/Lighting/Stepped Shading [Blend]"{
	Properties{
		shadingSteps("Shading Steps",float) = 3.0
	}
	SubShader{
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			fixed shadingIndex;
			fixed shadingSteps;
			fixed4 shadingColor;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 texcoord      : TEXCOORD0;
				float3 normal        : NORMAL;
				float4 color         : COLOR;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
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
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.lightNormal = ObjSpaceLightDir(input.vertex);
				output.view = ObjSpaceViewDir(input.vertex);
				output.normal = float4(input.normal,0);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				input = setupInput(input);
				input = setupLighting(input);
				input = setupSteppedLighting(input);
				output = applyDiffuseBlendShading(input,output);
				return output;
			}
			ENDCG
		}
	}
}