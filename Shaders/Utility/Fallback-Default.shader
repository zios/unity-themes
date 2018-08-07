Shader "Hidden/Zios/Fallback/Vertex Lit"{
Properties{
	diffuseColor ("Diffuse Color", Color) = (1,1,1,1)
	specularColor ("Spec Color", Color) = (1,1,1,1)
	emissiveColor ("Emissive Color", Color) = (0,0,0,0)
	specularAmount ("Shininess", Range (0.01, 1)) = 0.7
	diffuseMap ("Base (RGB)", 2D) = "white"{}
}

// 2/3 texture stage GPUs
SubShader{
	Tags{ "RenderType"="Opaque" }
	LOD 100
	
	// Non-lightmapped
	Pass{
		Tags{ "LightMode" = "Vertex" }
		
		Material{
			Diffuse [diffuseColor]
			Ambient [diffuseColor]
			Shininess [specularAmount]
			Specular [specularColor]
			Emission [emissiveColor]
		} 
		Lighting On
		SeparateSpecular On
		SetTexture [diffuseMap]{
			Combine texture * primary DOUBLE, texture * primary
		} 
	}
	
	// Lightmapped, encoded as dLDR
	Pass{
		Tags{ "LightMode" = "VertexLM" }
		
		BindChannels{
			Bind "Vertex", vertex
			Bind "normal", normal
			Bind "texcoord1", texcoord0 // lightmap uses 2nd uv
			Bind "texcoord", texcoord1 // main uses 1st uv
		}
		
		SetTexture [unity_Lightmap]{
			matrix [unity_LightmapMatrix]
			constantColor [diffuseColor]
			combine texture * constant
		}
		SetTexture [diffuseMap]{
			combine texture * previous DOUBLE, texture * primary
		}
	}
	
	// Lightmapped, encoded as RGBM
	Pass{
		Tags{ "LightMode" = "VertexLMRGBM" }
		
		BindChannels{
			Bind "Vertex", vertex
			Bind "normal", normal
			Bind "texcoord1", texcoord0 // lightmap uses 2nd uv
			Bind "texcoord1", texcoord1 // unused
			Bind "texcoord", texcoord2 // main uses 1st uv
		}
		
		SetTexture [unity_Lightmap]{
			matrix [unity_LightmapMatrix]
			combine texture * texture alpha DOUBLE
		}
		SetTexture [unity_Lightmap]{
			constantColor [diffuseColor]
			combine previous * constant
		}
		SetTexture [diffuseMap]{
			combine texture * previous QUAD, texture * primary
		}
	}
	
	// Pass to render object as a shadow caster
	Pass{
		Name "ShadowCaster"
		Tags{ "LightMode" = "ShadowCaster" }
		
		Fog{Mode Off}
		ZWrite On ZTest Less Cull Off
		Offset 1, 1

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_shadowcaster
#pragma fragmentoption ARB_precision_hint_fastest
#include "UnityCG.cginc"

struct v2f{ 
	V2F_SHADOW_CASTER;
};

v2f vert( appdata_base v )
{
	v2f o;
	TRANSFER_SHADOW_CASTER(o)
	return o;
}

float4 frag( v2f i ) : COLOR
{
	SHADOW_CASTER_FRAGMENT(i)
}
ENDCG

	}
	
	// Pass to render object as a shadow collector
	Pass{
		Name "ShadowCollector"
		Tags{ "LightMode" = "ShadowCollector" }
		
		Fog{Mode Off}
		ZWrite On ZTest Less

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#pragma multi_compile_shadowcollector

#define SHADOW_COLLECTOR_PASS
#include "UnityCG.cginc"

struct appdata{
	float4 vertex : POSITION;
};

struct v2f{
	V2F_SHADOW_COLLECTOR;
};

v2f vert (appdata v)
{
	v2f o;
	TRANSFER_SHADOW_COLLECTOR(o)
	return o;
}

fixed4 frag (v2f i) : COLOR
{
	SHADOW_COLLECTOR_FRAGMENT(i)
}
ENDCG

	}
}

// 1 texture stage GPUs
SubShader{
	Tags{ "RenderType"="Opaque" }
	LOD 100

	// Non-lightmapped
	Pass{
		Tags{ "LightMode" = "Vertex" }
		
		Material{
			Diffuse [diffuseColor]
			Ambient [diffuseColor]
			Shininess [specularAmount]
			Specular [specularColor]
			Emission [emissiveColor]
		} 
		Lighting On
		SeparateSpecular On
		SetTexture [diffuseMap]{
			Combine texture * primary DOUBLE, texture * primary
		} 
	}	
	// Lightmapped, encoded as dLDR
	Pass{
		// 1st pass - sample Lightmap
		Tags{ "LightMode" = "VertexLM" }

		BindChannels{
			Bind "Vertex", vertex
			Bind "texcoord1", texcoord0 // lightmap uses 2nd uv
		}		
		SetTexture [unity_Lightmap]{
			matrix [unity_LightmapMatrix]
			constantColor [diffuseColor]
			combine texture * constant
		}
	}
	Pass{
		// 2nd pass - multiply with diffuseMap
		Tags{ "LightMode" = "VertexLM" }
		ZWrite Off
		Fog{Mode Off}
		Blend DstColor Zero
		SetTexture [diffuseMap]{
			combine texture
		}
	}
}
}
