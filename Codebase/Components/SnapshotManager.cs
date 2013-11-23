using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
[AddComponentMenu("Zios/Component/General/Snapshot Manager")]
[ExecuteInEditMode]
public class SnapshotManager : MonoBehaviour{
	public List<SnapshotConfiguration> configurations = new List<SnapshotConfiguration>();
	public List<Component> components = new List<Component>();
	public List<string> attributes = new List<string>();
	private List<string> skipAttributes = new List<string>();
	private Component currentComponent;
	void Awake(){
		this.skipAttributes = this.ListAttributes();
	}
	void Update(){
	}
	public void LoadComponents(){
		this.components = new List<Component>();
		Component[] allComponents = GetComponents<Component>();
		foreach(Component component in allComponents){
			if(!component.GetType().Equals(this.GetType())){
				this.components.Add(component);
			}
		}
	}
	public void SelectComponent(int index){
		this.currentComponent = components[index];
		this.attributes = new List<string>();
		List<string> allAttributes = this.currentComponent.ListAttributes();
		foreach(string attribute in allAttributes){
			if(!skipAttributes.Contains(attribute)){
				this.attributes.Add(attribute);
			}
		}
	}
	public void Remove(SnapshotConfiguration setting){
		this.configurations.Remove(setting);
	}
	public void Add(int componentIndex,int attributeIndex){
		Component component = this.components[componentIndex];
		string attributeName = this.attributes[attributeIndex];
		foreach(SnapshotConfiguration configuration in this.configurations){
			if(configuration.component.Equals(component) && configuration.attributeName.Equals(attributeName)){
				return;
			}
		}
		this.configurations.Add(new SnapshotConfiguration(component,attributeName));
	}
}
[Serializable]
public class SnapshotConfiguration{
	public Component component;
	public string componentName;
	public string attributeName;
	public string type;
	public SerializationTypes sendType;
	public SnapshotConfiguration(Component component,string attributeName){
		this.component = component;
		this.componentName = this.component.GetType().Name;
		this.attributeName = attributeName;
		Accessor accessor = new Accessor(this.component,this.attributeName);
		this.type = accessor.Get().GetType().Name;
		this.sendType = SerializationTypes.Int;
		foreach(SerializationTypes serializationType in Enum.GetValues(typeof(SerializationTypes))){
			if(serializationType.ToString().ToLower().Equals(this.type.ToLower())){
				this.sendType = serializationType;
				break;
			}
		}
	}
}
public enum SerializationTypes{
	Bool,
	Char,
	Short,
	Int,
	Float,
	Quaternion,
	Vector3
};