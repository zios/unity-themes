using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.MeshOperations{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Unity.Extensions;
	using Zios.Unity.Locate;
	using Zios.Unity.Log;
	using Zios.Unity.Call;
	using Zios.Unity.EditorUI;
	using Zios.Unity.Proxy;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.Supports.MeshSource;
	using Zios.Unity.Supports.MeshWrap;
	using Zios.Unity.Time;
	using Zios.Supports.Worker;
	using Zios.Unity.Components.AnimationSettings;
	using Zios.Reflection;

	public enum VertexDisplay : int{
		Distance    = 0x001,
		Overlap     = 0x002,
		NonOverlap  = 0x004,
		Hidden      = 0x008
	}
	public enum VertexMode{Billboard,Surface}
	public enum OperationMode{Merge,Flatten}
	public class MeshOperations : EditorWindow{
		public static MeshOperations instance;
		public static bool operating;
		private Dictionary<GameObject,MeshSource[]> cached = new Dictionary<GameObject,MeshSource[]>();
		private List<GameObject> distanceMeshes = new List<GameObject>();
		private List<GameObject> vertexMeshes = new List<GameObject>();
		private Dictionary<GameObject,List<int>> matches = new Dictionary<GameObject,List<int>>();
		private Material vertexMaterial;
		private Material distanceMaterial;
		private VertexMode vertexMode;
		private OperationMode operationMode;
		private Worker overlapWorker = new Worker();
		private bool needsVertexUpdate;
		public bool visible;
		public bool busy;
		public bool alwaysDisplay = true;
		public bool lockSelection;
		public float distance = 0.185f;
		public float size = 0.1f;
		public int scaleFactor = 2;
		public float scale;
		public VertexDisplay vertexDisplay = (VertexDisplay)(-1) & ~VertexDisplay.Hidden;
		public Color vertexColor = new Color(0.396f,0.396f,0.396f,1);
		public Color vertexOverlapColor = new Color(1,0,0,0.7f);
		public Color vertexDistanceColor = new Color(0.27f,1,0.396f,0.09f);
		[MenuItem("Zios/Window/Mesh Operations",false,0)]
		private static void Init(){
			var window = EditorWindow.GetWindow<MeshOperations>();
			window.position = new Rect(400,450,350,600);
		}
		public static MeshOperations Get(){
			EditorWindow.GetWindow<MeshOperations>();
			return MeshOperations.instance;
		}
		public void OnEnable(){
			MeshOperations.instance = this;
			this.vertexMaterial = new Material(Shader.Find("Zios/Utility/Draw Vertexes"));
			this.distanceMaterial = new Material(Shader.Find("Zios/Utility/Draw Vertexes Outlined"));
			this.lockSelection = false;
			Selection.selectionChanged += this.RefreshDisplay;
			EditorApplication.update += this.CheckChanged;
			//EditorApplication.hierarchyWindowChanged += this.Reset;
			this.RefreshDisplay(true);
		}
		public void OnDisable(){
			Selection.selectionChanged -= this.RefreshDisplay;
			EditorApplication.update -= this.CheckChanged;
			this.RemoveMeshes();
			ProxyEditor.RepaintSceneView();
			//EditorApplication.hierarchyWindowChanged -= this.Reset;
		}
		//public void OnDestroy(){this.OnDisable();}
		//==================
		// Interface
		//==================
		public void OnGUI(){
			EditorUI.Reset();
			EditorUI.SetFieldSize(-1,175,false);
			EditorGUILayout.BeginVertical(new GUIStyle().Margin(8,8,15,8));
			EditorGUILayout.BeginHorizontal(new GUIStyle().Margin(0,0,0,10).Center(400));
			if("Merge".ToLabel().Layout(200,20).DrawButton(EditorStyles.miniButtonLeft.FixedHeight(0))){this.operationMode = OperationMode.Merge;}
			if("Flatten".ToLabel().Layout(200,20).DrawButton(EditorStyles.miniButtonRight.FixedHeight(0))){this.operationMode = OperationMode.Flatten;}
			EditorGUILayout.EndHorizontal();
			if(this.operationMode.Has("Merge")){
				this.titleContent = new GUIContent("Mesh");
				//"Vertex".DrawLabel(EditorStyles.boldLabel);
				this.vertexMode = this.vertexMode.Draw("Vertex Mode").As<VertexMode>();
				this.vertexDisplay = this.vertexDisplay.DrawMask("Vertex Display").As<VertexDisplay>();
				this.vertexColor = this.vertexColor.Draw("Vertex Color");
				this.vertexDistanceColor = this.vertexDistanceColor.Draw("Vertex Distance Color");
				this.vertexOverlapColor = this.vertexOverlapColor.Draw("Vertex Overlap Color");
				this.distance = this.distance.DrawSlider(0,1f,"Vertex Overlap Distance");
				this.size = this.size.DrawSlider(0,1,"Vertex Size");
				this.scaleFactor = this.scaleFactor.DrawSlider(1,10,"Vertex Display Scale");
				this.scale = (0.0625f * Mathf.Pow(2,this.scaleFactor-1));
				this.alwaysDisplay = this.alwaysDisplay.Draw("Always Display");
				var lockText = this.lockSelection ? "Unlock Selection" : "Lock Selection";
				GUI.enabled = this.lockSelection || this.cached.Count > 0;
				if(lockText.ToLabel().Layout(150,25).DrawButton() && (this.lockSelection || this.cached.Count > 0)){
					this.lockSelection = !this.lockSelection;
				}
				GUI.enabled = true;
			}
			else if(this.operationMode.Has("Flatten")){

			}
			if(EditorUI.anyChanged){
				ProxyEditor.RegisterUndo(this,"Mesh Operation Changes");
				this.Repaint();
				this.RefreshDisplay();
			}
			EditorGUILayout.EndVertical();
			this.visible = true;
		}
		public void Update(){
			//this.CheckChanged();
		}
		public void Setup(){
			if(this.vertexMaterial.IsNull()){
				this.OnDisable();
				this.OnEnable();
			}
		}
		public void OnLostFocus(){
			this.visible = false;
			this.RefreshDisplay();
		}
		public void OnFocus(){
			this.visible = true;
			this.RefreshDisplay();
		}
		public void RefreshShader(){
			var showHidden = this.vertexDisplay.Contains("Hidden");
			var mode = this.vertexMode.Contains("Billboard") ? "BILLBOARD" : "SURFACE";
			var modes = new string[2]{"QUADMODE_BILLBOARD","QUADMODE_SURFACE"};
			foreach(var keyword in modes){
				this.vertexMaterial.DisableKeyword(keyword);
				this.distanceMaterial.DisableKeyword(keyword);
			}
			var size = this.size * this.scale;
			var distance = this.distance * this.scale;
			var extrude = mode == "SURFACE" ? 0.1f : (showHidden ? 0 : size/2);
			this.vertexMaterial.SetFloat("zTestMode",showHidden ? 6 : 4);
			this.distanceMaterial.SetFloat("zTestMode",showHidden ? 6 : 4);
			this.distanceMaterial.EnableKeyword("QUADMODE_"+mode);
			this.vertexMaterial.EnableKeyword("QUADMODE_"+mode);
			this.distanceMaterial.EnableKeyword("QUADMODE_"+mode);
			this.vertexMaterial.SetFloat("displayExtrude",extrude);
			this.distanceMaterial.SetFloat("displayExtrude",extrude);
			this.vertexMaterial.SetFloat("displaySize",size);
			this.distanceMaterial.SetFloat("displaySize",distance);
		}
		public void RemoveMeshes(){
			Locate.SetDirty();
			foreach(var target in Locate.GetSceneObjects()){
				if(!target.IsNull() && target.name.Contains("@TempMesh")){
					target.Destroy();
				}
			}
		}
		public void RefreshDisplay(){this.RefreshDisplay(true);}
		public void RefreshDisplay(bool rebuild){
			this.Setup();
			this.RefreshShader();
			var meshLists = new List<GameObject>[2]{this.vertexMeshes,this.distanceMeshes};
			if(this.vertexMeshes.Count != this.cached.Values.SelectMany(x=>x).ToArray().Length){rebuild = true;}
			if(!this.lockSelection && rebuild){
				this.cached.Clear();
				foreach(var selected in Selection.gameObjects){
					if(!selected.IsNull() && !selected.name.Contains("@TempMesh")){
						this.cached[selected] = selected.GetMeshSources().Where(x=>x.target.activeInHierarchy).ToArray();
					}
				}
				this.RemoveMeshes();
				if(this.visible || this.alwaysDisplay){
					foreach(var displayMeshes in meshLists){
						displayMeshes.Clear();
						var material = displayMeshes == this.vertexMeshes ? this.vertexMaterial : this.distanceMaterial;
						foreach(var target in this.cached.SelectMany(x=>x.Value)){
							var instance = displayMeshes.AddNew();
							var mesh =  target.GetMesh(true).Copy();
							instance.name = "@TempMesh";
							instance.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
							instance.AddComponent<MeshFilter>().mesh = mesh;
							var renderer = instance.AddComponent<MeshRenderer>();
							var materials = new Material[mesh.subMeshCount];
							for(var index=0;index<mesh.subMeshCount;++index){
								materials[index] = material;
							}
							renderer.materials = materials;
						}
					}
				}
			}
			if(this.visible || this.alwaysDisplay){
				var cachedTargets = this.cached.SelectMany(x=>x.Value).ToArray();
				foreach(var displayMeshes in meshLists){
					for(var index=0;index<displayMeshes.Count;++index){
						if(cachedTargets[index].IsNull()){continue;}
						var target = cachedTargets[index].target;
						var instance = displayMeshes[index];
						if(target.IsNull() || instance.IsNull()){continue;}
						instance.transform.position = target.transform.position;
						instance.transform.rotation = target.transform.rotation;
						instance.transform.localScale = target.transform.localScale;
						if(!cachedTargets[index].skinnedRenderer){

						}
					}
				}
				this.ClearVertexes();
				this.FindOverlapping();
			}
			ProxyEditor.RepaintSceneView();
		}
		public void FindOverlapping(){
			Time.Start();
			this.matches.Clear();
			var combinations = new Dictionary<GameObject,List<GameObject>>();
			var size = this.size*this.scale;
			var requiredDistance = this.distance*this.scale;
			var cache = this.cached.Where(x=>!x.Key.IsNull() && x.Key.activeInHierarchy).ToDictionary();
			var positions = new Dictionary<MeshSource[],List<List<Vector3>>>();
			var transforms = new Dictionary<MeshSource[],List<Matrix4x4>>();
			foreach(var current in cache){
				var currentPositions = positions.AddNew(current.Value);
				var currentTransforms = transforms.AddNew(current.Value);
				foreach(var source in current.Value){
					currentPositions.Add(source.target.GetMeshWrap().positions.ToList());
					currentTransforms.Add(source.target.transform.localToWorldMatrix);
				}
			}
			Worker.Quit(this);
			Worker.Create(cache).Threads(1).Group(this).Monitor().OnStep((selectionA)=>{
				var worker = Worker.Get();
				var meshSetA = cache[selectionA];
				for(int sourceIndexA=0;sourceIndexA<meshSetA.Length;++sourceIndexA){
					if(worker.quit){return true;}
					var sourceA = meshSetA[sourceIndexA];
					var positionsA = positions[meshSetA][sourceIndexA];
					var transformA = transforms[meshSetA][sourceIndexA];
					foreach(var selectionB in cache.Keys){
						if(worker.quit){return true;}
						var existing = combinations.AddNew(selectionA).Contains(selectionB) || combinations.AddNew(selectionB).Contains(selectionA);
						if(selectionA == selectionB || existing){continue;}
						var meshSetB = cache[selectionB];
						for(int sourceIndexB=0;sourceIndexB<meshSetB.Length;++sourceIndexB){
							var sourceB = meshSetB[sourceIndexB];
							var positionsB = positions[meshSetB][sourceIndexB];
							var transformB = transforms[meshSetB][sourceIndexB];
							Worker.Create(positionsA).Group(this).OnStep((int indexA)=>{
								if(worker.quit){return true;}
								var pointA = transformA.MultiplyPoint3x4(positionsA[indexA]);
								for(int indexB=0;indexB<positionsB.Count;++indexB){
									if(worker.quit){return true;}
									var pointB = transformB.MultiplyPoint3x4(positionsB[indexB]);
									var distance = Vector3.Distance(pointA,pointB);
									if(distance-size < requiredDistance){
										lock(this.matches){
											try{
												this.matches.AddNew(sourceA.target).AddNew(indexA);
												this.matches.AddNew(sourceB.target).AddNew(indexB);
											}
											catch{return true;}
										}
									}
								}
								return true;
							}).Build();
						}
						combinations.AddNew(selectionA).Add(selectionB);
						combinations.AddNew(selectionB).Add(selectionA);
					}
				}
				return true;
			}).OnEnd(()=>{
				//Log.Show("[MeshOperations] Find Overlapping -- " + Time.Check());
				this.needsVertexUpdate = true;
			}).Build();
		}
		public void ClearVertexes(){
			var distanceColor = this.vertexDisplay.Contains("Distance") ? this.vertexDistanceColor : Color.clear;
			var vertexColor = this.vertexDisplay.Contains("NonOverlap") ? this.vertexColor : Color.clear;
			foreach(var target in this.vertexMeshes){
				if(target.IsNull()){continue;}
				var mesh = target.GetMesh();
				mesh.colors = Enumerable.Repeat(vertexColor,mesh.vertexCount).ToArray();
			}
			foreach(var target in this.distanceMeshes){
				if(target.IsNull()){continue;}
				var mesh = target.GetMesh();
				mesh.colors = Enumerable.Repeat(distanceColor,mesh.vertexCount).ToArray();
			}
		}
		public void SetupVertexes(){
			var time = Time.Get();
			var showOverlap = this.vertexDisplay.Contains("Overlap");
			var showDistance = this.vertexDisplay.Contains("Distance");
			var showNonOverlap = this.vertexDisplay.Contains("NonOverlap");
			var cachedTargets = this.cached.SelectMany(x=>x.Value).ToArray();
			if(this.distanceMeshes.Count != cachedTargets.Length){return;}
			for(int index=0;index<cachedTargets.Length;++index){
				if(this.distanceMeshes[index].IsNull()){continue;}
				var target = cachedTargets[index].target;
				if(target.IsNull() || !target.activeInHierarchy){continue;}
				var mesh = target.GetMeshWrap();
				var source = target.GetMeshSource();
				if(mesh.IsNull()){continue;}
				var transform = source.target.transform;
				var vertexColors = new Color[mesh.vertexCount];
				var distanceColors = new Color[mesh.vertexCount];
				for(int vertexIndex=0;vertexIndex<mesh.positions.Length;++vertexIndex){
					var position = source.target.transform.TransformPoint(mesh.positions[vertexIndex]);
					var overlaps = this.matches.AddNew(target).Contains(vertexIndex);
					var display = (overlaps && showOverlap) || showNonOverlap;
					distanceColors[vertexIndex] = Color.clear;
					vertexColors[vertexIndex] = Color.clear;
					if(display){
						if(showDistance){distanceColors[vertexIndex] = this.vertexDistanceColor;}
						vertexColors[vertexIndex] = overlaps && showOverlap ? this.vertexOverlapColor : this.vertexColor;
					}
				}
				this.distanceMeshes[index].GetMesh().colors = distanceColors;
				this.vertexMeshes[index].GetMesh().colors = vertexColors;
			}
			this.needsVertexUpdate = false;
			//Debug.Log("Setup Vertex -- " + time.Passed());
		}
		public void CheckChanged(){
			var cachedTargets = this.cached.SelectMany(x=>x.Value).ToArray();
			var needsRefresh = 0;
			if(this.needsVertexUpdate){this.SetupVertexes();}
			foreach(var current in cachedTargets){
				if(current.target.IsNull() || !current.target.activeInHierarchy){
					this.lockSelection = false;
					needsRefresh = 1;
					continue;
				}
				if(current.target.transform.hasChanged){
					current.target.transform.hasChanged = false;
					needsRefresh = 2;
				}
			}
			if(needsRefresh == 1){this.RefreshDisplay();}
			if(needsRefresh == 2){this.RefreshDisplay(false);}
		}
		//==================
		// Operations
		//==================
		[MenuItem("GameObject/Merge/As Blend States",false,-9001)]
		public static void MergeBlend(){
			if(MeshOperations.operating){return;}
			MeshOperations.operating = true;
			Call.Delay(()=>MeshOperations.operating = false);
			var targets = Selection.gameObjects.OrderBy(x=>x.name).ToArray();
			var primary = targets[0];
			var merged = primary.GetMeshSource().GetMesh().Copy();
			var wrap = merged.GetMeshWrap();
			var originalPositions = wrap.positions;
			var originalNormals = wrap.normals;
			var originalTangents = wrap.tangents;
			primary.SetActive(false);
			foreach(var current in targets.Skip(1)){
				var mesh = current.GetMeshWrap();
				if(merged.vertexCount != mesh.vertexCount){
					var meshCount = mesh.name + " (" + mesh.vertexCount + ")";
					var mergedCount = merged.name + " (" + merged.vertexCount + ")";
					Log.Warning("[MergeBlend] Vertex counts do not match. " + meshCount + " -- " + mergedCount + ". Truncating excess.");
				}
				var count = mesh.vertexCount;
				var deltaPositions = mesh.positions.Subtract(originalPositions);
				Vector3[] deltaNormals = null;
				Vector3[] deltaTangents = null;
				if(wrap.normals.Length > 0){deltaNormals = wrap.normals.Subtract(mesh.normals);}
				if(wrap.tangents.Length > 0){deltaTangents = wrap.tangents.Cast<Vector3>().ToArray().Subtract(mesh.tangents.Cast<Vector3>().ToArray());}
				merged.AddBlendShapeFrame(current.name.Split("-").Skip(1).Join("-"),100,deltaPositions.Resize(count),deltaNormals.Resize(count),deltaTangents.Resize(count));
				current.SetActive(false);
			}
			var target = GameObject.Instantiate(primary);
			var built = target.GetMeshSource().target;
			target.GetMeshSource().SetMesh(merged);
			Selection.activeGameObject = built;
			if(!built.Has<AnimationSettings>()){
				built.Add<AnimationSettings>().SetExpanded(true);
			}
			built.Remove<MeshFilter>();
			built.Remove<MeshRenderer>();
			target.name = primary.name.Split("-").First()+"-Merged";
			target.hideFlags = HideFlags.None;
			target.SetActive(true);
			MeshWrap.cache.Clear();
		}
		[MenuItem("GameObject/Merge/As Single",false,-9001)]
		public static void MergeSingle(){
			if(MeshOperations.operating){return;}
			MeshOperations.operating = true;
			Call.Delay(()=>MeshOperations.operating = false);
			var primary = Selection.gameObjects[0];
			var merged = primary.GetMeshSource().GetMesh().Copy();
			var wrap = merged.GetMeshWrap();
			foreach(var source in Selection.gameObjects.Skip(1)){
				var mesh = source.GetMeshWrap();
				var matches = new Dictionary<int,int>();
				for(int indexA=0;indexA<wrap.positions.Length;++indexA){
					var vertexA = wrap.positions[indexA];
					for(int indexB=0;indexB<mesh.positions.Length;++indexB){
						var vertexB = mesh.positions[indexB];
						var distance = Vector3.Distance(vertexA,vertexB);
						/*if(distance < this.distance){
							Log.Show("Difference is : " + distance);
							Log.Show(indexA + " matches " + indexB);
							matches[indexA] = indexB;
							break;
						}*/
					}
				}
			}
		}
		[MenuItem("GameObject/Split/Blend States",false,-9001)]
		public static void SplitBlendStates(){
			if(MeshOperations.operating){return;}
			MeshOperations.operating = true;
			Call.Delay(()=>MeshOperations.operating = false);
			var target = Selection.gameObjects[0];
			var wrap = target.GetMeshWrap();
			var source = target.GetMeshSource();
			if(wrap.IsNull() || source.skinnedRenderer.IsNull()){return;}
			var renderer = source.skinnedRenderer;
			var shapeCount = renderer.sharedMesh.blendShapeCount;
			for(int index=-1;index<shapeCount;++index){
				renderer.ResetBlendShapes();
				var mesh = new Mesh();
				var blendName = "";
				if(index >= 0){
					blendName = "-" + renderer.sharedMesh.GetBlendShapeName(index);
					renderer.SetBlendShapeWeight(index,100);
				}
				renderer.BakeMesh(mesh);
				var copy = GameObject.Instantiate(target);
				copy.GetMeshSource().SetMesh(mesh);
				copy.name = target.name + blendName;
			}
		}
	}
}