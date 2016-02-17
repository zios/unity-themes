using System.Collections.Generic;
using UnityEngine;
namespace Zios.Inputs{
	using Attributes;
	using Events;
	public class InputInstance : ManagedMonoBehaviour{
		public Dictionary<string,bool> active = new Dictionary<string,bool>();
		public Dictionary<string,float> intensity = new Dictionary<string,float>();
		public Dictionary<string,float> maxIntensity = new Dictionary<string,float>();
		public Dictionary<string,InputAction> actions = new Dictionary<string,InputAction>();
		public bool manuallyControlled;
		[Internal] public AttributeInt state = 0;
		[Internal] public InputProfile profile;
		[Internal] public string joystickID;
		//===============
		// Storage
		//===============
		public static void Load(){
			var file = FileManager.Find("InputDefaults.cfg",true,false) ?? FileManager.Create("InputDefaults.cfg");
			var contents = file.GetText().GetLines();
			foreach(var line in contents){
				if(line.IsEmpty()){continue;}
				var instanceName = line.Parse(""," ").ToTitleCase();
				var profileName = line.Parse(" ").ToTitleCase();
				var profile = InputManager.instance.profiles.Find(x=>x.name==profileName);
				InputManager.instance.instanceProfile[instanceName] = profile;
			}
		}
		public void Save(){
			if(this.profile.IsNull() || this.profile.name.IsEmpty()){return;}
			var file = FileManager.Find("InputDefaults.cfg",true,false) ?? FileManager.Create("InputDefaults.cfg");
			var contents = file.GetText();
			var alias = this.alias.ToPascalCase();
			var profile = this.profile.name.ToPascalCase();
			if(contents.Contains(this.alias)){
				var existing = contents.Cut(alias,"\n");
				contents = contents.Replace(existing,alias+" "+profile+"\n");
			}
			else{
				contents += alias+" "+profile+"\r\n";
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
			this.profile = InputManager.instance.GetInstanceProfile(this.alias.ToTitleCase());
			Event.Add("Hold Input",this.HoldInput);
			Event.Add("Release Input",this.ReleaseInput);
		}
		//===============
		// General
		//===============
		public override void Step(){
			if(this.manuallyControlled){
				this.PrepareInput();
				this.StepInput();
				return;
			}
			if(this.profile.IsNull() || this.profile.name.IsEmpty()){
				InputManager.instance.SelectProfile(this);
				return;
			}
			this.PrepareInput();
			this.CheckInput();
			this.StepInput();
		}
		public void PrepareInput(){
			if(this.actions.Count < 1){
				foreach(var group in InputManager.instance.groups){
					foreach(var action in group.actions){
						var actionName = group.name.ToPascalCase() + "-" + action.name.ToPascalCase();
						this.intensity[actionName] = 0;
						this.maxIntensity[actionName] = 1;
						this.active[actionName] = false;
						this.actions[actionName] = action;
					}
				}
			}
		}
		public void CheckInput(){
			foreach(var item in profile.mappings){
				string action = item.Key;
				string input = item.Value.Replace("*",this.joystickID);
				this.active[action] = false;
				if(input.Contains("*")){continue;}
				if(input.ContainsAll("Joystick","Axis") && !this.joystickID.IsEmpty()){
					string axisName = input.Remove("Negative","Positive");
					float axis = Input.GetAxisRaw(axisName);
					if(axis < 0 && input.Contains("Negative")){this.active[action] = true;}
					if(axis > 0 && input.Contains("Positive")){this.active[action] = true;}
					this.maxIntensity[action] = axis.Abs();
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
						this.active[action] = true;
					}
				}
				else{
					var key = new KeyCode().ParseEnum(input);
					this.active[action] = Input.GetKey(key);
				}
			}
		}
		public void StepInput(){
			foreach(var item in this.active){
				var action = item.Key;
				float goal = item.Value ? this.maxIntensity[action] : 0;
				this.intensity[action] = this.actions[action].transition.Step(this.intensity[action],goal);
			}
		}
		//===============
		// Interface
		//===============
		public void HoldInput(string name){this.active[name] = true;}
		public void ReleaseInput(string name){this.active[name] = false;}
	}
}