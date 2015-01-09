Shader "Zios/(Components)/Color/Overlay Color"{
	Properties{
		overlayColor("Overlay Color",Color) = (1.0,1.0,1.0,1.0)
		overlayIntensity("Overlay Intensity",Range(0,1)) = 0
	}
	SubShader{
		Pass{
			CGPROGRAM
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			fixed overlayIntensity;
			fixed4 overlayColor;
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
			pixelOutput applyOverlayColor(vertexOutput input,pixelOutput output){
				output.color = lerp(output.color,overlayColor,overlayIntensity);
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
				output = applyOverlayColor(input,output);
				return output;
			}
			ENDCG
		}
	}
}