using System;
using UnityEditor;
using UnityEngine;
namespace Zios.Editors{
	using Interface;
	using Class = BakeVertexData;
	using Undo = UnityEditor.Undo;
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
			Class.complete = false;
			if(EditorApplication.update != Class.Step && Selection.gameObjects.Length > 0){
				Class.index = 0;
				Class.target = Selection.gameObjects[0].transform;
				Class.baked = FileManager.GetAsset<Material>("Baked.mat");
				Class.bakedOutline = FileManager.GetAsset<Material>("BakedOutline.mat");
				Class.bakedShaded = FileManager.GetAsset<Material>("BakedShaded.mat");
				Class.renderers = Class.target.GetComponentsInChildren<MeshRenderer>();
				Class.time = Time.realtimeSinceStartup;
				Undo.RecordObjects(Class.renderers,"Undo Bake Vertex Colors");
			}
			int passesPerStep = 1000;
			while(passesPerStep > 0){
				EditorApplication.update += Class.Step;
				passesPerStep -= 1;
			}
		}
		private static void Step(){
			if(Class.complete){ return; }
			Renderer renderer = Class.renderers[Class.index];
			int size = Class.renderers.Length;
			GameObject current = renderer.gameObject;
			MeshFilter filter = current.GetComponent<MeshFilter>();
			string updateMessage = "Mesh " + Class.index + "/" + size;
			bool canceled = EditorUI.DrawProgressBar("Baking Vertex Colors",updateMessage,((float)Class.index) / size);
			if(canceled){ size = 0; }
			else if(filter && filter.sharedMesh && renderer.sharedMaterial){
				bool generateMesh = true;
				string meshPath = FileManager.GetPath(filter.sharedMesh);
				if(meshPath.Contains(".fbx",true)){
					FileData file = FileManager.Find(meshPath);
					int subMeshIndex = 0;
					string newPath = file.path.GetAssetPath() + "Baked/" + file.name + "%%.asset";
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
						Material targetMaterial = Class.baked;
						string shaderName = material.shader.name;
						if(shaderName.Contains("Lighted",true)){ targetMaterial = Class.bakedShaded; }
						if(shaderName.Contains("Outline",true)){ targetMaterial = Class.bakedOutline; }
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
						FileManager.Create(file.path.GetDirectory() + "/Baked");
						AssetDatabase.CreateAsset(newMesh,newPath);
						filter.sharedMesh = newMesh;
					}
					renderer.sharedMaterials = materials;
				}
			}
			Class.index += 1;
			if(Class.index >= size){
				TimeSpan span = TimeSpan.FromSeconds(Time.realtimeSinceStartup - Class.time);
				string totalTime = span.Minutes + " minutes and " + span.Seconds + " seconds";
				Debug.Log("[Bake Vertex Colors] Baked data for " + size + " renderers.");
				Debug.Log("[Bake Vertex Colors] Completed in " + totalTime + ".");
				AssetDatabase.SaveAssets();
				EditorUI.ClearProgressBar();
				EditorApplication.update -= Class.Step;
				Class.complete = true;
			}
		}
	}
}