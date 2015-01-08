Shader "Zios/ZEQ2/Terrain"{
	Properties{
		shadowColor("Shadow Color", Color) = (0.0,0.0,0.0,1.0)
		diffuseColor("Diffuse Color", Color) = (0.5,0.5,0.5,1.0)
		diffuseMap("Diffuse Map",2D) = "white"{}
		diffuseCutoff("Diffuse Cut Off",Range(0,1)) = 0
	}
	SubShader{
		LOD 200
		UsePass "Hidden/Zios/Shadow Pass/Diffuse Map/SHADOWCASTER"
		Pass{
			Tags{"LightMode"="ForwardBase"}
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma multi_compile_fwdbase
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D unity_Lightmap;
			sampler2D diffuseMap;
			fixed diffuseCutoff;
			fixed4 diffuseColor;
			fixed4 diffuseMap_ST;
			fixed4 unity_LightmapST;
			fixed4 shadowColor;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 texcoord      : TEXCOORD0;
				float4 texcoord1     : TEXCOORD1;
				float4 color         : COLOR;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float4 UV            : COLOR0;
				LIGHTING_COORDS(6,7)
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
			pixelOutput applyLightMap(vertexOutput input,pixelOutput output){
				output.color.rgb *= DecodeLightmap(tex2D(unity_Lightmap,input.UV.zw)) + shadowColor;
				return output;
			}
			pixelOutput applyDiffuseMap(vertexOutput input,pixelOutput output){
				output.color += tex2D(diffuseMap,TRANSFORM_TEX(input.UV.xy,diffuseMap));
				return output;
			}
			pixelOutput applyDiffuseColor(vertexOutput input,pixelOutput output,float cutoff){
				if(length(output.color.rgb) >= diffuseCutoff){
					output.color.rgb *= (diffuseColor.rgb * diffuseColor.a);
				}
				return output;
			}
			pixelOutput applyShadows(vertexOutput input,pixelOutput output){
				output.color.rgb *= LIGHT_ATTENUATION(input) + shadowColor;
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				float2 lightmapUV = input.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.UV = float4(input.texcoord.x,input.texcoord.y,lightmapUV.x,lightmapUV.y);
				TRANSFER_VERTEX_TO_FRAGMENT(output);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output = applyDiffuseMap(input,output);
				output = applyLightMap(input,output);
				output = applyDiffuseColor(input,output,diffuseCutoff);
				output = applyShadows(input,output);
				return output;
			}
			ENDCG
		}
	}
	Fallback "Hidden/Zios/Fallback/Vertex Lit"
}