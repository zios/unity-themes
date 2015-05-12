Shader "Zios/General/Diffuse Map + Lighting"{
	Properties{
		_MainTex("Diffuse Map",2D) = "white"{}
		intensity("Light Intensity",Range(1,4)) = 4.0
	}
	SubShader{
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D _MainTex;
			fixed4 _MainTex_ST;
			fixed intensity;
			struct vertexInput{
				float4 vertex        : POSITION;
				float3 normal        : NORMAL;
				float2 texcoord      : TEXCOORD0;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				fixed2 UV            : COLOR0;
				fixed  lighting		 : TEXCOORD0;
			};
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.lighting = dot(input.normal,ObjSpaceLightDir(input.vertex)) * intensity;
				output.UV = input.texcoord;
				return output;
			}
			fixed4 pixelPass(vertexOutput input):COLOR{
				return tex2D(_MainTex,input.UV) * input.lighting;
			}
			ENDCG
		}
	}
	CustomEditor "ExtendedMaterialEditor"
}
