using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
namespace Zios.Editors.AnimationEditors{
	using Actions.InputComponents;
	using Interface;
	using Inputs;
	[CustomEditor(typeof(InputHeld))]
	public class InputHeldEditor : DataMonoBehaviourEditor{
		public List<string> items = new List<string>();
		public int index = 0;
		public override void OnInspectorGUI(){
			var target = this.target.As<InputHeld>();
			base.OnInspectorGUI();
			if(items.Count < 1){
				foreach(var group in InputManager.instance.groups){
					foreach(var action in group.actions){
						this.items.Add(group.name.ToPascalCase()+"-"+action.name.ToPascalCase());
					}
					this.index = this.items.IndexOf(target.inputName);
					if(this.index == -1){this.index = 0;}
				}
			}
			this.index = this.items.Draw(this.index,"Input Action");
			target.inputName.Set(this.items[index]);
		}
	}
}