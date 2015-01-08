Shader "Zios/(Components)/Utility/Clipping"{
	Properties{
		clipUV("Clip UV",Vector) = (0,0,1,1)
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
			fixed4 clipUV;
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
			vertexOutput setupClipping(vertexOutput input){
				if(input.UV.x < clipUV.x){clip(-1);}
				if(input.UV.x > clipUV.z){clip(-1);}
				if(input.UV.y < 1-clipUV.w){clip(-1);}
				if(input.UV.y > 1-clipUV.y){clip(-1);}
				return input;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				input = setupClipping(input);
				return output;
			}
			ENDCG
		}
	}
}
