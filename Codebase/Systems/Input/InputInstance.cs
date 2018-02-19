using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Zios.Inputs{
	using Zios.Attributes.Supports;
	using Zios.Events;
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.File;
	using Zios.SystemAttributes;
	using Zios.Unity.Call;
	using Zios.Unity.Components.ManagedBehaviour;
	using Zios.Unity.Pack;
	using Zios.Unity.Proxy;
	//asm Zios.Shortcuts;
	//asm Zios.Unity.Components.DataBehaviour;
	//asm Zios.Unity.Shortcuts;
	public class InputInstance : ManagedBehaviour{
		public Dictionary<string,bool> active = new Dictionary<string,bool>();
		public Dictionary<string,float> intensity = new Dictionary<string,float>();
		public Dictionary<string,float> maxIntensity = new Dictionary<string,float>();
		public Dictionary<string,InputAction> lookup = new Dictionary<string,InputAction>();
		public bool manuallyControlled;
		[Internal] public List<InputAction> actions = new List<InputAction>();
		[Internal] public AttributeInt state = -1;
		[Internal] public InputProfile profile;
		[Internal] public string joystickID;
		//===============
		// Storage
		//===============
		public static void Load(){
			var file = File.Find("InputDefaults.cfg",false) ?? File.Create("InputDefaults.cfg");
			var contents = file.GetText().GetLines();
			foreach(var line in contents){
				if(line.IsEmpty()){continue;}
				var instanceName = line.Parse(""," ");
				var profileName = line.Parse(" ");
				var profile = InputManager.Get().profiles.Find(x=>x.name==profileName);
				InputManager.Get().instanceProfile[instanceName] = profile;
			}
		}
		public void Save(){
			if(this.profile.IsNull() || this.profile.name.IsEmpty() || this.profile.mappings.Count < 1){return;}
			var file = File.Find("InputDefaults.cfg",false) ?? File.Create("InputDefaults.cfg");
			var contents = file.GetText();
			var alias = this.alias.ToPascalCase();
			var profile = this.profile.name.ToPascalCase();
			var phrase = alias+" "+profile+"\r\n";
			if(contents.Contains(alias)){
				var existing = contents.Cut(alias,"\n");
				contents = contents.Replace(existing,phrase);
			}
			else{
				contents += phrase;
			}
			file.WriteText(contents);
		}
		//===============
		// Unity
		//===============
		public override void Awake(){
			base.Awake();
			this.DefaultRate("Update");
			this.state.Setup("State",this);
			Events.Add("Hold Input",this.HoldInput,this);
			Events.Add("Release Input",this.ReleaseInput,this);
			if(Proxy.IsPlaying()){
				this.profile = InputManager.Get().GetInstanceProfile(this);
				if(this.profile.IsNull() || this.profile.name.IsEmpty()){
					Call.Delay(()=>InputManager.Get().SelectProfile(this),0.1f);
				}
			}
			Events.Add("On Validate",this.PrepareInput,InputManager.Get());
			this.PrepareInput();
		}
		//===============
		// General
		//===============
		public override void Step(){
			if(this.manuallyControlled){
				if(this.state != -1){
					this.active.SetValues(Pack.UnpackBools(this.active.Count,this.state));
					this.state = -1;
				}
				this.StepInput();
				return;
			}
			if(!this.profile.IsNull() && !this.profile.name.IsEmpty()){
				this.PrepareGamepad();
				this.CheckInput();
				this.StepInput();
			}
		}
		public void PrepareGamepad(){
			if(!this.joystickID.IsEmpty()){return;}
			string gamepad = this.profile.requiredDevices.Find(x=>!x.MatchesAny("Keyboard","Mouse")) ?? "";
			var allGamepads = InputManager.Get().joystickNames;
			for(int index=0;index<allGamepads.Length;++index){
				string name = allGamepads[index].Trim();
				if(name == gamepad.Trim()){
					this.joystickID = (index+1).ToString();
					return;
				}
			}
			this.joystickID = "[None]";
		}
		public void PrepareInput(){
			if(!Proxy.IsPlaying() && !InputManager.Get().IsNull()){
				this.actions.Clear();
				foreach(var group in InputManager.Get().groups){
					foreach(var action in group.actions){
						var newAction = action.Copy();
						newAction.name = group.name.ToPascalCase() + "-" + action.name.ToPascalCase();
						newAction.Setup(this.alias,this);
						this.actions.Add(newAction);
					}
				}
			}
			if(this.lookup.Count < 1){
				foreach(var action in this.actions){
					action.Setup(this.alias,this);
					this.lookup[action.name] = action;
					this.intensity[action.name] = 0;
					this.maxIntensity[action.name] = 1;
					this.active[action.name] = false;
				}
			}
		}
		public void CheckInput(){
			foreach(var item in this.profile.mappings){
				string action = item.Key;
				string input = item.Value.Replace("*",this.joystickID);
				this.active[action] = false;
				if(input.Contains("*")){continue;}
				if(input.ContainsAll("Joystick","Axis") && !this.joystickID.IsEmpty()){
					string axisName = input.Remove("Negative","Positive");
					float axis = Input.GetAxis(axisName);
					if(Mathf.Abs(axis) > InputManager.Get().gamepadDeadZone){
						if(axis < 0 && input.Contains("Negative")){this.active[action] = true;}
						if(axis > 0 && input.Contains("Positive")){this.active[action] = true;}
						this.maxIntensity[action] = axis.Abs() * InputManager.Get().gamepadSensitivity;
						this.ClampIntensity(action);
					}
				}
				else if(input.ContainsAny("MouseScroll","MouseX","MouseY")){
					if(input.Contains("Scroll")){
						var scroll = -Input.mouseScrollDelta.y;
						if(scroll < 0 && input.Contains("Up")){this.active[action] = true;}
						if(scroll > 0 && input.Contains("Down")){this.active[action] = true;}
					}
					else{
						Vector2 change = InputManager.mouseChange;
						if(change.x >= 0 && input.Contains("X-")){continue;}
						if(change.x <= 0 && input.Contains("X+")){continue;}
						if(change.y >= 0 && input.Contains("Y-")){continue;}
						if(change.y <= 0 && input.Contains("Y+")){continue;}
						if(change.x != 0 && input.Contains("X")){this.maxIntensity[action] = change.x.Abs();}
						if(change.y != 0 && input.Contains("Y")){this.maxIntensity[action] = change.y.Abs();}
						this.ClampIntensity(action);
						this.active[action] = true;
					}
				}
				else{
					var key = new KeyCode().ParseEnum(input);
					this.active[action] = Input.GetKeyDown(key) || Input.GetKey(key);
				}
			}
		}
		public void StepInput(){
			int packed = Pack.PackBools(this.active.Values.ToArray());
			this.state.Set(packed);
			foreach(var item in this.active){
				var action = item.Key;
				float goal = item.Value ? this.maxIntensity[action] : 0;
				this.intensity[action] = this.lookup[action].transition.Step(this.intensity[action],goal);
			}
		}
		public void ClampIntensity(string name){
			if(!this.lookup[name].options.Contains(InputActionOptions.Unclamped)){
				this.maxIntensity[name] = this.maxIntensity[name].Clamp(0,1);
			}
		}
		//===============
		// Interface
		//===============
		public float GetIntensity(string name){return this.intensity.Get(name);}
		public bool GetHeld(string name){return this.active.Get(name);}
		public void HoldInput(string name){this.active[name] = true;}
		public void ReleaseInput(string name){this.active[name] = false;}
	}
}