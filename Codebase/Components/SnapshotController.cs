#pragma warning disable 0618
using System;
using System.Collections.Generic;
using UnityEngine;
namespace Zios.Snapshot{
	public delegate void OnEvent(int intValue,float floatValue,string stringValue,Vector3 vectorValue,Quaternion quaternionValue,NetworkViewID networkViewId,NetworkPlayer networkPlayer);
	[ExecuteInEditMode]
	[AddComponentMenu("Zios/Component/General/Snapshot Controller")]
	public class SnapshotController : MonoBehaviour{
		public List<EventItem> listedEvents = new List<EventItem>();
		public Dictionary<string,OnEvent> events = new Dictionary<string, OnEvent>();
		public List<Configuration> configurations = new List<Configuration>();
		public void OnEnable(){
			if(this.gameObject.GetComponent<NetworkView>() == null){
				this.gameObject.AddComponent<NetworkView>();
			}
			this.gameObject.GetComponent<NetworkView>().observed = this;
		}
		public void Start(){
			foreach(EventItem item in listedEvents){
				Zios.Snapshot.OnEvent onEvent = (Zios.Snapshot.OnEvent)MulticastDelegate.CreateDelegate(typeof(Zios.Snapshot.OnEvent),item.component,item.methodName);
				if(!this.events.ContainsKey(item.name)){
					this.events.Add(item.name,onEvent);
				}
				else{
					this.events[item.name] += onEvent;
				}
			}
		}
		public void OnSerializeNetworkView(BitStream stream,NetworkMessageInfo info){
			foreach(Configuration configuration in this.configurations){
				if(configuration.accessor == null){
					configuration.accessor = new Accessor(configuration.component,configuration.attributeName);
					configuration.CalculateNewType();
				}
				object value = stream.isReading ? configuration.defaultValue : Convert.ChangeType(configuration.accessor.Get(),configuration.newType);
				stream.Serialize(ref value);
				if(stream.isReading){
					configuration.accessor.Set(value);
				}
			}
		}
		[RPC]
		public void CheckEvents(string name,int intValue,float floatValue,string stringValue,Vector3 vectorValue,Quaternion quaternionValue,NetworkViewID networkViewId,NetworkPlayer networkPlayer){
			if(this.events.ContainsKey(name)){
				this.events[name](intValue,floatValue,stringValue,vectorValue,quaternionValue,networkViewId,networkPlayer);
			}
		}
	}
	[Serializable]
	public class EventItem{
		public string name;
		public string methodName;
		public string componentName;
		public Component component;
		public Zios.Snapshot.OnEvent onEvent;
		public EventItem(string name,Zios.Snapshot.OnEvent onEvent,Component component){
			this.name = name;
			this.onEvent = onEvent;
			this.methodName = onEvent.Method.Name;
			this.component = component;
			this.componentName = component.GetType().Name;
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
		public Type newType;
		public object defaultValue;
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
		public void CalculateNewType(){
			this.newType = typeof(bool);
			this.defaultValue = false;
			if(this.sendType == SerializeType.Char){
				this.newType = typeof(char);
				this.defaultValue = (char)0;
			}
			else if(this.sendType == SerializeType.Short){
				this.newType = typeof(short);
				this.defaultValue = (short)0;
			}
			else if(this.sendType == SerializeType.Float){
				this.newType = typeof(float);
				this.defaultValue = 0f;
			}
			else if(this.sendType == SerializeType.Int){
				this.newType = typeof(int);
				this.defaultValue = 0;
			}
			else if(this.sendType == SerializeType.Quaternion){
				this.newType = typeof(Quaternion);
				this.defaultValue = Quaternion.identity;
			}
			else if(this.sendType == SerializeType.Vector3){
				this.newType = typeof(Vector3);
				this.defaultValue = Vector3.zero;
			}
		}
	}
	public enum SerializeType{Bool,Char,Short,Int,Float,Quaternion,Vector3};
}