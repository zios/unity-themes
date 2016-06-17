#pragma warning disable 618
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEvent = UnityEngine.Event;
namespace Zios.Editors.SpriteEditors{
	using Sprites;
	public class SpriteAssets{
		public GUISkin UI;
		public Mesh spriteMesh;
		public Texture grid;
		public Texture gridBlue;
		public Texture gridSilver;
		public Material spriteMaterial;
		public Material spritePreview;
		public Material spriteBrush;
		public Material spriteEmbed;
		public Shader shaderEmbed;
		public Shader shaderNormal;
		public Shader shaderFlat;
		public Shader shaderEditor;
		public SpriteAssets(){
			this.grid = FileManager.GetAsset<Texture2D>("TransparencyGray.png");
			this.gridBlue = FileManager.GetAsset<Texture2D>("TransparencyBlue.png");
			this.gridSilver = FileManager.GetAsset<Texture2D>("TransparencySilver.png");
			this.UI = FileManager.GetAsset<GUISkin>("Sprite.guiskin");
			this.spriteMesh = FileManager.GetAsset<Mesh>("Sprite.fbx");
			this.shaderNormal = Shader.Find("Zios/Shared/Sprite");
			this.shaderFlat = Shader.Find("Zios/Shared/Sprite + Particle");
			this.shaderEditor = Shader.Find("Zios/Shared/Sprite + Clip");
			this.shaderEmbed = Shader.Find("Zios/Shared/Sprite + Embedded");
			this.spritePreview = new Material(this.shaderEditor);
			this.spriteBrush = new Material(this.shaderNormal);
			this.spriteEmbed = new Material(this.shaderEmbed);
		}
	}
	public static class ScaleMode{
		public static int Auto2D = 0;
		public static int Auto3D = 1;
	}
	public class OptionTooltips{
		public GUIContent editMode;
		public GUIContent placementMode;
		public GUIContent showAnimations;
		public GUIContent fitPreviews;
		public GUIContent fitSelected;
		public GUIContent delayRender;
		public GUIContent applyOnSelection;
		public GUIContent autoScale;
		public GUIContent forceOverwrite;
		public GUIContent animationActive;
		public GUIContent animationLoop;
		public GUIContent animationReverse;
		public GUIContent animationRandomStart;
		public GUIContent generateLookupAtlas;
		public GUIContent generateMaterials;
		public GUIContent generateMeshes;
		public GUIContent generatePrefabs;
		public GUIContent bakeSprites;
		public GUIContent deBakeSprites;
		public OptionTooltips(){
			string bakeSprites,forceOverwrite;
			bakeSprites = "Bakes all applicable scene sprite atlas/material information as vertex mesh data.  ";
			bakeSprites += "Ideal for performance draw call batching.";
			forceOverwrite = "Forces new assets to be generated for a sprite rather than using cached versions.";
			forceOverwrite +=  "Use this in conjuction with reset system to force reloading of all scene prefabs/materials.";
			this.editMode = new GUIContent("Edit Mode","Allows the user to use various hotkey operations related to scene object manipulation.");
			this.placementMode = new GUIContent("Placement Mode","Allows the user to stamp the selected sprite directly to the scene.");
			this.showAnimations = new GUIContent("Show Animations","Displays animated frames for selected sprite.  Warning : Unoptimized CPU Usage!");
			this.fitPreviews = new GUIContent("Fit Previews","Scales all spritesheets to fit this window.");
			this.fitSelected = new GUIContent("Fit Selected","Scales the selected sprite to fit this window.");
			this.applyOnSelection = new GUIContent("Apply On Selection","Apply the active sprite to all selected scene objects when clicked.");
			this.autoScale = new GUIContent("Auto Scale","Changes the selected sprite's scale to reflect the prefab state.");
			this.forceOverwrite = new GUIContent("Force Overwrite",forceOverwrite);
			this.animationActive = new GUIContent("Active","On/off state of the animation.");
			this.animationLoop = new GUIContent("Loop","Whether an animation will loop when reaching the last frame.");
			this.animationReverse = new GUIContent("Reverse","Causes an animation to play its frames in backwards order.");
			this.animationRandomStart = new GUIContent("Random Start","Causes an animation to always start its sequence on a randomly chosen frame.");
			this.generateLookupAtlas = new GUIContent("","Generate lookup atlas for any image.");
			this.generateMaterials = new GUIContent("","Generate all materials for this atlas.");
			this.generateMeshes = new GUIContent("","Generate all meshes for this atlas.");
			this.generatePrefabs = new GUIContent("","Generate all prefabs for this atlas.");
			this.bakeSprites = new GUIContent("Bake",bakeSprites);
			this.deBakeSprites = new GUIContent("DeBake",bakeSprites);
		}
	}
	public class Palette{
		public string name;
		public Color colorA = new Color(1,0,0,1);
		public Color colorB = new Color(0,1,0,1);
		public Color colorC = new Color(0,0,1,1);
		public Color colorD = new Color(1,1,0,1);
		public Color colorE = new Color(0,1,1,1);
		public Color colorF = new Color(1,0,1,1);
		public Color colorG = new Color(0,0,0,1);
		public Color colorH = new Color(1,1,1,1);
		public Palette(string name){
			this.name = name;
		}
	}
	public class RepairData{
		public GameObject gameObject;
		public Sprite sprite;
		public Material material;
		public RepairData(GameObject gameObject,Sprite sprite,Material material){
			this.gameObject = gameObject;
			this.sprite = sprite;
			this.material = material;
		}
	}
	public class SpriteWindow : EditorWindow{
		public OptionTooltips tooltips = new OptionTooltips();
		public Dictionary<SpriteSheet,Texture2D> spriteLookupMaps = new Dictionary<SpriteSheet,Texture2D>();
		public Dictionary<string,List<Palette>> spritePalettes = new Dictionary<string,List<Palette>>();
		public Dictionary<string,Material> spriteMaterials = new Dictionary<string,Material>();
		public Dictionary<string,GameObject> spritePrefabs = new Dictionary<string,GameObject>();
		public Dictionary<GameObject,bool> spriteBakes = new Dictionary<GameObject,bool>();
		public Dictionary<string,Mesh> spriteMeshes = new Dictionary<string,Mesh>();
		public List<SpriteSheet> collapsed = new List<SpriteSheet>();
		public SpriteSheet activeSheet;
		public Sprite selected;
		public GameObject brush;
		public GameObject brushPrefab;
		public string loadMessage = "-";
		public float totalX = 0;
		public float totalY = 0;
		public float selectedSpeed = 6;
		public int mouseState = 0;
		public int assetIndex = 0;
		public int repairIndex = 0;
		public int materialIndex = 0;
		public int bakeIndex = 0;
		public Material activeMaterial;
		public Palette activePalette = new Palette("Editor");
		public RepairData[] sceneSprites = new RepairData[0];
		public Sprite[] assetSprites = new Sprite[0];
		public SpriteSheet[] assetSheets = new SpriteSheet[0];
		public string assetPrefix = "Sprite";
		public bool visible = false;
		public bool loadScreen = false;
		public bool atlasCapable = false;
		public bool assetsReady = true;
		public bool disabled = false;
		public bool showConvert = true;
		public bool showOptions = true;
		public bool showSelected = true;
		public bool showPalette = true;
		public bool showAnimations = false;
		public bool sortByType = false;
		public bool applyOnSelection = false;
		public bool delayRender = true;
		public bool fitPreviews = true;
		public bool fitSelected = true;
		public bool editMode = false;
		public bool placementMode = false;
		public bool randomizeStart = true;
		public bool brushFlat = false;
		public bool brushShadows = false;
		public bool autoScale = false;
		public bool forceOverwrite = false;
		public bool forceUpdate = false;
		public bool repairSprites = false;
		public bool generateLookupAtlases = false;
		public bool generateMaterials = false;
		public bool generateMeshes = false;
		public bool generatePrefabs = false;
		public int scaleMode = ScaleMode.Auto2D;
		public float brushDistanceOffset = 1.0f;
		public Vector3 brushScale = new Vector3(1,1,1);
		public Vector3 brushPositionOffset = new Vector3(0,0,0);
		public Vector3 brushRotationOffset = new Vector3(0,0,0);
		public Vector2 controlPosition;
		public Vector2 scrollPosition = Vector2.zero;
		public SpriteAssets assets;
		//===========================
		// Unity Specific
		//===========================
		[MenuItem("Zios/Window/Sprite (Unstable)",false,0)]
		private static void Init(){
			SpriteWindow window = (SpriteWindow)EditorWindow.GetWindow(typeof(SpriteWindow));
			EditorWindow.FocusWindowIfItsOpen(typeof(SpriteWindow));
			window.wantsMouseMove = true;
			window.position = new Rect(100,150,400,800);
		}
		public void OnProjectChange(){
			if(this.assetsReady){
				this.ResetSystem();
			}
		}
		public void OnSelectionChange(){
			this.activeMaterial = null;
			this.materialIndex = 0;
			this.Repaint();
		}
		public void Update(){
			if(!this.assetsReady && this.loadScreen){
				if(this.generateLookupAtlases){this.CreateLookupAtlases();}
				else{this.ProgressStep();}
			}
			this.visible = true;
		}
		public void OnGUI(){
			if(this.assets == null || this.assets.spritePreview == null){this.ResetSystem();}
			//SceneView.lastActiveSceneView.Focus();
			this.DrawWindow();
		}
		public void OnEnable(){
			this.ResetSystem();
			if(SceneView.onSceneGUIDelegate != this.DrawScene){
				SceneView.onSceneGUIDelegate += this.DrawScene;
			}
		}
		public void OnDisable(){
			while(SceneView.onSceneGUIDelegate == this.DrawScene){
				SceneView.onSceneGUIDelegate -= this.DrawScene;
			}
		}
		public void OnInspectorUpdate(){
			this.disabled = !this.placementMode && !this.editMode;
		}
		//===========================
		// System & Utility
		//===========================
		public void DelayRender(){
			if(this.disabled || Application.isPlaying){return;}
			if(SceneView.lastActiveSceneView != null){
				SceneView.lastActiveSceneView.Repaint();
			}
		}
		public void ResetSystem(){
			Debug.Log("-------------------------------");
			Debug.Log("[SpriteWindow] Resetting system");
			FileManager.Refresh();
			this.assets = new SpriteAssets();
			this.assetSprites = new Sprite[0];
			this.assetSheets = new SpriteSheet[0];
			this.sceneSprites = new RepairData[0];
			this.activeSheet = null;
			this.brushPrefab = null;
			this.brush = null;
			this.collapsed.Clear();
			this.LoadSpriteSheets();
			//this.LoadPalettes();
			if(SpriteManager.sequences.Count > 0){
				this.selected = SpriteManager.sequences.First().Value;
				string lastSprite = PlayerPrefs.GetString("SpriteWindow-Selected");
				Sprite knownSprite = SpriteManager.GetSprite(lastSprite);
				if(knownSprite != null){
					this.selected = knownSprite;
					this.activeSheet = knownSprite.parent;
				}
				this.ResetBrush();
			}
		}
		public void ClearSelection(){
			Selection.objects = new Object[0];
		}
		public void CheckHotkeys(){
			bool useEvent = false;
			//bool mouseMove = UnityEvent.current.type == EventType.MouseMove;
			bool control = UnityEvent.current.control;
			bool shift = UnityEvent.current.shift;
			bool alt = UnityEvent.current.alt;
			bool mouseMove = UnityEvent.current.type == EventType.MouseMove;
			Vector2 mouseChange = mouseMove ? UnityEvent.current.delta : new Vector2(0,0);
			Cursor.visible = !((control || alt || shift) && placementMode);
			KeyShortcut CheckKey = Button.EventKeyDown;
			Transform[] selected = this.placementMode && this.brush != null ? new Transform[1]{this.brush.transform} : Selection.transforms;
			if((control||alt||shift) && mouseMove){
				if(this.controlPosition == Vector2.zero){
					Undo.RegisterUndo(Selection.transforms,"Brush Operation");
					this.controlPosition = UnityEvent.current.mousePosition;
				}
			}
			else if(!control && !alt && !shift){
				this.controlPosition = Vector2.zero;
			}
			if(CheckKey(KeyCode.Escape)){
				this.ClearSelection();
				useEvent = true;
			}
			if(CheckKey(KeyCode.F1)){
				this.placementMode = !this.placementMode;
				this.Repaint();
				useEvent = true;
			}
			if(CheckKey(KeyCode.F2)){
				this.editMode = !this.editMode;
				this.Repaint();
				useEvent = true;
			}
			if(CheckKey(KeyCode.F)){
				Undo.RegisterSceneUndo("Fix Scene Sprites");
				this.FixScene(true);
				useEvent = true;
			}
			if(CheckKey(KeyCode.P)){
				Undo.RegisterSceneUndo("Revert All to Prefab");
				this.RevertSprites();
				useEvent = true;
			}
			if(!this.editMode && !this.placementMode){
				if(useEvent){UnityEvent.current.Use();}
				return;
			}
			foreach(Transform active in selected){
				if(active == null){continue;}
				SpriteController controller = active.GetComponent<SpriteController>();
				bool hasRenderer = active.GetComponent<Renderer>() != null;
				bool isBrush = this.brush && active == this.brush.transform;
				bool isAnimated = controller != null;
				Vector3 scale = active.localScale;
				Vector3 position = active.localPosition;
				Vector3 rotation = active.localEulerAngles;
				if(hasRenderer){
					if(CheckKey(KeyCode.S)){
						active.GetComponent<Renderer>().castShadows = !active.GetComponent<Renderer>().castShadows;
						useEvent = true;
					}
				}
				if(!isBrush && CheckKey(KeyCode.L)){
					Undo.RegisterSceneUndo("Revert Selected to Prefab");
					PrefabUtility.RevertPrefabInstance(active.gameObject);
				}
				if(control && CheckKey(KeyCode.G)){
					GameObject newObject = new GameObject();
					newObject.name = active.name + "Child";
					newObject.transform.parent = active;
					useEvent = true;
				}
				if(UnityEvent.current.type == EventType.ScrollWheel){
					useEvent = true;
					float scroll = UnityEvent.current.delta[1];
					if(control){scale *= Mathf.Sign(scroll) >= 0 ? 0.95f : 1.05f;}
					else if(shift){position.y -= scroll*0.5f;}
					else{useEvent = false;}
				}
				if(CheckKey(KeyCode.Z)){
					Undo.RegisterUndo(Selection.transforms,"Zero Position");
					position = Vector3.zero;
					useEvent = true;
				}
				if(CheckKey(KeyCode.Minus)){scale.x *= -1;}
				if(CheckKey(KeyCode.Equals)){scale.y *= -1;}
				if(CheckKey(KeyCode.UpArrow)){
					if(control){scale *= 1.05f;}
					else if(shift){position.y += 1;}
					else{position.z += 1;}
					useEvent = true;
				}
				if(CheckKey(KeyCode.DownArrow)){
					if(control){scale *= 0.95f;}
					else if(shift){position.y -= 1;}
					else{position.z -= 1;}
					useEvent = true;
				}
				if(CheckKey(KeyCode.LeftArrow)){
					if(control){rotation.z -= 1;}
					else if(shift && isAnimated){controller.PreviousFrame();}
					else{position.x -= 1;}
					useEvent = true;
				}
				if(CheckKey(KeyCode.RightArrow)){
					if(control){rotation.z += 1;}
					else if(shift && isAnimated){controller.NextFrame();}
					else{position.x += 1;}
					useEvent = true;
				}
				if(mouseChange.x != 0 && alt){
					rotation.z += 0.1f * mouseChange.x;
					useEvent = true;
				}
				if(mouseChange.y != 0 && shift){
					position.y -= 0.25f * mouseChange.y;
					useEvent = true;
				}
				if(isBrush){
					this.brushScale -= active.localScale - scale;
					this.brushRotationOffset -= active.localEulerAngles - rotation;
					this.brushPositionOffset -= active.localPosition - position;
				}
				else{
					if(position != active.localPosition){active.localPosition = position;}
					if(rotation != active.localEulerAngles){active.localEulerAngles = rotation;}
					if(scale != active.localScale){active.localScale = scale;}
				}
			}
			if(useEvent){UnityEvent.current.Use();}
		}
		public void FixScene(bool allowForce = false){
			Debug.Log("[SpriteWindow] Fixing Scene = " + (allowForce?"Full":"Repair"));
			GameObject[] objects = Selection.gameObjects.Length > 0 ? Selection.gameObjects : (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
			Dictionary<string,Sprite> sprites = new Dictionary<string,Sprite>();
			List<RepairData> repairList = new List<RepairData>();
			bool forceState = this.forceOverwrite;
			this.forceOverwrite = false;
			foreach(GameObject current in objects){
				PrefabType type = PrefabUtility.GetPrefabType(current);
				if(type == PrefabType.Prefab || type == PrefabType.ModelPrefab || current == null){continue;}
				if(current.transform.localPosition == Vector3.zero){continue;}
				bool isSprite = this.IsSprite(current);
				bool repairable = false;
				if(current.GetComponent<Renderer>() != null && !isSprite){
					foreach(Material material in current.GetComponent<Renderer>().sharedMaterials){
						repairable = material == null || material.HasProperty("atlasUV");
						if(repairable){break;}
					}
				}
				Component[] components = current.GetComponentsInChildren<Component>(true);
				if(components.Length <= 1 && current.name.Contains("Group")){
					DestroyImmediate(current);
					continue;
				}
				if(isSprite || repairable){
					Material[] materialList = current.GetComponent<Renderer>() != null ? current.GetComponent<Renderer>().sharedMaterials : new Material[1];
					if(isSprite && materialList.Length == 0){materialList = new Material[1];}
					foreach(Material material in materialList){
						string materialName = material != null ? material.name.Replace(this.assetPrefix,"") : "";
						string spriteName = isSprite || material == null ? current.name : materialName;
						Sprite sprite = SpriteManager.GetSprite(spriteName);
						if(sprite != null){
							Material activeMaterial = material;
							if(material != null && materialName != current.name){
								activeMaterial = null;
							}
							sprites[current.name] = sprite;
							repairList.Add(new RepairData(current,sprite,activeMaterial));
						}
					}
				}
			}
			Debug.Log("[SpriteWindow] " + repairList.Count + " Sprites found. "  + sprites.Count + " Materials found.");
			this.PrepareAssets(sprites);
			if(allowForce && forceState){
				this.ProgressReport("Forcing Scene Rebuild-"+this.assetSprites.Length+" Sprites",false);
				this.generateMaterials = true;
				this.generatePrefabs = true;
			}
			else{
				this.assetSprites = new Sprite[0];
				this.sceneSprites = repairList.ToArray();
				this.repairSprites = true;
				this.ProgressReport("Scene Repair-0/"+this.sceneSprites.Length+" Objects",false);
			}
			this.forceOverwrite = forceState;
		}
		public void RevertSprites(){
			GameObject[] objects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
			foreach(GameObject instance in objects){
				bool hasPrefab = PrefabUtility.GetPrefabObject(instance) != null;
				PrefabType type = PrefabUtility.GetPrefabType(instance);
				if(type == PrefabType.Prefab || type == PrefabType.ModelPrefab){continue;}
				if(hasPrefab){
					PrefabUtility.RevertPrefabInstance(instance);
				}
			}
		}
		public bool IsSprite(GameObject instance){
			MeshFilter filter = instance.GetComponent<MeshFilter>();
			Sprite sprite = SpriteManager.GetSprite(instance.name);
			bool inGroup = instance.transform.parent != null && instance.transform.parent.name.Contains("SpriteGroup");
			return (inGroup && sprite != null) || (filter != null && filter.sharedMesh == this.assets.spriteMesh);
		}
		public void RepairSprite(RepairData data){
			if(data.gameObject == null){return;}
			string name = data.gameObject.name;
			GameObject current = data.gameObject;
			Sprite sprite = data.sprite;
			this.activeMaterial = data.material;
			//this.ProgressReport("Repairing Scene Sprite " + (this.repairIndex+1) + "/" + this.sceneSprites.Length + "-" + current.name,false);
			bool isSprite = this.IsSprite(current);
			current = this.ApplySprite(current,sprite,true,false);
			if(current != null && isSprite){
				if(current.GetComponent<Renderer>() != null){
					if(current.GetComponent<Renderer>().sharedMaterial == null){
						string fullName = this.assetPrefix+sprite.fullName;
						string sourcePath = FileManager.GetPath(sprite.parent.xml);
						sourcePath = this.FindAssetFolder(sourcePath,sprite.name,"Materials");
						string materialPath = sourcePath+fullName+".mat";
						current.GetComponent<Renderer>().sharedMaterial = FileManager.GetAsset<Material>(materialPath);
						if(current.GetComponent<Renderer>().sharedMaterial == null){
							Debug.LogError("[SpriteWindow] Material assignment could not be fixed -- " + name);
						}
						else{
							Debug.Log("[SpriteWindow] Fixed material assignment -- " + name);
						}
					}
					this.SortShader(current);
				}
			}
			else if(isSprite){
				Debug.LogError("[SpriteWindow] Object was destroyed while attempting to repair -- " + name);
			}
		}
		//===========================
		// Asset Creation/Management
		//===========================
		public void LoadSpriteSheets(){
			SpriteManager.Clear();
			foreach(FileData file in FileManager.FindAll("*.xml")){
				TextAsset data = file.GetAsset<TextAsset>();
				SpriteManager.Add(data);
			}
			foreach(var sheetData in SpriteManager.spriteSheets){
				if(EditorPrefs.GetBool(sheetData.Key + "Hide")){
					this.collapsed.Add(sheetData.Value);
				}
			}
			this.spriteMaterials.Clear();
			this.spriteMeshes.Clear();
			this.spritePrefabs.Clear();
		}
		public void LoadPalettes(){
			foreach(var data in SpriteManager.spriteSheets){
				SpriteSheet sheet = data.Value;
				string paletteName = sheet.imagePath.Replace("Atlas","Lookup");
				FileData paletteImage = FileManager.Find(paletteName);
				if(paletteImage != null){
					Texture2D lookupMap = paletteImage.GetAsset<Texture2D>();
					this.spriteLookupMaps[sheet] = lookupMap;
				}
			}
		}
		public void PrepareAssets(Dictionary<string,Sprite> sequences){
			this.ClearSelection();
			this.bakeIndex = 0;
			this.repairIndex = 0;
			this.assetIndex = 0;
			this.loadScreen = false;
			this.assetsReady = false;
			this.repairSprites = false;
			this.generateMeshes = false;
			this.generateMaterials = false;
			this.generatePrefabs = false;
			this.spriteBakes.Clear();
			this.sceneSprites = new RepairData[0];
			this.assetSprites = new Dictionary<string,Sprite>(sequences).Values.ToArray();
		}
		public void PrepareLookupAtlases(){
			this.assetIndex = 0;
			this.assetsReady = false;
			List<SpriteSheet> valid = new List<SpriteSheet>();
			foreach(var sheetData in new Dictionary<string,SpriteSheet>(SpriteManager.spriteSheets)){
				SpriteSheet sheet = sheetData.Value;
				valid.Add(sheet);
			}
			this.assetSheets = valid.ToArray();
			this.generateLookupAtlases = true;
		}
		public void SeparateAtlas(SpriteSheet sheet){
			/*string path = FileManager.Find(sheet.imagePath).GetFolderPath() + sheet.imagePath.Replace("Atlas","Lookup");
			foreach(var data in sheet.sequences){
				Sprite sprite = data.Value;
				foreach(Frame frame in sprite.frames){
					Texture2D current = new Texture2D((int)frame.fullBounds.width,(int)frame.fullBounds.height);
					Texture2D.ReadPixels(frame.bounds,
				}
			}
			lookupTexture.SetPixels32(pixels);*/
		}
		public void CreateLookupAtlases(){
			if(this.assetIndex < this.assetSheets.Length){
				SpriteSheet sheet = this.assetSheets[this.assetIndex];
				Texture2D lookupTexture = new Texture2D(sheet.width,sheet.height);
				lookupTexture.SetPixels32(sheet.image.GetPixels32());
				Color errorColor = new Color(1,0,0,1);
				Color blackColor = new Color(0,0,0,1);
				string path = FileManager.Find(sheet.imagePath).GetFolderPath() + sheet.imagePath.Replace("Atlas","Lookup");
				this.ProgressReport("Creating Lookup Atlas " + (this.assetIndex+1) + " / " + this.assetSheets.Length + "-" + sheet.imagePath,false);
				foreach(var data in sheet.sequences){
					Sprite sprite = data.Value;
					foreach(Frame frame in sprite.frames){
						int nextShade = 1;
						Dictionary<Color,Color> mapping = new Dictionary<Color,Color>();
						int[] bounds = frame.GetIntBounds();
						bounds[1] = sheet.height - (bounds[1] + bounds[3]);
						Color[] area = sheet.image.GetPixels(bounds[0],bounds[1],bounds[2],bounds[3]);
						int index = 0;
						foreach(Color pixel in area){
							Color color = pixel;
							if(mapping.ContainsKey(color)){color = mapping[color];}
							else if(pixel == blackColor){color = blackColor;}
							else{
								float value = nextShade * 0.125f;
								Color shade = nextShade > 8 ? errorColor : new Color(value,value,value,1);
								mapping[color] = color = shade;
								nextShade += 1;
							}
							area[index] = color;
							++index;
						}
						if(nextShade > 8){
							int limit = nextShade-8;
							string message = "[SpriteWindow] " + sprite.fullName + " could not be converted to lookup map.";
							message += "  Maximum colors [8] exceeded by " + limit + ".";
							Debug.LogWarning(message);
						}
						lookupTexture.SetPixels(bounds[0],bounds[1],bounds[2],bounds[3],area);
					}
				}
				FileManager.WriteFile(path,lookupTexture.EncodeToPNG());
				DestroyImmediate(lookupTexture);
				this.assetIndex += 1;
			}
			else{
				this.assetsReady = true;
				this.generateLookupAtlases = false;
				this.Repaint();
			}
		}
		public string FindAssetFolder(string path,string assetName,string assetType){
			string folderPath = path.GetDirectory() + "/";
			string targetPath = folderPath;
			string[] folders = Directory.GetDirectories(folderPath);
			foreach(string subFolder in folders){
				string folderName = new DirectoryInfo(subFolder).Name;
				string pascalName = folderName.Replace(" ","");
				if(assetName.Contains(pascalName)){
					targetPath += folderName + "/";
					break;
				}
			}
			if(this.sortByType){targetPath += assetType + "/";}
			return targetPath;
		}
		public void SortShader(GameObject instance){
			if(instance.GetComponent<Renderer>() == null || instance.GetComponent<Renderer>().sharedMaterial == null){return;}
			Shader shader = instance.GetComponent<Renderer>().sharedMaterial.shader;
			bool systemShader = shader.name.Contains("Sprite");
			bool dualSprite = instance.name.Contains("@Bottom") || instance.name.Contains("@Top");
			bool flat = instance.transform.localEulerAngles.x == 270 || instance.transform.localEulerAngles.x == -90;
			if(systemShader){
				bool viable = flat && !dualSprite;
				Shader targetShader = viable ? this.assets.shaderFlat : this.assets.shaderNormal;
				if(shader != targetShader){
					instance.GetComponent<Renderer>().sharedMaterial.shader = targetShader;
				}
				instance.GetComponent<Renderer>().castShadows = !flat;
			}
		}
		public void CreateMaterial(Sprite sprite,bool forceOverwrite=false,bool forceUpdate=false){
			string core = this.assetPrefix+sprite.fullName;
			string[] names ={core};
			string sourcePath = FileManager.GetPath(sprite.parent.xml);
			Shader spriteShader = this.assets.shaderNormal;
			sourcePath = this.FindAssetFolder(sourcePath,sprite.name,"Materials");
			foreach(string name in names){
				string materialPath = sourcePath+name+".mat";
				if(!FileManager.Exists(materialPath) || forceOverwrite || forceUpdate){
					if(forceOverwrite){
						AssetDatabase.DeleteAsset(materialPath);
					}
					this.ProgressReport(name+".mat");
					bool existed = true;
					Material spriteMaterial = FileManager.GetAsset<Material>(materialPath);
					if(spriteMaterial == null){
						Debug.Log("[SpriteWindow] Creating Material -- " + name);
						existed = false;
						spriteMaterial = new Material(spriteShader);
					}
					if(this.ApplyMaterial(spriteMaterial,sprite)){
						Debug.Log("[SpriteWindow] Updating Material -- " + name);
						//spriteMaterial.shader = spriteShader;
						if(!existed){AssetDatabase.CreateAsset(spriteMaterial,materialPath);}
						else{
							AssetDatabase.SaveAssets();
						}
						AssetDatabase.Refresh();
					}
					this.spriteMaterials[name] = spriteMaterial;
				}
				else{
					Debug.Log("[SpriteWindow] Loading Material -- " + name);
					this.spriteMaterials[name] = FileManager.GetAsset<Material>(materialPath);
					if(this.spriteMaterials[name] == null){
						Debug.LogWarning("[SpriteWindow] Material could not be loaded -- " + name);
						this.spriteMaterials.Remove(name);
					}
				}
				if(this.spriteMaterials.ContainsKey(name) && this.spriteMaterials[name].name != name){
					Debug.Log("[SpriteWindow] Fixing Material Name -- " + name);
					this.spriteMaterials[name].name = name;
				}
			}
		}
		public void CreateMesh(Sprite sprite,bool forceOverwrite=false,bool forceUpdate=false){
			string core = this.assetPrefix+sprite.fullName;
			string[] names ={core};
			string sourcePath = FileManager.GetPath(sprite.parent.xml);
			sourcePath = this.FindAssetFolder(sourcePath,sprite.name,"Meshes");
			foreach(string name in names){
				string meshPath = sourcePath+name+".asset";
				if(!FileManager.Exists(meshPath) || forceOverwrite || forceUpdate){
					if(forceOverwrite){
						AssetDatabase.DeleteAsset(meshPath);
					}
					Debug.Log("[SpriteWindow] Creating Mesh -- " + name);
					this.ProgressReport(name+".asset");
					bool existed = true;
					Mesh spriteMesh = FileManager.GetAsset<Mesh>(meshPath);
					if(spriteMesh == null){
						existed = false;
						spriteMesh = (Mesh)Mesh.Instantiate(this.assets.spriteMesh);
					}
					spriteMesh.name = name;
					Vector4 uv = sprite.current.uv;
					Debug.Log(sprite.current.bounds.ToString());
					Debug.Log(sprite.current.fullBounds.ToString());
					float xDifference = sprite.current.fullBounds.width-sprite.current.bounds.width;
					float yDifference = sprite.current.fullBounds.height-sprite.current.bounds.height;
					float left = (sprite.current.fullBounds.width-(xDifference*2))*0.001937f;
					float right = sprite.current.fullBounds.width*0.001937f;
					float top = (sprite.current.fullBounds.height*0.001937f) * 1.414f;
					float bottom = ((sprite.current.fullBounds.height-(yDifference*2))*0.001937f) * 1.414f;
					Vector3[] meshPosition = this.assets.spriteMesh.vertices;
					meshPosition[0] = Vector3.Scale(meshPosition[0],new Vector3(left,bottom,1));
					meshPosition[1] = Vector3.Scale(meshPosition[1],new Vector3(right,bottom,1));
					meshPosition[2] = Vector3.Scale(meshPosition[2],new Vector3(left,top,1));
					meshPosition[3] = Vector3.Scale(meshPosition[3],new Vector3(right,top,1));
					spriteMesh.vertices = meshPosition;
					Vector2[] meshUV = new Vector2[spriteMesh.vertices.Length];
					meshUV[0] = new Vector2(uv.x,uv.y);
					meshUV[1] = new Vector2(uv.z,uv.y);
					meshUV[2] = new Vector2(uv.x,uv.w);
					meshUV[3] = new Vector2(uv.z,uv.w);
					spriteMesh.uv = meshUV;
					this.CreateMaterial(sprite);
					if(this.spriteMaterials.ContainsKey(name) && this.spriteMaterials[name] != null){
						Material material = this.spriteMaterials[name];
						Color shading = material.HasProperty("shadingColor") ? material.GetColor("shadingColor") : new Color(0,0,0,0);
						float shadingIgnoreCutoff = material.HasProperty("shadingIgnoreCutoff") ? material.GetFloat("shadingIgnoreCutoff") : 0;
						Color lerpColor = material.HasProperty("lerpColor") ? material.GetColor("lerpColor") : new Color(0,0,0,0);
						float lerpCutoff = material.HasProperty("lerpCutoff") ? material.GetFloat("lerpCutoff") : 0;
						//float alpha = material.GetFloat("alpha");
						float shadingSteps = material.HasProperty("shadingSteps") ? 1.0f / (material.GetFloat("shadingSteps")-1) : 0;
						float packedA = Store.PackFloats(shading.r,shading.g,shading.b,shading.a);
						float packedB = Store.PackFloats(shadingSteps,lerpCutoff,shadingIgnoreCutoff);
						Vector2[] meshUV2 = new Vector2[4];
						Color32[] meshColors = new Color32[4];
						for(int index=0;index<4;++index){
							meshColors[index] = lerpColor;
							meshUV2[index] = new Vector2(packedA,packedB);
						}
						spriteMesh.uv2 = meshUV2;
						spriteMesh.colors32 = meshColors;
					}
					spriteMesh.RecalculateBounds();
					spriteMesh.RecalculateNormals();
					spriteMesh.RecalculateTangents();
					if(!existed){AssetDatabase.CreateAsset(spriteMesh,meshPath);}
					this.spriteMeshes[name] = spriteMesh;
					AssetDatabase.Refresh();
				}
				else{
					Debug.Log("[SpriteWindow] Loading Mesh -- " + name);
					this.spriteMeshes[name] = FileManager.GetAsset<Mesh>(meshPath);
				}
			}
		}
		public void CreatePrefab(Sprite sprite,bool forceOverwrite=false,bool forceUpdate=false){
			string core = this.assetPrefix+sprite.fullName;
			string[] names ={core};
			string sourcePath = FileManager.GetPath(sprite.parent.xml);
			sourcePath = this.FindAssetFolder(sourcePath,sprite.name,"Prefabs");
			foreach(string name in names){
				string prefabName = name.EndsWith("@Bottom") ? name.Replace("@Bottom","") : name;
				prefabName = prefabName.EndsWith("@Top") ? prefabName.Replace("@Top","") : prefabName;
				string prefabPath = sourcePath+prefabName+".prefab";
				if(!FileManager.Exists(prefabPath) || forceOverwrite || forceUpdate){
					if(forceOverwrite){
						AssetDatabase.DeleteAsset(prefabPath);
					}
					bool adjustScale = this.autoScale;
					GameObject spritePrefab;
					GameObject scalePrefab;
					UnityEngine.Object prefab = FileManager.GetAsset<GameObject>(prefabPath);
					if(prefab == null){
						Debug.Log("[SpriteWindow] Creating Prefab -- " + name);
						this.ProgressReport(prefabName+".prefab");
						prefab = PrefabUtility.CreateEmptyPrefab(prefabPath);
						spritePrefab = scalePrefab = new GameObject(name);
						MeshFilter meshFilter = spritePrefab.AddComponent<MeshFilter>();
						MeshRenderer meshRenderer = spritePrefab.AddComponent<MeshRenderer>();
						if(!this.spriteMaterials.ContainsKey(name)){
							Debug.LogError("[SpriteWindow] Material for prefab does not exist -- " + name);
							return;
						}
						if(this.spriteMaterials[name] == null){
							this.spriteMaterials.Remove(name);
							Debug.LogError("[SpriteWindow] Material key for prefab was corrupt.  Please try again -- " + name);
							return;
						}
						if(sprite.animated){
							SpriteController script = spritePrefab.AddComponent<SpriteController>();
							script.spriteTexture = sprite.parent.image;
							script.spriteXML = sprite.parent.xml;
							script.spriteName = sprite.name;
							script.spriteAnimation = sprite.sequence;
						}
						meshFilter.sharedMesh = this.assets.spriteMesh;
						meshRenderer.sharedMaterial = this.spriteMaterials[name];
						spritePrefab.transform.localEulerAngles = new Vector3(0,180,0);
						adjustScale = true;
					}
					else{
						spritePrefab = scalePrefab = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
						Vector3 localScale = spritePrefab.transform.localScale;
						this.ProgressReport(prefabName+".prefab");
						if(localScale == Vector3.one){
							Transform prefabSearch = spritePrefab.transform.Find(sprite.name);
							if(prefabSearch == null){
								DestroyImmediate(spritePrefab);
								continue;
							}
							scalePrefab = prefabSearch.gameObject;
						}
						Debug.Log("[SpriteWindow] Updating Prefab -- " + name);
					}
					if(adjustScale){
						Vector3 currentScale = scalePrefab.transform.localScale;
						currentScale.x = sprite.current.fullBounds.width*0.001937f;
						currentScale.y = sprite.current.fullBounds.height*0.001937f;
						if(this.scaleMode == ScaleMode.Auto3D){currentScale.y *= 1.414f;}
						scalePrefab.transform.localScale = currentScale;
					}
					PrefabUtility.ReplacePrefab(spritePrefab,prefab,ReplacePrefabOptions.ConnectToPrefab);
					DestroyImmediate(spritePrefab);
					AssetDatabase.Refresh();
				}
				Debug.Log("[SpriteWindow] Loading Prefab -- " + name);
				string path = prefabPath;
				this.spritePrefabs[name] = FileManager.GetAsset<GameObject>(path);
				if(this.spritePrefabs[name] == null){
					this.spritePrefabs.Remove(name);
					if(forceOverwrite){continue;}
					bool deleted = EditorUtility.DisplayDialog("Delete Corrupt Prefab",path + " appears corrupt.  Delete file?","Yes","No");
					if(deleted){
						this.CreatePrefab(sprite,true);
					}
					else{
						Debug.LogError("[SpriteWindow] Failed loading prefab -- " + path);
					}
				}
			}
		}
		public void ProgressStep(){
			if(this.assetIndex < this.assetSprites.Length){
				Sprite sprite = this.assetSprites[this.assetIndex];
				if(this.generateMeshes){this.CreateMesh(sprite,this.forceOverwrite,true);}
				if(this.generateMaterials){this.CreateMaterial(sprite,this.forceOverwrite,true);}
				if(this.generatePrefabs){this.CreatePrefab(sprite,this.forceOverwrite,true);}
				this.assetIndex += 1;
			}
			else if(this.repairIndex < this.sceneSprites.Length){
				if(this.repairSprites){
					int index = 0;
					for(index=0;index<423;++index){
						int totalIndex = this.repairIndex+index;
						if(totalIndex >= this.sceneSprites.Length){break;}
						RepairData data = this.sceneSprites[totalIndex];
						this.RepairSprite(data);
					}
					this.repairIndex += index;
					this.ProgressReport("Scene Repair-"+this.repairIndex+"/"+this.sceneSprites.Length+" Objects",false);
				}
			}
			else if(this.bakeIndex < this.spriteBakes.Count){
				bool bake = this.spriteBakes.Values.ToArray()[this.bakeIndex];
				GameObject current = this.spriteBakes.Keys.ToArray()[this.bakeIndex];
				this.BakeSprite(current,bake);
				this.ProgressReport("Baking Sprite-"+this.bakeIndex+"/"+this.spriteBakes.Count+" Objects",false);
				this.bakeIndex += 1;
			}
			else{
				this.assetsReady = true;
				this.Repaint();
			}
		}
		public void ProgressReport(string message,bool progress=true){
			string report = "";
			if(progress){
				report = "[" + (this.assetIndex+1) + "/" + this.assetSprites.Length + "] Creating assets -";
			}
			this.loadMessage = report + message;
			this.Repaint();
		}
		//===========================
		// Brush/Prefab Management
		//===========================
		public void ResetBrush(){
			Debug.Log("[SpriteWindow] Resetting Brush");
			this.ClearInstances("@Brush");
			this.activeMaterial = null;
			this.brush = new GameObject("@Brush");
			this.brush.tag = "EditorOnly";
			//this.brush.hideFlags = HideFlags.HideAndDontSave;
			this.brushDistanceOffset = 1.0f;
			this.brushScale = new Vector3(1,1,1);
			this.brushPositionOffset = new Vector3(0,0,0);
			this.brushRotationOffset = new Vector3(0,0,0);
			this.ApplySprite(this.brush);
		}
		public void ClearInstances(string name){
			GameObject[] all = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
			for(int index=0;index < all.Length;++index){
				GameObject current = all[index];
				if(current == null){continue;}
				PrefabType type = PrefabUtility.GetPrefabType(current);
				if(type == PrefabType.Prefab || type == PrefabType.ModelPrefab){continue;}
				if(current.name.Contains(name)){
					DestroyImmediate(current);
				}
			}
		}
		public GameObject ApplySprite(GameObject target = null,Sprite sprite = null,bool fixPath = true,bool allowGeneration = true){
			GameObject[] objects = target != null ? new GameObject[1]{target} : Selection.gameObjects;
			if(objects.Length < 1){return target;}
			if(sprite == null){sprite = this.selected;}
			if(this.assetsReady){
				string activeName = this.activeMaterial != null ? " to " + this.activeMaterial.name : "";
				Debug.Log("[SpriteWindow] Applying Sprite -- " + sprite.fullName + activeName);
			}
			string spriteName = this.assetPrefix+sprite.fullName;
			bool assetState = this.assetsReady;
			this.assetsReady = false;
			if(!this.spriteMaterials.ContainsKey(spriteName)){this.CreateMaterial(sprite);}
			if(!this.spritePrefabs.ContainsKey(spriteName)){this.CreatePrefab(sprite);}
			bool forceMaterialUpdate = this.forceUpdate;
			bool forceOverwrite = this.forceOverwrite && allowGeneration;
			bool materialExists = this.spriteMaterials.ContainsKey(spriteName) && this.spriteMaterials[spriteName] != null;
			bool prefabExists = this.spritePrefabs.ContainsKey(spriteName) && this.spritePrefabs[spriteName] != null;
			bool noBrushMaterial = (target == this.brush) && !materialExists;
			bool mismatchedAtlas = materialExists && !sprite.animated && this.spriteMaterials[spriteName].GetVector("atlasUV") != sprite.current.uv;
			//bool meshExists = this.spriteMeshes.ContainsKey(spriteName) && this.spriteMeshes[spriteName] != null;
			if((mismatchedAtlas || noBrushMaterial)){
				Debug.Log("[SpriteWindow] Mismatch = " + mismatchedAtlas + " | No Brush Material = " + noBrushMaterial);
				forceMaterialUpdate = true;
			}
			if(!materialExists || forceOverwrite || forceMaterialUpdate){this.CreateMaterial(sprite,forceOverwrite,forceMaterialUpdate);}
			if(!prefabExists  || forceOverwrite){this.CreatePrefab(sprite,forceOverwrite,this.forceUpdate);}
			this.assetsReady = assetState;
			if(!this.spriteMaterials.ContainsKey(spriteName) || !this.spritePrefabs.ContainsKey(spriteName)){
				string type = !this.spriteMaterials.ContainsKey(spriteName) ? "Material" : "Prefab";
				Debug.LogWarning("[SpriteWindow] " + type + " keys do not exist -- " + spriteName);
				return target;
			}
			foreach(GameObject current in objects){
				if(current != null){
					bool isSprite = this.IsSprite(current);
					Material spriteMaterial = this.spriteMaterials[spriteName];
					GameObject spritePrefab = this.spritePrefabs[spriteName];
					if(spriteMaterial == null || spritePrefab == null){
						string type = !spriteMaterial ? "material" : "prefab";
						Debug.LogError("[SpriteWindow] Problem finding/creating " + type + " -- " + spriteName);
						return target;
					}
					if(this.brush == current){
						DestroyImmediate(this.brush);
						this.brushPrefab = this.spritePrefabs[spriteName];
						this.brush = (GameObject)PrefabUtility.InstantiatePrefab(this.brushPrefab);
						this.brush.tag = "EditorOnly";
						//this.brush.hideFlags = HideFlags.HideAndDontSave;
						this.brush.name = "@Brush ["+sprite.fullName+"]";
						this.brushScale.x = spritePrefab.transform.localScale.x;
						this.brushScale.y = spritePrefab.transform.localScale.y;
						this.SortShader(this.brush);
						this.DelayRender();
					}
					else if(isSprite){
						string cleanName = sprite.fullName.Replace("@Bottom","").Replace("@Top","");
						bool root = current.transform.parent == null;
						bool dualSprite = sprite.fullName.Contains("@Bottom") || sprite.fullName.Contains("@Top");
						bool fixablePath = root || current.transform.parent.name.Contains("SpriteGroup");
						bool childOfPrefab = !root && PrefabUtility.GetPrefabObject(current.transform.parent) != null;
						if(fixPath && fixablePath){
							GameObject path = Locate.GetScenePath("Scene/SpriteGroup-"+cleanName);
							if(!dualSprite){current.transform.parent = path.transform;}
							if(dualSprite && current.transform.parent.parent == null){
								current.transform.parent.parent = path.transform;
							}
						}
						if(!childOfPrefab && this.spritePrefabs.ContainsKey(spriteName) && !dualSprite){
							GameObject instance = (GameObject)Instantiate(this.spritePrefabs[spriteName]);
							instance.name = cleanName;
							instance.transform.parent = current.transform.parent;
							instance.transform.position = current.transform.position;
							instance.transform.eulerAngles = current.transform.eulerAngles;
							Vector3 scale = current.transform.localScale;
							if(this.autoScale){
								scale.x = spritePrefab.transform.localScale.x * Mathf.Sign(scale.x);
								scale.y = spritePrefab.transform.localScale.y * Mathf.Sign(scale.y);
							}
							instance.transform.localScale = scale;
							if(current == target){target = instance;}
							DestroyImmediate(current);
						}
					}
					else if(this.activeMaterial == null){
						Debug.LogWarning("[SpriteWindow] Cannot apply atlas selection.  Object is not a sprite nor does it have an atlas-capable shader -- " + current.name);
					}
				}
			}
			if(this.activeMaterial != null){
				this.ApplyMaterial(this.activeMaterial,sprite);
			}
			return target;
		}
		public bool ApplyMaterial(Material material,Sprite sprite){
			bool changed = false;
			string[] names = new string[]{"diffuseMap"};
			if(material == null){return false;}
			if(material.GetVector("atlasUV") != sprite.current.uv){
				material.SetVector("atlasUV",sprite.current.uv);
				changed = true;
			}
			if(material.GetVector("paddingUV") != sprite.current.uvPadding){
				material.SetVector("paddingUV",sprite.current.uvPadding);
				changed = true;
			}
			foreach(string name in names){
				if(material.GetTexture(name) != sprite.parent.image){
					material.SetTexture(name,sprite.parent.image);
					changed = true;
				}
				if(material.GetTextureScale(name) != Vector2.one){
					material.SetTextureScale(name,Vector2.one);
					changed = true;
				}
				if(material.GetTextureOffset(name) != Vector2.zero){
					material.SetTextureOffset(name,Vector2.zero);
					changed = true;
				}
			}
			if(changed && material.shader != this.assets.shaderEditor){
				Debug.Log("[SpriteWindow] Material properties changed -- " + material.name);
			}
			return changed;
		}
		public void ApplyOptions(GameObject sprite){
			SpriteController controller = sprite.GetComponent<SpriteController>();
			if(controller != null){
				controller.spriteSpeed = this.selectedSpeed;
				controller.spriteLoop = this.selected.loop;
				controller.spriteReverse = this.selected.reverse;
				controller.spriteActive = this.selected.active;
				controller.spriteRandomStart = this.randomizeStart;
			}
		}
		//===========================
		// Rendering
		//===========================
		public void DrawAnimated(){
			if(this.showAnimations){
				if(this.selected != null && this.selected.animated && this.showSelected){
					this.Repaint();
				}
			}
		}
		public void DrawScene(SceneView view){
			if(Application.isPlaying){return;}
			this.CheckHotkeys();
			this.DrawAnimated();
			this.visible = false;
			if(this.disabled){return;}
			KeyShortcut CheckKeyDown = Button.EventKeyDown;
			bool useEvent = false;
			bool brushUsable = EditorWindow.mouseOverWindow == view;
			if((!this.placementMode || !brushUsable) && this.brush != null && this.brush.activeSelf){
				//DestroyImmediate(this.brush);
				this.brush.SetActive(false);
			}
			if(this.placementMode && brushUsable && UnityEvent.current.type != EventType.Repaint && UnityEvent.current.type != EventType.Layout){
				bool alt = UnityEvent.current.alt;
				bool control = UnityEvent.current.control;
				bool shift = UnityEvent.current.shift;
				bool mouseMove = UnityEvent.current.type == EventType.MouseMove;
				float brushXRotate = this.brushFlat ? 270 : 0;
				Vector2 mouse = UnityEvent.current.mousePosition;
				Vector3 brushPosition = new Vector3(0,0,0);
				Vector3 brushRotation = new Vector3(brushXRotate,180,0);
				if((control||alt||shift) && mouseMove){
					mouse = this.controlPosition;
				}
				Ray ray = HandleUtility.GUIPointToWorldRay(mouse);
				RaycastHit collision = new RaycastHit();
				bool mousePressed = false;
				useEvent = true;
				if(this.brush == null || !this.brush.name.Contains(this.selected.fullName)){
					this.ResetBrush();
				}
				this.brush.SetActive(true);
				if(this.brush.GetComponentsInChildren<Transform>().Length > 1){
					brushRotation.x = 0;
					brushRotation.y = 0;
				}
				this.ClearSelection();
				int terrainMask = (1 << 13);
				if(Physics.Raycast(ray,out collision,Mathf.Infinity,terrainMask)){
					brushPosition = collision.point;
					brushPosition += collision.normal;
				}
				if(UnityEvent.current.type == EventType.MouseDown && this.mouseState < 2){
					this.mouseState = 2;
					mousePressed = true;
				}
				else if(UnityEvent.current.type == EventType.MouseUp && this.mouseState != 1){
					this.mouseState = 1;
				}
				if(UnityEvent.current.type == EventType.ScrollWheel){
					float scroll = UnityEvent.current.delta[1];
					if(control){
						float amount = 1.0f - (scroll * 0.015f);
						this.brushScale *= amount;
					}
					else if(shift){
						this.brushPositionOffset.y -= scroll*0.5f;
					}
					else{useEvent = false;}
				}
				else if(CheckKeyDown(KeyCode.R)){
					this.ResetBrush();
				}
				else if(CheckKeyDown(KeyCode.Return) || mousePressed && UnityEvent.current.button == 2){
					this.brushFlat = !this.brushFlat;
				}
				else if(UnityEvent.current.button == 1){
					if(mousePressed){
						string prefabName = this.selected.name.Replace("@Bottom","").Replace("@Top","");
						GameObject path = Locate.GetScenePath("Scene/SpriteGroup-"+prefabName);
						GameObject stamp = (GameObject)PrefabUtility.InstantiatePrefab(this.brushPrefab);
						Undo.RegisterCreatedObjectUndo(stamp,"Stamp Brush");
						Renderer renderer = stamp.GetComponentInChildren<Renderer>();
						if(this.selected.animated && renderer != null){
							for(int index=0;index<renderer.sharedMaterials.Length;++index){
								renderer.sharedMaterials[index] = (Material)Instantiate(renderer.sharedMaterials[index]);
							}
						}
						stamp.transform.localPosition = brushPosition + this.brushPositionOffset;
						stamp.transform.localEulerAngles = brushRotation + this.brushRotationOffset;
						stamp.transform.localScale = this.brushScale;
						stamp.transform.parent = path.transform;
						stamp.name = prefabName;
						this.SortShader(stamp);
						this.ApplyOptions(stamp);
					}
				}
				else{useEvent = false;}
				this.brush.transform.localPosition = brushPosition + this.brushPositionOffset;
				this.brush.transform.localEulerAngles = brushRotation + this.brushRotationOffset;
				this.brush.transform.localScale = this.brushScale;
				view.Repaint();
			}
			if(useEvent){UnityEvent.current.Use();}
		}
		public void DrawResetButton(float x,float y,float width,float height){
			if(GUI.Button(new Rect(x,y,width,height),"Reset")){
				Undo.RegisterSceneUndo("Reset Sprite System");
				this.ResetSystem();
			}
		}
		public void DrawGenerateButtons(float currentX,float currentY,Dictionary<string,Sprite> sequences){
			Rect area = new Rect(currentX+Screen.width-95,currentY-2,24,24);
			if(GUI.Button(area,this.tooltips.generateMaterials,this.assets.UI.GetStyle("Material Button"))){
				this.PrepareAssets(sequences);
				this.generateMaterials = true;
			}
			area = new Rect(currentX+Screen.width-69,currentY-2,24,24);
			if(GUI.Button(area,this.tooltips.generateMeshes,this.assets.UI.GetStyle("Mesh Button"))){
				this.PrepareAssets(sequences);
				this.generateMeshes = true;
			}
			area = new Rect(currentX+Screen.width-45,currentY-2,24,24);
			if(GUI.Button(area,this.tooltips.generatePrefabs,this.assets.UI.GetStyle("Prefab Button"))){
				this.PrepareAssets(sequences);
				this.generatePrefabs = true;
			}
		}
		public void SetupBakes(bool bake){
			GameObject[] objects = Selection.gameObjects.Length > 0 ? Selection.gameObjects : (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
			List<string> handled = new List<string>();
			foreach(GameObject current in objects){
				MeshFilter filter = current.GetComponent<MeshFilter>();
				if(filter == null || filter.sharedMesh == null || handled.Contains(current.name)){continue;};
				if(bake && filter.sharedMesh == this.assets.spriteMesh && current.GetComponent<Renderer>() != null){
					if(current.GetComponent<Renderer>().sharedMaterial == null){continue;}
					this.spriteBakes.Add(current,bake);
				}
				else{
					FileData materialFile = FileManager.Find(filter.sharedMesh.name+".mat");
					if(materialFile != null && current.GetComponent<Renderer>().sharedMaterial.name.Contains("[")){
						this.spriteBakes.Add(current,bake);
					}
				}
				GameObject root = PrefabUtility.FindPrefabRoot(current);
				PrefabType type = PrefabUtility.GetPrefabType(root);
				if(type == PrefabType.PrefabInstance || type == PrefabType.ModelPrefabInstance){
					handled.Add(current.name);
				}
			}
		}
		public void BakeSprite(GameObject current,bool bake){
			MeshFilter filter = current.GetComponent<MeshFilter>();
			if(bake){
				Renderer renderer = current.GetComponent<Renderer>();
				FileData meshFile = FileManager.Find(renderer.sharedMaterial.name+".asset");
				if(meshFile != null){
					string materialName = renderer.sharedMaterial.name.Replace(this.assetPrefix,"");
					Sprite sprite = SpriteManager.GetSprite(materialName);
					string atlasName = "["+sprite.parent.name+"].mat";
					Material atlasMaterial = new Material(this.assets.shaderEmbed);
					FileData materialFile = FileManager.Find(atlasName);
					if(materialFile == null){
						string atlasPath = FileManager.GetPath(sprite.parent.xml);
						atlasPath = atlasPath.GetDirectory() + "/" + atlasName;
						AssetDatabase.CreateAsset(atlasMaterial,atlasPath);
						AssetDatabase.Refresh();
						atlasMaterial = (Material)AssetDatabase.LoadMainAssetAtPath(atlasPath);
					}
					else{atlasMaterial = materialFile.GetAsset<Material>();};
					filter.sharedMesh = meshFile.GetAsset<Mesh>();
					renderer.sharedMaterial = atlasMaterial;
					current.transform.localScale = new Vector3(1,1,1);
				}
			}
			else{
				FileData materialFile = FileManager.Find(filter.sharedMesh.name+".mat");
				Material spriteMaterial = materialFile.GetAsset<Material>();
				string materialName = spriteMaterial.name.Replace(this.assetPrefix,"");
				Sprite sprite = SpriteManager.GetSprite(materialName);
				if(sprite != null){
					Vector3 scale = current.transform.localScale;
					scale.x = sprite.current.fullBounds.width*0.001937f;
					scale.y = (sprite.current.fullBounds.height*0.001937f) * 1.414f;
					current.transform.localScale = scale;
				}
				current.GetComponent<Renderer>().sharedMaterial = spriteMaterial;
				filter.sharedMesh = this.assets.spriteMesh;
			}
			GameObject root = PrefabUtility.FindPrefabRoot(current);
			PrefabType type = PrefabUtility.GetPrefabType(root);
			if(type == PrefabType.PrefabInstance || type == PrefabType.ModelPrefabInstance){
				PrefabUtility.ReplacePrefab(root,PrefabUtility.GetPrefabParent(root),ReplacePrefabOptions.ConnectToPrefab);
			}
		}
		public void DrawWindow(){
			this.disabled = false;
			this.title = "Sprites";
			if(Application.isPlaying){
				Rect area = new Rect(0,5,Screen.width,28);
				GUI.Label(area,"Sprite Window disabled in Play Mode.",this.assets.UI.GetStyle("Title Options"));
			}
			else if(SpriteManager.sequences.Count == 0){
				this.DrawResetButton(5,3,100,22);
			}
			else if(!this.assetsReady){
				Rect area = new Rect(0,5,Screen.width,28);
				GUI.Label(area,this.loadMessage.Split('-')[0],this.assets.UI.GetStyle("Title Active"));
				area = new Rect(0,38,Screen.width,28);
				GUI.Label(area,this.loadMessage.Split('-')[1],this.assets.UI.GetStyle("Title Options"));
				area = new Rect(Screen.width/2-40,80,100,30);
				if(GUI.Button(area,"Cancel")){
					this.assetsReady = true;
				}
				this.loadScreen = true;
			}
			else if(this.assets != null){
				Sprite ghost = new Sprite();
				Rect scrollMin = new Rect(0,-20,Screen.width,Screen.height);
				Rect scrollMax = new Rect(0,-20,this.totalX,this.totalY);
				this.scrollPosition = GUI.BeginScrollView(scrollMin,this.scrollPosition,scrollMax);
				this.totalX = Screen.width;
				float currentX = this.scrollPosition.x+5;
				float currentY = 5;
				/*Rect area = new Rect(currentX+Screen.width-45,currentY-2,24,24);
				if(GUI.Button(area,this.tooltips.generateLookupAtlas,this.assets.UI.GetStyle("Mesh Button"))){
					this.PrepareLookupAtlases();
				}*/
				Rect area = new Rect(currentX-5,currentY,Screen.width,28);
				GUI.Label(area,"Options",this.assets.UI.GetStyle("Title Options"));
				this.DrawGenerateButtons(currentX-5,currentY,SpriteManager.sequences);
				if(GUI.Button(area,"",GUIStyle.none)){
					this.showOptions = !this.showOptions;
				}
				currentY += 33;
				if(this.showOptions){
					int columnWidth = (Screen.width/3)-15;
					GUI.Label(new Rect(currentX,currentY,columnWidth,20),"Editor",this.assets.UI.GetStyle("Section Title"));
					this.placementMode = GUI.Toggle(new Rect(currentX,currentY+22,columnWidth,20),this.placementMode,this.tooltips.placementMode);
					this.editMode = GUI.Toggle(new Rect(currentX,currentY+44,columnWidth,20),this.editMode,this.tooltips.editMode);
					this.showAnimations = GUI.Toggle(new Rect(currentX,currentY+66,columnWidth,20),this.showAnimations,this.tooltips.showAnimations);
					this.fitPreviews = GUI.Toggle(new Rect(currentX,currentY+88,columnWidth,20),this.fitPreviews,this.tooltips.fitPreviews);
					this.fitSelected = GUI.Toggle(new Rect(currentX,currentY+110,columnWidth,20),this.fitSelected,this.tooltips.fitSelected);
					this.DrawResetButton(currentX,currentY+129,columnWidth,20);
					currentX = this.scrollPosition.x+10+columnWidth;
					columnWidth += 5;
					GUI.Label(new Rect(currentX,currentY,columnWidth,20),"Selection",this.assets.UI.GetStyle("Section Title"));
					this.applyOnSelection = GUI.Toggle(new Rect(currentX,currentY+22,columnWidth,20),this.applyOnSelection,this.tooltips.applyOnSelection);
					this.autoScale = GUI.Toggle(new Rect(currentX,currentY+44,columnWidth-50,20),this.autoScale,this.tooltips.autoScale);
					this.scaleMode = EditorGUI.Popup(new Rect(currentX+81,currentY+45,columnWidth-90,20),this.scaleMode,new string[2]{"2D","3D"});
					this.forceOverwrite = GUI.Toggle(new Rect(currentX,currentY+66,columnWidth,20),this.forceOverwrite,this.tooltips.forceOverwrite);
					this.DrawMaterialSelect(currentX,currentY+110,columnWidth);
					if(GUI.Button(new Rect(currentX,currentY+129,columnWidth,20),this.tooltips.bakeSprites)){
						this.PrepareAssets(null);
						this.SetupBakes(true);
					}
					columnWidth -= 5;
					currentX = this.scrollPosition.x+20+columnWidth*2;
					GUI.Label(new Rect(currentX,currentY,columnWidth,20),"Animation",this.assets.UI.GetStyle("Section Title"));
					GUI.enabled = this.selected.animated;
					this.selected.active = GUI.Toggle(new Rect(currentX,currentY+22,columnWidth,20),this.selected.active,this.tooltips.animationActive);
					this.selected.loop = GUI.Toggle(new Rect(currentX,currentY+44,columnWidth,20),this.selected.loop,this.tooltips.animationLoop);
					this.selected.reverse = GUI.Toggle(new Rect(currentX,currentY+66,columnWidth,20),this.selected.reverse,this.tooltips.animationReverse);
					this.randomizeStart = GUI.Toggle(new Rect(currentX,currentY+88,columnWidth,20),this.randomizeStart,this.tooltips.animationRandomStart);
					this.selectedSpeed = (int)(GUI.HorizontalSlider(new Rect(currentX,currentY+110,columnWidth-45,20),(float)this.selectedSpeed,1.0f,30.0f));
					GUI.Label(new Rect(currentX+columnWidth-43,currentY+110,45,20),this.selectedSpeed.ToString() + " fps");
					GUI.enabled = true;
					if(GUI.Button(new Rect(currentX,currentY+129,columnWidth,20),this.tooltips.deBakeSprites)){
						this.PrepareAssets(null);
						this.SetupBakes(false);
					}
					currentY += 158;
				}
				currentX = this.scrollPosition.x;
				area = new Rect(currentX,currentY,Screen.width,28);
				GUI.Label(area,"Selected [" + this.selected.name + "]",this.assets.UI.GetStyle("Title Selected"));
				this.DrawGenerateButtons(currentX,currentY,new Dictionary<string,Sprite>(){{this.selected.name,this.selected}});
				if(GUI.Button(area,"",GUIStyle.none)){
					this.showSelected = !this.showSelected;
				}
				currentY += 33;
				if(this.brush != null){
					this.ApplyOptions(this.brush);
				}
				if(this.showSelected){
					float sheetScale = this.fitSelected ? (float)Screen.width*0.5f / (float)this.selected.current.fullBounds.width : 1.0f;
					int selectedWidth = (int)(this.selected.current.fullBounds.width*sheetScale);
					float offsetX = this.scrollPosition.x+(Screen.width/2 - selectedWidth/2);
					this.DrawWindowSprite(this.selected,offsetX,currentY,sheetScale,true,false,this.showAnimations);
					currentY += (int)(this.selected.current.fullBounds.height*sheetScale) + 5;
				}
				foreach(var sheetData in new Dictionary<string,SpriteSheet>(SpriteManager.spriteSheets)){
					SpriteSheet sheet = sheetData.Value;
					float sheetScale = this.fitPreviews ? (float)Screen.width*0.95f / (float)sheet.width : 1.0f;
					bool iscollapsed = this.collapsed.Contains(sheet);
					GUIStyle titleStyle = this.assets.UI.GetStyle("Title");
					if(this.activeSheet == sheet){
						titleStyle = this.assets.UI.GetStyle("Title Active");
					}
					else if(iscollapsed){
						titleStyle = this.assets.UI.GetStyle("Title Collapsed");
					}
					area = new Rect(currentX,currentY,Screen.width,28);
					GUI.Label(area,"[" + sheetData.Key + "] " + sheet.sequences.Count + " Sprite[s]",titleStyle);
					if(sheet == this.activeSheet){
						this.DrawGenerateButtons(currentX,currentY,sheet.sequences);
					}
					area = new Rect(currentX,currentY,Screen.width,30);
					if(GUI.Button(area,"",GUIStyle.none)){
						if(iscollapsed){this.collapsed.Remove(sheet);}
						else{this.collapsed.Add(sheet);}
						this.activeSheet = sheet;
						EditorPrefs.SetBool(sheetData.Key + "Hide",!iscollapsed);
					}
					currentY += 33;
					if(this.collapsed.Contains(sheet)){
						currentY += 5;
						continue;
					}
					if(sheet.width > this.totalX){this.totalX = (int)(sheet.width*sheetScale)+10;}
					Rect sheetRect = new Rect(0,currentY,sheet.width*sheetScale,sheet.height*sheetScale);
					Rect background = new Rect(0,currentY,sheetRect.width/this.assets.grid.width,sheetRect.height/this.assets.grid.height);
					GUI.DrawTextureWithTexCoords(sheetRect,this.assets.grid,background);
					foreach(var item in sheet.sequences){
						Sprite sprite = item.Value;
						this.DrawWindowSprite(sprite,0,currentY,sheetScale,false,false,false);
						if(sprite.animated){
							for(int index=1;index < sprite.frames.Count;++index){
								ghost.Clear();
								ghost.AddFrame(sprite.frames[index]);
								ghost.parent = sprite.parent;
								ghost.fullName = sprite.name + "\n" + sprite.sequence + " (" + (index + 1) + "/" + sprite.frames.Count + ")";
								this.DrawWindowSprite(ghost,0,currentY,sheetScale,false,true,false);
							}
						}
					}
					currentY += (int)(sheetRect.height) + 5;
				}
				this.totalY = currentY + 10;
				GUI.EndScrollView();
			}
		}
		public void DrawMaterialSelect(float offsetX,float offsetY,float width){
			if(Selection.transforms.Length == 1){
				Dictionary<string,Material> options = new Dictionary<string,Material>();
				Renderer[] renderers = Selection.activeTransform.GetComponentsInChildren<Renderer>();
				foreach(Renderer renderer in renderers){
					foreach(Material material in renderer.sharedMaterials){
						if(material != null){
							options[material.name] = material;
						}
					}
				}
				string[] items = options.Keys.ToArray();
				if(items.Length > 0){
					this.materialIndex = EditorGUI.Popup(new Rect(offsetX,offsetY,width,20),this.materialIndex,items);
					if(this.materialIndex < options.Count){
						this.activeMaterial = options[items[this.materialIndex]];
					}
				}
			}
		}
		public void DrawWindowSprite(Sprite sprite,float offsetX,float offsetY,float scale,bool inPlace,bool ghost,bool previewAnimation){
			bool selectable = !inPlace && !ghost;
			string tooltip = sprite.animated ? sprite.name + "\n" + sprite.sequence + " (1/" + sprite.frames.Count + ")" : sprite.fullName;
			Frame current = previewAnimation ? sprite.current : sprite.frames[0];
			Rect bounds = inPlace ? current.fullBounds : current.bounds;
			GUIStyle buttonStyle = ghost || inPlace ? GUIStyle.none : this.assets.UI.GetStyle("Sprite Background");
			if(!inPlace && this.selected == sprite){buttonStyle = this.assets.UI.GetStyle("Sprite Background Selected");}
			GUIContent spriteTooltip = new GUIContent("",tooltip + "\nSize [" + bounds.height + "x" + bounds.height + "]");
			Vector4 uvPadding = inPlace ? current.uvPadding : new Vector4(0,0,1,1);
			float areaX = bounds.x*scale+offsetX;
			float areaY = bounds.y*scale+offsetY;
			float areaWidth = bounds.width*scale;
			float areaHeight = bounds.height*scale;
			spriteTooltip.tooltip += "\nArea [" + (int)bounds.x + "x" + (int)bounds.y + "]";
			Rect area = new Rect(areaX,areaY,areaWidth,areaHeight);
			area.x = inPlace ? offsetX : areaX;
			area.y = inPlace ? offsetY : areaY;
			Vector4 clip = new Vector4(0,0,1,1);
			float limitLeft = area.x-this.scrollPosition.x;
			float limitRight = area.x-this.scrollPosition.x+area.width;
			float limitTop = area.y-this.scrollPosition.y;
			float limitBottom = area.y-this.scrollPosition.y+area.height;
			if(limitLeft > Screen.width || limitRight < 0){return;}
			if(limitTop > Screen.height || limitBottom < 0){return;}
			float overX = limitRight - (Screen.width-16);
			float overY = limitBottom - (Screen.height-36);
			if(limitTop < 0){clip.y = -limitTop / area.height;}
			if(overX > 0){clip.z = 1.0f - (overX / area.width);}
			if(overY > 0){clip.w = 1.0f - (overY / area.height);}
			Material material = this.assets.spritePreview;
			if(material == null){return;}
			this.ApplyMaterial(material,sprite);
			material.SetVector("atlasUV",current.uv);
			material.SetVector("paddingUV",uvPadding);
			material.SetVector("clipUV",clip);
			material.SetFloat("alpha",1.0f);
			material.SetFloat("alphaCutoff",0.01f);
			if(ghost){material.SetFloat("alpha",0.2f);}
			Rect background = new Rect(area.x,area.y,area.width/this.assets.grid.width,area.height/this.assets.grid.height);
			if(GUI.Button(area,spriteTooltip,buttonStyle) && selectable){
				GameObject target = this.placementMode ? this.brush : null;
				this.activeSheet = sprite.parent;
				this.selected = sprite;
				PlayerPrefs.SetString("SpriteWindow-Selected",sprite.fullName);
				if(this.applyOnSelection){
					if(target != this.brush){Undo.RegisterSceneUndo("Apply Sprites");}
					this.ApplySprite(target);
				}
			}
			if(this.selected == sprite && !inPlace){
				GUI.DrawTextureWithTexCoords(area,this.assets.gridBlue,background);
			}
			EditorGUI.DrawPreviewTexture(area,sprite.parent.image,material);
		}
	}
}