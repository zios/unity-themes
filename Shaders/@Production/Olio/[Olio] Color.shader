Shader "Zios/Olio/Solid Color"{
	Properties{
		alpha("Alpha",Range(0.0,1.0)) = 1.0
		lerpColor("Lerp Color",Color) = (0,0,0,0)
		lerpCutoff("Lerp Cutoff",Range(0,1)) = 0.8
	}
	SubShader{
		Pass{
			Tags{"LightMode"="ForwardBase" "Queue"="Transparent-2"}
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma multi_compile_fwdbase
			#pragma fragmentoption ARB_precision_hint_fastest
			fixed alphaCutoff;
			fixed alphaCutoffGlobal;
			fixed alpha;
			fixed4 lerpColor;
			fixed lerpCutoff;
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
				output.color = float4(0,0,0,1);
				return output;
			}
			pixelOutput applyAlpha(vertexOutput input,pixelOutput output,float alpha){
				output.color.a *= alpha;
				if(alphaCutoff < 0){alphaCutoff = alphaCutoffGlobal;}
				if(output.color.a <= alphaCutoff){clip(-1);}
				return output;
			}
			pixelOutput applyLerpColor(vertexOutput input,pixelOutput output,fixed4 color,fixed cutoff){
				if(length(output.color.rgb) >= cutoff){
					output.color.rgb = lerp(output.color.rgb,color.rgb,color.a);
				}
				return output;
			}
			pixelOutput applyLerpColor(vertexOutput input,pixelOutput output){
				return applyLerpColor(input,output,lerpColor,lerpCutoff);
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.UV = float4(input.texcoord.xy,0,0);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output = applyLerpColor(input,output);
				output = applyAlpha(input,output,alpha);
				return output;
			}
			ENDCG
		}
	}
}