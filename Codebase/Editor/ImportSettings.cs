using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Zios.Editors{
	public class ImportSettings : AssetPostprocessor{
		public void OnPreprocessModel(){
			ModelImporter importer = (ModelImporter)assetImporter;
			if(importer.assetPath.ContainsAny(".fbx")){
				importer.globalScale = 1.0f;
			}
			if(importer.importMaterials){
				importer.meshCompression = ModelImporterMeshCompression.Off;
				importer.optimizeMesh = true;
				importer.importMaterials = false;
				#if UNITY_5_3_OR_NEWER
				importer.importTangents = ModelImporterTangents.None;
				#else
				importer.tangentImportMode = ModelImporterTangentSpaceMode.None;
				#endif
				importer.animationCompression = ModelImporterAnimationCompression.Off;
				importer.animationType = ModelImporterAnimationType.None;
				importer.generateAnimations = ModelImporterGenerateAnimations.None;
			}
			if(importer.assetPath.ContainsAny("@","Shared","Animation","Actions") && importer.animationType != ModelImporterAnimationType.Legacy){
				importer.animationType = ModelImporterAnimationType.Legacy;
				importer.generateAnimations = ModelImporterGenerateAnimations.GenerateAnimations;
				importer.animationCompression = ModelImporterAnimationCompression.Off;
				importer.animationPositionError = 0;
				importer.animationWrapMode = WrapMode.ClampForever;
				importer.importAnimation = true;
			}
		}
		public void OnPreprocessTexture(){
			TextureImporter importer = (TextureImporter)assetImporter;
			string assetName = importer.assetPath.Split("/").Last();
			importer.npotScale = assetName.Contains(".exr") ? TextureImporterNPOTScale.ToNearest : TextureImporterNPOTScale.None;
			if(assetName.ContainsAny("Index","Outlines","Shading","Interface")){
				importer.filterMode = FilterMode.Point;
				importer.wrapMode = TextureWrapMode.Clamp;
				importer.mipmapEnabled = false;
				importer.SetTextureFormat(TextureImporterFormat.RGBA32);
			}
		}
	}
}