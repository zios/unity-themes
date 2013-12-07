Shader "Zios/Color/Simple Alpha"{
	Properties{
		alpha("Alpha",Range(0.0,1.0)) = 0.5
	}
	SubShader{
		Pass{
			CGPROGRAM
			#include "../Utility/Unity-CG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma multi_compile_fwdbase
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
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				return output;
			}
			pixelOutput applyAlphaSimple(vertexOutput input,pixelOutput output){
				output.color.a *= alpha;
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output;
				output.color = float4(0,0,0,1);
				output = applyAlphaSimple(input,output);
				return output;
			}
			ENDCG
		}
	}
}