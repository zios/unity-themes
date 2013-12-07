Shader "Zios/Color/Ambient"{
	Properties{
		ambientColor("Ambient Color", Color) = (0.5,0.5,0.5,1)
	}
	SubShader{
		Pass{
			CGPROGRAM
			#include "../Utility/Unity-CG.cginc"
			#include "../Utility/Unity-Light.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma multi_compile_fwdbase
			#pragma fragmentoption ARB_precision_hint_fastest
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
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				output = applyAmbientColor(input,output);
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
