Shader "Zios/(Components)/Color/Desaturate"{
	Properties{
		desaturateAmount("Desaturate Amount",Range(0.0,1)) = 0.0
	}
	SubShader{
		Pass{
			CGPROGRAM
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			fixed desaturateAmount;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 color         : COLOR;
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
			pixelOutput applyDesaturate(vertexOutput input,pixelOutput output){
				float grayscale = (output.color.r + output.color.g + output.color.b) / 3;
				float3 grayscaleColor = float3(grayscale,grayscale,grayscale);
				output.color.rgb = lerp(output.color.rgb,grayscaleColor,desaturateAmount);
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
				output = applyDesaturate(input,output);
				return output;
			}
			ENDCG
		}
	}
}