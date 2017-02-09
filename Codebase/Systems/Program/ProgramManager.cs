using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Zios{
	using Interface;
	using Utilities;
	using Events;
	[AddComponentMenu("Zios/Singleton/Program")][ExecuteInEditMode]
	public class ProgramManager : MonoBehaviour{
		public static ProgramManager instance;
		public static ProgramManager Get(){return ProgramManager.instance;}
		public bool[] pixelSnap = new bool[3]{false,false,false};
		public int targetFPS = -1;
		private int[] resolution = new int[3]{640,480,60};
		private bool allowResolution = true;
		public void OnEnable(){this.Setup();}
		public void Awake(){this.Setup();}
		public void Start(){
			Console.AddShortcut("changeResolution","res","resolution");
			Console.AddShortcut("closeProgram","quit");
			Console.AddShortcut("verticalSync","vsync");
			Console.AddKeyword("hide",this.DisableGameObject,1);
			Console.AddKeyword("show",this.EnableGameObject,1);
			Console.AddKeyword("form",this.ToggleGameObject,1);
			Console.AddKeyword("instance",this.InstanceGameObject,1);
			Console.AddKeyword("destroy",this.DestroyGameObject,1);
			Console.AddKeyword("closeProgram",this.CloseProgram);
			Console.AddCvarMethod("changeResolution",this,"resolution","Change Resolution","",this.ChangeResolution);
			Console.AddCvar("maxfps",typeof(Application),"targetFrameRate","Maximum FPS");
			Console.AddCvar("verticalSync",typeof(QualitySettings),"vSyncCount","Vertical Sync");
		}
		public void Setup(){
			ProgramManager.instance = this;
			Event.Register("On Resolution Change");
			Event.Add("On Editor Update",this.EditorUpdate);
			Event.Add("On Enter Play",this.UpdateEffects);
			Application.targetFrameRate = this.targetFPS;
			Resolution screen = Screen.currentResolution;
			this.resolution = new int[3]{Screen.width,Screen.height,screen.refreshRate};
			Locate.GetSceneComponents<Persistent>().Where(x=>x.activateOnLoad).ToList().ForEach(x=>x.gameObject.SetActive(true));
			this.DetectResolution();
		}
		public void Update(){
			this.DetectResolution();
		}
		public void OnValidate(){
			if(!this.CanValidate()){return;}
			this.Setup();
		}
		public void EditorUpdate(){
			#if UNITY_EDITOR
			if(this.CanValidate()){
				this.UpdateEffects();
			}
			#endif
		}
		public void UpdateEffects(){
			#if UNITY_EDITOR
			bool updateShaders = Utility.GetPref<bool>("EditorSettings-AlwaysUpdateShaders");
			bool updateParticles = Utility.GetPref<bool>("EditorSettings-AlwaysUpdateParticles");
			if(updateShaders){Shader.SetGlobalFloat("timeConstant",Time.realtimeSinceStartup);}
			foreach(var system in Locate.GetSceneComponents<ParticleSystem>()){
				if(system.IsNull()){continue;}
				var updater = system.gameObject.GetComponent<UpdateParticle>();
				if(updateParticles && updater.IsNull()){
					updater = system.gameObject.AddComponent<UpdateParticle>();
					updater.hideFlags = HideFlags.DontSaveInBuild | HideFlags.NotEditable | HideFlags.HideInInspector;
				}
				if(!updateParticles && !updater.IsNull()){
					DestroyImmediate(updater);
				}
			}
			if(updateShaders || updateParticles){Utility.RepaintSceneView();}
			#endif
		}
		//================================
		// Internal
		//================================
		public void InstanceGameObject(string[] values){
			var target = Locate.GetAssets<GameObject>().Where(x=>x.name==values[1]).FirstOrDefault();
			if(!target.IsNull()){
				var instance = GameObject.Instantiate<GameObject>(target);
				instance.SetActive(true);
			}
		}
		public void DestroyGameObject(string[] values){Locate.GetSceneObjects().Where(x=>x.name==values[1]).ToList().ForEach(x=>Destroy(x));}
		public void DisableGameObject(string[] values){Locate.GetSceneObjects().Where(x=>x.name==values[1]).ToList().ForEach(x=>x.SetActive(false));}
		public void EnableGameObject(string[] values){Locate.GetSceneObjects().Where(x=>x.name==values[1]).ToList().ForEach(x=>x.SetActive(true));}
		public void ToggleGameObject(string[] values){Locate.GetSceneObjects().Where(x=>x.name==values[1]).ToList().ForEach(x=>x.SetActive(!x.activeInHierarchy));}
		public void DetectResolution(){
			if(!Application.isPlaying){return;}
			Resolution screen = Screen.currentResolution;
			int[] size = this.resolution;
			bool changedWidth = Screen.width != size[0];
			bool changedHeight = Screen.height != size[1];
			bool changedRefresh = screen.refreshRate != size[2];
			if(changedWidth || changedHeight || changedRefresh){
				Event.Call("On Resolution Change");
				if(!this.allowResolution){
					this.allowResolution = true;
					if(Application.isPlaying){Debug.Log("^7Screen settings auto-adjusted to closest allowed values.");}
					if(changedWidth){this.resolution[0] = Screen.width;}
					if(changedHeight){this.resolution[1] = Screen.height;}
					if(changedRefresh){this.resolution[2] = screen.refreshRate;}
				}
				else{
					Screen.SetResolution(size[0],size[1],Screen.fullScreen,size[2]);
					this.allowResolution = false;
				}
			}
			else if(!this.allowResolution){
				this.allowResolution = true;
				//string log = "^10Program resolution is : ^8| " + size[0] + "^7x^8|" + size[1];
				//if(Application.isPlaying){Debug.Log(log);}
			}
		}
		public void ChangeResolution(string[] values){
			if(values.Length < 3){
				this.allowResolution = false;
				return;
			}
			this.resolution[0] = Convert.ToInt32(values[1]);
			this.resolution[1] = Convert.ToInt32(values[2]);
		}
		public void SnapPixels(string[] values){
			if(values.Length < 2){
				Debug.Log("@pixelSnap*");
				return;
			}
			bool[] states = new List<bool>(this.pixelSnap).ToArray();
			if(values.Length == 2){
				values = new string[]{"",values[1],values[1],values[1]};
			}
			for(int index=1;index<values.Length;++index){
				if(index > 4){break;}
				string value = values[index].ToLower();
				states[index-1] = value == "true" || value == "1" ? true : false;
			}
			Debug.Log("@pixelSnapX "+states[0]);
			Debug.Log("@pixelSnapY "+states[1]);
			Debug.Log("@pixelSnapZ "+states[2]);
			this.pixelSnap = states;
		}
		public void CloseProgram(){
			Application.Quit();
		}
	}
}