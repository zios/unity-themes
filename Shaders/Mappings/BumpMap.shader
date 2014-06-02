Shader "Zios/(Components)/Mappings/Bump Map"{
	Properties{
		bumpMapContrast("Bump Contrast",Float) = 0
		bumpMapSpread("Bump Spread",Float) = 0
		bumpMap("Bump Map",2D) = "white"{}
	}
	SubShader{
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D bumpMap;
			fixed4 bumpMap_ST;
			fixed bumpMapContrast;
			fixed bumpMapSpread;
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
				float3 view	         : TEXCOORD4;
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
			vertexOutput setupInput(vertexOutput input){
				input.normal.xyz = normalize(input.normal.xyz);
				input.lightNormal = normalize(input.lightNormal);
				input.view = normalize(input.view);
				return input;
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
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.lightNormal = ObjSpaceLightDir(input.vertex);
				output.UV = float4(input.texcoord.xy,0,0);
				output.normal = float4(input.normal,0);
				output.tangent = input.tangent;
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				input = setupInput(input);
				input = setupBumpMap(input);
				return output;
			}
			ENDCG
		}
	}
}