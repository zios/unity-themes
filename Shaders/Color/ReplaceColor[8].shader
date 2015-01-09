Shader "Zios/(Components)/Color/Replace Color"{
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
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 texcoord      : TEXCOORD0;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float4 UV            : COLOR0;
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
			pixelOutput applyColorReplace8(vertexOutput input,pixelOutput output){
				fixed lookup = floor(tex2D(indexMap,TRANSFORM_TEX(input.UV.xy,indexMap)).r * 8.5);
				if(lookup == 1){output.color = lookupColorAStart;}
				if(lookup == 2){output.color = lookupColorBStart;}
				if(lookup == 3){output.color = lookupColorCStart;}
				if(lookup == 4){output.color = lookupColorDStart;}
				if(lookup == 5){output.color = lookupColorEStart;}
				if(lookup == 6){output.color = lookupColorFStart;}
				if(lookup == 7){output.color = lookupColorGStart;}
				if(lookup == 8){output.color = lookupColorHStart;}
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.UV = float4(input.texcoord.xy,0,0);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output = applyColorReplace8(input,output);
				return output;
			}
			ENDCG
		}
	}
}