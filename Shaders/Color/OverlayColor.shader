Shader "Zios/Color/Overlay Color"{
	Properties{
		overlayColor("Overlay Color",Color) = (1.0,1.0,1.0,1.0)
		overlayIntensity("Overlay Intensity",Range(0,1)) = 0
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
				output.color = float4(0,0,0,0);
				return output;
			}
			pixelOutput applyOverlayColor(vertexOutput input,pixelOutput output){
				output.color = lerp(output.color,overlayColor,overlayIntensity);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				output = applyOverlayColor(input,output);
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
