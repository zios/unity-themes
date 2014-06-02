Shader "Zios/(Components)/Mappings/LOD Shading Map"{
	Properties{
		shadingMap("Shading Map",2D) = "white"{}
		zMin("Z Min",Float) = 1.0
		zRange("Range",Float) = 50.0
	}
	SubShader{
		Pass{
			Tags{"LightMode"="ForwardBase" "RenderType"="Geometry"}
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexStep
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D shadingMap;
			fixed4 ShadingMap_ST;
			fixed zMin;
			fixed zRange;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 texcoord      : TEXCOORD0;
				float3 normal        : NORMAL;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float3 lightNormal	 : TEXCOORD0;
				float4 normal        : TEXCOORD1;
				float3 view	         : TEXCOORD4;
				float  lighting      : TEXCOORD5;
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
			vertexOutput setupLighting(vertexOutput input){
				input.lighting = saturate(dot(input.normal.xyz,input.lightNormal));
				return input;
			}
			vertexOutput setupLighting(float3 lightDirection,vertexOutput input){
				input.lighting = saturate(dot(lightDirection,input.lightNormal));
				return input;
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
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.lightNormal = ObjSpaceLightDir(input.vertex);
				output.view = ObjSpaceViewDir(input.vertex);
				output.normal = float4(input.normal,0);
				return output;
			}
			vertexOutput vertexStep(vertexInput input){
				vertexOutput output = vertexPass(input);
				output = prepareLODShadingMap(input,output);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				input = setupInput(input);
				input = setupLighting(input);
				output = applyLODShadingMap(input,output);
				return output;
			}
			ENDCG
		}
	}
}