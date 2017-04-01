Shader "Zios/(Megashader)#"{
	Properties{
		[Section(General)]
			baseColor("Color",Color) = (1,1,1,1)
			alphaClip("Alpha Clip",Range(0,1)) = 0.1
			[KeywordEnum(Normal,Vertex)] colorMode("Color Mode",Float) = 0
			[Enum(None,0,Front,1,Back,2)] cullMode("Cull",Float) = 2
			[Enum(Off,0,On,1)] zWriteMode("ZWrite",Float) = 1
			[Enum(Less,0,Greater,1,LEqual,2,GEqual,3,Equal,4,NotEqual,5,Always,6)] zTestMode("ZTest",Float) = 4
		[SectionToggle(Texture)]
			textureState("State",Float) = 1
			[KeywordEnum(UV,Triplanar)] textureMapping("Mapping",Float) = 0
			[KeywordEnum(Multiply,Add,Lerp,Overlay,Screen,SoftLight,LinearLight)] textureBlend("Blend",Float) = 0
			[KeywordEnum(All,Shadows,Midtones,Highlights)] textureBlendZone("Blend Range",Float) = 0
			textureMap("Texture",2D) = "white"{}
		[Header(Triplanar)]
			triplanarScale("Scale",Range(0.05,50)) = 1
			[Toggle] xBlending("X Blending",Float) = 1
			[Toggle] yBlending("Y Blending",Float) = 1
			[Toggle] zBlending("Z Blending",Float) = 1
		[Header(Mask)]
			[KeywordEnum(Off,On)] textureMaskState("State",Float) = 1
			textureMaskMap("Mask Texture",2D) = "white"{}
			textureBlendMap("Blend Texture",2D) = "white"{}
		[Header(Reflection)]
			[KeywordEnum(None,Simple,Sphere,View)] reflectionType("Type",Float) = 0
			reflectionColor("Color",Color) = (1,1,1,0.3)
			reflectionMap("Texture",2D) = "white"{}
		[Header(Lighting)]
			[KeywordEnum(None,Lambert,LambertHalf,LambertStepped)] lightingType("Type",Float) = 3
			[Int] lightingSteps("Lighting Steps",Range(2,256)) = 3
			[KeywordEnum(Off,On)] directionalState("Use Directional Lights",Float) = 1
			[KeywordEnum(Off,On)] pointState("Use Point Lights",Float) = 1
		[Header(Shading)]
			[KeywordEnum(Normal,Manual,Texture)] shadingType("Type",Float) = 1
				[Toggle] blendLights("Blend Light Color",Float) = 0
				shadingLitColor("Shading Manual Lit Color",Color) = (1,1,1,1)
				shadingUnlitColor("Shading Manual Unlit Color",Color) = (0,0,0,0.5)
				shadingTexture("Shading Texture",2D) = "white"{}
			[Header(Shadows)]
			[KeywordEnum(Off,On)] shadowState("State",Float) = 1
				//shadowColor("Shadow Color", Color) = (0,0,0,1)
				[Header(Lightmap)]
			[KeywordEnum(Off,On)] lightmapState("State",Float) = 1
				[Header(Visibility)]
			[KeywordEnum(Off,On)] fadeState("Distance Fade",Float) = 1
				[KeywordEnum(Off,On)] distanceCull("Distance Cull",Float) = 1
				[Header(Atlas)]
			[KeywordEnum(Off,On)] atlasState("State",Float) = 0
				atlasUV("Atlas UV",Vector) = (0,0,1,1)
				atlasUVScale("Atlas UV Scale",Vector) = (1,1,0,0)
				[Header(Scroll)]
			[KeywordEnum(Off,On)] scrollState("State",Float) = 0
				scrollX("UV Scroll X",Range(0.0,5)) = 0.05
				scrollY("UV Scroll Y",Range(0.0,5)) = 0.03
				[Header(Warp)]
			[KeywordEnum(None,Offset,Center)] warpType("Type",Float) = 0
				[KeywordEnum(Off,On)] warpDistort("Distort",Float) = 0
				warpFrequency("UV Warp Frequency",Range(0,256)) = 5
				warpPowerX("UV Warp Speed X",Range(-5,5)) = 0.03
				warpPowerY("UV Warp Speed Y",Range(-5,5)) = 0.03
				warpScale("UV Warp Scale",Range(-10,10)) = 0.1
	}
	SubShader{
		Tags{"LightMode"="ForwardBase" "Queue"="Geometry"}
		Pass{
			CGPROGRAM
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			float4 vertexPass(float4 vertex:POSITION) : SV_POSITION{return UnityObjectToClipPos(vertex);}
			fixed4 pixelPass() : SV_Target{return fixed4(0,0,0,1);}
			ENDCG
		}
	}
	Fallback "Hidden/Zios/Fallback/Vertex Lit"
	CustomEditor "Zios.Editors.VariableMaterialEditor"
}
