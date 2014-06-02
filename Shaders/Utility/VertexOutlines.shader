Shader "Hidden/Zios/Utility/Vertex Outlines"{
	Properties{
	}
	SubShader{
		Tags{"RenderType"="Opaque"}
		Pass{
			Name "Normal"
			Tags {"LightMode"="ForwardBase"}
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Front
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPassOutline
			#pragma fragment pixelPassOutline
			#pragma fragmentoption ARB_precision_hint_fastest
			fixed4 outlineColor;
			fixed outlineLength;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 texcoord      : TEXCOORD0;
				float3 normal        : NORMAL;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float4 UV            : COLOR0;
			};
			struct pixelOutput{
				float4 color         : COLOR0;
			};
			pixelOutput pixelPassOutline(vertexOutput input){
				pixelOutput output;
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output.color = outlineColor;
				return output;
			}
			vertexOutput vertexPassOutline(vertexInput input){ 
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				float3 view = ObjSpaceViewDir(input.vertex);
				float3 outline = input.vertex + input.normal * outlineLength;
				output.pos = mul(UNITY_MATRIX_MVP,float4(outline,1));
				output.UV.xy = float2(input.texcoord.x,input.texcoord.y);
				return output;
			}
			ENDCG
		}
		Pass{
			Name "Test"
			Cull Front
			Lighting Off
            ZWrite On
			Tags {"LightMode"="ForwardBase"}
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPassOutline
			#pragma fragment pixelPassOutline
			#pragma fragmentoption ARB_precision_hint_fastest
			fixed outlineLength;
			fixed4 outlineColor;
			struct vertexInput{
				float4 vertex        : POSITION;
				float3 normal        : NORMAL;
				float4 tangent       : TANGENT;
			};
			struct vertexOutput{
				float4 pos           : POSITION;				
			};
			struct pixelOutput{
				float4 color         : COLOR0;
			};
			pixelOutput pixelPassOutline(vertexOutput input){
				pixelOutput output;
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output.color = outlineColor;
				return output;
			}
			vertexOutput vertexPassOutline(vertexInput input){ 
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)			
				float4 position = mul(UNITY_MATRIX_MV,input.vertex);
				float3 outline = mul((float3x3)UNITY_MATRIX_IT_MV,input.normal);
                outline.z = -0.4;
                position = position + float4(normalize(outline),0) * outlineLength;
                output.pos = mul(UNITY_MATRIX_P,position);			
				return output;
			}
			ENDCG
		}
		Pass{
			Name "DiffuseMap"
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Front
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vertexPassOutline
			#pragma fragment pixelPassBlendOutline
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D diffuseMap;
			fixed4 diffuseMap_ST;
			fixed4 outlineColor;
			fixed outlineLength;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 texcoord      : TEXCOORD0;
				float3 normal        : NORMAL;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float4 UV            : COLOR0;
			};
			struct pixelOutput{
				float4 color         : COLOR0;
			};
			pixelOutput pixelPassBlendOutline(vertexOutput input){
				pixelOutput output;
				output.color = lerp(tex2D(diffuseMap,TRANSFORM_TEX(input.UV.xy,diffuseMap)),outlineColor,outlineColor.a);
				return output;
			}
			vertexOutput vertexPassOutline(vertexInput input){ 
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				float3 view = ObjSpaceViewDir(input.vertex);
				float3 outline = input.vertex + input.normal * outlineLength;
				output.pos = mul(UNITY_MATRIX_MVP,float4(outline,1));
				output.UV.xy = float2(input.texcoord.x,input.texcoord.y);
				return output;
			}
			ENDCG
		}
	}
}

