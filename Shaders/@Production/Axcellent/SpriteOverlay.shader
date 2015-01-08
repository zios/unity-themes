Shader "Zios/Sprite/Overlay"{
	Properties{
		[PerRendererData]_MainTex("Diffuse Map",2D) = "white"{}
	}
	SubShader{
		Tags{"Queue"="Overlay" "RenderType"="Overlay"}
		Cull Off
		ZTest Off
		Blend SrcAlpha OneMinusSrcAlpha
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D _MainTex;
			fixed4 _MainTex_ST;
			struct vertexInput{
				float4 vertex        : POSITION;
				float2 texcoord      : TEXCOORD0;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				fixed2 UV            : COLOR0;
			};
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.UV = input.texcoord;
				return output;
			}
			fixed4 pixelPass(vertexOutput input):COLOR{
				return tex2D(_MainTex,input.UV.xy);
			}
			ENDCG
		}
	}
	CustomEditor "ExtendedMaterialEditor"
}
