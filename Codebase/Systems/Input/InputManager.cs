using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEvent = UnityEngine.Event;
namespace Zios.Inputs{
	using Interface;
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
			if(wasNull){InputManager.instance.Setup();}
		}
	}
	public enum InputUIState{None,SelectProfile,EditProfile}
	public enum GamepadKey{None,Up,Down,Left,Right,Square,X,Triangle,Circle,L1,L2,R1,R2,Start,Select,LeftAnalog,LeftAnalogXAxis,LeftAnalogYAxis,LeftAnalogUp,LeftAnalogRight,LeftAnalogDown,LeftAnalogLeft,RightAnalog,RightAnalogXAxis,RightAnalogYAxis,RightAnalogUp,RightAnalogRight,RightAnalogDown,RightAnalogLeft}
	public class InputManager : MonoBehaviour{
		public static InputManager instance;
		public float deadZone = 0.1f;
		public List<InputGroup> groups = new List<InputGroup>();
		[Internal] public List<InputDevice> devices = new List<InputDevice>();
		[Internal] public List<InputProfile> profiles = new List<InputProfile>();
		[Internal] public List<InputInstance> instances = new List<InputInstance>();
		[Internal] public string[] joystickNames = new string[0];
		[Internal] public Sprite[] sprites = new Sprite[0];
		[Internal] public GameObject uiObject;
		private Dictionary<string,bool> joystickAxis = new Dictionary<string,bool>();
		private InputProfile activeProfile;
		private string lastInput;
		private float lastInputTime;
		private Vector3 mousePosition;
		private Vector3 mouseChange;
		private Vector3 mouseChangeAverage;
		private Vector2 mouseScroll;
		private InputUIState uiState;
		private int uiGroupIndex;
		private int uiIndex;
		private bool hasGroups;
		public void Setup(){
			this.sprites = FileManager.GetAssets<Sprite>("Gamepad*.png");
			this.uiObject = Locate.Find("@Main/InputUI");
			if(this.uiObject.IsNull()){
				this.uiObject = GameObject.Instantiate(FileManager.GetAsset<GameObject>("InputUI.prefab"));
				this.uiObject.name = this.uiObject.name.Remove("(Clone)");
				this.uiObject.transform.SetParent(Locate.Find("@Main").transform);
			}
		}
		public void Awake(){
			InputManager.instance = this;
			this.hasGroups = this.groups.Count > 0 && this.groups[0].actions.Count > 0;
			this.CheckJoysticks();
			Console.AddKeyword("createProfile",this.CreateProfile);
			Console.AddKeyword("editProfile",this.EditProfile);
			Console.AddKeyword("removeProfile",this.RemoveProfile);
			InputProfile.Load();
			if(this.profiles.Count < 1){Utility.DelayCall(this.DefaultProfile,0.5f);}
		}
		public void Update(){
			this.UpdateMouse();
			this.CheckConfigure();
		}
		public void FixedUpdate(){
			this.CheckJoysticks();
		}
		public void OnGUI(){
			if(!Application.isPlaying){return;}
			var current = UnityEvent.current;
			if(current.isKey || current.shift || current.alt || current.control || current.command){
				if(!this.devices.Exists(x=>x.name=="Keyboard")){
					this.devices.Add(new InputDevice("Keyboard"));
				}
			}
			this.uiObject.SetActive(this.uiState != InputUIState.None);
			if(this.uiState == InputUIState.EditProfile){
				var profile = this.activeProfile;
				var group = this.groups[this.uiGroupIndex];
				var action = group.actions[this.uiIndex];
				var path = "@Main/InputUI/ProfileCreate/";
				var iconName = "Gamepad-"+action.recommendedGamepadKey.ToName();
				Locate.Find(path+"Text-Key").GetComponent<Text>().text = action.name;
				Locate.Find(path+"Text-Profile").GetComponent<Text>().text = "<size=100><color=#888888FF>"+profile.name+"</color></size>\nProfile";
				Locate.Find(path+"Icon-Gamepad").GetComponent<Image>().sprite = this.sprites.Where(x=>x.name==iconName).FirstOrDefault();
				Locate.Find(path+"Icon-Gamepad").SetActive(action.recommendedGamepadKey != GamepadKey.None);
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
		public void CheckConfigure(){
			if(this.uiState == InputUIState.EditProfile){
				Console.Close(true);
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
		public void CheckJoysticks(){
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
		public void UpdateMouse(){
			this.mouseScroll = Input.mouseScrollDelta != Vector2.zero ? -Input.mouseScrollDelta : Vector2.zero;
			if(this.mouseScroll != Vector2.zero){
				this.lastInputTime = Time.realtimeSinceStartup;
				if(this.mouseScroll.y < 0){this.lastInput = "MouseScrollUp";}
				if(this.mouseScroll.y > 0){this.lastInput = "MouseScrollDown";}
			}
			if(Input.mousePosition != this.mousePosition){
				this.lastInputTime = Time.realtimeSinceStartup;
				if(!this.devices.Exists(x=>x.name=="Mouse")){
					this.devices.Add(new InputDevice("Mouse"));
				}
				this.mouseChange = this.mousePosition - Input.mousePosition;
				this.mouseChange.x *= -1;
				this.mouseChangeAverage = (this.mouseChangeAverage+this.mouseChange)/2;
				this.mousePosition = Input.mousePosition;
				if(this.uiState != InputUIState.EditProfile){return;}
				var change = this.mouseChangeAverage.Abs();
				if(change.x >= change.y){
					if(this.mouseChangeAverage.x < 0){this.lastInput = "MouseX-";}
					if(this.mouseChangeAverage.x > 0){this.lastInput = "MouseX+";}
				}
				else{
					if(this.mouseChangeAverage.y < 0){this.lastInput = "MouseY-";}
					if(this.mouseChangeAverage.y > 0){this.lastInput = "MouseY+";}
				}
				return;
			}
			this.mouseChange = Vector3.zero;
			this.mouseChangeAverage = Vector3.zero;
		}
		public void DefaultProfile(){this.CreateProfile("Default".AsArray());}
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