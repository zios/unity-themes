Shader "Zios/Component/Alpha"{
	Properties{
		alpha("Alpha",Range(0.0,1.0)) = 0.5
		alphaCutoff("Alpha Cutoff",Range(-0.001,1.0)) = 0
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
			fixed alphaCutoff;
			fixed alphaCutoffGlobal;
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
			pixelOutput applyAlpha(vertexOutput input,pixelOutput output,float alpha){
				output.color.a *= alpha;
				if(alphaCutoff < 0){alphaCutoff = alphaCutoffGlobal;}
				if(output.color.a <= alphaCutoff){clip(-1);}
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output;
				output.color = float4(0,0,0,1);
				output = applyAlpha(input,output,alpha);
				return output;
			}
			ENDCG
		}
	}
}