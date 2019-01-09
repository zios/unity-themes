using UnityEditor;
namespace Zios.Unity.Editor.Extensions{
	using Zios.Extensions.Convert;
	public static class TextureImporterExtensions{
		public static void SetTextureFormat(this TextureImporter current,TextureImporterFormat format){
			#if UNITY_5_5_OR_NEWER
			var settings = current.GetPlatformTextureSettings("Standalone");
			settings.overridden = true;
			settings.format = format;
			current.SetPlatformTextureSettings(settings);
			#else
			current.textureFormat = format;
			#endif
		}
		public static void SetTextureType(this TextureImporter current,string type){
			#if UNITY_5_5_OR_NEWER
			if(type == "Advanced"){type = "Default";}
			#endif
			current.textureType = type.ToEnum<TextureImporterType>();
		}
	}
}