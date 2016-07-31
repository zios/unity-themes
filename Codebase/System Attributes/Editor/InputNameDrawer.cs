using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
namespace Zios.Editors{
	using Attributes;
	using Interface;
	using Inputs;
	using Events;
	[CustomPropertyDrawer(typeof(InputNameAttribute))]
	public class InputNameDrawer : PropertyDrawer{
		public List<string> items = new List<string>();
		public int index = 0;
		public override void OnGUI(Rect position,SerializedProperty property,GUIContent label){
			var parent = property.serializedObject.targetObject;
			var target = property.GetObject();
			var value = "";
			if(target is AttributeString){
				var attribute = target.As<AttributeString>();
				if(!attribute.isSetup){return;}
				value = attribute.Get();
			}
			if(target is string){value = property.stringValue;}
			if(this.items.Count < 1){
				foreach(var group in InputManager.instance.groups){
					foreach(var action in group.actions){
						this.items.Add(group.name.ToPascalCase()+"-"+action.name.ToPascalCase());
					}
					this.index = this.items.IndexOf(value);
					if(this.index == -1){this.index = 0;}
				}
			}
			Utility.RecordObject(parent,"Input Name Changes");
			this.index = this.items.Draw(position,this.index,"Input Action");
			if(GUI.changed || value.IsEmpty()){
				value = this.items[index];
				if(target is AttributeString){target.As<AttributeString>().Set(value);}
				if(target is string){property.stringValue = value;}
				parent.CallEvent("On Validate");
				property.serializedObject.Update();
				Utility.SetDirty(parent);
			}
		}
	}
}