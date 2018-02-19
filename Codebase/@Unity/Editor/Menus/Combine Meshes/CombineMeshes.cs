using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.Menus{
	using Zios.Extensions;
	using Zios.File;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.EditorUI;
	using Zios.Unity.Locate;
	using Zios.Unity.Log;
	using Zios.Unity.Time;
	using Class = CombineMeshes;
	public static class CombineMeshes{
		private static List<Mesh> meshes = new List<Mesh>();
		private static GameObject[] selection;
		private static MeshFilter[] filters;
		private static CombineInstance[] combines;
		private static int index;
		private static int subIndex;
		private static int meshCount;
		private static int vertexCount;
		private static int meshNumber = 1;
		private static float time;
		private static bool inline;
		private static bool complete;
		private static string path;
		[MenuItem("Zios/Utility/Combine Meshes")]
		private static void Combine(){
			if(Selection.gameObjects.Length < 1){return;}
			List<MeshFilter> filters = new List<MeshFilter>();
			Class.meshes.Clear();
			Class.meshes.Add(new Mesh());
			Class.selection = Selection.gameObjects.Copy();
			foreach(GameObject current in Class.selection){
				filters.AddRange(current.GetComponentsInChildren<MeshFilter>());
			}
			Class.filters = filters.ToArray();
			Class.meshCount = Class.filters.Length;
			Class.combines = new CombineInstance[Class.meshCount];
			Class.index = 0;
			Class.subIndex = 0;
			Class.vertexCount = 0;
			Class.time = Time.Get();
			Class.complete = false;
			Class.inline = true;
			int passesPerStep = 1000;
			while(passesPerStep > 0){
				EditorApplication.update += Class.Step;
				passesPerStep -= 1;
			}
		}
		private static void StepLast(){
			int end = Class.index - Class.subIndex;
			List<CombineInstance> range = new List<CombineInstance>(Class.combines).GetRange(Class.subIndex,end);
			Mesh finalMesh = Class.meshes.Last();
			finalMesh.CombineMeshes(range.ToArray());
			Unwrapping.GenerateSecondaryUVSet(finalMesh);
		}
		private static void Step(){
			if(Class.complete){ return; }
			int index = Class.index;
			MeshFilter filter = Class.filters[index];
			string updateMessage = "Mesh " + index + "/" + Class.meshCount;
			bool canceled = EditorUI.DrawProgressBar("Combining Meshes",updateMessage,((float)index) / Class.meshCount);
			if(canceled){ Class.meshCount = 0; }
			else if(filter != null && filter.sharedMesh != null){
				if((Class.vertexCount + filter.sharedMesh.vertexCount) >= 65534){
					Log.Show("[Combine Meshes] Added extra submesh due to vertices at " + Class.vertexCount);
					Class.StepLast();
					Class.meshes.Add(new Mesh());
					Class.subIndex = index;
					Class.vertexCount = 0;
				}
				Mesh currentMesh = filter.sharedMesh;
				if(filter.sharedMesh.subMeshCount > 1){
					currentMesh = (Mesh)UnityEngine.Object.Instantiate(filter.sharedMesh);
					currentMesh.triangles = currentMesh.triangles;
				}
				Class.combines[index].mesh = currentMesh;
				Class.combines[index].transform = filter.transform.localToWorldMatrix;
				Class.vertexCount += currentMesh.vertexCount;
				if(Class.inline){
					Component.DestroyImmediate(filter.gameObject.GetComponent<MeshRenderer>());
					Component.DestroyImmediate(filter.gameObject.GetComponent<MeshFilter>());
				}
			}
			Class.index += 1;
			if(Class.index >= Class.meshCount){
				if(!canceled){
					Class.StepLast();
					//Material material = File.GetAsset<Material>("Baked.mat");
					if(!Class.inline){
						foreach(GameObject current in Class.selection){
							GameObject target = (GameObject)GameObject.Instantiate(current);
							target.name = target.name.Replace("(Clone)","");
							target.transform.parent = Locate.GetScenePath("Scene-Combined").transform;
							MeshFilter[] filters = target.GetComponentsInChildren<MeshFilter>();
							foreach(MeshFilter nullFilter in filters){
								Component.DestroyImmediate(nullFilter.gameObject.GetComponent<MeshRenderer>());
								Component.DestroyImmediate(nullFilter.gameObject.GetComponent<MeshFilter>());
							}
							current.SetActive(false);
						}
					}
					bool singleRoot = Class.selection.Length == 1;
					string start = singleRoot ? Class.selection[0].name + "/" : "";
					foreach(Mesh mesh in Class.meshes){
						GameObject container = new GameObject("@Mesh" + Class.meshNumber);
						if(Class.inline && singleRoot){
							container.transform.parent = Class.selection[0].transform;
						}
						else{
							container.transform.parent = Locate.GetScenePath("Scene-Combined/" + start).transform;
						}
						//MeshRenderer containerRenderer = container.AddComponent<MeshRenderer>();
						MeshFilter containerFilter = container.AddComponent<MeshFilter>();
						if(Class.path.IsEmpty()){
							Class.path = EditorUtility.SaveFolderPanel("Combine Meshes",Application.dataPath,"").GetAssetPath();
						}
						File.Create(path);
						ProxyEditor.CreateAsset(mesh,path+"/Combined"+meshNumber+".asset");
						containerFilter.mesh = mesh;
						//containerRenderer.material = new Material(material);
						Class.meshNumber += 1;
					}
				}
				TimeSpan span = TimeSpan.FromSeconds(Time.Get() - Class.time);
				string totalTime = span.Minutes + " minutes and " + span.Seconds + " seconds";
				Log.Show("[Combine Meshes] Reduced " + Class.meshCount + " meshes to " + Class.meshes.Count + ".");
				Log.Show("[Combine Meshes] Completed in " + totalTime + ".");
				ProxyEditor.SaveAssets();
				EditorUI.ClearProgressBar();
				Class.complete = true;
				while(EditorApplication.update == Class.Step){
					EditorApplication.update -= Class.Step;
				}
			}
		}
	}
}