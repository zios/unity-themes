Shader "Zios/(Components)/Color/Alpha"{
	Properties{
		alpha("Alpha",Range(0.0,1.0)) = 0.5
		alphaCutoff("Alpha Cutoff",Range(-0.001,1.0)) = 0
	}
	SubShader{
		Pass{
			AlphaTest Greater 0
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			fixed alphaCutoff;
			fixed alphaCutoffGlobal;
			fixed alpha;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 texcoord      : TEXCOORD0;
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
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output.color = float4(0,0,0,0);
				return output;
			}
			pixelOutput applyAlpha(vertexOutput input,pixelOutput output,float alpha){
				output.color.a *= alpha;
				if(alphaCutoff < 0){alphaCutoff = alphaCutoffGlobal;}
				if(output.color.a <= alphaCutoff){clip(-1);}
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = UnityObjectToClipPos(input.vertex);
				output.UV = float4(input.texcoord.xy,0,0);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output = applyAlpha(input,output,alpha);
				return output;
			}
			ENDCG
		}
	}
}