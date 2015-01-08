Shader "Zios/ZEQ2/Triplanar Diffuse Map"{
	Properties{
		diffuseMap("Diffuse Map",2D) = "white"{}
	}
	SubShader{
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D diffuseMap;
			fixed4 diffuseMap_ST;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 texcoord      : TEXCOORD0;
				float4 texcoord1     : TEXCOORD1;
				float3 normal        : NORMAL;
				fixed4 color         : COLOR;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float4 UV            : COLOR0;
				float4 normal        : TEXCOORD1;
				float3 worldNormal   : TEXCOORD6;
				float3 worldPosition : TEXCOORD7;
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
			float4 setupTriplanarMap(sampler2D triplanar,float4 offset,vertexOutput input){
				float4 color1 = tex2D(triplanar,input.worldPosition.xy * offset.xy + offset.zw);
				float4 color2 = tex2D(triplanar,input.worldPosition.zx * offset.xy + offset.zw);
				float4 color3 = tex2D(triplanar,input.worldPosition.zy * offset.xy + offset.zw);
				input.worldNormal = normalize(input.worldNormal);
				float3 projectedNormal = saturate(pow(input.worldNormal*1.5,4));
				float3 color = lerp(color2,color1,projectedNormal.z);
				color = lerp(color,color3,projectedNormal.x);
				return float4(color,1.0);
			}
			pixelOutput applyTriplanarDiffuseMap(vertexOutput input,pixelOutput output){
				output.color = setupTriplanarMap(diffuseMap,diffuseMap_ST,input);
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				output.UV = float4(input.texcoord.x,input.texcoord.y,0,0);
				output.worldNormal = mul(_Object2World,float4(input.normal,0.0f)).xyz;
				output.worldPosition = mul(_Object2World,input.vertex);
				TRANSFER_VERTEX_TO_FRAGMENT(output);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output = applyTriplanarDiffuseMap(input,output);
				return output;
			}
			ENDCG
		}
	}
}