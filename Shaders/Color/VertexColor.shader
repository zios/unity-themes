Shader "Zios/Color/Vertex Color"{
	Properties{}
	SubShader{
		Pass{
			CGPROGRAM
			#include "../Utility/Unity-CG.cginc"
			#include "../Utility/Unity-Light.cginc"
			#pragma vertex vertexPassSimple
			#pragma fragment pixelPass
			#pragma multi_compile_fwdbase
			#pragma fragmentoption ARB_precision_hint_fastest
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 color         : COLOR;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float3 lightNormal	 : TEXCOORD0;
			};
			struct pixelOutput{
				float4 color         : COLOR0;
			};
			pixelOutput setupPixel(vertexOutput input){
				pixelOutput output;
				output.color = float4(0,0,0,0);
				return output;
			}			
			pixelOutput applyVertexColor(vertexOutput input,pixelOutput output){
				output.color.rgb = input.lightNormal.rgb;
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				output = applyVertexColor(input,output);
				return output;
			}
			vertexOutput vertexPassSimple(vertexInput input){
				vertexOutput output;
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.lightNormal = ObjSpaceLightDir(input.vertex);
				return output;
			}
			ENDCG
		}
	}
}
