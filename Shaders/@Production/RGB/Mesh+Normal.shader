Shader "Zios/RGB/Mesh + Normal"{
	Properties{
		diffuseMap("Diffuse Map",2D) = "white"{}
		normalMap("Normal Map",2D) = "black"{}
		normalMapIntensity("Normal Map Intensity",Range(0,20)) = 2
		shadingColor("Shading Color",Color) = (0.5,0.5,0.5,1)
		shadowColor("Shadow Color", Color) = (0.0,0.0,0.0,1.0)
	}
	SubShader{
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
			sampler2D diffuseMap;
			fixed4 diffuseMap_ST;
			sampler2D normalMap;
			fixed normalMapIntensity;
			fixed normalMapPower;
			fixed4 normalMap_ST;
			fixed4 shadingColor;
			fixed4 shadowColor;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 texcoord      : TEXCOORD0;
				float4 texcoord1     : TEXCOORD1;
				float3 normal        : NORMAL;
				float4 color         : COLOR;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float4 UV            : COLOR0;
				float3 lightNormal	 : TEXCOORD0;
				float4 normal        : TEXCOORD1;
				float4 tangent       : TEXCOORD2;
				float  lighting      : TEXCOORD3;
				LIGHTING_COORDS(4,5)
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
			vertexOutput setupLighting(vertexOutput input){
				input.lighting = saturate(dot(input.normal.xyz,input.lightNormal));
				return input;
			}
			vertexOutput setupLighting(float3 lightDirection,vertexOutput input){
				input.lighting = saturate(dot(lightDirection,input.lightNormal));
				return input;
			}
			vertexOutput setupTangentSpace(vertexOutput input){
				float3 binormal = cross(input.normal.xyz,input.tangent.xyz) * input.tangent.w;
				float3x3 tangentRotate = float3x3(input.tangent.xyz,binormal,input.normal.xyz);
				input.lightNormal = mul(tangentRotate,input.lightNormal);
				return input;
			}
			vertexOutput setupNormalMap(vertexOutput input){
				input = setupTangentSpace(input);
				fixed4 lookup = tex2D(normalMap,TRANSFORM_TEX(input.UV.xy,normalMap));
				fixed offset = (normalMapIntensity-1) - (normalMapIntensity*0.5);
				input.normal.xyz = (lookup.rgb*normalMapIntensity) - offset;
				input.normal.w = lookup.a;
				return input;
			}
			pixelOutput applyShading(vertexOutput input,pixelOutput output){
				fixed lighting = 1+input.lighting-shadingColor.a;
				output.color = lerp(shadingColor,output.color,saturate(lighting));
				return output;
			}
			pixelOutput applyDiffuseMap(vertexOutput input,pixelOutput output){
				output.color += tex2D(diffuseMap,TRANSFORM_TEX(input.UV.xy,diffuseMap));
				return output;
			}
			pixelOutput applyShadows(vertexOutput input,pixelOutput output){
				fixed darkness = 1+LIGHT_ATTENUATION(input)-shadowColor.a;
				output.color = lerp(shadowColor,output.color,darkness);
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.UV = float4(input.texcoord.xy,0,0);
				output.lightNormal = ObjSpaceLightDir(input.vertex);
				output.normal = float4(input.normal,0);
				TRANSFER_VERTEX_TO_FRAGMENT(output);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				input = setupNormalMap(input);
				input = setupLighting(input);
				output = applyDiffuseMap(input,output);
				output = applyShading(input,output);
				output = applyShadows(input,output);
				return output;
			}
			ENDCG
		}
	}
	Fallback "Hidden/Zios/Fallback/Vertex Lit"
}

 