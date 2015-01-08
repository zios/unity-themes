Shader "Zios/(Components)/Lighting/Gradient Shading"{
	Properties{
		indexMap("Lookup Map",2D) = "white"{}
		lookupColorAStart("Replacement Color A",Color) = (1,0,0,1)
		lookupColorBStart("Replacement Color B",Color) = (0,1,0,1)
		lookupColorCStart("Replacement Color C",Color) = (0,0,1,1)
		lookupColorDStart("Replacement Color D",Color) = (1,1,0,1)
		lookupColorEStart("Replacement Color E",Color) = (0,1,1,1)
		lookupColorFStart("Replacement Color F",Color) = (1,0,1,1)
		lookupColorGStart("Replacement Color G",Color) = (0,0,0,1)
		lookupColorHStart("Replacement Color H",Color) = (1,1,1,1)
	}
	SubShader{
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D indexMap;
			fixed4 indexMap_ST;
			float4 lookupColorAStart;
			float4 lookupColorBStart;
			float4 lookupColorCStart;
			float4 lookupColorDStart;
			float4 lookupColorEStart;
			float4 lookupColorFStart;
			float4 lookupColorGStart;
			float4 lookupColorHStart;
			float4 lookupColorAEnd;
			float4 lookupColorBEnd;
			float4 lookupColorCEnd;
			float4 lookupColorDEnd;
			float4 lookupColorEEnd;
			float4 lookupColorFEnd;
			float4 lookupColorGEnd;
			float4 lookupColorHEnd;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 texcoord      : TEXCOORD0;
				float3 normal        : NORMAL;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float3 lightNormal	 : TEXCOORD0;
				float4 normal        : TEXCOORD1;
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
			pixelOutput applyGradientShading8(vertexOutput input,pixelOutput output){
				fixed lookup = floor(tex2D(indexMap,TRANSFORM_TEX(input.UV.xy,indexMap)).r * 8.5);
				if(lookup == 1){output.color = lerp(lookupColorAStart,lookupColorAEnd,input.lighting);}
				if(lookup == 2){output.color = lerp(lookupColorBStart,lookupColorBEnd,input.lighting);}
				if(lookup == 3){output.color = lerp(lookupColorCStart,lookupColorCEnd,input.lighting);}
				if(lookup == 4){output.color = lerp(lookupColorDStart,lookupColorDEnd,input.lighting);}
				if(lookup == 5){output.color = lerp(lookupColorEStart,lookupColorEEnd,input.lighting);}
				if(lookup == 6){output.color = lerp(lookupColorFStart,lookupColorFEnd,input.lighting);}
				if(lookup == 7){output.color = lerp(lookupColorGStart,lookupColorGEnd,input.lighting);}
				if(lookup == 8){output.color = lerp(lookupColorHStart,lookupColorHEnd,input.lighting);}
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.UV = float4(input.texcoord.xy,0,0);
				output.lightNormal = ObjSpaceLightDir(input.vertex);
				output.normal = float4(input.normal,0);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				input = setupLighting(input);
				output = applyGradientShading8(input,output);
				return output;
			}
			ENDCG
		}
	}
}