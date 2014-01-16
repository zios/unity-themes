using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
public class ImportSettings : AssetPostprocessor{
	public void OnPreprocessModel(){
		ModelImporter importer = (ModelImporter)assetImporter;
		if(importer.importMaterials){
			importer.globalScale = 0.01f;
			importer.meshCompression = ModelImporterMeshCompression.High;
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
			importer.animationWrapMode = WrapMode.ClampForever;
		}
	}
	public void OnPreprocessTexture(){
		TextureImporter importer = (TextureImporter)assetImporter;
		importer.textureType = TextureImporterType.Advanced;
		/*if(importer.textureFormat != TextureImporterFormat.AutomaticTruecolor){
			importer.textureFormat = TextureImporterFormat.AutomaticTruecolor;
			importer.mipmapEnabled = false;
			if(importer.assetPath.Contains("Index") || importer.assetPath.Contains("Shading")){
				importer.filterMode = FilterMode.Point;
				if(importer.assetPath.Contains("Shading")){
					importer.wrapMode = TextureWrapMode.Clamp;
				}
			}
			if(importer.assetPath.Contains("Normal")){}
			if(importer.assetPath.Contains("Atlas")){
				//importer.isReadable = true;
			}
		}*/
	}
	public void OnPreprocessAudio(){
		AudioImporter importer = (AudioImporter)assetImporter;
		//importer.format = AudioImporterFormat.Compressed;
		if(importer.compressionBitrate != 48000){
			//importer.compressionBitrate = 48000;
			//importer.forceToMono = true;
			importer.threeD = false;
			//importer.loadType = AudioImporterLoadType.DecompressOnLoad;
		}
	}
}
