using System;
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
			if(wasNull){
				var instance = InputManager.instance;
				instance.uiObject = Locate.Find("@Main/InputUI");
				if(instance.uiObject.IsNull()){
					instance.uiObject = GameObject.Instantiate(FileManager.GetAsset<GameObject>("InputUI.prefab"));
					instance.uiObject.name = instance.uiObject.name.Remove("(Clone)");
					instance.uiObject.transform.SetParent(Locate.Find("@Main").transform);
				}
				InputGroup.Load();
			}
		}
	}
	public enum InputUIState{None,SelectProfile,EditProfile}
	public class InputManager : MonoBehaviour{
		[NonSerialized] public static InputManager instance;
		[NonSerialized] public static Vector3 mouseChange;
		[NonSerialized] public static Vector2 mouseScroll;
		[NonSerialized] public static Vector3 mousePosition;
		[NonSerialized] public static Vector3 mouseChangeAverage;
		public float deadZone = 0.1f;
		public List<InputGroup> groups = new List<InputGroup>();
		[Internal] public Dictionary<string,InputProfile> instanceProfile = new Dictionary<string,InputProfile>();
		[Internal] public List<InputDevice> devices = new List<InputDevice>();
		[Internal] public List<InputProfile> profiles = new List<InputProfile>();
		[Internal] public string[] joystickNames = new string[0];
		[Internal] public GameObject uiObject;
		private Dictionary<string,bool> joystickAxis = new Dictionary<string,bool>();
		private InputInstance activeInstance;
		private InputProfile activeProfile;
		private string lastInput;
		private float lastInputTime;
		private InputUIState uiState;
		private int uiGroupIndex;
		private int uiIndex;
		private bool hasGroups;
		//===============
		// Unity
		//===============
		public void OnValidate(){
			InputManager.instance = this;
			InputGroup.Setup();
		}
		public void Awake(){
			InputManager.instance = this;
			InputProfile.Load();
			InputInstance.Load();
			InputGroup.Load();
			InputGroup.Setup();
			this.hasGroups = this.groups.Count > 0 && this.groups[0].actions.Count > 0;
			this.DetectGamepads();
			//Event.Call("Add Console Keyword","createProfile",this.CreateProfile);
			Console.AddKeyword("showProfiles",this.ShowProfiles);
			Console.AddKeyword("createProfile",this.CreateProfile);
			Console.AddKeyword("editProfile",this.EditProfile);
			Console.AddKeyword("removeProfile",this.RemoveProfile);
			Event.Add("On Hierarchy Changed",InputGroup.Save);
			Event.Add("On Enter Play",InputGroup.Save);
			if(this.profiles.Count < 1){Utility.DelayCall(this.CreateDefaultProfile,0.5f);}
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
			if(uiActive){Console.Close(true);}
			this.uiObject.SetActive(uiActive);
			Locate.Find("@Main/InputUI/ProfileCreate/").SetActive(false);
			Locate.Find("@Main/InputUI/ProfileSelect/").SetActive(false);
			this.DrawProfileSelect();
			this.DrawProfileEdit();
		}
		//===============
		// GUI
		//===============
		[ContextMenu("Save Settings")] public static void Save(){InputGroup.Save();}
		[ContextMenu("Load Settings")] public static void Load(){InputGroup.Load();}
		public void DrawProfileSelect(){
			if(this.uiState == InputUIState.SelectProfile){
				//var path = "@Main/InputUI/ProfileSelect/";
				//Locate.Find(path).SetActive(true);
				var buttonWidth = Screen.width * 0.5f;
				var buttonHeight = Screen.height * 0.09f;
				var area = new Rect((Screen.width/2)-buttonWidth/2,10,buttonWidth,buttonHeight);
				GUI.skin = FileManager.GetAsset<GUISkin>("Gentleface-Light.guiskin");
				var style = GUI.skin.button.Font("Bombardier.otf").FontSize((int)(buttonHeight*0.7f));
				foreach(var profile in this.profiles){
					if(GUI.Button(area,profile.name,style)){
						this.activeProfile = profile;
						this.uiState = InputUIState.None;
					}
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
						if(Mathf.Abs(value) > this.deadZone){
							axisName += value < 0 ? "Negative" : "Positive";
							if(axisName == this.lastInput){continue;}
							if(this.joystickAxis.AddNew(axisName)){continue;}
							this.joystickAxis[axisName] = true;
							//Debug.Log("[InputManager] Joystick Axis -- " + axisName);
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
							//Debug.Log("[InputManager] Button press -- " + keyCode);
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
			if(InputManager.mouseScroll != Vector2.zero){
				this.lastInputTime = Time.realtimeSinceStartup;
				if(InputManager.mouseScroll.y < 0){this.lastInput = "MouseScrollUp";}
				if(InputManager.mouseScroll.y > 0){this.lastInput = "MouseScrollDown";}
			}
			if(Input.mousePosition != InputManager.mousePosition){
				this.lastInputTime = Time.realtimeSinceStartup;
				if(!this.devices.Exists(x=>x.name=="Mouse")){
					this.devices.Add(new InputDevice("Mouse"));
				}
				InputManager.mouseChange = InputManager.mousePosition - Input.mousePosition;
				InputManager.mouseChange.x *= -1;
				InputManager.mouseChangeAverage = (InputManager.mouseChangeAverage+InputManager.mouseChange)/2;
				InputManager.mousePosition = Input.mousePosition;
				if(this.uiState != InputUIState.EditProfile){return;}
				var change = InputManager.mouseChangeAverage.Abs();
				if(change.x >= change.y){
					if(InputManager.mouseChangeAverage.x < 0){this.lastInput = "MouseX-";}
					if(InputManager.mouseChangeAverage.x > 0){this.lastInput = "MouseX+";}
				}
				else{
					if(InputManager.mouseChangeAverage.y < 0){this.lastInput = "MouseY-";}
					if(InputManager.mouseChangeAverage.y > 0){this.lastInput = "MouseY+";}
				}
				return;
			}
			InputManager.mouseChange = Vector3.zero;
			InputManager.mouseChangeAverage = Vector3.zero;
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
			if(this.profiles.Count > 0 && !this.activeInstance.IsNull() && this.activeInstance != instance){return;}
			this.activeInstance = instance;
			instance.profile = this.activeProfile;
			if(instance.profile.IsNull()){
				if(this.profiles.Count > 1){
					this.ShowProfiles();
					return;
				}
				this.activeProfile = this.profiles[0];
				return;
			}
			instance.Save();
		}
		public void ShowProfiles(){
			this.activeProfile = null;
			this.uiState = InputUIState.SelectProfile;
		}
		public void CreateDefaultProfile(){this.CreateProfile(new string[2]{"CreateProfile","Default"});}
		public void CreateProfile(string[] values){
			if(values.Length < 2 || !this.hasGroups){return;}
			var name = values[1];
			int index = 2;
			while(this.profiles.Exists(x=>x.name==name)){
				name = "Profile"+index;
				index += 1;
			}
			var newProfile = this.profiles.AddNew(new InputProfile(name));
			this.EditProfile(newProfile);
		}
		public void EditProfile(InputProfile profile){
			if(!this.hasGroups){return;}
			this.lastInput = "";
			this.uiState = InputUIState.EditProfile;
			this.uiGroupIndex = 0;
			this.uiIndex = 0;
			this.activeProfile = profile;
		}
		public void EditProfile(string[] values){
			if(!this.hasGroups){return;}
			if(values.Length > 1){
				var profile = this.profiles.Find(x=>x.name==values[1]);
				if(profile.IsNull()){
					Console.AddLog("[InputManager] : Profile not found -- " + values[1]);
					return;
				}
				this.EditProfile(profile);
			}
		}
		public void RemoveProfile(string[] values){}
	}
}