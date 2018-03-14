using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Zios.Unity.ProgramManager{
	using Zios.Console;
	using Zios.Events;
	using Zios.Extensions;
	using Zios.Unity.Components.ParticleUpdater;
	using Zios.Unity.Components.Persistent;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.Locate;
	using Zios.Unity.Log;
	using Zios.Unity.Pref;
	using Zios.Unity.Proxy;
	using Zios.Unity.Supports.Singleton;
	using Zios.Unity.Time;
	//asm Zios.Shortcuts;
	//asm Zios.Unity.Shortcuts;
	public class ProgramManager : Singleton{
		public static ProgramManager singleton;
		public bool[] pixelSnap = new bool[3]{false,false,false};
		public int maxFPS = -1;
		public int vsync;
		public int refreshRate = 60;
		public int[] resolution = new int[2]{1920,1080};
		private bool allowResolution = true;
		public static ProgramManager Get(){
			ProgramManager.singleton = ProgramManager.singleton ?? Singleton.Get<ProgramManager>();
			return ProgramManager.singleton;
		}
		public void Setup(){
			ProgramManager.singleton = this;
			this.vsync = QualitySettings.vSyncCount;
			Events.Register("On Resolution Change");
			Events.Add("On Editor Update",this.UpdateEffects);
			Events.Add("On Enter Play",this.UpdateEffects);
			Application.targetFrameRate = this.maxFPS;
			Resolution screen = Screen.currentResolution;
			this.resolution = new int[3]{Screen.width,Screen.height,screen.refreshRate};
			Locate.GetSceneComponents<Persistent>().Where(x=>x.activateOnLoad).ToList().ForEach(x=>x.gameObject.SetActive(true));
			this.DetectResolution();
		}
		public void Awake(){
			this.Setup();
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
		public void OnEnable(){
			Events.Add("On Awake",this.Awake);
		}
		public void Update(){
			this.DetectResolution();
		}
		public void OnValidate(){
			this.Setup();
		}
		public void UpdateEffects(){
			#if UNITY_EDITOR
			bool updateShaders = PlayerPref.Get<bool>("EditorSettings-AlwaysUpdateShaders");
			bool updateParticles = PlayerPref.Get<bool>("EditorSettings-AlwaysUpdateParticles");
			if(updateShaders){Shader.SetGlobalFloat("timeConstant",Time.Get());}
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
			if(updateShaders || updateParticles){ProxyEditor.RepaintSceneView();}
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
			if(!Proxy.IsPlaying()){return;}
			Resolution screen = Screen.currentResolution;
			int[] size = this.resolution;
			bool changedWidth = Screen.width != size[0];
			bool changedHeight = Screen.height != size[1];
			bool changedRefresh = screen.refreshRate != this.refreshRate;
			if(changedWidth || changedHeight || changedRefresh){
				Events.Call("On Resolution Change");
				if(!this.allowResolution){
					this.allowResolution = true;
					if(Proxy.IsPlaying()){Log.Show("^7Screen settings auto-adjusted to closest allowed values.");}
					if(changedWidth){this.resolution[0] = Screen.width;}
					if(changedHeight){this.resolution[1] = Screen.height;}
					if(changedRefresh){this.refreshRate = screen.refreshRate;}
				}
				else{
					Screen.SetResolution(size[0],size[1],Screen.fullScreen,this.refreshRate);
					this.allowResolution = false;
				}
			}
			else if(!this.allowResolution){
				this.allowResolution = true;
				string log = "^10Program resolution is : ^8| " + size[0] + "^7x^8|" + size[1];
				if(Proxy.IsPlaying()){Log.Show(log);}
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
				Log.Show("@pixelSnap*");
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
			Log.Show("@pixelSnapX "+states[0]);
			Log.Show("@pixelSnapY "+states[1]);
			Log.Show("@pixelSnapZ "+states[2]);
			this.pixelSnap = states;
		}
		public void CloseProgram(){
			Application.Quit();
		}
	}
}