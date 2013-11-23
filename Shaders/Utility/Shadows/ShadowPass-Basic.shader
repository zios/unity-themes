Shader "Zios/Shadow Pass/Basic"{
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
			#include "../Unity-CG.cginc"
			struct vertexOutput{ 
				V2F_SHADOW_CASTER;
			};
			vertexOutput vertexPassShadow(appdata_base v){
				vertexOutput o;
				TRANSFER_SHADOW_CASTER(o);
				return o;
			}
			float4 pixelPassShadow(vertexOutput i) : COLOR{
				SHADOW_CASTER_FRAGMENT(i);
			}
			ENDCG
		}
		Pass{
			Name "ShadowCollector"
			Tags{"LightMode" = "ShadowCollector"}
			Fog{Mode Off}
			ZWrite On ZTest Less
			CGPROGRAM
			#pragma vertex vertexPassShadow
			#pragma fragment pixelPassShadow
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_shadowcollector
			#define SHADOW_COLLECTOR_PASS
			#include "../Unity-CG.cginc"
			struct vertexOutput{
				V2F_SHADOW_COLLECTOR;
			};
			vertexOutput vertexPassShadow(appdata_base v){
				vertexOutput o;
				TRANSFER_SHADOW_COLLECTOR(o);
				return o;
			}
			fixed4 pixelPassShadow(vertexOutput i) : COLOR{
				SHADOW_COLLECTOR_FRAGMENT(i);
			}
			ENDCG
		}
	}
}
