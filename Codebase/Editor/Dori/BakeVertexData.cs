using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using bake = BakeVertexData;
public static class BakeVertexData{
	static MeshRenderer[] renderers;
	static Transform target;
	static int index = 0;
	static bool force = false;
	static Material baked;
	static Material bakedOutline;
	static Material bakedShaded;
	static float time;
    [MenuItem ("Zios/Dori/Bake Vertex Colors (Force)")]
    static void BakeForce(){
		bake.force = true;
		bake.Bake();
	}
    [MenuItem ("Zios/Dori/Bake Vertex Colors")]
    static void Bake(){
		if(EditorApplication.update != bake.Step && Selection.gameObjects.Length > 0){
			bake.index = 0;
			bake.target = Selection.gameObjects[0].transform;
			bake.baked = FileManager.GetAsset<Material>("Baked.mat");
			bake.bakedOutline = FileManager.GetAsset<Material>("BakedOutline.mat");
			bake.bakedShaded = FileManager.GetAsset<Material>("BakedShaded.mat");
			bake.renderers = bake.target.GetComponentsInChildren<MeshRenderer>();
			bake.time = Time.realtimeSinceStartup;
			EditorApplication.update += bake.Step;
			Undo.RecordObjects(bake.renderers,"Undo Bake Vertex Colors");
		}
	}
	static void Step(){
		Renderer renderer = bake.renderers[bake.index];
		int size = bake.renderers.Length;
		GameObject current = renderer.gameObject;
		MeshFilter filter = current.GetComponent<MeshFilter>();
		string updateMessage = "Mesh " + bake.index + "/" + size;
		bool canceled = EditorUtility.DisplayCancelableProgressBar("Baking Vertex Colors",updateMessage,((float)bake.index)/size);
		if(canceled){size = 0;}
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
					string colorValue = color.ToString().Strip("RGBA("," ",",",")");
					string pathID = complex ? "" : "-" + colorValue;
					newPath = newPath.Replace("%%",pathID);
					Mesh existing = FileManager.GetAsset<Mesh>(newPath,false);
					Material targetMaterial = bake.baked;
					string shaderName = material.shader.name;
					if(shaderName.Contains("Lighted",true)){targetMaterial = bake.bakedShaded;}
					if(shaderName.Contains("Outline",true)){targetMaterial = bake.bakedOutline;}
					if(existing != null && !force){
						//Debug.Log("Bake Vertex Colors : Already exists -- " + newPath);
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
					//Debug.Log("Bake Vertex Colors : Generating -- " + newPath);
					Directory.CreateDirectory(Path.GetDirectoryName(file.path)+"/Baked");
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
			Debug.Log("Bake Vertex Colors : Baked data for " + size + " renderers.");
			Debug.Log("Bake Vertex Colors : Completed in " + totalTime + ".");
			AssetDatabase.SaveAssets();
			EditorUtility.ClearProgressBar();
			EditorApplication.update -= bake.Step;
			bake.force = false;
		}
	}
}