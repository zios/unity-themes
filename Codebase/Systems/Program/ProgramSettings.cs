using UnityEngine;
using System;
using System.Collections.Generic;
namespace Zios{
	[ExecuteInEditMode][AddComponentMenu("Zios/Singleton/Program")]
	public class ProgramSettings : MonoBehaviour{
		public int targetFPS = -1;
		public bool[] pixelSnap = new bool[3]{false,false,false};
		public void Awake(){
			Program.settings = this;
			Program.Awake();
		}
		public void Start(){
			Program.Start();
		}
		public void Update(){
			Program.Update();
		}
	}
	public static class Program{
		public static ProgramSettings settings;
		public static int[] resolution;
		private static bool allowResolution = true;
		public static void Awake(){
			Persistent[] instances = (Persistent[])Resources.FindObjectsOfTypeAll(typeof(Persistent));
			foreach(Persistent script in instances){
				if(script.activateOnLoad){
					script.gameObject.SetActive(true);
				}
			}
		}
		public static void Start(){
			if(!Application.isPlaying){
				return;
			}
			//object unitySettings = typeof(QualitySettings);
			Application.targetFrameRate = Program.settings.targetFPS;
			Resolution screen = Screen.currentResolution;
			Program.resolution = new int[3]{Screen.width,Screen.height,screen.refreshRate};
			Program.DetectResolution();
		}
		public static void Update(){
			Program.DetectResolution();
		}
		public static void DetectResolution(){
			Resolution screen = Screen.currentResolution;
			int[] size = Program.resolution;
			bool changedWidth = Screen.width != size[0];
			bool changedHeight = Screen.height != size[1];
			bool changedRefresh = screen.refreshRate != size[2];
			if(changedWidth || changedHeight || changedRefresh){
				if(!Program.allowResolution){
					Program.allowResolution = true;
					Debug.Log("^7Screen settings auto-adjusted to closest allowed values.");
					if(changedWidth){Debug.Log("@screenWidth "+Screen.width);}
					if(changedHeight){Debug.Log("@screenHeight "+Screen.height);}
					if(changedRefresh){Debug.Log("@screenRefreshRate "+screen.refreshRate);}
				}
				else{
					Screen.SetResolution(size[0],size[1],Screen.fullScreen,size[2]);
					Program.allowResolution = false;
				}
			}
			else if(!Program.allowResolution){
				Events.Call("OnResolutionChange");
				Program.allowResolution = true;
				string log = "^10Program resolution is : ^8| " + size[0] + "^7x^8|" + size[1];
				Debug.Log(log);
			}
		}
		public static void ChangeResolution(string[] values){
			if(values.Length < 3){
				Program.allowResolution = false;
				return;
			}
			Program.resolution[0] = Convert.ToInt32(values[1]);
			Program.resolution[1] = Convert.ToInt32(values[2]);
		}
		public static void SnapPixels(string[] values){
			if(values.Length < 2){
				Debug.Log("@pixelSnap*");
				return;
			}
			bool[] states = new List<bool>(Program.settings.pixelSnap).ToArray();
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
			Program.settings.pixelSnap = states;
		}
		public static void CloseProgram(){
			Application.Quit();
		}
	}
}