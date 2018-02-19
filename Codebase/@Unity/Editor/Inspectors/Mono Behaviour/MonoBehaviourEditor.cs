using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using MenuFunction = UnityEditor.GenericMenu.MenuFunction;
namespace Zios.Unity.Editor.MonoBehaviourEditor{
	using Zios.Events;
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Reflection;
	using Zios.Shortcuts;
	using Zios.SystemAttributes;
	using Zios.Unity.Call;
	using Zios.Unity.Editor.Extensions;
	using Zios.Unity.Editor.Inspectors;
	using Zios.Unity.Editor.Pref;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.EditorUI;
	using Zios.Unity.Extensions;
	using Zios.Unity.Proxy;
	using Zios.Unity.Time;
	//asm Zios.Shortcuts;
	//asm Zios.Unity.Shortcuts;
	using Editor = UnityEditor.Editor;
	[CustomEditor(typeof(MonoBehaviour),true)][CanEditMultipleObjects]
	public class MonoBehaviourEditor : HeaderEditor{
		public static Dictionary<Type,Dictionary<string,object>> defaults = new Dictionary<Type,Dictionary<string,object>>();
		public static float resumeHierarchyTime = -1;
		public static Dictionary<Editor,bool> offScreen = new Dictionary<Editor,bool>();
		public bool hideDefault;
		public bool setup;
		public bool changed;
		public List<SerializedProperty> properties = new List<SerializedProperty>();
		public List<SerializedProperty> hidden = new List<SerializedProperty>();
		public Dictionary<string,object> dictionaries = new Dictionary<string,object>();
		public Dictionary<SerializedProperty,Rect> propertyArea = new Dictionary<SerializedProperty,Rect>();
		public Dictionary<SerializedProperty,bool> propertyVisible = new Dictionary<SerializedProperty,bool>();
		public Rect area;
		public Rect areaStart;
		public Rect areaEnd;
		public bool areaBegan;
		public bool showAll;
		public bool visible = true;
		public Method dirtyEvent;
		public override void OnInspectorGUI(){
			EditorUI.Reset();
			ProxyEditor.GetInspector(this).SetTitle(this.title);
			ProxyEditor.GetInspectors().ForEach(x=>x.wantsMouseMove = true);
			if(!Event.current.IsUseful()){return;}
			if(this.target is MonoBehaviour && this.target.As<MonoBehaviour>().InPrefabFile()){return;}
			this.BeginArea();
			bool fastInspector = EditorPref.Get<bool>("MonoBehaviourEditor-FastInspector");
			/*if(fastInspector && MonoBehaviourEditor.offScreen.ContainsKey(this)){
				GUILayout.Space(this.area.height);
				this.CheckChanges();
				return;
			}*/
			if(Event.current.type == EventType.MouseMove){
				Call.Delay(ProxyEditor.RepaintInspectors,0.1f);
			}
			bool hideAllDefault = EditorPref.Get<bool>("MonoBehaviourEditor-HideAllDefault",false);
			this.hideDefault = EditorPref.Get<bool>("MonoBehaviourEditor-"+this.target.GetInstanceID()+"HideDefault",false);
			bool hideDefault = hideAllDefault || this.hideDefault;
			if(hideDefault){this.SortDefaults();}
			this.serializedObject.Update();
			this.SortProperties();
			this.Setup();
			Type type = this.target.GetType();
			this.changed = false;
			bool showAdvanced = EditorPref.Get<bool>("MonoBehaviourEditor-Advanced");
			bool showInternal = EditorPref.Get<bool>("MonoBehaviourEditor-Internal");
			bool showDictionary = EditorPref.Get<bool>("MonoBehaviourEditor-Dictionary");
			EditorGUILayout.BeginVertical();
			foreach(var property in this.properties){
				string[] attributes = this.serializedObject.targetObject.ListAttributes(property.name).Select(x=>x.GetType().Name).ToArray();
				bool isInternal = attributes.Contains("InternalAttribute");
				bool isAdvanced = attributes.Contains("AdvancedAttribute");
				bool isReadOnly = isInternal || attributes.Contains("ReadOnlyAttribute");
				bool isHidden = !this.showAll && this.hidden.Contains(property);
				if(isAdvanced && !showAdvanced){isHidden = true;}
				if(isInternal && !showInternal){isHidden = true;}
				object currentValue = property.GetObject<object>();
				bool hasDefault = MonoBehaviourEditor.defaults.ContainsKey(type) && MonoBehaviourEditor.defaults[type].ContainsKey(property.name);
				if(!this.showAll && hideDefault && hasDefault){
					object defaultValue = MonoBehaviourEditor.defaults[type][property.name];
					if(defaultValue.IsNull()){continue;}
					bool isDefault = defaultValue.Equals(currentValue);
					if(isDefault){isHidden = true;}
				}
				if(!isHidden){
					bool hasArea = this.propertyArea.ContainsKey(property);
					if(hasArea){
						if(Event.current.shift){
							bool canHide = (this.properties.Count - this.hidden.Count) > 1;
							if(this.propertyArea[property].Clicked(0) && canHide){
									string path = "MonoBehaviourEditor-PropertyHide-"+this.target.GetInstanceID()+"-"+property.propertyPath;
									EditorPref.Set<bool>(path,true);
									this.hidden.Add(property);
							}
							if(this.propertyArea[property].Clicked(1)){this.DrawMenu();}
						}
						if(fastInspector && this.propertyVisible.ContainsKey(property) && !this.propertyVisible[property]){
							GUILayout.Space(this.propertyArea[property].height);
							continue;
						}
					}
					string propertyName = null;
					if(isReadOnly){GUI.enabled = false;}
					GUI.changed = false;
					EditorGUILayout.BeginVertical();
					if(hasArea){EditorGUI.BeginProperty(this.propertyArea[property],new GUIContent(property.displayName),property);}
					property.Draw(propertyName);
					if(hasArea){EditorGUI.EndProperty();}
					EditorGUILayout.EndVertical();
					this.changed = this.changed || GUI.changed;
					if(isReadOnly){GUI.enabled = true;}
					if(Proxy.IsRepainting()){
						Rect area = GUILayoutUtility.GetLastRect();
						if(!area.IsEmpty()){this.propertyArea[property] = area.AddHeight(2);}
					}
				}
			}
			if(showDictionary){
				GUI.enabled = false;
				foreach(var item in this.dictionaries){
					item.Value.DrawAuto(item.Key,null,true);
				}
				GUI.enabled = true;
			}
			EditorGUILayout.EndVertical();
			this.EndArea();
			if(this.changed){
				this.serializedObject.ApplyModifiedProperties();
				//this.serializedObject.targetObject.CallMethod("OnValidate");
				ProxyEditor.SetDirty(this.serializedObject.targetObject,false,true);
			}
			this.CheckChanges();
			if(Proxy.IsRepainting()){
				ProxyEditor.GetInspector(this).SetTitle("Inspector");
			}
		}
		public void CheckChanges(){
			if(Proxy.IsRepainting()){
				bool fastInspector = EditorPref.Get<bool>("MonoBehaviourEditor-FastInspector");
				Vector2 mousePosition = Event.current.mousePosition;
				this.showAll = Event.current.alt && this.area.Contains(mousePosition);
				if(this.dirtyEvent != null){
					this.dirtyEvent();
					this.dirtyEvent = null;
				}
				bool needsRepaint = false;
				if(fastInspector){
					foreach(var property in this.properties){
						if(this.propertyArea.ContainsKey(property)){
							bool valid = this.propertyArea[property].InInspectorWindow();
							if(valid != this.propertyVisible.AddNew(property)){
								this.propertyVisible[property] = valid;
								needsRepaint = true;
							}
						}
					}
					/*if(!this.area.IsEmpty() && !this.area.InInspectorWindow()){
						MonoBehaviourEditor.offScreen[this] = true;
					}*/
				}
				if(this.showAll || needsRepaint){this.Repaint();}
			}
		}
		public void BeginArea(){
			//if(this.areaBegan){return;}
			Rect areaStart = GUILayoutUtility.GetRect(0,0);
			if(!areaStart.IsEmpty() && this.areaStart != areaStart){
				this.AddDirty(()=>{
					this.areaStart = areaStart;
					this.propertyArea.Clear();
					MonoBehaviourEditor.offScreen.Clear();
					this.Repaint();
				});
			}
			this.areaBegan = true;
		}
		public void EndArea(){
			Rect areaEnd = GUILayoutUtility.GetRect(0,0);
			if(!areaEnd.IsEmpty() && this.areaEnd != areaEnd){
				this.AddDirty(()=>{
					this.area = this.areaStart;
					this.area.height = (areaEnd.y - this.areaStart.y);
				});
			}
			this.areaBegan = false;
		}
		public void Setup(){
			if(this.properties.Count > 0 && !this.setup){
				foreach(var property in this.properties){
					string path = "MonoBehaviourEditor-PropertyHide-"+this.target.GetInstanceID()+"-"+property.propertyPath;
					if(EditorPref.Get<bool>(path,false)){
						this.hidden.Add(property);
					}
				}
				this.setup = true;
			}
		}
		public void SortDefaults(){
			Type type = this.target.GetType();
			var defaults = MonoBehaviourEditor.defaults;
			if(!(this.target is MonoBehaviour)){return;}
			if(!defaults.ContainsKey(type)){
				Events.Pause("On Hierarchy Changed");
				var state = Events.disabled;
				Events.disabled = (EventDisabled)(-1);
				defaults.AddNew(type);
				var script = (MonoBehaviour)this.target;
				var component = script.gameObject.AddComponent(type);
				foreach(var item in component.GetVariables()){
					try{
						string name = item.Key;
						object defaultValue = item.Value;
						defaults[type][name] = defaultValue;
					}
					catch{}
				}
				component.Destroy();
				Events.disabled = state;
				MonoBehaviourEditor.resumeHierarchyTime = Time.Get() + 0.5f;
			}
			else if(MonoBehaviourEditor.resumeHierarchyTime != -1 && Time.Get() > MonoBehaviourEditor.resumeHierarchyTime){
				MonoBehaviourEditor.resumeHierarchyTime = -1;
				Events.Resume("On Hierarchy Changed");
			}
		}
		public void SortProperties(){
			if(this.properties.Count < 1){
				var target = this.serializedObject.targetObject;
				if(target.IsNull()){return;}
				var property = this.serializedObject.GetIterator();
				property.NextVisible(true);
				while(property.NextVisible(false)){
					var realProperty = this.serializedObject.FindProperty(property.propertyPath);
					this.properties.Add(realProperty);
				}
				this.properties = this.properties.OrderBy(x=>target.HasAttribute(x.name,typeof(InternalAttribute))).ToList();
				foreach(var item in target.GetVariables(null,Reflection.publicFlags)){
					if(item.Value == null){continue;}
					var type = item.Value.GetType();
					if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)){
						this.dictionaries[item.Key] = item.Value;
					}
				}
			}
		}
		public void AddDirty(Method method){this.dirtyEvent += method;}
		public void DrawMenu(){
			GenericMenu menu = new GenericMenu();
			MenuFunction toggleAdvanced = ()=>EditorPref.Toggle("MonoBehaviourEditor-Advanced");
			MenuFunction toggleInternal = ()=>EditorPref.Toggle("MonoBehaviourEditor-Internal");
			MenuFunction toggleDictionary = ()=>EditorPref.Toggle("MonoBehaviourEditor-Dictionary");
			MenuFunction hideAllDefaults = ()=>EditorPref.Toggle("MonoBehaviourEditor-HideAllDefault");
			MenuFunction hideLocalDefaults = ()=>{
				this.hideDefault = !this.hideDefault;
				EditorPref.Set<bool>("MonoBehaviourEditor-"+this.target.GetInstanceID()+"HideDefault",this.hideDefault);
			};
			menu.AddItem(new GUIContent("Advanced"),EditorPref.Get<bool>("MonoBehaviourEditor-Advanced"),toggleAdvanced);
			menu.AddItem(new GUIContent("Internal"),EditorPref.Get<bool>("MonoBehaviourEditor-Internal"),toggleInternal);
			menu.AddItem(new GUIContent("Dictionary"),EditorPref.Get<bool>("MonoBehaviourEditor-Dictionary"),toggleDictionary);
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Defaults/Hide All"),EditorPref.Get<bool>("MonoBehaviourEditor-HideAllDefault"),hideAllDefaults);
			menu.AddItem(new GUIContent("Defaults/Hide Local"),this.hideDefault,hideLocalDefaults);
			if(this.hidden.Count > 0){
				MenuFunction unhideAll = ()=>{
					foreach(var property in this.hidden){
						string path = "MonoBehaviourEditor-PropertyHide-"+this.target.GetInstanceID()+"-"+property.propertyPath;
						EditorPref.Set<bool>(path,false);
					}
					this.hidden.Clear();
				};
				menu.AddSeparator("");
				menu.AddItem(new GUIContent("Unhide/All"),false,unhideAll);
				foreach(var property in this.hidden){
					SerializedProperty target = property;
					MenuFunction unhide = ()=>{
						string path = "MonoBehaviourEditor-PropertyHide-"+this.target.GetInstanceID()+"-"+property.propertyPath;
						EditorPref.Set<bool>(path,false);
						this.hidden.Remove(target);
					};
					menu.AddItem(new GUIContent("Unhide/"+property.displayName),false,unhide);
				}
			}
			menu.ShowAsContext();
			Event.current.Use();
		}
	}
}