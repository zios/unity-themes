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
			importer.npotScale = assetName.Contains(".exr") ? TextureImporterNPOTScale.ToNearest : TextureImporterNPOTScale.None;
			if(assetName.ContainsAny("Index","Outline","Shading","Interface")){
				importer.wrapMode = TextureWrapMode.Clamp;
				importer.mipmapEnabled = false;
				if(assetName.ContainsAny("Outline","Shading","Interface")){importer.SetTextureFormat(TextureImporterFormat.BC7);}
				if(assetName.ContainsAny("Index")){importer.SetTextureFormat(TextureImporterFormat.BC5);}
				if(assetName.ContainsAny("Outline","Interface")){importer.filterMode = FilterMode.Trilinear;}
				if(assetName.ContainsAny("Index","Shading")){importer.filterMode = FilterMode.Point;}
			}
		}
	}
}