Shader "Zios/SuperCity/Transparent + Scrolling"{
	Properties{
		diffuseMap("Diffuse Map",2D) = "white"{}
		lerpColor("Lerp Color",Color) = (0,0,0,0)
		lerpCutoff("Lerp Cutoff",Range(0,1)) = 0.8
		UVScrollX("UV Scroll X",Float) = 0
		UVScrollY("UV Scroll Y",Float) = 0
	}
	SubShader{
		Tags{"LightMode"="Always" "Queue"="Transparent+2"}
		ZWrite Off
		Pass{
			AlphaTest Greater 0
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D diffuseMap;
			fixed4 diffuseMap_ST;
			fixed UVScrollX;
			fixed UVScrollY;
			float timeConstant;
			fixed4 lerpColor;
			fixed lerpCutoff;
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
			vertexOutput setupLighting(vertexOutput input){
				input.lighting = saturate(dot(input.normal.xyz,input.lightNormal));
				return input;
			}
			vertexOutput setupLighting(float3 lightDirection,vertexOutput input){
				input.lighting = saturate(dot(lightDirection,input.lightNormal));
				return input;
			}
			vertexOutput setupUVScroll(vertexOutput input,float xScroll,float yScroll,float scale){
				input.UV.x += (xScroll * scale);
				input.UV.y += (yScroll * scale);
				return input;
			}
			vertexOutput setupUVScroll(vertexOutput input,float xScroll,float yScroll){
				input = setupUVScroll(input,xScroll,yScroll,timeConstant);
				return input;
			}
			vertexOutput setupUVScroll(vertexOutput input,float scale){
				input = setupUVScroll(input,UVScrollX,UVScrollY,scale);
				return input;
			}
			vertexOutput setupUVScroll(vertexOutput input){
				input = setupUVScroll(input,UVScrollX,UVScrollY,1);
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
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.UV = input.texcoord;
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				input = setupUVScroll(input,timeConstant);
				input = setupLighting(input);
				output = applyDiffuseMap(input,output);
				output = applyLerpColor(input,output);
				return output;
			}
			ENDCG
		}
	}
	CustomEditor "ExtendedMaterialEditor"
}