using UnityEditor;
using UnityEngine;
namespace Zios{
	public static class TextureImporterExtensions{
		public static void SetTextureFormat(this TextureImporter current,TextureImporterFormat format){
			#if UNITY_5_5_OR_NEWER
			var settings = current.GetDefaultPlatformTextureSettings();
			settings.overridden = true;
			settings.format = format;
			settings = current.GetPlatformTextureSettings("Standalone");
			settings.overridden = true;
			settings.format = format;
			#else
			current.textureFormat = format;
			#endif
		}
	}
}