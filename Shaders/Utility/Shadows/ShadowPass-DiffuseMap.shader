Shader "Hidden/Zios/Shadow Pass/Diffuse Map"{
	Properties{
	}
	SubShader{
		LOD 200
		Pass{
			Name "ShadowCaster"
			Tags{"LightMode"="ShadowCaster"}
			Offset 3,3
			Fog{Mode Off}
			ZWrite On ZTest Less Cull Off
			CGPROGRAM
			#pragma vertex vertexPassShadow
			#pragma fragment pixelPassShadow
			#pragma multi_compile_shadowcaster
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			sampler2D diffuseMap;
			float4 diffuseMap_ST;
			fixed alphaCutoff;
			fixed4 shadowOffset;
			struct vertexOutput{ 
				V2F_SHADOW_CASTER;
				float2 UV  : TEXCOORD1;
			};
			vertexOutput vertexPassShadow(appdata_base v){
				vertexOutput o;
				TRANSFER_SHADOW_CASTER(o);
				o.UV = TRANSFORM_TEX(v.texcoord,diffuseMap);
				return o;
			}
			float4 pixelPassShadow(vertexOutput i) : COLOR{
				clip(tex2D(diffuseMap,i.UV).a-alphaCutoff);
				SHADOW_CASTER_FRAGMENT(i);
			}
			ENDCG
		}
		Pass{
			Name "ShadowCollector"
			Tags{"LightMode"="ShadowCollector"}
			Fog{Mode Off}
			ZWrite On ZTest Less
			CGPROGRAM
			#pragma vertex vertexPassShadow
			#pragma fragment pixelPassShadow
			#pragma multi_compile_shadowcollector
			#pragma fragmentoption ARB_precision_hint_fastest
			#define SHADOW_COLLECTOR_PASS
			#include "UnityCG.cginc"
			sampler2D diffuseMap;
			float4 diffuseMap_ST;
			fixed alphaCutoff;
			struct vertexOutput{
				V2F_SHADOW_COLLECTOR;
				float2 UV : TEXCOORD5;
			};
			vertexOutput vertexPassShadow(appdata_base v){
				vertexOutput o;
				TRANSFER_SHADOW_COLLECTOR(o);
				o.UV = TRANSFORM_TEX(v.texcoord,diffuseMap);
				return o;
			}
			fixed4 pixelPassShadow(vertexOutput i) : COLOR{
				clip(tex2D(diffuseMap,i.UV).a-alphaCutoff);
				SHADOW_COLLECTOR_FRAGMENT(i);
			}
			ENDCG
		}
	}
}
