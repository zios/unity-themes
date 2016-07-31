Shader "Hidden/Zios/Fallback/Cutout Vertex Lit" {
Properties {
	diffuseColor ("Diffuse Color", Color) = (1,1,1,1)
	specularColor ("Spec Color", Color) = (1,1,1,0)
	emissiveColor ("Emissive Color", Color) = (0,0,0,0)
	specularAmount ("Shininess", Range (0.1, 1)) = 0.7
	diffuseMap ("Base (RGB) Trans (A)", 2D) = "white" {}
	alphaCutoff ("Alpha cutoff", Range(0,1)) = 0.5
}

// 2/3 texture stage GPUs
SubShader {
	Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
	LOD 100
	
	// Non-lightmapped
	Pass {
		Tags { "LightMode" = "Vertex" }
		Alphatest Greater [alphaCutoff]
		AlphaToMask True
		ColorMask RGB
		Material {
			Diffuse [diffuseColor]
			Ambient [diffuseColor]
			Shininess [specularAmount]
			Specular [specularColor]
			Emission [emissiveColor]	
		}
		Lighting On
		SeparateSpecular On
		SetTexture [diffuseMap] {
			Combine texture * primary DOUBLE, texture * primary 
		} 
	}
	
	// Lightmapped, encoded as dLDR
	Pass {
		Tags { "LightMode" = "VertexLM" }
		Alphatest Greater [alphaCutoff]
		AlphaToMask True
		ColorMask RGB
		
		BindChannels {
			Bind "Vertex", vertex
			Bind "normal", normal
			Bind "texcoord1", texcoord0 // lightmap uses 2nd uv
			Bind "texcoord", texcoord1 // main uses 1st uv
		}
		SetTexture [unity_Lightmap] {
			matrix [unity_LightmapMatrix]
			constantColor [diffuseColor]
			combine texture * constant
		}
		SetTexture [diffuseMap] {
			combine texture * previous DOUBLE, texture * primary
		}
	}
	
	// Lightmapped, encoded as RGBM
	Pass {
		Tags { "LightMode" = "VertexLMRGBM" }
		Alphatest Greater [alphaCutoff]
		AlphaToMask True
		ColorMask RGB
		
		BindChannels {
			Bind "Vertex", vertex
			Bind "normal", normal
			Bind "texcoord1", texcoord0 // lightmap uses 2nd uv
			Bind "texcoord1", texcoord1 // unused
			Bind "texcoord", texcoord2 // main uses 1st uv
		}
		
		SetTexture [unity_Lightmap] {
			matrix [unity_LightmapMatrix]
			combine texture * texture alpha DOUBLE
		}
		SetTexture [unity_Lightmap] {
			constantColor [diffuseColor]
			combine previous * constant
		}
		SetTexture [diffuseMap] {
			combine texture * previous QUAD, texture * primary
		}
	}
	
	// Pass to render object as a shadow caster
	Pass {
		Name "ShadowCaster"
		Tags { "LightMode" = "ShadowCaster" }
		Offset 1, 1
		
		Fog {Mode Off}
		ZWrite On ZTest Less Cull Off

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_shadowcaster
#pragma fragmentoption ARB_precision_hint_fastest
#include "UnityCG.cginc"

struct v2f { 
	V2F_SHADOW_CASTER;
	float2  uv : TEXCOORD1;
};

uniform float4 diffuseMap_ST;

v2f vert( appdata_base v )
{
	v2f o;
	TRANSFER_SHADOW_CASTER(o)
	o.uv = TRANSFORM_TEX(v.texcoord, diffuseMap);
	return o;
}

uniform sampler2D diffuseMap;
uniform fixed alphaCutoff;
uniform fixed4 diffuseColor;

float4 frag( v2f i ) : COLOR
{
	fixed4 texcol = tex2D( diffuseMap, i.uv );
	clip( texcol.a*diffuseColor.a - alphaCutoff );
	
	SHADOW_CASTER_FRAGMENT(i)
}
ENDCG

	}
	
	// Pass to render object as a shadow collector
	Pass {
		Name "ShadowCollector"
		Tags { "LightMode" = "ShadowCollector" }
		
		Fog {Mode Off}
		ZWrite On ZTest Less

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#pragma multi_compile_shadowcollector

#define SHADOW_COLLECTOR_PASS
#include "UnityCG.cginc"

struct v2f {
	V2F_SHADOW_COLLECTOR;
	float2  uv : TEXCOORD5;
};

uniform float4 diffuseMap_ST;

v2f vert (appdata_base v)
{
	v2f o;
	TRANSFER_SHADOW_COLLECTOR(o)
	o.uv = TRANSFORM_TEX(v.texcoord, diffuseMap);
	return o;
}

uniform sampler2D diffuseMap;
uniform fixed alphaCutoff;
uniform fixed4 diffuseColor;

fixed4 frag (v2f i) : COLOR
{
	fixed4 texcol = tex2D( diffuseMap, i.uv );
	clip( texcol.a*diffuseColor.a - alphaCutoff );
	
	SHADOW_COLLECTOR_FRAGMENT(i)
}
ENDCG

	}
}

// 1 texture stage GPUs
SubShader {
	Tags {"IgnoreProjector"="True" "RenderType"="TransparentCutout"}
	LOD 100
	
	Pass {
		Tags { "LightMode" = "Always" }
		Alphatest Greater [alphaCutoff]
		AlphaToMask True
		ColorMask RGB
		Material {
			Diffuse [diffuseColor]
			Ambient [diffuseColor]
			Shininess [specularAmount]
			Specular [specularColor]
			Emission [emissiveColor]	
		}
		Lighting On
		SeparateSpecular On
		SetTexture [diffuseMap] {
			Combine texture * primary DOUBLE, texture * primary 
		} 
	}	
}
}
