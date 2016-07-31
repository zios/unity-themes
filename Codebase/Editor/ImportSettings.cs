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
				importer.importTangents = ModelImporterTangents.None;
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
			importer.textureType = TextureImporterType.Advanced;
			importer.npotScale = TextureImporterNPOTScale.None;
			if(assetName.Contains("Outlines")){
				importer.wrapMode = TextureWrapMode.Clamp;
				importer.textureFormat = TextureImporterFormat.DXT5;
				importer.mipmapEnabled = false;
			}
			if(assetName.Contains("Index") || assetName.Contains("Shading") || importer.assetPath.Contains("Interface")){
				if(!importer.assetPath.Contains("Interface")){
					importer.filterMode = FilterMode.Point;
					importer.wrapMode = TextureWrapMode.Clamp;
				}
				importer.mipmapEnabled = false;
				//if(importer.assetPath.Contains("Index")){
				//	importer.textureFormat = TextureImporterFormat.DXT1;
				//}
				if(assetName.Contains("Shading")){
					importer.textureFormat = TextureImporterFormat.DXT5;
				}
			}
		}
	}
}