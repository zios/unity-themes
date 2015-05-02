Shader "Zios/SuperCity/Sprite + Particle (Additive)"{
	Properties{
		alpha("Alpha",Range(0.0,1.0)) = 1.0
		alphaCutoff("Alpha Cutoff",Range(0.0,1.0)) = 0
		diffuseMap("Diffuse Map",2D) = "white"{}
		diffuseColor("Diffuse Color",Color) = (1,1,1,1)
		diffuseCutoff("Diffuse Cutoff",Range(0,1)) = 0
		atlasUV("Atlas UV",Vector) = (0,0,1,1)
		atlasUVScale("Atlas UV Scale",Vector) = (1,1,0,0)
		paddingUV("Padding UV",Vector) = (0,0,1,1)
	}
	SubShader{
		Tags{"LightMode"="Always" "Queue"="Transparent"}
		ZWrite Off
		Cull Off
		Blend One One
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D diffuseMap;
			fixed4 diffuseMap_ST;
			fixed4 diffuseColor;
			fixed diffuseCutoff;
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
			pixelOutput applyIntensity(vertexOutput input,pixelOutput output,float intensity){
				output.color.rgb *= intensity;
				if(alphaCutoff == 0){alphaCutoff = alphaCutoffGlobal;}
				if(output.color.a <= alphaCutoff){clip(-1);}
				return output;
			}
			pixelOutput applyIntensity(vertexOutput input,pixelOutput output){
				return applyIntensity(input,output,intensity);
			}
			pixelOutput applyDiffuseColor(vertexOutput input,pixelOutput output,float cutoff){
				if(length(output.color.rgb) >= diffuseCutoff){
					output.color.rgb *= (diffuseColor.rgb * diffuseColor.a);
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
				input = setupPadding(input);
				input = setupAtlas(input);
				output = applyDiffuseMap(input,output);
				output = applyDiffuseColor(input,output,diffuseCutoff);
				output = applyIntensity(input,output,alpha);
				return output;
			}
			ENDCG
		}
	}
	CustomEditor "Zios.ExtendedMaterialEditor"
}