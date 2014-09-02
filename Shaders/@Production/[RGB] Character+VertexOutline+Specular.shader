Shader "Zios/RGB/Character + Vertex Outlines + Specular"{
	Properties{
		outlineColor("Outline Color",Color) = (0.0,0.0,0.0,1.0)
		outlineLength("Outline Length",float) = 0.004
		indexMap("Index Map",2D) = "white"{}
		shadingAtlas("Shading Atlas",2D) = "white"{}
		outlineMap("Outline Map",2D) = "white"{}
		//normalMap("Normal Map",2D) = "white"{}
		specularASize("Specular A Size",Range(0.0,1)) = 0.00
		//specularAIntensity("Specular A Intensity",float) = 1.0
		//specularAHardness("Specular A Hardness",Range(0.01,1)) = 0.01
		specularAColor("Specular A Color",Color) = (1,1,1,1)
		specularBSize("Specular B Size",Range(0.0,1)) = 0.00
		//specularBIntensity("Specular B Intensity",float) = 1.0
		//specularBHardness("Specular B Hardness",Range(0.01,1)) = 0.01
		specularBColor("Specular B Color",Color) = (1,1,1,1)
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
			#include "AutoLight.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma multi_compile_fwdbase
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D indexMap;
			sampler2D normalMap;
			sampler2D outlineMap;
			sampler2D shadingAtlas;
			fixed4 shadowColor;
			fixed4 shadingAtlas_ST;
			fixed shadingIndex;
			fixed4 indexMap_ST;
			fixed4 outlineMap_ST;
			fixed4 normalMap_ST;
			fixed normalMapSpread;
			fixed normalMapContrast;
			fixed shadingIgnoreCutoff;
			fixed4 specularColor;
			fixed specularSize;
			fixed specularHardness;
			fixed specularIntensity;
			fixed4 specularAColor;
			fixed specularASize;
			fixed specularAHardness;
			fixed specularAIntensity;
			fixed4 specularBColor;
			fixed specularBSize;
			fixed specularBHardness;
			fixed specularBIntensity;
			float3 lightOffset;
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
				output.color = float4(0,0,0,1);
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
			pixelOutput applyShadingAtlas(float shadeRow,vertexOutput input,pixelOutput output){
				float2 shading = float2(input.lighting,shadeRow);
				fixed4 lookup = tex2D(shadingAtlas,shading);
				shadingIndex = shadeRow;
				if(shadeRow == 0){clip(-1);}
				output.color.rgb += lookup.rgb * lookup.a;
				output.color.a = lookup.a;
				return output;
			}
			pixelOutput applyShadingAtlas(sampler2D indexMap,vertexOutput input,pixelOutput output){
				float shadeRow = 1.0 - tex2D(indexMap,TRANSFORM_TEX(input.UV.xy,indexMap)).r;
				output = applyShadingAtlas(shadeRow,input,output);
				return output;
			}
			pixelOutput applyShadingAtlas(vertexOutput input,pixelOutput output){
				float shadeRow = 1.0 - input.normal.a;
				output = applyShadingAtlas(shadeRow,input,output);
				return output;
			}
			pixelOutput applyOutlineMap(vertexOutput input,pixelOutput output){
				float4 lookup = tex2D(outlineMap,TRANSFORM_TEX(input.UV.xy,outlineMap));
				output.color.rgb = lerp(output.color.rgb,0,lookup.a);
				return output;
			}
			pixelOutput applySpecular(vertexOutput input,pixelOutput output,fixed specularSize,fixed specularIntensity,fixed specularHardness,fixed4 specularColor){
				float3 reflect = normalize(input.lightNormal + input.view);
				float intensity = pow(saturate(dot(input.normal,reflect)),10/specularSize);
				intensity = floor((intensity / specularHardness)+0.5) * specularHardness;
				output.color.rgb += specularColor * intensity * specularIntensity;
				return output;
			}
			pixelOutput applySpecular(vertexOutput input,pixelOutput output){
				return applySpecular(input,output,specularSize,specularIntensity,specularHardness,specularColor);
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.UV = float4(input.texcoord.xy,0,0);
				output.lightNormal = ObjSpaceLightDir(input.vertex) + lightOffset;
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
				//input = setupNormalMap(input);
				input = setupLighting(input);
				output = applyShadingAtlas(indexMap,input,output);
				output = applyOutlineMap(input,output);
				output = applySpecular(input,output,specularASize,1.0f,0.01f,specularAColor);
				output = applySpecular(input,output,specularBSize,1.0f,0.01f,specularBColor);
				return output;
			}
			ENDCG
		}
	}
	Fallback "Hidden/Zios/Fallback/Vertex Lit"
}