using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace Zios.Inputs{
	[Serializable]
	public class InputGroup{
		public string name;
		public List<InputAction> actions = new List<InputAction>();
		public static void Setup(){
			foreach(var group in InputManager.instance.groups){
				foreach(var action in group.actions){
					action.Setup(group.name,InputManager.instance);
				}
			}
		}
		public static void Save(){
			if(InputManager.instance.groups.Count < 1){return;}
			var manager = InputManager.instance;
			var contents = "";
			var file = FileManager.Find("InputControls.cfg",true,false) ?? FileManager.CreateFile("InputControls.cfg");
			contents = contents.AddLine("[InputSettings]");
			if(manager.instanceOptions.ToInt() != 2){contents = contents.AddLine("InstanceOptions " + manager.instanceOptions.ToInt());}
			if(manager.gamepadDeadZone != 0.1f){contents = contents.AddLine("GamepadDeadZone " + manager.gamepadDeadZone);}
			if(manager.gamepadSensitivity != 1){contents = contents.AddLine("GamepadSensitivity " + manager.gamepadSensitivity);}
			if(manager.mouseSensitivity != 1){contents = contents.AddLine("MouseSensitivity " + manager.mouseSensitivity);}
			InputGroup.Setup();
			foreach(var group in InputManager.instance.groups){
				foreach(var action in group.actions){
					var helpPath = FileManager.GetPath(action.helpImage);
					var options = action.options.ToInt();
					contents = contents.AddLine("["+group.name.ToPascalCase()+"-"+action.name.ToPascalCase()+"]");
					if(options != 0){contents = contents.AddLine("Options " + options);}
					if(!helpPath.IsEmpty()){contents = contents.AddLine("HelpImage " + helpPath);}
					var transition = action.transition;
					if(transition.time.Get() != 0.5f){contents = contents.AddLine("Transition-Time " + transition.time.Get());}
					if(transition.speed.Get() != 3){contents = contents.AddLine("Transition-Speed " + transition.speed.Get());}
					if(transition.acceleration.Serialize() != "0-0-0-0|1-1-0-0"){contents = contents.AddLine("Transition-Acceleration " + transition.acceleration.Serialize());}
					if(transition.acceleration.Serialize() != "0-0-0-0|1-1-0-0"){contents = contents.AddLine("Transition-Acceleration " + transition.acceleration.Serialize());}
					if(transition.deceleration.Serialize() != "1-1-0-0|1-1-0-0"){contents = contents.AddLine("Transition-Deceleration " + transition.deceleration.Serialize());}
				}
			}
			file.WriteText(contents);
		}
		public static void Load(){
			if(!Application.isEditor){return;}
			var file = FileManager.Find("InputControls.cfg",true,false);
			if(file.IsNull()){return;}
			string group = "";
			string action = "";
			var fileText = file.GetText();
			var settings = fileText.Parse("[InputSettings]","[").GetLines();
			var remaining = fileText.Contains("[InputSettings]") ? fileText.Substring(fileText.IndexOf("[",0,2,true)).GetLines() : fileText.GetLines();
			var manager = InputManager.instance;
			foreach(var line in settings){
				if(line.IsEmpty()){continue;}
				var name = line.Parse(""," ").Trim();
				var value = line.Parse(" ").Trim();
				if(name.Contains("InstanceOptions")){manager.instanceOptions = value.ToInt().ToEnum<InputInstanceOptions>();}
				if(name.Contains("GamepadDeadZone")){manager.gamepadDeadZone = value.ToFloat();}
				if(name.Contains("GamepadSensitivity")){manager.gamepadSensitivity = value.ToFloat();}
				if(name.Contains("MouseSensitivity")){manager.mouseSensitivity = value.ToFloat();}
			}
			InputGroup.Setup();
			foreach(var line in remaining){
				if(line.IsEmpty()){continue;}
				if(line.ContainsAll("[","-")){
					var parts = line.Remove("[","]").Split("-");
					group = parts[0].ToTitleCase().Trim();
					action = parts[1].ToTitleCase().Trim();
					continue;
				}
				var name = line.Parse(""," ").Trim();
				var value = line.Parse(" ").Trim();
				var inputGroup = manager.groups.Find(x=>x.name.Trim()==group) ?? manager.groups.AddNew();
				var inputAction = inputGroup.actions.Find(x=>x.name.Trim()==action) ?? inputGroup.actions.AddNew();
				inputGroup.name = group;
				inputAction.name = action;
				inputAction.Setup("InputGroup",manager);
				if(name.Contains("Options")){inputAction.options = value.ToInt().ToEnum<InputActionOptions>();}
				if(name.Contains("HelpImage")){inputAction.helpImage = FileManager.GetAsset<Sprite>(value);}
				if(name.Contains("Time")){inputAction.transition.time.Set(value.ToFloat());}
				if(name.Contains("Speed")){inputAction.transition.speed.Set(value.ToFloat());}
				if(name.Contains("Acceleration")){inputAction.transition.acceleration.Deserialize(value);}
				if(name.Contains("Deceleration")){inputAction.transition.deceleration.Deserialize(value);}
			}
		}
	}
}