Shader "Zios/(Components)/Lighting/Specular"{
	Properties{
		specularSize("Specular Size",Float) = 1
		specularHardness("Specular Hardness",Range(0.01,1)) = 0.01
		specularColor("Specular Color",Color) = (1,1,1,1)
	}
	SubShader{
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			fixed4 specularColor;
			fixed specularSize;
			fixed specularHardness;
			float3 lightOffset;
			fixed shadingIndex;
			struct vertexInput{
				float4 vertex        : POSITION;
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
			pixelOutput applySpecularFull(vertexOutput input,pixelOutput output){
				float3 reflect = normalize(2*input.lighting*input.normal-input.lightNormal.xyz);
				float intensity = pow(saturate(dot(reflect,input.view)),10/specularSize);
				output.color.rgb += specularColor * intensity;
				return output;
			}
			pixelOutput applySpecular(vertexOutput input,pixelOutput output){
				float3 reflect = normalize(input.lightNormal + input.view);
				float intensity = pow(saturate(dot(input.normal,reflect)),10/specularSize);
				intensity = floor((intensity / specularHardness)+0.5) * specularHardness;
				output.color.rgb += specularColor * intensity;
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.lightNormal = ObjSpaceLightDir(input.vertex) + lightOffset;
				output.view = ObjSpaceViewDir(input.vertex);
				output.normal = float4(input.normal,0);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				input = setupInput(input);
				input = setupLighting(input);
				output = applySpecularFull(input,output);
				return output;
			}
			ENDCG
		}
	}
}