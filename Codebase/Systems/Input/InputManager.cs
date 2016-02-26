using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEvent = UnityEngine.Event;
namespace Zios.Inputs{
	using Interface;
	using Events;
	[InitializeOnLoad]
	public static class InputHook{
		public static Hook<InputManager> hook;
		static InputHook(){
			if(Application.isPlaying){return;}
			InputHook.hook = new Hook<InputManager>(null,InputHook.Create);
		}
		public static void Create(){
			bool wasNull = InputManager.instance.IsNull();
			InputHook.hook.Create();
			var instance = InputManager.instance;
			if(wasNull){
				instance.uiObject = Locate.Find("@Main/InputUI");
				if(instance.uiObject.IsNull()){
					instance.uiObject = GameObject.Instantiate(FileManager.GetAsset<GameObject>("InputUI.prefab"));
					instance.uiObject.name = instance.uiObject.name.Remove("(Clone)");
					instance.uiObject.transform.SetParent(Locate.Find("@Main").transform);
				}
				InputGroup.Load();
			}
			Event.Add("On Enter Play",InputGroup.Save).SetPermanent();
			Event.Add("On Validate",instance.Refresh,instance).SetPermanent();
		}
	}
	public enum InputUIState{None,SelectProfile,EditProfile}
	public enum InputInstanceOptions{AllowCurrentlyUsedProfiles=1,AllowMultipleProfiles=2,ReassignInvalidProfiles=4}
	public class InputManager : MonoBehaviour{
		[NonSerialized] public static InputManager instance;
		[NonSerialized] public static Vector2 mouseChange;
		[NonSerialized] public static Vector2 mouseScroll;
		[NonSerialized] public static Vector2 mousePosition;
		[NonSerialized] public static Vector2 mouseChangeAverage;
		[EnumMask] public InputInstanceOptions instanceOptions = (InputInstanceOptions)(2);
		public float gamepadDeadZone = 0.1f;
		public float gamepadSensitivity = 1;
		public float mouseSensitivity = 1;
		public List<InputGroup> groups = new List<InputGroup>();
		[Internal] public Dictionary<string,InputProfile> instanceProfile = new Dictionary<string,InputProfile>();
		[Internal] public List<InputDevice> devices = new List<InputDevice>();
		[Internal] public List<InputProfile> profiles = new List<InputProfile>();
		[Internal] public string[] joystickNames = new string[0];
		[Internal] public GameObject uiObject;
		[NonSerialized] public InputUIState uiState;
		private Dictionary<string,bool> joystickAxis = new Dictionary<string,bool>();
		private string selectionHeader;
		private InputProfile activeProfile;
		private string lastInput;
		private float lastInputTime;
		private int uiGroupIndex;
		private int uiIndex;
		//===============
		// Unity
		//===============
		public void OnValidate(){
			this.DelayEvent("InputManager","On Validate",1);
		}
		public void Refresh(){
			if(Application.isEditor){
				InputManager.instance = this;
				InputGroup.Setup();
				InputGroup.Save();
				InputGroup.Load();
			}
		}
		public void Awake(){
			InputManager.instance = this;
			InputProfile.Load();
			InputInstance.Load();
			InputGroup.Load();
			InputGroup.Setup();
			Console.AddKeyword("showProfiles",this.ShowProfiles);
			Console.AddKeyword("assignProfile",this.AssignProfile);
			Console.AddKeyword("createProfile",this.CreateProfile);
			Console.AddKeyword("editProfile",this.EditProfile);
			Console.AddKeyword("removeProfile",this.RemoveProfile);
			Event.Register("On Profile Selected",this);
			Event.Register("On Profile Edited",this);
			this.DetectGamepads();
		}
		public void Update(){
			this.DetectMouse();
			this.DetectKey();
		}
		public void FixedUpdate(){
			this.DetectGamepads();
		}
		public void OnGUI(){
			if(!Application.isPlaying){return;}
			var current = UnityEvent.current;
			if(current.isKey || current.shift || current.alt || current.control || current.command){
				if(!this.devices.Exists(x=>x.name=="Keyboard")){
					this.devices.Add(new InputDevice("Keyboard"));
				}
			}
			bool uiActive = this.uiState != InputUIState.None;
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
		[ContextMenu("Prepare Settings")] public void Setup(){InputGroup.Setup();}
		[ContextMenu("Save Settings")] public void Save(){InputGroup.Save();}
		[ContextMenu("Load Settings")] public void Load(){InputGroup.Load();}
		public void DrawProfileSelect(){
			if(this.uiState == InputUIState.SelectProfile){
				//var path = "@Main/InputUI/ProfileSelect/";
				//Locate.Find(path).SetActive(true);
				var buttonWidth = Screen.width * 0.5f;
				var buttonHeight = Screen.height * 0.09f;
				var area = new Rect((Screen.width/2)-buttonWidth/2,10,buttonWidth,buttonHeight);
				GUI.skin = FileManager.GetAsset<GUISkin>("Gentleface-Light.guiskin");
				var style = GUI.skin.button.Font("Bombardier.otf").FontSize((int)(buttonHeight*0.7f));
				GUI.Label(area,this.selectionHeader,style.Background(""));
				area = area.AddY(buttonHeight+8);
				foreach(var profile in this.profiles){
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
				Locate.Find(path+"Icon-Gamepad").GetComponent<Image>().sprite = action.helpImage;
				Locate.Find(path+"Icon-Gamepad").SetActive(!action.helpImage.IsNull());
				if(!this.lastInput.IsEmpty() && this.lastInputTime + 0.1f < Time.realtimeSinceStartup){
					string device = "Keyboard";
					string groupName = group.name.ToPascalCase();
					string actionName = action.name.ToPascalCase();
					if(this.lastInput.Contains("Joystick")){
						int id = (int)Char.GetNumericValue(this.lastInput.Remove("Joystick")[0]);
						device = this.joystickNames[id-1];
						this.lastInput = this.lastInput.ReplaceFirst(id.ToString(),"*");
					}
					else if(this.lastInput.Contains("Mouse")){device = "Mouse";}
					var existsText = Locate.Find(path+"Text-Exists");
					var match = profile.mappings.collection.Where(x=>x.Key.Contains(groupName) && x.Value.Contains(this.lastInput)).FirstOrDefault();
					existsText.SetActive(!match.Key.IsEmpty());
					if(!match.Key.IsEmpty()){
						existsText.GetComponent<Text>().text = this.lastInput.Remove("*") + " already mapped to : <color=#FF9999FF>" + match.Key.Split("-")[1] + "</color>";
						this.lastInput = "";
						return;
					}
					profile.requiredDevices.AddNew(device);
					profile.mappings[groupName+"-"+actionName] = this.lastInput;
					this.lastInput = "";
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
						string axisName = "Joystick"+joystickNumber+"-Axis"+ axisNumber;
						float value = Input.GetAxisRaw(axisName);
						if(Mathf.Abs(value) > this.gamepadDeadZone){
							axisName += value < 0 ? "Negative" : "Positive";
							if(axisName == this.lastInput){continue;}
							if(this.joystickAxis.AddNew(axisName)){continue;}
							this.joystickAxis[axisName] = true;
							this.lastInput = axisName;
							this.lastInputTime = Time.realtimeSinceStartup;
							continue;
						}
						this.joystickAxis[axisName+"Negative"] = false;
						this.joystickAxis[axisName+"Positive"] = false;
					}
				}
				if(Input.anyKeyDown){
					foreach(var keyCode in Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>()){
						var keyName = keyCode.ToName();
						if(keyName.Contains("JoystickButton")){continue;}
						if(keyName == this.lastInput){continue;}
						if(Input.GetKeyDown(keyCode)){
							this.lastInput = keyCode.ToName();
							this.lastInputTime = Time.realtimeSinceStartup;
							break;
						}
					}
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
			if(InputManager.mouseChange != Vector2.zero || InputManager.mouseScroll != Vector2.zero){
				this.lastInputTime = Time.realtimeSinceStartup;
				if(!this.devices.Exists(x=>x.name=="Mouse")){
					this.devices.Add(new InputDevice("Mouse"));
				}
				InputManager.mouseChangeAverage = (InputManager.mouseChangeAverage+InputManager.mouseChange)/2;
				if(this.uiState == InputUIState.EditProfile){
					var average = InputManager.mouseChangeAverage;
					var change = new Vector2(average.x.Abs(),average.y.Abs());
					if(InputManager.mouseScroll.y < 0){this.lastInput = "MouseScrollUp";}
					if(InputManager.mouseScroll.y > 0){this.lastInput = "MouseScrollDown";}
					if(change.x >= change.y){
						if(average.x < 0){this.lastInput = "MouseX-";}
						if(average.x > 0){this.lastInput = "MouseX+";}
					}
					else{
						if(average.y < 0){this.lastInput = "MouseY-";}
						if(average.y > 0){this.lastInput = "MouseY+";}
					}
				}
				return;
			}
			InputManager.mouseChangeAverage = Vector2.zero;
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
				Event.AddLimited("On Profile Edited",()=>this.SelectProfile(instance),1,this);
				return;
			}
			if(this.uiState == InputUIState.SelectProfile){
				Event.AddLimited("On Profile Selected",()=>this.SelectProfile(instance),1,this);
				return;
			}
			this.ShowProfiles();
			this.selectionHeader = instance.alias;
			Method selected = ()=>{
				this.instanceProfile[instance.name] = this.activeProfile;
				instance.joystickID = "";
				instance.profile = this.activeProfile;
				instance.actions.Clear();
				instance.Save();
			};
			Event.AddLimited("On Profile Selected",selected,1,this);
		}
		public void ShowProfiles(){
			this.activeProfile = null;
			this.selectionHeader = "";
			this.profiles.RemoveAll(x=>!File.Exists(x.name+".profile"));
			this.uiState = InputUIState.SelectProfile;
		}
		public void RemoveProfile(string name){
			if(FileManager.Find(name+".profile",true,false).IsNull()){
				this.ShowProfiles();
				this.selectionHeader = "Remove Profile";
				Event.AddLimited("On Profile Selected",()=>this.RemoveProfile(this.activeProfile.name),1,this);
				return;
			}
			this.profiles.RemoveAll(x=>x.name==name);
			FileManager.DeleteFile(name+".profile");
			foreach(var instance in Locate.GetSceneComponents<InputInstance>()){
				if(!instance.profile.IsNull() && instance.profile.name == name){
					this.instanceProfile.Remove(instance.alias);
					InputManager.instance.SelectProfile(instance);
				}
			}
		}
		public void EditProfile(string name){
			this.lastInput = "";
			this.uiState = InputUIState.EditProfile;
			this.uiGroupIndex = 0;
			this.uiIndex = 0;
			this.activeProfile = this.profiles.Find(x=>x.name==name);
			if(this.activeProfile.IsNull()){
				this.ShowProfiles();
				this.selectionHeader = "Edit Profile";
				Event.AddLimited("On Profile Selected",()=>this.EditProfile(this.activeProfile.name),1,this);
			}
		}
		public void CreateProfile(string name){
			int index = 0;
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