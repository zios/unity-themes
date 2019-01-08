using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.Attribute.Drawers{
	using Zios.Attributes.Supports;
	using Zios.Events;
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Inputs;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.EditorUI;
	using Zios.Unity.Editor.Extensions;
	using Zios.Unity.SystemAttributes;
	//asm Zios.Shortcuts;
	//asm Zios.Unity.Shortcuts;
	[CustomPropertyDrawer(typeof(InputNameAttribute))]
	public class InputNameDrawer : PropertyDrawer{
		public List<string> items = new List<string>();
		public int index = 0;
		public override void OnGUI(Rect position,SerializedProperty property,GUIContent label){
			EditorUI.Reset();
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
				foreach(var group in InputManager.Get().groups){
					foreach(var action in group.actions){
						this.items.Add(group.name.ToPascalCase()+"-"+action.name.ToPascalCase());
					}
					this.index = this.items.IndexOf(value);
					if(this.index == -1){this.index = 0;}
				}
			}
			ProxyEditor.RecordObject(parent,"Input Name Changes");
			this.index = this.items.Draw(position,this.index,"Input Action");
			if(GUI.changed || value.IsEmpty()){
				value = this.items[index];
				if(target is AttributeString){target.As<AttributeString>().Set(value);}
				if(target is string){property.stringValue = value;}
				parent.CallEvent("On Validate");
				property.serializedObject.Update();
				ProxyEditor.SetDirty(parent);
			}
		}
	}
}