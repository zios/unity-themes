Shader "Zios/Mappings/Outline Map"{
	Properties{
		outlineMap("Outline Map",2D) = "white"{}
	}
	SubShader{
		Pass{
			CGPROGRAM
			#include "../Utility/Unity-CG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D outlineMap;
			fixed4 outlineMap_ST;
			fixed outlineMapAlias;
			fixed outlineMapIntensity;
			fixed outlineMapFading;
			fixed outlineMapCutoff;
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
				float  lighting      : TEXCOORD5;
			};
			struct pixelOutput{
				float4 color         : COLOR0;
			};
			pixelOutput setupPixel(vertexOutput input){
				pixelOutput output;
				output.color = float4(0,0,0,0);
				return output;
			}
			pixelOutput applyOutlineMap(vertexOutput input,pixelOutput output){
				float4 lookup = tex2D(outlineMap,TRANSFORM_TEX(input.UV.xy,outlineMap));
				output.color.rgb = lerp(output.color.rgb,0,lookup.a);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				output = applyOutlineMap(input,output);
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.UV = float4(input.texcoord.xy,0,0);
				return output;
			}
			ENDCG
		}
	}
}
