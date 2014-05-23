Shader "Zios/Lighting/Self Shadow"{
	Properties{
		selfShadowSpread("Self-Shadow Spread",Float) = 0
		selfShadowContrast("Self-Shadow Contrast",Float) = 0
	}
	SubShader{
		Pass{
			CGPROGRAM
			#include "../Utility/Unity-CG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			fixed selfShadowSpread;
			fixed selfShadowContrast;
			sampler2D bumpMap;
			fixed4 bumpMap_ST;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 texcoord      : TEXCOORD0;
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
			vertexOutput setupSelfShadow(vertexOutput input){
				float height = 1.0-tex2D(bumpMap,TRANSFORM_TEX(input.UV.xy,bumpMap)).r;
				float shadow = saturate((input.lighting - height) * (1.8+selfShadowContrast)) + (0.5+selfShadowSpread);
				//shadow = shadow * shadow * (3.0 - 2.0 * shadow);
				input.lighting *= shadow;
				return input;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				input = setupSelfShadow(input);
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
