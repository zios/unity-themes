Shader "Zios/Component/LOD Shading Map"{
	Properties{
		shadingMap("Shading Map",2D) = "white"{}
		zMin("Z Min",Float) = 1.0
		zRange("Range",Float) = 50.0
	}
	SubShader{
		Pass{
			CGPROGRAM
			#include "../Utility/Unity-CG.cginc"
			#include "../Utility/Unity-Light.cginc"
			#pragma vertex vertexStep
			#pragma fragment pixelPass
			#pragma multi_compile_fwdbase
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D shadingMap;
			fixed zMin;
			fixed zRange;
			struct vertexInput{
				float4 vertex        : POSITION;
				float3 normal        : NORMAL;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float4 normal        : TEXCOORD1;
				float4 original      : TEXCOORD3;
				float  lighting      : TEXCOORD5;
			};
			struct pixelOutput{
				float4 color         : COLOR0;
			};
			pixelOutput setupPixel(vertexOutput input){
				pixelOutput output;
				output.color = float4(0,0,0,0);
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.normal = float4(input.normal,0);
				output.original = input.vertex;
				return output;
			}
			vertexOutput prepareLODShadingMap(vertexInput input,vertexOutput output){
				float zMax = zRange*zMin;
				float4 cameraPosition = mul(_World2Object,float4(_WorldSpaceCameraPos,1.0));
				float z = distance(cameraPosition,input.vertex);
				output.normal.a = 1 - log(z/zMin)/log(zMax/zMin);
				return output;
			}
			pixelOutput applyLODShadingMap(vertexOutput input,pixelOutput output){
				float2 shading = float2(input.lighting,input.normal.a);
				output.color += tex2D(shadingMap,shading);
				return output;
			}
			vertexOutput vertexStep(vertexInput input){
				vertexOutput output = vertexPass(input);
				output = prepareLODShadingMap(input,output);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				output = applyLODShadingMap(input,output);
				return output;
			}
			ENDCG
		}
	}
}
