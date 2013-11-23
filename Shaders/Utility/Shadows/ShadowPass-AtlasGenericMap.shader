Shader "Zios/Shadow Pass/Atlas Generic Map"{
	Properties{
		atlasUV("Atlas UV",Vector) = (0,0,0,0)
	}
	SubShader{
		LOD 200
		Pass{
			Name "ShadowCaster"
			Tags{"LightMode"="ShadowCaster"}
			Offset 3, 3
			Fog{Mode Off}
			ZWrite On ZTest Less Cull Off
			CGPROGRAM
			#pragma vertex vertexPassShadow
			#pragma fragment pixelPassShadow
			#pragma multi_compile_shadowcaster
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "../Unity-CG.cginc"
			sampler2D genericMap;
			float4 genericMap_ST;	
			float4 atlasUV;
			fixed alphaCutoff;
			struct vertexOutput{ 
				V2F_SHADOW_CASTER;
				float2 UV  : TEXCOORD1;
			};
			vertexOutput vertexPassShadow(appdata_base v){
				vertexOutput o;
				TRANSFER_SHADOW_CASTER(o);
				o.UV = TRANSFORM_TEX(v.texcoord,genericMap);
				return o;
			}
			float4 pixelPassShadow(vertexOutput input) : COLOR{
				input.UV.xy = lerp(atlasUV.xy,atlasUV.zw,input.UV.xy);
				clip(tex2D(genericMap,input.UV).a-alphaCutoff);
				SHADOW_CASTER_FRAGMENT(input);
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
			sampler2D genericMap;
			float4 genericMap_ST;
			float4 atlasUV;
			fixed alphaCutoff;
			struct vertexOutput{
				V2F_SHADOW_COLLECTOR;
				float2 UV : TEXCOORD5;
			};
			vertexOutput vertexPassShadow(appdata_base v){
				vertexOutput o;
				TRANSFER_SHADOW_COLLECTOR(o);
				o.UV = TRANSFORM_TEX(v.texcoord,genericMap);
				return o;
			}
			float4 pixelPassShadow(vertexOutput input) : COLOR{
				input.UV.xy = lerp(atlasUV.xy,atlasUV.zw,input.UV.xy);
				clip(tex2D(genericMap,input.UV).a-alphaCutoff);
				SHADOW_COLLECTOR_FRAGMENT(input);
			}
			ENDCG
		}
	}
}
