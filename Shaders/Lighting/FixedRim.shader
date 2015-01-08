Shader "Zios/(Components)/Lighting/Fixed Rim"{
	Properties{
		rimSpread("Rim Spread",Range(-1.0,2.0)) = 1.0
		rimSoftness("Rim Softness",Range(0.0,20.0)) = 5.0
		rimAlpha("Rim Alpha",Range(0.0,1.0)) = 0.0
		rimColor("Rim Color",Color) = (1,1,1,1)
	}
	SubShader{
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			fixed4 rimColor;
			fixed rimAlpha;
			fixed rimSpread;
			fixed rimSoftness;
			fixed shadingSteps;
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
			pixelOutput applyFixedRim(vertexOutput input,pixelOutput output){
				rimColor = ((rimColor - 0.5) * 2) * rimColor.a;
				half rimPower = rimSpread - max(dot(input.normal,input.view),0.01);
				float stepSize = 1.0 / (shadingSteps-1);
				float rimPotency = ceil((rimPower / stepSize)-0.5) * stepSize;
				//half rimPotency = pow(rimPower,25/rimSoftness);
				output.color.rgb += rimPotency * rimColor;
				output.color.a -= rimPotency * rimAlpha;
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
				output = applyFixedRim(input,output);
				return output;
			}
			ENDCG
		}
	}
}