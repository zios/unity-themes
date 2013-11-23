Shader "Zios/Shadow Pass/Atlas Padded Diffuse Map"{
	Properties{
		atlasUV("Atlas UV",Vector) = (0,0,0,0)
		paddingUV("Padding UV",Vector) = (0,0,0,0)
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
			sampler2D diffuseMap;
			float4 diffuseMap_ST;
			float4 atlasUV;
			float4 paddingUV;
			fixed alphaCutoff;
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
			float2 clampRange(float2 min,float2 max,float2 value){return saturate((value-min)/(max-min));}
			float4 pixelPassShadow(vertexOutput input) : COLOR{
				input.UV.xy = clampRange(paddingUV.xy,paddingUV.zw,input.UV.xy);
				input.UV.xy = lerp(atlasUV.xy,atlasUV.zw,input.UV.xy);
				clip(tex2D(diffuseMap,input.UV).a-alphaCutoff);
				SHADOW_CASTER_FRAGMENT(input);
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
			#include "../Unity-CG.cginc"
			sampler2D diffuseMap;
			float4 diffuseMap_ST;
			float4 atlasUV;
			float4 paddingUV;
			fixed alphaCutoff;
			struct vertexOutput{
				V2F_SHADOW_COLLECTOR;
				float2 UV : TEXCOORD5;
			};
			float2 clampRange(float2 min,float2 max,float2 value){return saturate((value-min)/(max-min));}
			vertexOutput vertexPassShadow(appdata_base v){
				vertexOutput o;
				TRANSFER_SHADOW_COLLECTOR(o);
				o.UV = TRANSFORM_TEX(v.texcoord,diffuseMap);
				return o;
			}
			float4 pixelPassShadow(vertexOutput input) : COLOR{
				input.UV.xy = clampRange(paddingUV.xy,paddingUV.zw,input.UV.xy);
				input.UV.xy = lerp(atlasUV.xy,atlasUV.zw,input.UV.xy);
				clip(tex2D(diffuseMap,input.UV).a-alphaCutoff);
				SHADOW_COLLECTOR_FRAGMENT(input);
			}
			ENDCG
		}
	}
}
