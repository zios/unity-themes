using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Controller = Zios.Snapshot.SnapshotController;
namespace Zios.Snapshot{
	[CustomEditor(typeof(Controller))]
	public class SnapshotControllerEditor : Editor{
		private CustomAddElement addElement;
		public override void OnInspectorGUI(){
			if(this.addElement == null){
				this.addElement = new CustomAddElement("",this.target,new CustomListElement(target));
			}
			this.addElement.Draw();
			if(this.addElement.list.shouldRepaint){
				this.Repaint();
			}
			if(GUI.changed){
				EditorUtility.SetDirty(target);
			}
		}
		class RemoveItemAction : ListAction{
			public override void OnAction(UnityEngine.Object target,object targetItem){
				float width = 70f;
				if(GUILayout.Button("Remove",GUILayout.Width(width))){
					((Controller)target).Remove((Configuration)targetItem);
				}
			}
		}
		class CustomListElement : ListElementsTemplate{
			public CustomListElement(UnityEngine.Object target):base(target){
			}
			public override void CreateActions(){
				this.actions.Add(new RemoveItemAction());
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
		}
		class ComponentsSelectbox : Selectbox{
			public Controller target;
			public ComponentsSelectbox(float width,Controller target):base(width){
				this.target = target;
			}
			public override void OnClick(){
				this.target.LoadComponents();
				base.OnClick();
			}
		}
		class CustomAddElement : AddElementTemplate{
			public CustomAddElement(string title,UnityEngine.Object target,CustomListElement list):base(title,target,list){
			}
			public override void CreateSelectboxes(){
				float width = 100f;
				this.selectboxes.Add(new ComponentsSelectbox(width,(Controller)this.target));
				this.selectboxes.Add(new Selectbox(width));
			}
			public override void UpdateSelectboxes(){
				Controller controller = (Controller)this.target;
				if(controller.components.Count == 0){
					controller.LoadComponents();
				}
				Selectbox componentsBox = this.selectboxes[0];
				Selectbox attributesBox = this.selectboxes[1];
				if(componentsBox.Changed()){
					string[] componentsNames = new string[controller.components.Count];
					foreach(Component component in controller.components){
						componentsNames[controller.components.IndexOf(component)] = component.GetType().Name;
					}
					componentsBox.options = componentsNames;
					controller.SelectComponent(componentsBox.index);
					attributesBox.options = controller.attributes.ToArray();
					if(attributesBox.index > attributesBox.options.Length){
						attributesBox.index = 0;
					}
				}
			}
			public override void AddElement(){
				Selectbox componentsBox = this.selectboxes[0];
				Selectbox attributesBox = this.selectboxes[1];
				Controller controller = (Controller)this.target;
				controller.Add(componentsBox.index,attributesBox.index);
			}
		}
	}
}