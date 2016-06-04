using System;
using System.Collections.Generic;
using UnityEngine;
namespace Zios.Inputs{
	using Containers;
	[Serializable]
	public class InputProfile{
		public string name;
		public List<string> requiredDevices = new List<string>();
		public StringContainer mappings = new StringContainer();
		public InputProfile(string name){this.name = name;}
		public void Save(){
			var contents = "";
			var file = FileManager.Find(this.name+".profile",true,false) ?? FileManager.Create(this.name+".profile");
			contents = contents.AddLine("[Input-Devices]");
			this.requiredDevices.ForEach(x=>contents = contents.AddLine(x));
			var activeGroup = "";
			foreach(var item in this.mappings){
				var name = item.Key.Split("-");
				var groupName = name[0];
				if(activeGroup != groupName){
					activeGroup = groupName;
					contents = contents.AddLine("[InputGroup-"+groupName+"]");
				}
				contents = contents.AddLine(name[1] + " " + item.Value);
			}
			file.WriteText(contents);
		}
		public static void Load(){
			foreach(var file in FileManager.FindAll("*.profile",true,false)){
				var profile = new InputProfile(file.name);
				var text = file.GetText().GetLines();
				int mode = 0;
				string group = "";
				foreach(var line in text){
					if(line.IsEmpty()){continue;}
					if(line.Contains("[Input-Devices]")){mode = 1;}
					else if(line.Contains("[InputGroup-")){
						mode = 2;
						group = line.Parse("-","]");
					}
					else if(mode == 1){profile.requiredDevices.Add(line.Trim());}
					else if(mode == 2){
						var actionName = line.Parse(""," ");
						var buttonName = line.Parse(" ");
						profile.mappings[group+"-"+actionName] = buttonName;
					}
				}
				InputManager.instance.profiles.Add(profile);
			}
		}
	}
}