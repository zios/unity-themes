using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
[ExecuteInEditMode]
[AddComponentMenu("Zios/Singleton/Scene")]
public class SceneManager : MonoBehaviour{
	public List<string> scenes;
	public bool[] pixelSnap = new bool[3]{false,false,false};
	public Transform sectionContainer;
	public float shaderAlphaCutoff = 0.3f;
	[NonSerialized] public int[] programResolution;
	[NonSerialized] public Matrix4x4 guiMatrix;
	private string currentMap = "";
	private bool allowResolution = true;
	private Dictionary<string,Transform> sections = new Dictionary<string,Transform>();
	private string[] help = new string[]{
		"^3map ^9<^7name^9> :^10 Removes the current scene and loads a specific new scene.",
		"^3quit :^10 Closes program when not in editor or browser.",
		"^3maxfps ^9<^7number^9> :^10 Maximum framerate allowed by the application.",
		"^3pixelSnap ^9<^7true/false^9> ^9<^7true/false^9> ^9<^7true/false^9> :^10 Clamps position values of all entities to the closest integer.",
		"^3pixelSnapX ^9<^7true/false^9> :^10 Clamps the x position of all entities to the closest integer.",
		"^3pixelSnapY ^9<^7true/false^9> :^10 Clamps the y position of all entities to the closest integer.",
		"^3pixelSnapZ ^9<^7true/false^9> :^10 Clamps the z position of all entities to the closest integer.",
		"^3antiAliasing ^9<^7number^9> :^10 Performs a multi-sample operation on an area of pixels.",
		"^3vSync ^9<^7true/false^9> :^10 Synchronize rendering to monitor refresh rate.",
		"^3screen ^9<^7number^9> ^9<^7number9> :^10 Desired program window resolution as specificed by width and height.",
		"^3screenWidth ^9<^7number^9> :^10 Desired program window width.",
		"^3screenHeight ^9<^7number^9> :^10 Desired program window height.",
		"^3screenRefreshRate ^9<^7number^9> :^10 Desired program window height.",
		"^3screenFullScreen ^9<^7true/false^9> :^10 Places program in full-screen rendering mode rather than window.",
		"^3shaderAlphaCutoff ^9<^7number^9> ^9<^7number^9> :^10 Controls the global alpha cutoff value used by shaders.",
	};
	public void Awake(){
		Global.Scene = this;
		DontDestroyOnLoad(this.gameObject);
		this.AwakenObjects();
		this.CalculateGUIMatrix();
	}
	public void AwakenObjects(){
		Persistent[] instances = (Persistent[])Resources.FindObjectsOfTypeAll(typeof(Persistent));
		foreach(Persistent script in instances){
			if(script.activateOnLoad){
				script.gameObject.SetActive(true);
			}
		}
	}
	public void Start(){
		if(!Application.isPlaying){
			this.AdjustAlphaCutoff();
			return;
		}
		object settings = typeof(QualitySettings);
		//Application.targetFrameRate = 60;
		Resolution screen = Screen.currentResolution;
		this.programResolution = new int[3]{Screen.width,Screen.height,screen.refreshRate};
		Global.Console.AddShortcut("load","map");
		Global.Console.AddShortcut("scene","map");
		Global.Console.AddShortcut("AA","antiAliasing");
		Global.Console.AddShortcut("vsync","verticalSync");
		Global.Console.AddShortcut("res","resolution");
		Global.Console.AddShortcut("resolution","screen");
		Global.Console.AddShortcut("refreshRate","screenRefreshRate");
		Global.Console.AddShortcut("screenFullScreen","fullScreen");
		Global.Console.AddShortcut("maximize","fullScreen true");
		Global.Console.AddCvar("maxfps",typeof(Application),"targetFrameRate","Maximum FPS",this.help[2]);
		Global.Console.AddCvar("pixelSnapX",this,"pixelSnap[0]","Pixel Snap X",this.help[4]);
		Global.Console.AddCvar("pixelSnapY",this,"pixelSnap[1]","Pixel Snap Y",this.help[5]);
		Global.Console.AddCvar("pixelSnapZ",this,"pixelSnap[2]","Pixel Snap Z",this.help[6]);
		Global.Console.AddCvar("antiAliasing",settings,"antiAliasing","Anti-Aliasing",this.help[7]);
		Global.Console.AddCvar("verticalSync",settings,"vSyncCount","Vertical Sync",this.help[8]);
		Global.Console.AddCvar("screenWidth",this,"programResolution[0]","Screen Width",this.help[10]);
		Global.Console.AddCvar("screenHeight",this,"programResolution[1]","Screen Height",this.help[11]);
		Global.Console.AddCvar("screenRefreshRate",this,"programResolution[2]","Screen Refresh Rate",this.help[12]);
		Global.Console.AddCvar("fullScreen",typeof(Screen),"fullScreen","Full Screen",this.help[13]);
		Global.Console.AddCvarMethod("shaderAlphaCutoff",this,"shaderAlphaCutoff","Shader Global - Alpha Cutoff",this.help[14],this.AdjustAlphaCutoff);
		Global.Console.AddKeyword("screen",this.ChangeResolution);
		Global.Console.AddKeyword("pixelSnap",this.SnapPixels);
		Global.Console.AddKeyword("map",this.LoadMap,1,this.help[0]);
		Global.Console.AddKeyword("quit",this.CloseProgram,0,this.help[1]);
		this.DetectResolution();
		this.DetectSections();
	}
	public void DetectSections(){
		if(this.sectionContainer != null){
			Transform[] sections = this.sectionContainer.GetComponentsInChildren<Transform>();
			foreach(Transform section in sections){
				if(section == this.sectionContainer){continue;}
				this.sections[section.name] = section;
			}
		}
	}
	public void Update(){
		if(Application.isPlaying){
			this.DetectResolution();
		}
		else{Global.Scene = this;}
	}
	public void CalculateGUIMatrix(){
		float xScale = Screen.width / 1280.0f;
		float yScale = Screen.height / 720.0f;
		this.guiMatrix = Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(xScale,yScale,1));
	}
	public void DetectResolution(){
		Resolution screen = Screen.currentResolution;
		int[] size = this.programResolution;
		bool changedWidth = Screen.width != size[0];
		bool changedHeight = Screen.height != size[1];
		bool changedRefresh = screen.refreshRate != size[2];
		if(changedWidth || changedHeight || changedRefresh){
			if(!this.allowResolution){
				this.allowResolution = true;
				Global.Console.AddLog("^7Screen settings auto-adjusted to closest allowed values.");
				if(changedWidth){Global.Console.AddCommand("screenWidth "+Screen.width);}
				if(changedHeight){Global.Console.AddCommand("screenHeight "+Screen.height);}
				if(changedRefresh){Global.Console.AddCommand("screenRefreshRate "+screen.refreshRate);}
			}
			else{
				Screen.SetResolution(size[0],size[1],Screen.fullScreen,size[2]);
				this.allowResolution = false;
			}
			this.CalculateGUIMatrix();
		}
		else if(!this.allowResolution){
			this.allowResolution = true;
			string log = "^10Program resolution is : ^8| " + size[0] + "^7x^8|" + size[1];
			Global.Console.AddLog(log);
		}
	}
	public void ChangeResolution(string[] values,bool help){
		if(help || values.Length < 3){
			if(help){Global.Console.AddLog(this.help[9]);}
			else{this.allowResolution = false;}
			return;
		}
		this.programResolution[0] = Convert.ToInt32(values[1]);
		this.programResolution[1] = Convert.ToInt32(values[2]);
	}
	public void SnapPixels(string[] values,bool help){
		if(help || values.Length < 2){
			if(help){Global.Console.AddLog(this.help[3]);}
			else{Global.Console.AddCommand("pixelSnap*");}
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
		Global.Console.AddCommand("pixelSnapX "+states[0]);
		Global.Console.AddCommand("pixelSnapY "+states[1]);
		Global.Console.AddCommand("pixelSnapZ "+states[2]);
		this.pixelSnap = states;
	}
	public void AdjustAlphaCutoff(){
		Shader.SetGlobalFloat("alphaCutoffGlobal",this.shaderAlphaCutoff);
	}
	public int GetMapID(string name){
		for(int index=0;index<this.scenes.Count;++index){
			if(this.scenes[index] == name){
				return index;
			}
		}
		return -1;
	}
	public void LoadMap(string[] values){
		string mapName = Application.loadedLevelName;
		if(values.Length > 1){
			try{
				Application.LoadLevel(values[1]);
				mapName = values[1];
			}
			catch{
				Global.Console.AddLog("^1Map not found : " + values[1]);
				return;
			}
		}
		this.currentMap = mapName;
		Global.Console.AddLog("^10Current Map is :^3 " + this.currentMap);
	}
	public void CloseProgram(string[] values){
		Application.Quit();
	}
}