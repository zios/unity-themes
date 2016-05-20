Shader "Zios/Dori/Diffuse Map + Diffuse Color + Sprite + Shading + Outline"{
	Properties{
		diffuseMap("Diffuse Map",2D) = "white"{}
		lerpColor("Lerp Color",Color) = (1,1,1,0)
		lerpCutoff("Lerp Cutoff",Range(0,1)) = 0.3
		shadingColor("Shading Color",Color) = (0.11,0,0.11,0)
		shadingSteps("Shading Steps",float) = 3.0
		shadingIgnoreCutoff("Shading Cutoff",Range(0,1)) = 0.3
		outlineColor("Outline Color",Color) = (0.0,0.0,0.0,1.0)
		outlineLength("Outline Length",float) = 0.09
		[HideInInspector] atlasUV("Atlas UV",Vector) = (0,0,1,1)
		[HideInInspector] atlasUVScale("Atlas UV Scale",Vector) = (1,1,0,0)
	}
	SubShader{
		Tags{"LightMode"="ForwardBase" "Queue"="Geometry-1"}
		UsePass "Hidden/Zios/(Components)/Utility/Vertex Outlines/TEST"
		Usepass "Hidden/Zios/Shadow Pass/Normal Index Map/SHADOWCOLLECTOR"
		Pass{
			AlphaTest Greater 0
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma multi_compile_fwdbase
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D diffuseMap;
			fixed4 lerpColor;
			fixed4 diffuseMap_ST;
			fixed4 atlasUV;
			fixed4 atlasUVScale;
			fixed4 shadingColor;
			fixed shadingIgnoreCutoff;
			fixed lerpCutoff;
			fixed shadingSteps;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 texcoord      : TEXCOORD0;
				float3 normal        : NORMAL;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float4 UV            : COLOR0;
				float3 lightNormal	 : TEXCOORD0;
				float4 normal        : TEXCOORD1;
				float3 view	         : TEXCOORD2;
				float  lighting      : TEXCOORD3;
				float3 worldNormal   : TEXCOORD4;
				float3 worldPosition : TEXCOORD5;
			};
			struct pixelOutput{
				float4 color         : COLOR0;
			};
			pixelOutput setupPixel(vertexOutput input){
				pixelOutput output;
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output.color = float4(0,0,0,1);
				return output;
			}
			vertexOutput setupInput(vertexOutput input){
				//input.normal.xyz = normalize(input.normal.xyz);
				//input.lightNormal = normalize(input.lightNormal);
				input.view = normalize(input.view);
				return input;
			}
			vertexOutput setupSteppedLighting(vertexOutput input,float shadingSteps){
				input.lighting = saturate(dot(input.normal.xyz,input.lightNormal));
				float stepSize = shadingSteps;
				input.lighting = ceil((input.lighting / stepSize)-0.5) * stepSize;
				return input;
			}
			pixelOutput applyDiffuseMap(vertexOutput input,pixelOutput output){
				output.color += tex2D(diffuseMap,TRANSFORM_TEX(input.UV.xy,diffuseMap));
				return output;
			}
			pixelOutput applyLerpColor(vertexOutput input,pixelOutput output){
				if(length(output.color.rgb) > lerpCutoff){
					output.color.rgb = lerp(output.color.rgb,lerpColor.rgb,lerpColor.a);
				}
				return output;
			}
			pixelOutput applyDiffuseLerpShading(vertexOutput input,pixelOutput output,fixed4 shadingColor,float shadingCutoff){
				if(length(output.color.rgb) > shadingCutoff){
					float shadeValue = saturate(input.lighting+(1-shadingColor.a));
					output.color.rgb = lerp(shadingColor.rgb,output.color.rgb,shadeValue);
				}
				return output;
			}
			pixelOutput applyDiffuseLerpShading(vertexOutput input,pixelOutput output){
				return applyDiffuseLerpShading(input,output,shadingColor,shadingIgnoreCutoff);
			}
			vertexOutput setupSteppedLighting(vertexOutput input){
				return setupSteppedLighting(input,1.0 / (shadingSteps-1));
			}
			vertexOutput setupAtlas(vertexOutput input){
				input.UV.xy = lerp(atlasUV.xy,atlasUV.zw,fmod(input.UV.xy*atlasUVScale.xy,1));
				return input;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.UV = float4(input.texcoord.xy,0,0);
				output.lightNormal = ObjSpaceLightDir(input.vertex);
				output.view = ObjSpaceViewDir(input.vertex);
				output.normal = float4(input.normal,0);
				output.worldNormal = mul(unity_ObjectToWorld,float4(input.normal,0.0f)).xyz;
				output.worldPosition = mul(unity_ObjectToWorld,input.vertex);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				input = setupInput(input);
				input = setupAtlas(input);
				input = setupSteppedLighting(input);
				output = applyDiffuseMap(input,output);
				output = applyLerpColor(input,output);
				output = applyDiffuseLerpShading(input,output);
				return output;
			}
			ENDCG
		}
	}
	Fallback "Hidden/Zios/Fallback/Vertex Lit"
}