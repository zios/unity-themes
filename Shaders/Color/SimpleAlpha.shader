Shader "Zios/(Components)/Color/Simple Alpha"{
	Properties{
		alpha("Alpha",Range(0.0,1.0)) = 0.5
	}
	SubShader{
		Pass{
			AlphaTest Greater 0
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			fixed alpha;
			struct vertexInput{
				float4 vertex        : POSITION;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
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
			pixelOutput applyAlphaSimple(vertexOutput input,pixelOutput output){
				output.color.a *= alpha;
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output = applyAlphaSimple(input,output);
				return output;
			}
			ENDCG
		}
	}
}