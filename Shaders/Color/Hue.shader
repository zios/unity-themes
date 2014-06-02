Shader "Zios/Color/Hue"{
	Properties{
		hueIntensity("Hue Intensity",Range(0.0,1)) = 0.0
	}
	SubShader{
		Pass{
			CGPROGRAM
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			fixed hue;
			fixed hueIntensity;
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
			pixelOutput applyHue(vertexOutput input,pixelOutput output){
				float3 hueColor;
				hueColor.r = abs(hue * 6 - 3) - 1;
				hueColor.g = 2 - abs(hue * 6 - 2);
				hueColor.b = 2 - abs(hue * 6 - 4);
				//output.color.rgb = fmod(output.color.rgb * saturate(hueColor),1);
				output.color.rgb = lerp(output.color.rgb,saturate(hueColor),hueIntensity);
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
				output = applyHue(input,output);
				return output;
			}
			ENDCG
		}
	}
}