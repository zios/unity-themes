Shader "Zios/Olio/Mesh"{
	Properties{
		alpha("Alpha",Range(0.0,1.0)) = 1.0
		diffuseMap("Diffuse Map",2D) = "white"{}
		normalMap("Normal Map",2D) = "white"{}
		lerpColor("Lerp Color",Color) = (0,0,0,0)
		lerpCutoff("Lerp Cutoff",Range(0,1)) = 0.8
		shadingColor("Shading Color",Color) = (0.11,0,0.11,0)
		shadingSteps("Shading Steps",float) = 3.0
		shadingIgnoreCutoff("Shading Ignore Cutoff",Range(0,1)) = 0.3
		atlasUV("Atlas UV",Vector) = (0,0,1,1)
		atlasUVScale("Atlas UV Scale",Vector) = (1,1,0,0)
		lightOffset("Light Offset",Vector) = (0,0,0,0)
	}
	SubShader{
		Tags{"LightMode"="ForwardBase" "Queue"="Transparent-1"}
		Pass{
			AlphaTest Greater 0
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			sampler2D normalMap;
			fixed4 normalMap_ST;
			sampler2D diffuseMap;
			fixed4 diffuseMap_ST;
			fixed4 lerpColor;
			fixed lerpCutoff;
			fixed alphaCutoff;
			fixed alphaCutoffGlobal;
			fixed alpha;
			fixed4 atlasUV;
			fixed4 atlasUVScale;
			fixed4 shadingColor;
			fixed shadingIgnoreCutoff;
			fixed shadingSteps;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 texcoord      : TEXCOORD0;
				float4 texcoord1     : TEXCOORD1;
				float3 normal        : NORMAL;
				float4 tangent       : TANGENT;
				float4 color         : COLOR;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float4 UV            : COLOR0;
				float3 lightNormal	 : TEXCOORD0;
				float4 normal        : TEXCOORD1;
				float4 tangent       : TEXCOORD2;
				float3 view	         : TEXCOORD4;
				float  lighting      : TEXCOORD5;
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
			vertexOutput setupAtlas(vertexOutput input){
				input.UV.xy = lerp(atlasUV.xy,atlasUV.zw,fmod(input.UV.xy*atlasUVScale.xy,1));
				return input;
			}
			vertexOutput setupInput(vertexOutput input){
				input.normal.xyz = normalize(input.normal.xyz);
				input.lightNormal = normalize(input.lightNormal);
				input.view = normalize(input.view);
				return input;
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
			pixelOutput applyAlpha(vertexOutput input,pixelOutput output,float alpha){
				output.color.a *= alpha;
				if(alphaCutoff == 0){alphaCutoff = alphaCutoffGlobal;}
				if(output.color.a <= alphaCutoff){clip(-1);}
				return output;
			}
			pixelOutput applyAlpha(vertexOutput input,pixelOutput output){
				return applyAlpha(input,output,alpha);
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.UV = float4(input.texcoord.xy,0,0);
				output.lightNormal = ObjSpaceLightDir(input.vertex);
				output.view = ObjSpaceViewDir(input.vertex);
				output.normal = float4(input.normal,0);
				output.tangent = input.tangent;
				TRANSFER_VERTEX_TO_FRAGMENT(output);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				input = setupInput(input);
				input = setupAtlas(input);
				output = applyDiffuseMap(input,output);
				output = applyLerpColor(input,output);
				output = applyDiffuseLerpShading(input,output);
				output = applyAlpha(input,output);
				return output;
			}
			ENDCG
		}
	}
}