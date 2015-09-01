using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using Controller = Zios.Snapshot.SnapshotController;
namespace Zios.UI{
/*	[CustomEditor(typeof(Controller))]
	public class SnapshotControllerEditor : Editor{
		private AddAttributeElement addAttribute;
		private AddEventElement addEvent;
		public override void OnInspectorGUI(){
			if(this.addAttribute == null){
				this.addAttribute = new AddAttributeElement("Attributes",this.target,new AttributesListElement(target));
			}
			if(this.addEvent == null){
				this.addEvent = new AddEventElement("Events",this.target,new EventsListElement(target));
			}
			this.addAttribute.Draw();
			this.addEvent.Draw();
			if(this.addAttribute.list.shouldRepaint || this.addEvent.list.shouldRepaint){
				this.Repaint();
			}
		}
		class AttributesListElement : ListElementsTemplate{
			public AttributesListElement(UnityEngine.Object target):base(target){
			}
			public override void CreateActions(){
				this.actions.Add(new RemoveAttributeAction());
			}
			public override void CreateItems(){
				float width = 100f;
				this.listItems.Add(new ListItem("Component","componentName",width,ItemTypes.Label));
				this.listItems.Add(new ListItem("Attribute","attributeName",width,ItemTypes.Label));
				this.listItems.Add(new ListItem("Type","type",width,ItemTypes.Label));
				this.listItems.Add(new ListItem("Send Type","sendType",width,ItemTypes.Enumeration));
			}
			public override List<object> GetList(){
				List<object> elements = new List<object>();
				foreach(Configuration configuration in ((Controller)this.target).configurations){
					elements.Add(configuration);
				}
				return elements;
			}
			class RemoveAttributeAction : ListAction{
				public override void OnAction(UnityEngine.Object target,object targetItem){
					float width = 70f;
					if(GUILayout.Button("Remove",GUILayout.Width(width))){
						((Controller)target).configurations.Remove((Configuration)targetItem);
					}
				}
			}
		}
		class AddAttributeElement : AddElementTemplate{
			public List<Component> components = new List<Component>();
			public List<string> elementsNames = new List<string>();
			public List<string> skipElements = new List<string>();
			public Component currentComponent;
			public Controller controller;
			public AddAttributeElement(string title,UnityEngine.Object target,ListElementsTemplate list):base(title,target,list){
				this.controller = (Controller)this.target;
				skipElements = this.controller.ListAttributes();
			}
			public override void CreateSelectboxes(){
				float width = 100f;
				this.selectboxes.Add(new ComponentsSelectbox(width,this));
				this.selectboxes.Add(new Selectbox(width));
			}
			public void LoadComponents(){
				this.components = new List<Component>();
				Component[] allComponents = this.controller.GetComponents<Component>();
				foreach(Component component in allComponents){
					if(!component.GetType().Equals(this.GetType())){
						this.components.Add(component);
					}
				}
			}
			public override void UpdateSelectboxes(){
				if(this.components.Count == 0){
					this.LoadComponents();
				}
				Selectbox componentsBox = this.selectboxes[0];
				Selectbox attributesBox = this.selectboxes[1];
				if(componentsBox.Changed()){
					string[] componentsNames = new string[this.components.Count];
					foreach(Component component in this.components){
						componentsNames[this.components.IndexOf(component)] = component.GetType().Name;
					}
					componentsBox.options = componentsNames;
					this.currentComponent = this.components[componentsBox.index];
					this.elementsNames = new List<string>();
					List<string> allAttributes = this.currentComponent.ListAttributes();
					foreach(string attribute in allAttributes){
						if(!this.skipElements.Contains(attribute)){
							this.elementsNames.Add(attribute);
						}
					}
					attributesBox.options = this.elementsNames.ToArray();
					if(attributesBox.index > attributesBox.options.Length){
						attributesBox.index = 0;
					}
				}
			}
			public override void AddElement(){
				Selectbox componentsBox = this.selectboxes[0];
				Selectbox attributesBox = this.selectboxes[1];
				Component component = this.components[componentsBox.index];
				string attributeName = this.elementsNames[attributesBox.index];
				foreach(Configuration configuration in this.controller.configurations){
					if(configuration.component.Equals(component) && configuration.attributeName.Equals(attributeName)){
						return;
					}
				}
				this.controller.configurations.Add(new Configuration(component,attributeName));
			}
		}
		class ComponentsSelectbox : Selectbox{
			public AddAttributeElement addElement;
			public ComponentsSelectbox(float width,AddAttributeElement addElement):base(width){
				this.addElement = addElement;
			}
			public override void OnClick(){
				this.addElement.LoadComponents();
				base.OnClick();
			}
		}
		class EventsListElement : ListElementsTemplate{
			public EventsListElement(UnityEngine.Object target):base(target){
			}
			public override void CreateActions(){
				this.actions.Add(new RemoveEventAction());
			}
			public override void CreateItems(){
				float width = 100f; 
				this.listItems.Add(new ListItem("Component","componentName",width,ItemTypes.Label));
				this.listItems.Add(new ListItem("Method","methodName",width,ItemTypes.Label));
				this.listItems.Add(new ListItem("Name","name",width,ItemTypes.Label));
			}
			public override List<object> GetList(){
				List<object> elements = new List<object>();
				foreach(EventItem eventItem in ((Controller)this.target).listedEvents){
					elements.Add(eventItem); 
				}
				return elements;
			}
			class RemoveEventAction : ListAction{
				public override void OnAction(UnityEngine.Object target,object targetItem){
					float width = 70f;
					if(GUILayout.Button("Remove",GUILayout.Width(width))){
						((Controller)target).listedEvents.Remove((EventItem)targetItem);
					}
				}
			}
		}
		class AddEventElement : AddAttributeElement{
			public List<Type> argumentTypes;
			public string eventName;
			public AddEventElement(string title,UnityEngine.Object target,ListElementsTemplate list):base(title,target,list){
				this.argumentTypes = new List<Type>(){typeof(int),typeof(float),typeof(string),typeof(Vector3),typeof(Quaternion),typeof(NetworkViewID),typeof(NetworkPlayer)};
			}
			public override void CreateSelectboxes(){
				float width = 100f;
				this.selectboxes.Add(new ComponentsSelectbox(width,this));
				this.selectboxes.Add(new Selectbox(width));
			}
			public override void DrawCustomElements(){
				float width = 100f;
				eventName = EditorGUILayout.TextField(eventName,GUILayout.Width(width));
			}
			new public void LoadComponents(){
				this.components = new List<Component>();
				Component[] allComponents = this.controller.GetComponents<Component>();
				foreach(Component component in allComponents){
					if(!component.GetType().Equals(this.GetType()) && component.ListMethods(this.argumentTypes).Count > 0){
						this.components.Add(component);
					}
				}
			}
			public override void UpdateSelectboxes(){
				if(this.components.Count == 0){
					this.LoadComponents();
				}
				Selectbox componentsBox = this.selectboxes[0];
				Selectbox methodsBox = this.selectboxes[1];
				if(componentsBox.Changed() && this.components.Count > componentsBox.index){
					string[] componentsNames = new string[this.components.Count];
					foreach(Component component in this.components){
						componentsNames[this.components.IndexOf(component)] = component.GetType().Name;
					}
					componentsBox.options = componentsNames;
					this.currentComponent = this.components[componentsBox.index];
					this.elementsNames = new List<string>();
					List<string> allMethods = this.currentComponent.ListMethods(this.argumentTypes);
					foreach(string method in allMethods){
						this.elementsNames.Add(method);
					}
					methodsBox.options = this.elementsNames.ToArray();
					if(methodsBox.index > methodsBox.options.Length){
						methodsBox.index = 0;
					}
				}
			}
			public override void AddElement(){
				Selectbox componentsBox = this.selectboxes[0];
				Selectbox methodsBox = this.selectboxes[1];
				if(componentsBox.index < this.components.Count && methodsBox.index < this.elementsNames.Count && !String.IsNullOrEmpty(this.eventName)){
					Component component = this.components[componentsBox.index];
					string methodName = this.elementsNames[methodsBox.index];
					Zios.Snapshot.OnEvent onEvent = (Zios.Snapshot.OnEvent)MulticastDelegate.CreateDelegate(typeof(Zios.Snapshot.OnEvent),component,methodName);
					if(onEvent != null){
						foreach(EventItem item in this.controller.listedEvents){
							if(item.name.Equals(this.eventName) && item.methodName.Equals(methodName) && item.componentName.Equals(component.name)){
								return;
							}
						}
						this.controller.listedEvents.Add(new EventItem(this.eventName,onEvent,component));
					}
				}
			}
		}
	}*/
}