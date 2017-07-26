using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEvent = UnityEngine.Event;
namespace Zios.Inputs{
	using Interface;
	using Event;
	public enum InputUIState{None,SelectProfile,EditProfile}
	public enum InputInstanceOptions{AllowCurrentlyUsedProfiles=1,AllowMultipleProfiles=2,ReassignInvalidProfiles=4}
	public class InputManager : Singleton{
		public bool disabled;
		public static InputManager singleton;
		public static Vector2 mouseChange;
		public static Vector2 mouseScroll;
		public static Vector2 mousePosition;
		public static Vector2 mouseChangeAverage;
		public static float lastMouseChange;
		public static float registerTime = 1;
		public GameObject uiPrefab;
		[EnumMask] public InputInstanceOptions instanceOptions = (InputInstanceOptions)(2);
		public float gamepadDeadZone = 0.1f;
		public float gamepadSensitivity = 1;
		public float mouseSensitivity = 1;
		public List<InputGroup> groups = new List<InputGroup>();
		[Internal] public Dictionary<string,InputProfile> instanceProfile = new Dictionary<string,InputProfile>();
		[Internal] public List<InputDevice> devices = new List<InputDevice>();
		[Internal] public List<InputProfile> profiles = new List<InputProfile>();
		[Internal] public string[] joystickNames = new string[0];
		private GameObject uiObject;
		private InputUIState uiState;
		private Dictionary<string,bool> joystickAxis = new Dictionary<string,bool>();
		private bool waitForRelease;
		private string selectionHeader;
		private InputProfile activeProfile;
		private Dictionary<string,float> lastInput = new Dictionary<string,float>();
		private int uiGroupIndex;
		private int uiIndex;
		public static InputManager Get(){
			InputManager.singleton = InputManager.singleton ?? Utility.GetSingleton<InputManager>();
			return InputManager.singleton;
		}
		//===============
		// Unity
		//===============
		public void Refresh(){
			if(Application.isEditor){
				InputGroup.Save();
				InputGroup.Load();
			}
		}
		public void Setup(){
			InputManager.singleton = this;
			this.uiState = 0;
			this.activeProfile = null;
			this.devices.Clear();
			this.profiles.Clear();
			this.joystickNames = this.joystickNames.Clear();
		}
		public void OnEnable(){
			this.Setup();
			InputGroup.Load();
			if(!Utility.IsPlaying()){return;}
			this.Validate();
			if(this.disabled){return;}
			InputProfile.Load();
			InputInstance.Load();
			Console.AddKeyword("inputShowProfiles",this.ShowProfiles);
			Console.AddKeyword("inputAssignProfile",this.AssignProfile);
			Console.AddKeyword("inputCreateProfile",this.CreateProfile);
			Console.AddKeyword("inputEditProfile",this.EditProfile);
			Console.AddKeyword("inputRemoveProfile",this.RemoveProfile);
			Events.Register("On Profile Selected",this);
			Events.Register("On Profile Edited",this);
			Events.Add("On Update",this.Update);
			Events.Add("On Fixed Update",this.FixedUpdate);
			Events.Add("On GUI",this.OnGUI);
			this.DetectGamepads();
		}
		public bool Validate(){
			try{Input.GetAxis("Joystick1-Axis1");}
			catch{
				Debug.LogWarning("[InputManager] Unity input not setup. Please copy provided InputManager.asset to Assets/ProjectSettings");
				this.disabled = true;
				return false;
			}
			this.disabled = false;
			return true;
		}
		public void Update(){
			if(this.disabled){return;}
			this.DetectMouse();
			this.DetectKey();
		}
		public void FixedUpdate(){
			if(this.disabled){return;}
			this.DetectGamepads();
		}
		public void OnGUI(){
			if(!Application.isPlaying || this.disabled){return;}
			var current = UnityEvent.current;
			if(current.isKey || current.shift || current.alt || current.control || current.command){
				if(!this.devices.Exists(x=>x.name=="Keyboard")){
					this.devices.Add(new InputDevice("Keyboard"));
				}
			}
			bool uiActive = this.uiState != InputUIState.None;
			if(this.uiObject.IsNull()){
				this.uiObject = Locate.Find("@Main/InputUI");
				if(this.uiObject.IsNull()){
					this.uiObject = GameObject.Instantiate(this.uiPrefab);
					this.uiObject.name = this.uiObject.name.Remove("(Clone)");
					this.uiObject.transform.SetParent(Locate.GetScenePath("@Main").transform);
					Locate.SetDirty();
				}
			}
			this.uiObject.SetActive(uiActive);
			Locate.Find("@Main/InputUI/ProfileCreate/").SetActive(false);
			Locate.Find("@Main/InputUI/ProfileSelect/").SetActive(false);
			if(uiActive){
				Console.Close(true);
				InputState.disabled = true;
				this.DrawProfileSelect();
				this.DrawProfileEdit();
				bool hitEscape = UnityEvent.current.keyCode == KeyCode.Escape;
				if(UnityEvent.current.type == EventType.KeyDown && hitEscape){
					this.uiState = InputUIState.None;
					InputState.disabled = false;
				}
			}
		}
		//===============
		// GUI
		//===============
		[ContextMenu("Prepare Settings")] public void Prepare(){InputGroup.Setup();}
		[ContextMenu("Save Settings")] public void Save(){InputGroup.Save();}
		[ContextMenu("Load Settings")] public void Load(){InputGroup.Load();}
		public void DrawProfileSelect(){
			if(this.uiState == InputUIState.SelectProfile){
				//var path = "@Main/InputUI/ProfileSelect/";
				//Locate.Find(path).SetActive(true);
				var buttonWidth = Screen.width * 0.5f;
				var buttonHeight = Screen.height * 0.09f;
				var area = new Rect((Screen.width/2)-buttonWidth/2,10,buttonWidth,buttonHeight);
				var style = GUI.skin.button.Font("Bombardier.otf").FontSize((int)(buttonHeight*0.7f));
				GUI.Label(area,this.selectionHeader,style.Background(""));
				area = area.AddY(buttonHeight+8);
				foreach(var profile in this.profiles.Copy()){
					bool usable = this.devices.Select(x=>x.name).ContainsAll(profile.requiredDevices);
					if(!usable){GUI.enabled = false;}
					if(GUI.Button(area,profile.name,style)){
						this.activeProfile = profile;
						this.uiState = InputUIState.None;
						InputState.disabled = false;
						this.DelayEvent("On Profile Selected",0);
					}
					GUI.enabled = true;
					area = area.AddY(buttonHeight+5);
				}
			}
		}
		public void DrawProfileEdit(){
			if(this.uiState == InputUIState.EditProfile){
				var profile = this.activeProfile;
				var group = this.groups[this.uiGroupIndex];
				var action = group.actions[this.uiIndex];
				var path = "@Main/InputUI/ProfileCreate/";
				Locate.Find(path).SetActive(true);
				Locate.Find(path+"Text-Key").GetComponent<Text>().text = action.name;
				Locate.Find(path+"Text-Profile").GetComponent<Text>().text = "<size=100><color=#888888FF>"+profile.name+"</color></size>\nProfile";
				Locate.Find(path+"Icon-Gamepad").SetActive(!action.helpImage.IsNull());
				Locate.Find(path+"Icon-Gamepad/"+action.helpImage).SetActive(!action.helpImage.IsNull());
				if(this.waitForRelease){
					foreach(var key in this.lastInput.Keys.ToList()){this.lastInput[key] = 0;}
					this.waitForRelease = this.waitForRelease && this.lastInput.Count != 0;
				}
				var progress = Locate.Find(path+"Image-Timer");
				var highest = this.lastInput.OrderBy(x=>x.Value).FirstOrDefault();
				var timeHeld = highest.Key.IsEmpty() ? Time.realtimeSinceStartup + InputManager.registerTime : highest.Value;
				var targetInput = this.lastInput.Where(x=>Time.realtimeSinceStartup>x.Value).FirstOrDefault();
				progress.SetActive(highest.Value > 0);
				progress.GetComponent<Image>().fillAmount = InputManager.registerTime-(timeHeld-Time.realtimeSinceStartup);
				if(!this.waitForRelease && !targetInput.Key.IsEmpty()){
					this.waitForRelease = true;
					Locate.Find(path+"Icon-Gamepad/"+action.helpImage).SetActive(false);
					var inputName = targetInput.Key;
					string device = "Keyboard";
					string groupName = group.name.ToPascalCase();
					string actionName = action.name.ToPascalCase();
					if(inputName.Contains("Joystick")){
						int id = (int)Char.GetNumericValue(inputName.Remove("Joystick")[0]);
						device = this.joystickNames[id-1];
						inputName = inputName.ReplaceFirst(id.ToString(),"*");
					}
					else if(inputName.Contains("Mouse")){device = "Mouse";}
					var existsText = Locate.Find(path+"Text-Exists");
					var match = profile.mappings.collection.Where(x=>x.Key.Contains(groupName+"-",true) && x.Value.Matches(inputName,true)).FirstOrDefault();
					existsText.SetActive(!match.Key.IsEmpty());
					if(!match.Key.IsEmpty()){
						existsText.GetComponent<Text>().text = inputName.Remove("*") + " already mapped to : <color=#FF9999FF>" + match.Key.Split("-")[1] + "</color>";
						return;
					}
					profile.requiredDevices.AddNew(device);
					profile.mappings[groupName+"-"+actionName] = inputName;
					this.uiIndex += 1;
					if(this.uiIndex >= group.actions.Count){
						this.uiGroupIndex += 1;
						if(this.uiGroupIndex >= this.groups.Count){
							profile.Save();
							this.activeProfile = null;
							this.uiState = InputUIState.None;
							InputState.disabled = false;
							this.DelayEvent("On Profile Edited",0);
						}
					}
				}
			}
		}
		//===============
		// Detection
		//===============
		public void DetectKey(){
			if(this.uiState == InputUIState.EditProfile){
				for(int joystickNumber=1;joystickNumber<5;++joystickNumber){
					for(int axisNumber=1;axisNumber<9;++axisNumber){
						string axisName = "Joystick"+joystickNumber+"-Axis"+axisNumber;
						float value = Input.GetAxisRaw(axisName);
						if(Mathf.Abs(value) > this.gamepadDeadZone){
							axisName += value < 0 ? "Negative" : "Positive";
							this.joystickAxis[axisName] = true;
							if(!this.lastInput.ContainsKey(axisName)){
								this.lastInput[axisName] = Time.realtimeSinceStartup + InputManager.registerTime;
							}
							continue;
						}
						this.lastInput.Remove(axisName+"Positive");
						this.lastInput.Remove(axisName+"Negative");
						this.joystickAxis[axisName+"Positive"] = false;
						this.joystickAxis[axisName+"Negative"] = false;
					}
				}
				if(this.joystickAxis.ContainsValue(true)){return;}
				if(Input.anyKey){
					foreach(var keyCode in Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>()){
						var keyName = keyCode.ToName();
						if(keyName.Contains("JoystickButton")){continue;}
						if(Input.GetKey(keyCode)){
							if(!this.lastInput.ContainsKey(keyName)){
								this.lastInput[keyName] = Time.realtimeSinceStartup + InputManager.registerTime;
							}
							continue;
						}
						this.lastInput.Remove(keyName);
					}
				}
				else{
					this.lastInput = this.lastInput.Where(x=>x.Key.ContainsAny("MouseX","MouseY","MouseScroll")).ToDictionary();
				}
			}
		}
		public void DetectGamepads(){
			var names = Input.GetJoystickNames();
			if(!Enumerable.SequenceEqual(names,this.joystickNames)){
				foreach(var change in names.Except(this.joystickNames)){
					if(change.IsEmpty()){continue;}
					int id = names.IndexOf(change);
					Debug.Log("[InputManager] Joystick #" + id + " plugged in -- " + change);
					this.devices.Add(new InputDevice(change,id));
				}
				foreach(var change in this.joystickNames.Except(names)){
					if(change.IsEmpty()){continue;}
					int id = this.joystickNames.IndexOf(change);
					Debug.Log("[InputManager] Joystick #" +id + " unplugged -- " + change);
					this.devices.RemoveAll(x=>x.name==change&&x.id==id);
				}
				this.joystickNames = names;
			}
		}
		public void DetectMouse(){
			InputManager.mouseScroll = Input.mouseScrollDelta != Vector2.zero ? -Input.mouseScrollDelta : Vector2.zero;
			InputManager.mouseChange =  new Vector2(Input.GetAxis("MouseX"),-Input.GetAxis("MouseY")) * this.mouseSensitivity;
			InputManager.mousePosition = Input.mousePosition;
			var editing = this.uiState == InputUIState.EditProfile;
			var mouseChanges = InputManager.mouseChange != Vector2.zero || InputManager.mouseScroll != Vector2.zero;
			if(editing || mouseChanges){
				InputManager.mouseChangeAverage = (InputManager.mouseChangeAverage+InputManager.mouseChange)/2;
				if(mouseChanges){
					InputManager.lastMouseChange = Time.realtimeSinceStartup;
					if(!this.devices.Exists(x=>x.name=="Mouse")){
						this.devices.Add(new InputDevice("Mouse"));
					}
				}
				if(editing){
					var inputName = "";
					var group = this.groups[this.uiGroupIndex];
					var action = group.actions[this.uiIndex];
					var average = InputManager.mouseChangeAverage;
					var change = new Vector2(average.x.Abs(),average.y.Abs());
					if(InputManager.mouseScroll.y < 0){inputName = "MouseScrollUp";}
					else if(InputManager.mouseScroll.y > 0){inputName = "MouseScrollDown";}
					else if(action.options.Has("AllowMouseMove")){
						if(change.x >= change.y){
							if(average.x < 0){inputName = "MouseX-";}
							if(average.x > 0){inputName = "MouseX+";}
						}
						else{
							if(average.y < 0){inputName = "MouseY-";}
							if(average.y > 0){inputName = "MouseY+";}
						}
					}
					if(!inputName.IsEmpty() && !this.lastInput.ContainsKey(inputName)){
						var time = inputName.Contains("Scroll") ? 0 : Time.realtimeSinceStartup + InputManager.registerTime;
						this.lastInput.Clear();
						this.lastInput[inputName] = time;
					}
				}
			}
			if((Time.realtimeSinceStartup-InputManager.lastMouseChange) > 0.1f){
				InputManager.mouseChangeAverage = Vector2.zero;
				this.lastInput = this.lastInput.Where(x=>!x.Key.ContainsAny("MouseX","MouseY","MouseScroll")).ToDictionary();
			}
		}
		//===============
		// Interface
		//===============
		public InputProfile GetInstanceProfile(InputInstance instance){
			string name = instance.alias.ToPascalCase();
			if(this.instanceProfile.ContainsKey(name)){
				return this.instanceProfile[name];
			}
			return null;
		}
		public void SelectProfile(InputInstance instance){
			if(instance.IsNull()){return;}
			if(this.profiles.Count < 1){
				this.CreateProfile("Default");
				Events.AddLimited("On Profile Edited",()=>this.SelectProfile(instance),1,this);
				return;
			}
			if(this.uiState == InputUIState.SelectProfile){
				Events.AddLimited("On Profile Selected",()=>this.SelectProfile(instance),1,this);
				return;
			}
			this.ShowProfiles();
			this.selectionHeader = instance.alias;
			Method selected = ()=>{
				this.instanceProfile[instance.name] = this.activeProfile;
				instance.joystickID = "";
				instance.profile = this.activeProfile;
				instance.Save();
			};
			Events.AddLimited("On Profile Selected",selected,1,this);
		}
		public void ShowProfiles(){
			this.activeProfile = null;
			this.selectionHeader = "";
			this.profiles.RemoveAll(x=>!FileManager.Exists(x.name+".profile"));
			this.uiState = InputUIState.SelectProfile;
		}
		public void RemoveProfile(string name){
			if(FileManager.Find(name+".profile",false).IsNull()){
				this.ShowProfiles();
				this.selectionHeader = "Remove Profile";
				Events.AddLimited("On Profile Selected",()=>this.RemoveProfile(this.activeProfile.name),1,this);
				return;
			}
			this.profiles.RemoveAll(x=>x.name==name);
			FileManager.Delete(name+".profile");
			foreach(var instance in Locate.GetSceneComponents<InputInstance>()){
				if(!instance.profile.IsNull() && instance.profile.name == name){
					this.instanceProfile.Remove(instance.alias);
					InputManager.Get().SelectProfile(instance);
				}
			}
		}
		public void EditProfile(string name){
			this.lastInput.Clear();
			this.uiState = InputUIState.EditProfile;
			this.uiGroupIndex = 0;
			this.uiIndex = 0;
			this.activeProfile = this.profiles.Find(x=>x.name==name);
			if(this.activeProfile.IsNull()){
				this.ShowProfiles();
				this.selectionHeader = "Edit Profile";
				Events.AddLimited("On Profile Selected",()=>this.EditProfile(this.activeProfile.name),1,this);
			}
		}
		public void CreateProfile(string name){
			int index = 1;
			while(this.profiles.Exists(x=>x.name==name)){
				name = "Profile"+index;
				index += 1;
			}
			this.profiles.AddNew(new InputProfile(name));
			this.EditProfile(name);
		}
		//===============
		// Console
		//===============
		public void CreateProfile(string[] values){
			var profileName = values.Length > 1 ? values[1] : "Default";
			this.CreateProfile(profileName);
		}
		public void EditProfile(string[] values){
			var profileName = values.Length > 1 ? values[1] : "#%^@&$";
			this.EditProfile(profileName);
		}
		public void AssignProfile(string[] values){
			if(values.Length < 2){return;}
			var inputInstances = Locate.GetSceneComponents<InputInstance>().ToList();
			var instance = inputInstances.Find(x=>x.alias==values[1]);
			this.SelectProfile(instance);
		}
		public void RemoveProfile(string[] values){
			var profileName = values.Length > 1 ? values[1] : "#%^@&$";
			this.RemoveProfile(profileName);
		}
	}
}