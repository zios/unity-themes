using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using bake = Zios.BakeVertexData;
namespace Zios{
	public static class BakeVertexData{
		private static MeshRenderer[] renderers;
		private static Transform target;
		private static int index = 0;
		private static Material baked;
		private static Material bakedOutline;
		private static Material bakedShaded;
		private static float time;
		private static bool complete;
		[MenuItem("Zios/Dori/Bake Vertex Colors")]
		private static void Bake(){
			bake.complete = false;
			if(EditorApplication.update != bake.Step && Selection.gameObjects.Length > 0){
				bake.index = 0;
				bake.target = Selection.gameObjects[0].transform;
				bake.baked = FileManager.GetAsset<Material>("Baked.mat");
				bake.bakedOutline = FileManager.GetAsset<Material>("BakedOutline.mat");
				bake.bakedShaded = FileManager.GetAsset<Material>("BakedShaded.mat");
				bake.renderers = bake.target.GetComponentsInChildren<MeshRenderer>();
				bake.time = Time.realtimeSinceStartup;
				Undo.RecordObjects(bake.renderers,"Undo Bake Vertex Colors");
			}
			int passesPerStep = 1000;
			while(passesPerStep > 0){
				EditorApplication.update += bake.Step;
				passesPerStep -= 1;
			}
		}
		private static void Step(){
			if(bake.complete){ return; }
			Renderer renderer = bake.renderers[bake.index];
			int size = bake.renderers.Length;
			GameObject current = renderer.gameObject;
			MeshFilter filter = current.GetComponent<MeshFilter>();
			string updateMessage = "Mesh " + bake.index + "/" + size;
			bool canceled = EditorUtility.DisplayCancelableProgressBar("Baking Vertex Colors",updateMessage,((float)bake.index) / size);
			if(canceled){ size = 0; }
			else if(filter && filter.sharedMesh && renderer.sharedMaterial){
				bool generateMesh = true;
				string meshPath = FileManager.GetPath(filter.sharedMesh);
				if(meshPath.Contains(".fbx",true)){
					FileData file = FileManager.Find(meshPath);
					int subMeshIndex = 0;
					string newPath = file.GetAssetPath(false) + "Baked/" + file.name + "%%.asset";
					int vertexCount = filter.sharedMesh.vertices.Length;
					Color32[] colorValues = new Color32[vertexCount];
					Material[] materials = renderer.sharedMaterials;
					bool complex = renderer.sharedMaterials.Length > 1;
					foreach(Material material in materials){
						bool hasColor = material.HasProperty("_Color");
						Color32 color = hasColor ? material.GetColor("_Color") : Colors.Get("Violet");
						//color.a = shaderName.Contains("Outline",true) ? 255 : 0;
						string colorValue = color.ToString().Remove("RGBA("," ",",",")");
						string pathID = complex ? "" : "-" + colorValue;
						newPath = newPath.Replace("%%",pathID);
						Mesh existing = FileManager.GetAsset<Mesh>(newPath,false);
						Material targetMaterial = bake.baked;
						string shaderName = material.shader.name;
						if(shaderName.Contains("Lighted",true)){ targetMaterial = bake.bakedShaded; }
						if(shaderName.Contains("Outline",true)){ targetMaterial = bake.bakedOutline; }
						if(existing != null && !complex){
							//Debug.Log("[Bake Vertex Colors] Already exists -- " + newPath);
							filter.sharedMesh = existing;
							materials[subMeshIndex] = targetMaterial;
							generateMesh = false;
							subMeshIndex += 1;
							continue;
						}
						int[] indices = filter.sharedMesh.GetIndices(subMeshIndex);
						foreach(int index in indices){
							colorValues[index] = color;
						}
						materials[subMeshIndex] = targetMaterial;
						subMeshIndex += 1;
					}
					if(generateMesh){
						Mesh newMesh = (Mesh)UnityEngine.Object.Instantiate(filter.sharedMesh);
						newMesh.colors32 = colorValues;
						//Debug.Log("[Bake Vertex Colors] Generating -- " + newPath);
						Directory.CreateDirectory(Path.GetDirectoryName(file.path) + "/Baked");
						AssetDatabase.CreateAsset(newMesh,newPath);
						filter.sharedMesh = newMesh;
					}
					renderer.sharedMaterials = materials;
				}
			}
			bake.index += 1;
			if(bake.index >= size){
				TimeSpan span = TimeSpan.FromSeconds(Time.realtimeSinceStartup - bake.time);
				string totalTime = span.Minutes + " minutes and " + span.Seconds + " seconds";
				Debug.Log("[Bake Vertex Colors] Baked data for " + size + " renderers.");
				Debug.Log("[Bake Vertex Colors] Completed in " + totalTime + ".");
				AssetDatabase.SaveAssets();
				EditorUtility.ClearProgressBar();
				EditorApplication.update -= bake.Step;
				bake.complete = true;
			}
		}
	}
}