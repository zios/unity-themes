Shader "Zios/(Components)/Color/Diffuse Color"{
	Properties{
		diffuseColor("Diffuse Color", Color) = (0.5,0.5,0.5,1)
		diffuseCutoff("Diffuse Cut Off",Range(0,1)) = 0
	}
	SubShader{
		Pass{
			CGPROGRAM
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			fixed4 diffuseColor;
			fixed diffuseCutoff;
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
			pixelOutput applyDiffuseColor(vertexOutput input,pixelOutput output,float cutoff){
				if(length(output.color.rgb) >= diffuseCutoff){
					output.color.rgb *= (diffuseColor.rgb * diffuseColor.a);
				}
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
				output = applyDiffuseColor(input,output,diffuseCutoff);
				return output;
			}
			ENDCG
		}
	}
}