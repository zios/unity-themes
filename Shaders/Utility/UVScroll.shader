Shader "Zios/(Components)/Utility/UV Scroll"{
	Properties{
		UVScrollX("UV Scroll X",Float) = 1.0
		UVScrollY("UV Scroll Y",Float) = 1.0
	}
	SubShader{
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma multi_compile_fwdbase
			#pragma fragmentoption ARB_precision_hint_fastest
			float timeConstant;
			float UVScrollX;
			float UVScrollY;
			struct vertexInput{
				float4 vertex        : POSITION;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float4 UV            : COLOR0;
			};
			struct pixelOutput{
				float4 color         : COLOR0;
			};
			pixelOutput setupPixel(vertexOutput input){
				pixelOutput output;
				output.color = float4(0,0,0,0);
				return output;
			}
			vertexOutput setupUVScroll(vertexOutput input,float xScroll,float yScroll,float scale){
				input.UV.x += (xScroll * scale);
				input.UV.y += (yScroll * scale);
				return input;
			}
			vertexOutput setupUVScroll(vertexOutput input,float xScroll,float yScroll){
				input = setupUVScroll(input,xScroll,yScroll,timeConstant);
				return input;
			}
			vertexOutput setupUVScroll(vertexOutput input,float scale){
				input = setupUVScroll(input,UVScrollX,UVScrollY,scale);
				return input;
			}
			vertexOutput setupUVScroll(vertexOutput input){
				input = setupUVScroll(input,UVScrollX,UVScrollY,1);
				return input;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				input = setupUVScroll(input,timeConstant);
				return output;
			}
			ENDCG
		}
	}
}
