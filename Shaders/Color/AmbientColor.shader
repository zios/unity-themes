Shader "Zios/(Components)/Color/Ambient"{
	Properties{
		ambientColor("Ambient Color", Color) = (0.5,0.5,0.5,1)
		ambientCutoff("Ambient Cutoff",Range(0,1)) = 0.8
	}
	SubShader{
		Pass{
			CGPROGRAM
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			fixed4 ambientColor;
			fixed ambientCutoff;
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
			pixelOutput applyAmbientColor(vertexOutput input,pixelOutput output){
				output.color.rgb += (ambientColor * ambientColor.a);
				return output;
			}
			pixelOutput applyAmbientColor(vertexOutput input,pixelOutput output,float cutoff){
				if(length(output.color.rgb) >= ambientCutoff){
					output.color.rgb += (ambientColor * ambientColor.a);
				}
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = UnityObjectToClipPos(input.vertex);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output = applyAmbientColor(input,output,ambientCutoff);
				return output;
			}
			ENDCG
		}
	}
}
