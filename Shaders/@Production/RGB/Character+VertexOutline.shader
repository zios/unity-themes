Shader "Zios/RGB/Character + Vertex Outlines"{
	Properties{
		outlineColor("Outline Color",Color) = (0.0,0.0,0.0,1.0)
		outlineLength("Outline Length",float) = 0.004
		indexMap("Index Map",2D) = "white"{}
		shadingAtlas("Shading Atlas",2D) = "white"{}
		outlineMap("Outline Map",2D) = "white"{}
		normalMap("Normal Map",2D) = "white"{}
	}
	SubShader{
		Tags{"LightMode"="ForwardBase"}
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
				input = setupLighting(input);
				input = setupNormalMap(input);
				output = applyShadingAtlas(indexMap,input,output);
				output = applyOutlineMap(input,output);
				return output;
			}
			ENDCG
		}
	}
	Fallback "Hidden/Zios/Fallback/Vertex Lit"
}