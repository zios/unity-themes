Shader "Hidden/Zios/Sprite + Embedded"{
	Properties{
		diffuseMap("Diffuse Map",2D) = "white"{}
		normalMap("Normal Map",2D) = "white"{}
	}
	SubShader{
		Tags{"LightMode"="Always" "Queue"="Transparent"}
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma target 3.0
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D diffuseMap;
			fixed4 diffuseMap_ST;
			fixed4 lerpColor;
			fixed lerpCutoff;
			sampler2D normalMap;
			fixed4 normalMap_ST;
			fixed normalMapSpread;
			fixed normalMapContrast;
			float shadingSteps;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 texcoord      : TEXCOORD0;
				float3 normal        : NORMAL;
				float4 color         : COLOR;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float4 UV            : COLOR0;
				float3 lightNormal	 : TEXCOORD0;
				float4 normal        : TEXCOORD1;
				float4 tangent       : TEXCOORD2;
			    float4 original      : TEXCOORD3;
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
				input.normal.xyz = (lookup.rgb*2)-1.0;
				input.normal.w = lookup.a;
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
			pixelOutput applyDiffuseLerpShading(vertexOutput input,pixelOutput output,fixed4 shadingColor,float shadingCutoff){
				if(length(output.color.rgb) > shadingCutoff){
					float shadeValue = saturate(input.lighting+(1-shadingColor.a));
					output.color.rgb = lerp(shadingColor.rgb,output.color.rgb,shadeValue);
				}
				return output;
			}
			pixelOutput applyDiffuseMap(vertexOutput input,pixelOutput output){
				output.color += tex2D(diffuseMap,TRANSFORM_TEX(input.UV.xy,diffuseMap));
				return output;
			}
			pixelOutput applyLerpColor(vertexOutput input,pixelOutput output,fixed4 color,fixed cutoff){
				if(length(output.color.rgb) >= cutoff){
					output.color.rgb = lerp(output.color.rgb,color.rgb,color.a);
				}
				return output;
			}
			pixelOutput applyLerpColor(vertexOutput input,pixelOutput output){
				return applyLerpColor(input,output,lerpColor,lerpCutoff);
			}
			float3 UnpackFloat3(float value){
				value = value * 10000000;
				return fmod(float3(value/65536.0,value/256.0,value),256.0) / 256.0;
			}
			float4 UnpackFloat4(float value){
				value = value * 10000000;
				return fmod(float4(value/262144.0,value/4096.0,value/64.0,value),64.0) / 64.0;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = UnityObjectToClipPos(input.vertex);
				output.UV = input.texcoord;
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				float4 unpackedA = UnpackFloat4(input.UV.z);
				float3 unpackedB = UnpackFloat3(input.UV.w);
				input = setupNormalMap(input);
				input = setupSteppedLighting(input,unpackedB.x);
				output = applyDiffuseMap(input,output);
				output = applyLerpColor(input,output,input.original,unpackedB.y);
				output = applyDiffuseLerpShading(input,output,unpackedA,unpackedB.z);
				return output;
			}
			ENDCG
		}
	}
	CustomEditor "Zios.Editors.MaterialEditors.ExtendedMaterialEditor"
}
