using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using combine = CombineMeshes;
public static class CombineMeshes{
	static List<Mesh> meshes = new List<Mesh>();
	static GameObject[] selection;
	static Transform target;
	static MeshFilter[] filters;
	static CombineInstance[] combines;
	static int index;
	static int subIndex;
	static int meshCount;	
	static int vertexCount;
	static int meshNumber = 1;
	static float time;
    [MenuItem ("Zios/Dori/Combine Meshes")]
    static void Combine(){
		if(EditorApplication.update != combine.Step && Selection.gameObjects.Length > 0){
			List<MeshFilter> filters = new List<MeshFilter>();
			combine.meshes.Clear();
			combine.meshes.Add(new Mesh());
			combine.selection = Selection.gameObjects.Copy();
			foreach(GameObject current in combine.selection){
				filters.AddRange(current.GetComponentsInChildren<MeshFilter>());
			}
			combine.filters = filters.ToArray();
			combine.meshCount = combine.filters.Length;
			combine.combines = new CombineInstance[combine.meshCount];
			combine.index = 0;
			combine.subIndex = 0;
			combine.vertexCount = 0;
			combine.time = Time.realtimeSinceStartup;
			EditorApplication.update += combine.Step;
		}
	}
	static void StepLast(){
		int end = combine.index - combine.subIndex;
		List<CombineInstance> range = new List<CombineInstance>(combine.combines).GetRange(combine.subIndex,end);
		combine.meshes.Last().CombineMeshes(range.ToArray());
	}
	static void Step(){
		int index = combine.index;
		MeshFilter filter = combine.filters[index];
		string updateMessage = "Mesh " + index + "/" + combine.meshCount;
		bool canceled = EditorUtility.DisplayCancelableProgressBar("Combining Meshes",updateMessage,((float)index)/combine.meshCount);
		if(canceled){combine.meshCount = 0;}
		else if(filter != null && filter.sharedMesh != null){
			if((combine.vertexCount + filter.sharedMesh.vertexCount) >= 65534){
				Debug.Log("Combine Meshes : Added extra submesh due to vertices at " + combine.vertexCount);
				combine.StepLast();
				combine.meshes.Add(new Mesh());
				combine.subIndex = index;
				combine.vertexCount = 0;
			}
			Mesh currentMesh = filter.sharedMesh;
			if(filter.sharedMesh.subMeshCount > 1){
				currentMesh = (Mesh)UnityEngine.Object.Instantiate(filter.sharedMesh);
				currentMesh.triangles = currentMesh.triangles;
			}
			combine.combines[index].mesh = currentMesh;
			combine.combines[index].transform = filter.transform.localToWorldMatrix;
			combine.vertexCount += currentMesh.vertexCount;
			GameObject target = (GameObject)GameObject.Instantiate(filter.gameObject);
			target.name = target.name.Replace("(Clone)","");
			target.transform.parent = Locate.GetScenePath("Scene-Combined").transform;
			Component.DestroyImmediate(target.GetComponent<MeshRenderer>());
			Component.DestroyImmediate(target.GetComponent<MeshFilter>());
			filter.gameObject.SetActive(false);
		}
		combine.index += 1;
		if(combine.index >= combine.meshCount){
			if(!canceled){
				combine.StepLast();
				Material material = FileManager.GetAsset<Material>("BakedOutline.mat");
				foreach(Mesh mesh in combine.meshes){
					GameObject container = Locate.GetScenePath("Scene-Combined/@Mesh" + combine.meshNumber);
					MeshRenderer containerRenderer = container.AddComponent<MeshRenderer>();
					MeshFilter containerFilter = container.AddComponent<MeshFilter>();
					string path = Path.GetDirectoryName(EditorApplication.currentScene);
					string folder = "@" + Path.GetFileName(EditorApplication.currentScene).Replace(".unity","");
					Directory.CreateDirectory(path+"/"+folder+"/");
					AssetDatabase.CreateAsset(mesh,path+"/"+folder+"/Combined"+meshNumber+".asset");
					containerFilter.mesh = mesh;
					containerRenderer.material = new Material(material);
					combine.meshNumber += 1;
				}
				foreach(GameObject current in combine.selection){current.SetActive(false);}
				foreach(MeshFilter current in combine.filters){current.gameObject.SetActive(true);}
			}
			TimeSpan span = TimeSpan.FromSeconds(Time.realtimeSinceStartup - combine.time);
			string totalTime = span.Minutes + " minutes and " + span.Seconds + " seconds";
			Debug.Log("Combine Meshes : Reduced " + combine.meshCount + " meshes to " + combine.meshes.Count + ".");
			Debug.Log("Combine Meshes : Completed in " + totalTime + ".");
			AssetDatabase.SaveAssets();
			EditorUtility.ClearProgressBar();
			EditorApplication.update -= combine.Step;
		}
	}
}