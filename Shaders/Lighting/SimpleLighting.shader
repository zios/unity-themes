Shader "Hidden/Zios/Lighting/SimpleLighting"{
	Properties{
	}
		SubShader{
		Pass{
			CGPROGRAM
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			struct vertexInput{
				float4 vertex        : POSITION;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float  lighting      : TEXCOORD5;
			};
			struct pixelOutput{
				fixed4 color         : COLOR0;
			};
			pixelOutput setupPixel(vertexOutput input){
				pixelOutput output;
				output.color = float4(0,0,0,0);
				return output;
			}
			vertexOutput setupSimpleLighting(vertexOutput input){
				input.lighting = 1.0;
				return input;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				input = setupSimpleLighting(input);
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				return output;
			}
			ENDCG
		}
	}
}