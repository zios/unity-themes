using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
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
			importer.tangentImportMode = ModelImporterTangentSpaceMode.None;
			importer.animationCompression = ModelImporterAnimationCompression.Off;
			importer.animationType = ModelImporterAnimationType.None;
			importer.generateAnimations = ModelImporterGenerateAnimations.None;
		}
		if(importer.assetPath.ContainsAny("@","Shared","Animation") && importer.animationType != ModelImporterAnimationType.Legacy){
			importer.animationType = ModelImporterAnimationType.Legacy;
			importer.generateAnimations = ModelImporterGenerateAnimations.GenerateAnimations;
			importer.animationCompression = ModelImporterAnimationCompression.KeyframeReduction;
			importer.animationPositionError = 0;
			importer.animationWrapMode = WrapMode.ClampForever;
			importer.importAnimation = true;
		}
	}
	public void OnPreprocessTexture(){
		TextureImporter importer = (TextureImporter)assetImporter;
		importer.textureType = TextureImporterType.Advanced;
		if(importer.assetPath.Contains("Outlines")){
			importer.wrapMode = TextureWrapMode.Clamp;
			importer.textureFormat = TextureImporterFormat.DXT5;
			importer.mipmapEnabled = false;
		}
		if(importer.assetPath.Contains("Index") || importer.assetPath.Contains("Shading")){
			importer.filterMode = FilterMode.Point;
			importer.wrapMode = TextureWrapMode.Clamp;
			importer.mipmapEnabled = false;
			//if(importer.assetPath.Contains("Index")){
			//	importer.textureFormat = TextureImporterFormat.DXT1;
			//}
			if(importer.assetPath.Contains("Shading")){
				importer.textureFormat = TextureImporterFormat.DXT5;
			}
		}
	}
}
