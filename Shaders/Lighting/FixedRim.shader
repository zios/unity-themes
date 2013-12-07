Shader "Zios/Lighting/Fixed Rim"{
	Properties{
		rimSpread("Rim Spread",Range(-1.0,2.0)) = 1.0
		rimSoftness("Rim Softness",Range(0.0,20.0)) = 5.0
		rimAlpha("Rim Alpha",Range(0.0,1.0)) = 0.0
		rimColor("Rim Color",Color) = (1,1,1,1)
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
			fixed4 rimColor;
			fixed rimAlpha;
			fixed rimSpread;
			fixed rimSoftness;
			fixed shadingSteps;
			struct vertexInput{
				float4 vertex        : POSITION;
				float3 normal        : NORMAL;
				float4 color         : COLOR;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float4 normal        : TEXCOORD1;
				float3 view	         : TEXCOORD4;
			};
			struct pixelOutput{
				float4 color         : COLOR0;
			};
			pixelOutput setupPixel(vertexOutput input){
				pixelOutput output;
				output.color = float4(0,0,0,0);
				return output;
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
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				output = applyFixedRim(input,output);
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.normal = float4(input.normal,0);
				return output;
			}
			ENDCG
		}
	}
}
