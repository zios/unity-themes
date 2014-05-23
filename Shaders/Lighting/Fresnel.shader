Shader "Zios/Lighting/Fresnel"{
	Properties{
		fresnelSize("Fresnel Size",Float) = 2.0
		fresnelIntensity("Fresnel Intensity",Float) = 6.0
		fresnelColor("Fresnel Color",Color) = (1,1,1,1)
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
			fixed fresnelSize;
			fixed fresnelIntensity;
			fixed4 fresnelColor;
			struct vertexInput{
				float4 vertex        : POSITION;
				float3 normal        : NORMAL;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float4 UV            : COLOR0;
				float3 lightNormal	 : TEXCOORD0;
				float4 normal        : TEXCOORD1;
				float3 view	         : TEXCOORD4;
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
			pixelOutput applyFresnel(vertexOutput input,pixelOutput output){
				float4 specularDot = dot(input.lightNormal,input.view);
				float3 fresnelDot = saturate(dot(input.view,input.normal));
				float fresnelStrength = saturate(1 - fresnelDot) * 10/fresnelIntensity + 1;
				float fresnelLight = fresnelDot * 10/fresnelSize + 1;
				output.color.rgb += pow(saturate(specularDot),fresnelLight) * fresnelStrength * fresnelColor;
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				output = applyFresnel(input,output);
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