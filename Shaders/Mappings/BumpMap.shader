Shader "Zios/Mappings/Bump Map"{
	Properties{
		bumpMapContrast("Bump Contrast",Float) = 0
		bumpMapSpread("Bump Spread",Float) = 0
		bumpMap("Bump Map",2D) = "white"{}
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
			sampler2D bumpMap;
			fixed4 bumpMap_ST;
			fixed bumpMapContrast;
			fixed bumpMapSpread;
			float3 lightOffset;
			struct vertexInput{
				float4 vertex        : POSITION;
				float3 normal        : NORMAL;
				float4 texcoord      : TEXCOORD0;
				float4 tangent       : TANGENT;
				float4 color         : COLOR;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float4 UV            : COLOR0;
				float3 lightNormal	 : TEXCOORD0;
				float4 normal        : TEXCOORD1;
				float4 tangent       : TEXCOORD2;
			};
			struct pixelOutput{
				float4 color         : COLOR0;
			};
			pixelOutput setupPixel(vertexOutput input){
				pixelOutput output;
				output.color = float4(0,0,0,0);
				return output;
			}
			vertexOutput setupTangentSpace(vertexOutput input){
				float3 binormal = cross(input.normal.xyz,input.tangent.xyz) * input.tangent.w;
				float3x3 tangentRotate = float3x3(input.tangent.xyz,binormal,input.normal.xyz);
				input.lightNormal = mul(tangentRotate,input.lightNormal);
				return input;
			}
			vertexOutput setupBumpMap(vertexOutput input){
				input = setupTangentSpace(input);
				float height = tex2D(bumpMap,TRANSFORM_TEX(input.UV.xy,bumpMap)).r;
				float shade = ((height*2)-1.0)*(1.0+bumpMapContrast)+bumpMapSpread;
				input.normal.xyz = float3(shade,shade,shade);
				return input;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				input = setupBumpMap(input);
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.lightNormal = ObjSpaceLightDir(input.vertex) + lightOffset;
				output.UV = float4(input.texcoord.xy,0,0);
				output.normal = float4(input.normal,0);
				output.tangent = input.tangent;
				return output;
			}
			ENDCG
		}
	}
}
