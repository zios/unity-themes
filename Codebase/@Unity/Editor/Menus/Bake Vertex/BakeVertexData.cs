using System;
using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.Menus{
	using Zios.Extensions;
	using Zios.File;
	using Zios.Unity.Colors;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.EditorUI;
	using Zios.Unity.Log;
	using Zios.Unity.Time;
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
			BakeVertexData.complete = false;
			if(EditorApplication.update != BakeVertexData.Step && Selection.gameObjects.Length > 0){
				BakeVertexData.index = 0;
				BakeVertexData.target = Selection.gameObjects[0].transform;
				BakeVertexData.baked = File.GetAsset<Material>("Baked.mat");
				BakeVertexData.bakedOutline = File.GetAsset<Material>("BakedOutline.mat");
				BakeVertexData.bakedShaded = File.GetAsset<Material>("BakedShaded.mat");
				BakeVertexData.renderers = BakeVertexData.target.GetComponentsInChildren<MeshRenderer>();
				BakeVertexData.time = Time.Get();
				ProxyEditor.RecordObjects(BakeVertexData.renderers,"Undo Bake Vertex Colors");
			}
			int passesPerStep = 1000;
			while(passesPerStep > 0){
				EditorApplication.update += BakeVertexData.Step;
				passesPerStep -= 1;
			}
		}
		private static void Step(){
			if(BakeVertexData.complete){ return; }
			Renderer renderer = BakeVertexData.renderers[BakeVertexData.index];
			int size = BakeVertexData.renderers.Length;
			GameObject current = renderer.gameObject;
			MeshFilter filter = current.GetComponent<MeshFilter>();
			string updateMessage = "Mesh " + BakeVertexData.index + "/" + size;
			bool canceled = EditorUI.DrawProgressBar("Baking Vertex Colors",updateMessage,((float)BakeVertexData.index) / size);
			if(canceled){size = 0;}
			else if(filter && filter.sharedMesh && renderer.sharedMaterial){
				bool generateMesh = true;
				string meshPath = File.GetAssetPath(filter.sharedMesh);
				if(meshPath.Contains(".fbx",true)){
					FileData file = File.Find(meshPath);
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
						Mesh existing = File.GetAsset<Mesh>(newPath,false);
						Material targetMaterial = BakeVertexData.baked;
						string shaderName = material.shader.name;
						if(shaderName.Contains("Lighted",true)){ targetMaterial = BakeVertexData.bakedShaded; }
						if(shaderName.Contains("Outline",true)){ targetMaterial = BakeVertexData.bakedOutline; }
						if(existing != null && !complex){
							//Log.Show("[Bake Vertex Colors] Already exists -- " + newPath);
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
						var newMesh = (Mesh)UnityEngine.Object.Instantiate(filter.sharedMesh);
						newMesh.colors32 = colorValues;
						//Log.Show("[Bake Vertex Colors] Generating -- " + newPath);
						File.Create(file.path.GetDirectory() + "/Baked");
						ProxyEditor.CreateAsset(newMesh,newPath);
						filter.sharedMesh = newMesh;
					}
					renderer.sharedMaterials = materials;
				}
			}
			BakeVertexData.index += 1;
			if(BakeVertexData.index >= size){
				var span = TimeSpan.FromSeconds(Time.Get() - BakeVertexData.time);
				string totalTime = span.Minutes + " minutes and " + span.Seconds + " seconds";
				Log.Show("[Bake Vertex Colors] Baked data for " + size + " renderers.");
				Log.Show("[Bake Vertex Colors] Completed in " + totalTime + ".");
				ProxyEditor.SaveAssets();
				EditorUI.ClearProgressBar();
				EditorApplication.update -= BakeVertexData.Step;
				BakeVertexData.complete = true;
			}
		}
	}
}