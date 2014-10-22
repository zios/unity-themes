Shader "Zios/SuperCity/Sprite + Particle + Non-Atlas"{
	Properties{
		alpha("Alpha",Range(0.0,1.0)) = 1.0
		alphaCutoff("Alpha Cutoff",Range(0.0,1.0)) = 0
		diffuseMap("Diffuse Map",2D) = "white"{}
		ambientColor("Ambient Color",Color) = (0,0,0,0)
		ambientCutoff("Ambient Cutoff",Range(0,1)) = 0.8
	}
	SubShader{
		Tags{"LightMode"="Always" "Queue"="Transparent"}
		ZWrite Off
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D diffuseMap;
			fixed4 diffuseMap_ST;
			fixed4 ambientColor;
			fixed ambientCutoff;
			fixed alphaCutoff;
			fixed alphaCutoffGlobal;
			fixed alpha;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 texcoord      : TEXCOORD0;
				float3 normal        : NORMAL;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float3 lightNormal	 : TEXCOORD0;
				float4 normal        : TEXCOORD1;
				float3 view	         : TEXCOORD4;
				float  lighting      : TEXCOORD5;
				float4 UV            : COLOR0;
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
			pixelOutput applyDiffuseMap(vertexOutput input,pixelOutput output){
				output.color += tex2D(diffuseMap,TRANSFORM_TEX(input.UV.xy,diffuseMap));
				return output;
			}
			pixelOutput applyAlphaSimple(vertexOutput input,pixelOutput output){
				output.color.a *= alpha;
				return output;
			}
			pixelOutput applyAmbientColor(vertexOutput input,pixelOutput output){
				output.color.rgb += (ambientColor * ambientColor.a);
				return output;
			}
			pixelOutput applyAmbientColor(vertexOutput input,pixelOutput output,float cutoff){
				if(length(output.color.rgb) >= ambientCutoff){
					output.color.rgb += (ambientColor * ambientColor.a);
				}
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.UV = input.texcoord;
				output.lightNormal = ObjSpaceLightDir(input.vertex);
				output.view = ObjSpaceViewDir(input.vertex);
				output.normal = float4(input.normal,0);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output = applyDiffuseMap(input,output);
				output = applyAmbientColor(input,output,ambientCutoff);
				output = applyAlphaSimple(input,output);
				return output;
			}
			ENDCG
		}
	}
	CustomEditor "ExtendedMaterialEditor"
}