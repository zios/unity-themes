Shader "Zios/ZEQ2/Billboard"{
	Properties{
		diffuseMap("Diffuse Map",2D) = "white"{}
		alpha("Alpha",Range(0.0,1.0)) = 0.5
		alphaCutoff("Alpha Cutoff",Range(-0.001,1.0)) = 0
	}
	SubShader{
		Pass{
			Tags{"LightMode"="ForwardBase"}
			Alphatest Greater [alphaCutoff]
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			sampler2D diffuseMap;
			fixed4 diffuseMap_ST;
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
			pixelOutput applyDiffuseMap(vertexOutput input,pixelOutput output){
				output.color += tex2D(diffuseMap,TRANSFORM_TEX(input.UV.xy,diffuseMap));
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
				output.pos = mul(UNITY_MATRIX_P,mul(UNITY_MATRIX_MV,float4(0.0f,0.0f,0.0f,1.0f))-float4(input.vertex.x,input.vertex.z,0.0f,0.0f));
				output.UV = float4(input.texcoord.xy,0.0f,0.0f);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output = applyDiffuseMap(input,output);
				output = applyAlpha(input,output,alpha);
				return output;
			}
			ENDCG
		}
	}
}