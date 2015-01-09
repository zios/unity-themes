Shader "Zios/SuperCity/Sprite + Particle (Multiply)"{
	Properties{
		alpha("Alpha",Range(0.0,1.0)) = 1.0
		alphaCutoff("Alpha Cutoff",Range(0.0,1.0)) = 0
		diffuseMap("Diffuse Map",2D) = "white"{}
		ambientColor("Ambient Color",Color) = (0,0,0,0)
		ambientCutoff("Ambient Cutoff",Range(0,1)) = 0.8
		atlasUV("Atlas UV",Vector) = (0,0,1,1)
		atlasUVScale("Atlas UV Scale",Vector) = (1,1,0,0)
		paddingUV("Padding UV",Vector) = (0,0,1,1)
	}
	SubShader{
		Tags{"LightMode"="Always" "Queue"="Transparent"}
		ZWrite Off
		Cull Off
		Blend DstColor Zero
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D diffuseMap;
			fixed4 diffuseMap_ST;
			fixed4 meshNormal;
			fixed4 ambientColor;
			fixed ambientCutoff;
			fixed alphaCutoff;
			fixed alphaCutoffGlobal;
			fixed alpha;
			fixed4 atlasUV;
			fixed4 atlasUVScale;
			fixed4 paddingUV;
			float2 clampRange(float2 min,float2 max,float2 value){return saturate((value-min)/(max-min));}
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
			vertexOutput setupPadding(vertexOutput input){
				input.UV.xy = clampRange(paddingUV.xy,paddingUV.zw,input.UV.xy);
				return input;
			}
			vertexOutput setupAtlas(vertexOutput input){
				input.UV.xy = lerp(atlasUV.xy,atlasUV.zw,fmod(input.UV.xy*atlasUVScale.xy,1));
				return input;
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
				output.lightNormal = ObjSpaceLightDir(input.vertex);
				output.UV.xy = float2(input.texcoord.x,input.texcoord.y);
				output.normal = float4(input.normal+meshNormal.xyz,0);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				input = setupPadding(input);
				input = setupAtlas(input);
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