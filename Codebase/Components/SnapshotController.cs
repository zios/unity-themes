using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Zios.Snapshot{
	[ExecuteInEditMode]
	[AddComponentMenu("Zios/Component/General/Snapshot Controller")]
	public class SnapshotController : MonoBehaviour{
		public List<Configuration> configurations = new List<Configuration>();
		public List<Component> components = new List<Component>();
		public List<string> attributes = new List<string>();
		private List<string> skipAttributes = new List<string>();
		private Component currentComponent;
		void Awake(){
			this.skipAttributes = this.ListAttributes();
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
				if(!this.skipAttributes.Contains(attribute)){
					this.attributes.Add(attribute);
				}
			}
		}
		public void Remove(Configuration setting){
			this.configurations.Remove(setting);
		}
		public void Add(int componentIndex,int attributeIndex){
			Component component = this.components[componentIndex];
			string attributeName = this.attributes[attributeIndex];
			foreach(Configuration configuration in this.configurations){
				if(configuration.component.Equals(component) && configuration.attributeName.Equals(attributeName)){
					return;
				}
			}
			this.configurations.Add(new Configuration(component,attributeName));
		}
		public void OnSerializeNetworkView(BitStream stream,NetworkMessageInfo info){
			foreach(Configuration configuration in this.configurations){
				if(configuration.accessor == null){
					configuration.accessor = new Accessor(configuration.component,configuration.attributeName);
				}
				object value = configuration.accessor.Get();
				stream.Serialize(ref value);
				if(stream.isWriting){configuration.accessor.Set(value);}
			}
		}
	}
	[Serializable]
	public class Configuration{
		public Component component;
		public string componentName;
		public string attributeName;
		public string type;
		public SerializeType sendType;
		public Accessor accessor;
		public Configuration(Component component,string attributeName){
			this.component = component;
			this.componentName = this.component.GetType().Name;
			this.attributeName = attributeName;
			this.accessor = new Accessor(this.component,this.attributeName);
			this.type = this.accessor.Get().GetType().Name;
			this.sendType = SerializeType.Int;
			foreach(SerializeType type in Enum.GetValues(typeof(SerializeType))){
				if(type.ToString().ToLower().Equals(this.type.ToLower())){
					this.sendType = type;
					break;
				}
			}
		}
	}
	public enum SerializeType{Bool,Char,Short,Int,Float,Quaternion,Vector3};
}