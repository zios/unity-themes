using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.Windows{
	using Zios.Extensions;
	using Zios.File;
	using Zios.Unity.Colors;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.EditorUI;
	using Zios.Unity.Editor.Extensions;
	public class Operations : EditorWindow{
		public bool preserveScale = true;
		public ReplaceSelector optionA = new ReplaceSelector();
		public ReplaceSelector optionB = new ReplaceSelector();
		public bool materialIncludeInactive = true;
		public string materialRenameFrom;
		public string materialRenameTo;
		public string renameFrom;
		public string renameTo;
		public string selectName;
		public string selectComponent;
		public string shaderName;
		[MenuItem("Zios/Window/Operations",false,0)]
		private static void Init(){
			Operations window = (Operations)EditorWindow.GetWindow(typeof(Operations));
			window.position = new Rect(100,150,200,200);
			window.Start();
		}
		public void Start(){}
		public void OnGUI(){
			EditorWindowExtensions.SetTitle(this, "Utility");
			GUILayout.Label("--------------------------------------------------------------");
			this.DrawMeshCombiner();
			GUILayout.Label("--------------------------------------------------------------");
			this.DrawMaterialReplacer();
			GUILayout.Label("--------------------------------------------------------------");
			this.DrawPrefaber();
			GUILayout.Label("--------------------------------------------------------------");
			this.DrawMaterialSelector();
			GUILayout.Label("--------------------------------------------------------------");
			this.DrawSelector();
			GUILayout.Label("--------------------------------------------------------------");
			this.DrawShaderSelector();
			//GUILayout.Label("--------------------------------------------------------------");
			//this.DrawAssigner();
			GUILayout.Label("--------------------------------------------------------------");
			this.DrawRenamer();
			GUILayout.Label("--------------------------------------------------------------");
			this.DrawSelectorRow(this.optionA,"Replace");
			this.DrawSelectorRow(this.optionB,"With");
			bool ready = this.optionA.found.Count>0 && this.optionB.found.Count>0;
			if(!ready){return;}
			if(this.optionA.type == ReplaceOption.Prefab && this.optionA.targetPath == ""){
				GUI.skin.label.normal.textColor = Colors.names["chestnut"];
				string warning = "Actual .prefab source must exist to perform replacement.";
				GUI.Label(new Rect(5,55,400,20),warning);
				GUI.skin.label.normal.textColor = Color.white;
			}
			else{
				GUILayout.BeginHorizontal("");
				if(GUILayout.Button("Apply",GUILayout.Width(100))){
					this.PerformReplace();
				}
				this.preserveScale = GUILayout.Toggle(this.preserveScale,"Preserve Scale",GUILayout.Width(120));
				GUILayout.EndHorizontal();
			}
		}
		public void DrawMeshCombiner(){
			GUILayout.BeginHorizontal("box");
			if("Combine Selected Meshes".ToLabel().Layout(200).DrawButton()){
				var meshes = new Dictionary<Mesh,Transform>();
				var instance = new GameObject("Combined").AddComponent<SkinnedMeshRenderer>();
				instance.sharedMesh = new Mesh();
				foreach(var target in Selection.gameObjects){
					var filter = target.GetComponentInChildren<MeshFilter>();
					var skinned = target.GetComponentInChildren<SkinnedMeshRenderer>();
					var mesh = !filter.IsNull() ? filter.sharedMesh : null;
					mesh = !skinned.IsNull() ? skinned.sharedMesh : mesh;
					meshes[mesh] = target.transform;
				}
				var combine = new CombineInstance[meshes.Count];
				var meshTargets = meshes.Keys.ToList();
				var componentTargets = meshes.Values.ToList();
				for(int index=0;index<meshes.Count;++index){
					combine[index].mesh = meshTargets[index];
					combine[index].transform = componentTargets[index].localToWorldMatrix;
					componentTargets[index].gameObject.SetActive(false);
				}
				instance.sharedMesh.CombineMeshes(combine);
			}
			GUILayout.EndHorizontal();
		}
		//====================================
		// Replace
		//====================================
		public void PerformReplace(){
			if(this.optionA.type == ReplaceOption.Prefab){
				ProxyEditor.RegisterSceneUndo("Replace Operation");
				Object prefab = PrefabUtility.CreateEmptyPrefab(this.optionA.targetPath);
				PrefabUtility.ReplacePrefab(this.optionB.found.First(),prefab);
			}
			if(this.optionA.type == ReplaceOption.GameObject){
				foreach(GameObject current in new List<GameObject>(this.optionA.found)){
					GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(this.optionB.found.First());
					newObject.transform.parent = current.transform.parent;
					newObject.transform.position = current.transform.position;
					newObject.transform.rotation = current.transform.rotation;
					if(this.preserveScale){newObject.transform.localScale = current.transform.localScale;}
					DestroyImmediate(current);
				}
			}
		}
		public void DrawSelectorRow(ReplaceSelector selector,string label){
			GUILayout.BeginHorizontal("box");
			EditorGUILayout.LabelField(label,GUILayout.Width(60));
			selector.type = (ReplaceOption)EditorGUILayout.EnumPopup(selector.type,GUILayout.Width(100));
			bool isGameObject = selector.type == ReplaceOption.GameObject;
			selector.manual = EditorGUILayout.Toggle(selector.manual,GUILayout.Width(15));
			if(selector.manual){
				if(selector.targetObject != null){
					selector.targetObject = null;
					selector.found.Clear();
				}
				selector.targetName = EditorGUILayout.TextField(selector.targetName,GUILayout.Width(150));
				if(selector.targetName != ""){this.DrawFound(selector,selector.targetName);}
			}
			else{
				if(selector.targetName != ""){
					selector.targetName = "";
					selector.found.Clear();
				}
				selector.targetObject = EditorGUILayout.ObjectField(selector.targetObject,typeof(GameObject),isGameObject,GUILayout.Width(168));
				if(selector.targetObject != null){this.DrawFound(selector,selector.targetObject.name);}
			}
			GUILayout.EndHorizontal();
		}
		public void DrawFound(ReplaceSelector selector,string name){
			selector.found.Clear();
			selector.targetPath = "";
			if(selector.type == ReplaceOption.GameObject){
				GameObject[] all = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
				foreach(GameObject current in all){
					bool allowed = current != null && (current.transform.parent == null || current.transform.parent.name != name);
					if(allowed && current.name == name){
						selector.found.Add(current);
					}
				}
				EditorGUILayout.LabelField(selector.found.Count + " objects found.");
			}
			else if(selector.type == ReplaceOption.Prefab){
				bool exists = false;
				FileData file = File.Find(name+".prefab");
				GameObject target = (GameObject)selector.targetObject;
				if(file != null){
					selector.targetPath = file.GetAssetPath();
					target = file.GetAsset<GameObject>();
				}
				if(target != null){
					selector.found.Add(target);
					exists = true;
				}
				string result = exists ? "Found." : "Not Found.";
				string note = exists ? "Valid .prefab source." : "";
				GUI.skin.label.normal.textColor = Colors.names["gray"];
				if(exists && selector.targetPath == ""){
					GUI.skin.label.normal.textColor = Colors.names["orange"];
					note = "Not a .prefab source.";
				}
				EditorGUILayout.LabelField(result,GUILayout.Width(65));
				EditorGUILayout.LabelField(note,GUI.skin.label);
				GUI.skin.label.normal.textColor = Color.white;
			}
		}
		//====================================
		// Rename
		//====================================
		public void DrawRenamer(){
			GUILayout.BeginHorizontal("box");
			EditorGUILayout.LabelField("Rename",GUILayout.Width(60));
			this.renameFrom = EditorGUILayout.TextField(this.renameFrom,GUILayout.Width(100));
			EditorGUILayout.LabelField("To",GUILayout.Width(25));
			this.renameTo = EditorGUILayout.TextField(this.renameTo,GUILayout.Width(100));
			if(GUILayout.Button("Apply",GUILayout.Width(100))){
				GameObject[] all = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
				bool wildcard = this.renameFrom.EndsWith("*");
				string search = this.renameFrom.Replace("*","");
				foreach(GameObject current in all){
					if(wildcard && current.name.Contains(search)){
						current.name = current.name.Replace(search,this.renameTo);
					}
					else if(current.name == search){
						current.name = this.renameTo;
					}
				}
			}
			GUILayout.EndHorizontal();
		}
		//====================================
		// Material Replacer
		//====================================
		public void DrawMaterialReplacer(){
			GUILayout.BeginHorizontal("box");
			EditorGUILayout.LabelField("Replace Material",GUILayout.Width(105));
			this.materialRenameFrom = EditorGUILayout.TextField(this.materialRenameFrom,GUILayout.Width(100));
			EditorGUILayout.LabelField("To",GUILayout.Width(25));
			this.materialRenameTo = EditorGUILayout.TextField(this.materialRenameTo,GUILayout.Width(100));
			if(GUILayout.Button("Apply",GUILayout.Width(100))){
				ProxyEditor.RegisterSceneUndo("Revert Material Replace");
				GameObject[] selection = Selection.gameObjects;
				bool global = this.materialRenameFrom == "*";
				string search = this.materialRenameFrom.Replace("*","");
				foreach(GameObject current in selection){
					Renderer[] renderers = current.GetComponentsInChildren<Renderer>(this.materialIncludeInactive);
					foreach(Renderer renderer in renderers){
						List<Material> replacedMaterials = new List<Material>();
						foreach(Material material in renderer.sharedMaterials){
							string name = material.name;
							Material currentMaterial = material;
							if(name.Contains(search)){
								string replace = name.Replace(search,this.materialRenameTo);
								if(global){replace = this.materialRenameTo;}
								Material swapMaterial = File.GetAsset<Material>(replace+".mat",false);
								if(swapMaterial != null){
									currentMaterial = swapMaterial;
								}
							}
							replacedMaterials.Add(currentMaterial);
						}
						renderer.sharedMaterials = replacedMaterials.ToArray();
					}
				}
			}
			GUILayout.EndHorizontal();
			this.materialIncludeInactive = EditorGUILayout.ToggleLeft("Include Inactive",this.materialIncludeInactive);
		}
		//====================================
		// Select By Material
		//====================================
		public void DrawMaterialSelector(){
			GUILayout.BeginHorizontal("box");
			EditorGUILayout.LabelField("Select By Material",GUILayout.Width(60));
			this.selectName = EditorGUILayout.TextField(this.selectName,GUILayout.Width(100));
			if(GUILayout.Button("Find",GUILayout.Width(100))){
				GameObject[] all = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
				List<GameObject> selection = new List<GameObject>();
				string search = this.selectName.Replace("*","");
				foreach(GameObject current in all){
					Renderer renderer = current.GetComponent<Renderer>();
					if(renderer != null){
						foreach(Material material in renderer.sharedMaterials){
							if(material.name == search){
								selection.Add(current);
							}
						}
					}
				}
				Selection.objects = selection.ToArray();
			}
			GUILayout.EndHorizontal();
		}
		//====================================
		// Selector
		//====================================
		public void DrawSelector(){
			GUILayout.BeginHorizontal("box");
			EditorGUILayout.LabelField("Select",GUILayout.Width(60));
			this.selectName = EditorGUILayout.TextField(this.selectName,GUILayout.Width(100));
			EditorGUILayout.LabelField("With Component",GUILayout.Width(105));
			this.selectComponent = EditorGUILayout.TextField(this.selectComponent,GUILayout.Width(100));
			if(GUILayout.Button("Find",GUILayout.Width(100))){
				GameObject[] all = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
				List<GameObject> selection = new List<GameObject>();
				bool wildcard = this.selectName.Contains("*");
				string search = this.selectName.Replace("*","");
				foreach(GameObject current in all){
					if(search == "" || (wildcard && current.name.Contains(search)) || current.name == search){
						if(this.selectComponent != ""){
							if(current.GetComponent(this.selectComponent) == null){continue;}
						}
						selection.Add(current);
					}
				}
				Selection.objects = selection.ToArray();
			}
			GUILayout.EndHorizontal();
		}
		//====================================
		// Shader Selector
		//====================================
		public void DrawShaderSelector(){
			GUILayout.BeginHorizontal("box");
			EditorGUILayout.LabelField("Select With Shader",GUILayout.Width(125));
			this.shaderName = EditorGUILayout.TextField(this.shaderName,GUILayout.Width(100));
			if(GUILayout.Button("Find",GUILayout.Width(100))){
				List<GameObject> selection = new List<GameObject>();
				Renderer[] all = (Renderer[])GameObject.FindObjectsOfType(typeof(Renderer));
				foreach(Renderer current in all){
					if(selection.Contains(current.gameObject)){continue;}
					foreach(Material material in current.sharedMaterials){
						string shaderName = material.shader.name;
						if(shaderName.Contains(this.shaderName,true)){
							selection.Add(current.gameObject);
						}
					}
				}
				Selection.objects = selection.ToArray();
			}
			GUILayout.EndHorizontal();
		}
		//====================================
		// Prefaber
		//====================================
		public void DrawPrefaber(){
			GUILayout.BeginHorizontal("box");
			EditorGUILayout.LabelField("Prefab",GUILayout.Width(60));
			if(GUILayout.Button("Revert",GUILayout.Width(100))){
				ProxyEditor.RegisterSceneUndo("Revert Selected Prefabs");
				foreach(GameObject current in Selection.gameObjects){
					PrefabUtility.RevertPrefabInstance(current);
				}
			}
			if(GUILayout.Button("Apply",GUILayout.Width(100))){
				ProxyEditor.RegisterSceneUndo("Apply Selected Prefabs");
				foreach(GameObject current in Selection.gameObjects){
					GameObject root = PrefabUtility.FindPrefabRoot(current);
					#if UNITY_2018_2_OR_NEWER
					PrefabUtility.ReplacePrefab(root,PrefabUtility.GetCorrespondingObjectFromSource(root),ReplacePrefabOptions.ConnectToPrefab);
					#else
					PrefabUtility.ReplacePrefab(root,PrefabUtility.GetPrefabParent(root),ReplacePrefabOptions.ConnectToPrefab);
					#endif
				}
			}
			if(GUILayout.Button("Detach",GUILayout.Width(100))){
				ProxyEditor.RegisterSceneUndo("Apply Selected Prefabs");
				foreach(GameObject current in Selection.gameObjects){
					PrefabUtility.DisconnectPrefabInstance(current);
				}
			}
			GUILayout.EndHorizontal();
		}
	}
	public enum ReplaceOption{Prefab=0,GameObject=1};
	public class ReplaceSelector{
		public ReplaceOption type;
		public List<GameObject> found = new List<GameObject>();
		public Object targetObject;
		public string targetName = "";
		public string targetPath = "";
		public bool manual = false;
	}
}