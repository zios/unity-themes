//====================================
// General
//====================================
using UnityEngine;
using System;
using System.Collections.Generic;
//====================================
// Enumerations
//====================================
namespace ShaderExtended{
	public enum MultiCompile{
		None,
		multi_compile_fwdbase,
		multi_compile_fwdadd,
		multi_compile_fwdbase_fullshadows,
		multi_compile_fwdadd_fullshadows,
		multi_compile_vertex,
		multi_compile_shadowcaster,
		multi_compile_shadowcollector,
	}
	public enum SurfaceOption{
		None,
		alpha,
		alphaTest,
		finalcolor,
		exclude_path__prepass,
		exclude_path__forward,
		addshadow,
		dualforward,
		fullforwardshadows,
		decal__add,
		decal__blend,
		softvegetation,
		noambient,
		novertexlights,
		nolightmap,
		nodirlightmap,
		noforwardadd,
		approxview,
		halfasview,
		//tesselate__<function>
		//vertex__<function>
	}
	public enum Renderer{
		d3d9,
		d3d11,
		openGL,
		gles,
		xbox360,
		ps3,
		flash,
	}
	public enum RendererOption{
		None,
		noshadows,
		shaderonly
	}
	public enum FogOption{
		None,
		ARB_fog_exp,
		ARB_fog_exp2,
		ARB_fog_linear
	}
	public enum PrecisionOption{
		None,
		ARB_precision_hint_fastest,
		ARB_precision_hint_nicest
	}
	public enum ColorOption{
		None,
		ARB_draw_buffers,
		ATI_draw_buffers
	}
	public enum ShadowOption{
		None,
		ARB_fragment_program_shadow
	}
	[Serializable]
	public class FragmentOptions{
		FogOption fog = FogOption.None;
		PrecisionOption precision = PrecisionOption.None;
		ColorOption color = ColorOption.None;
		ShadowOption shadow = ShadowOption.None;
	}
	[Serializable]
	public class ExtendedShader{
		public bool debug;                                  // #pragma debug
		public bool forceGLSL;			                    // #pragma glsl
		public bool disableGLSLNormalize;                   // #pragma glsl_no_auto_normalization
		public string vertexShader;                         // #pragma vertex <name>
		public string pixelShader;                          // #pragma fragment <name>
		public string geometryShader;                       // #pragma geometry <name>
		public string hullShader;                           // #pragma hull <name>
		public string domainShader;                         // #pragma domain <name>
		public int shaderModel;                             // #pragma target <number>
		public List<Renderer> onlyRenderers;                // #pragma only_renderers <renderer>
		public List<Renderer> excludeRenderers;             // #pragma exclude_renderers <renderer>
		public List<RendererOption> excludeOptions;         // <noshadows,shaderonly>
		public List<SurfaceOption> multiCompileOptions;     // <nolightmap>
		public MultiCompile multiCompile;                   // #pragma multi_compile_<name>
		public FragmentOptions options;                     // #pragma fragmentoption <option>
	}
	[Serializable]
	public class ExtendedShaderEditor{
		public bool showProperties;
		public bool showExtendedPragma;
	}
}

/*

+ Setup (Pragma/Include/Options)
+ Vertex Input
+ Pixel Input
+ Vertex Shader
+ Pixel Shader

*/