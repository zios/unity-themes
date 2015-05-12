Shader "Zios/Mobile/Lighting"{
	Properties{
		_MainTex("Diffuse Map",2D) = "white"{}
		lightDirection("Light Direction",Vector) = (-1,1,0,1)
	}
	SubShader{
		Tags{"Queue"="Geometry" "RenderType"="Opaque"}
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D _MainTex;
			fixed3 lightDirection;
			struct vertexInput{
				float4 vertex        : POSITION;
				fixed3 normal        : NORMAL;
				fixed2 texcoord      : TEXCOORD0;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				fixed2 UV            : COLOR0;
				fixed lighting		 : TEXCOORD1;
			};
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.lighting = dot(input.normal,lightDirection);
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
