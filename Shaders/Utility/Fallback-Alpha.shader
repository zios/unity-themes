Shader "Hidden/Zios/Fallback/Alpha Vertex Lit"{
Properties{
	diffuseColor ("Diffuse Color", Color) = (1,1,1,1)
	specularColor ("Spec Color", Color) = (1,1,1,0)
	emissiveColor ("Emissive Color", Color) = (0,0,0,0)
	specularAmount ("Shininess", Range (0.1, 1)) = 0.7
	diffuseMap ("Base (RGB) Trans (A)", 2D) = "white"{}
}

// 2/3 texture stage GPUs
SubShader{
	Tags{"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 100
	
	Alphatest Greater 0
	ZWrite Off
	Blend SrcAlpha OneMinusSrcAlpha 
	ColorMask RGB
		
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
}

// 1 texture stage GPUs
SubShader{
	Tags{"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 100
	
	Alphatest Greater 0
	ZWrite Off
	Blend SrcAlpha OneMinusSrcAlpha 
	ColorMask RGB
		
	Pass{
		Tags{ "LightMode" = "Always" }
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
}
}
