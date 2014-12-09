using Zios;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using MenuFunction = UnityEditor.GenericMenu.MenuFunction;
using UnityObject = UnityEngine.Object;
namespace Zios{
	[CustomPropertyDrawer(typeof(Attribute),true)]
	public class AttributeDrawer : PropertyDrawer{
		public IAttributeAccess access;
		public new Attribute attribute;
		public float overallHeight;
		public bool isPrefab;
		public override float GetPropertyHeight(SerializedProperty property,GUIContent label){
			this.OnGUI(new Rect(-10000,-10000,0,0),property,label);
			return this.overallHeight;
		}
		public override void OnGUI(Rect area,SerializedProperty property,GUIContent label){
			if(this.attribute == null){
				this.attribute = property.GetObject<Attribute>();
				MonoBehaviour script = (MonoBehaviour)property.serializedObject.targetObject;
				this.isPrefab = script.IsPrefab();
			}
			if(!this.attribute.showInEditor){
				this.overallHeight = -2;
				return;
			}
			if(this.isPrefab){
				/*this.overallHeight = EditorGUIUtility.singleLineHeight;
				//base.OnGUI(area,property,label);
				Type utility = Utility.GetEditorType("ScriptAttributeUtility");
				utility.GetVariable<Stack<PropertyDrawer>>("s_DrawerStack").Pop();
				base.OnGUI(area,property,label);
				utility.GetVariable<Stack<PropertyDrawer>>("s_DrawerStack").Push(this);*/
				return;
			}
			if(!Attribute.ready || !area.InspectorValid()){return;}
			this.overallHeight = base.GetPropertyHeight(property,label);
			if(this.access == null){
				if(this.attribute is AttributeFloat){this.access = new AttributeAccess<float,AttributeFloat,AttributeFloatData,SpecialNumeral>();}
				if(this.attribute is AttributeInt){this.access = new AttributeAccess<int,AttributeInt,AttributeIntData,SpecialNumeral>();}
				if(this.attribute is AttributeString){this.access = new AttributeAccess<string,AttributeString,AttributeStringData,SpecialString>();}
				if(this.attribute is AttributeBool){this.access = new AttributeAccess<bool,AttributeBool,AttributeBoolData,SpecialBool>();}
				if(this.attribute is AttributeVector3){this.access = new AttributeAccess<Vector3,AttributeVector3,AttributeVector3Data,SpecialVector3>();}
				if(this.attribute is AttributeGameObject){this.access = new AttributeAccess<GameObject,AttributeGameObject,AttributeGameObjectData,SpecialGameObject>();}
			}
			if(this.access != null){
				this.access.Setup(this,area,property,label);
			}
		}
	}
	public interface IAttributeAccess{
		void Setup(AttributeDrawer drawer,Rect area,SerializedProperty property,GUIContent label);
	}
	public class AttributeAccess<BaseType,AttributeType,DataType,Special> : IAttributeAccess
		where Special  : struct
		where AttributeType     : Attribute<BaseType,AttributeType,DataType,Special>,new()
		where DataType : AttributeData<BaseType,AttributeType,DataType,Special>,new(){
		public Attribute attribute;
		public AttributeType attributeCast;
		public AttributeDrawer drawer;
		public SerializedProperty property;
		public GUIContent label;
		public Rect fullRect;
		public Rect labelRect;
		public Rect valueRect;
		public Rect iconRect;
		public bool contextOpen;
		public GUISkin skin;
		public Dictionary<AttributeData,bool> targetMode = new Dictionary<AttributeData,bool>();
		public void Setup(AttributeDrawer drawer,Rect area,SerializedProperty property,GUIContent label){
			if(skin == null){
				string skinName = EditorGUIUtility.isProSkin ? "Dark" : "Light";
				this.skin = FileManager.GetAsset<GUISkin>("Gentleface-" + skinName + ".guiskin");
				this.attribute = property.GetObject<Attribute>();
				this.attributeCast = (AttributeType)this.attribute;
			}
			GUI.skin = this.skin;
			this.drawer = drawer;
			this.property = property;
			this.label = label;
			this.fullRect = area.SetHeight(EditorGUIUtility.singleLineHeight);
			this.iconRect = this.fullRect.SetSize(14,14);
			this.labelRect = this.fullRect.SetWidth(EditorGUIUtility.labelWidth);
			this.valueRect = this.fullRect.Add(labelRect.width,0,-labelRect.width,0);
			this.iconRect = this.fullRect.SetSize(14,14);
			List<UnityObject> sources = new List<UnityObject>(){property.serializedObject.targetObject};
			foreach(var data in this.attribute.data){
				if(!data.IsNull()){sources.Add(data);}
			}
			GUI.changed = false;
			Undo.RecordObjects(sources.ToArray(),"Attribute Changes");
			EditorGUI.BeginProperty(area,label,property);
			this.Draw();
			EditorGUI.EndProperty();
			if(GUI.changed){
				property.serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(sources[0]);
				//property.serializedObject.UpdateIfDirtyOrScript();
				if(EditorWindow.mouseOverWindow != null){
					EditorWindow.mouseOverWindow.Repaint();
				}
			}
		}
		public void Draw(){
			DataType firstData = this.attributeCast.GetFirst();
			if(firstData.IsNull()){return;}
			SerializedObject firstProperty = new SerializedObject(firstData);
			this.DrawContext(firstData);
			if(this.attribute.info.mode == AttributeMode.Normal){
				if(firstData.usage == AttributeUsage.Direct){
					this.DrawDirect(firstData,this.label);
				}
				if(firstData.usage == AttributeUsage.Shaped){
					GUI.Box(this.iconRect,"",GUI.skin.GetStyle("IconShaped"));
					this.labelRect = this.labelRect.AddX(16);
					this.DrawShaped(firstProperty,this.label,true);
				}
				if(GUI.changed){Utility.SetDirty(firstData);}
			}
			if(this.attribute.info.mode == AttributeMode.Linked){
				this.attributeCast.usage = AttributeUsage.Shaped;
				GUI.Box(this.iconRect,"",GUI.skin.GetStyle("IconLinked"));
				this.labelRect = this.labelRect.AddX(16);
				this.DrawShaped(firstProperty,this.label);
				if(GUI.changed){Utility.SetDirty(firstData);}
			}
			if(this.attribute.info.mode == AttributeMode.Formula){
				this.DrawFormula(this.label);
			}
		}
		public void DrawDirect(AttributeData data,GUIContent label,bool? drawSpecial=null,bool? drawOperator=null){
			float labelSize = EditorGUIUtility.labelWidth;
			Rect comboRect = this.fullRect;
			Rect extraRect = this.valueRect;
			EditorGUIUtility.labelWidth = this.labelRect.width;
			if(drawOperator != null){EditorGUIUtility.labelWidth += 81;}
			if(drawSpecial != null){EditorGUIUtility.labelWidth += 81;}
			if(drawOperator != null){
				this.DrawOperator(extraRect,data,(bool)!drawOperator);
				extraRect = extraRect.Add(81,0,-81,0);
			}
			if(drawSpecial != null){
				this.DrawSpecial(extraRect,data);
			}
			if(data is AttributeFloatData){
				AttributeFloatData floatData = (AttributeFloatData)data;
				floatData.value = floatData.value.DrawLabeled(comboRect,label);
			}
			if(data is AttributeIntData){
				AttributeIntData intData = (AttributeIntData)data;
				intData.value = intData.value.DrawLabeledInt(comboRect,label);
			}
			if(data is AttributeStringData){
				AttributeStringData stringData = (AttributeStringData)data;
				stringData.value = stringData.value.DrawLabeled(comboRect,label);
			}
			if(data is AttributeBoolData){
				AttributeBoolData boolData = (AttributeBoolData)data;
				boolData.value = boolData.value.DrawLabeled(comboRect,label);
			}
			if(data is AttributeVector3Data){
				AttributeVector3Data vector3Data = (AttributeVector3Data)data;
				vector3Data.value = vector3Data.value.DrawLabeled(comboRect,label);
			}
			EditorGUIUtility.labelWidth = labelSize;
		}
		public void DrawShaped(SerializedObject property,GUIContent label,bool? drawSpecial=null,bool? drawOperator=null){
			label.DrawLabel(labelRect);
			AttributeData data = (AttributeData)property.targetObject;
			Target target = data.target;
			Rect toggleRect = this.valueRect.SetWidth(16);
			bool toggleActive = this.targetMode.ContainsKey(data) ? this.targetMode[data] : !data.referenceID.IsEmpty();
			this.targetMode[data] = toggleActive.Draw(toggleRect,GUI.skin.GetStyle("CheckmarkToggle"));
			if(toggleActive != this.targetMode[data]){
				if(this.attribute is AttributeGameObject){
					data.referenceID = toggleActive ? "" : data.referenceID;
				}
			}
			if(!this.targetMode[data]){
				Rect targetRect = this.valueRect.Add(18,0,-18,0);
				property.FindProperty("target").Draw(targetRect);
				return;
			}
			List<string> attributeNames = new List<string>();
			List<string> attributeIDs = new List<string>();
			int attributeIndex = -1;
			if(target.direct != null && Attribute.lookup.ContainsKey(target.direct)){
				var lookup = Attribute.lookup[target.direct];
				foreach(var item in lookup){
					if(item.Value.info.dataType != data.GetType()){continue;}
					bool feedback = (item.Value.info.id == this.attribute.info.id || item.Value.data[0].referenceID == this.attribute.info.id);
					if(!feedback){
						attributeNames.Add(item.Value.info.path);
					}
				}
				attributeNames = attributeNames.Order().OrderBy(item=>item.Contains("/")).ToList();
				foreach(string name in attributeNames){
					string id = lookup.Values.ToList().Find(x=>x.info.path == name).info.id;
					attributeIDs.Add(id);
				}
				if(!data.referenceID.IsEmpty()){
					attributeIndex = attributeIDs.IndexOf(data.referenceID);
				}
			}
			if(attributeNames.Count > 0){
				Rect line = this.valueRect;
				if(attributeIndex == -1){
					string message = data.referenceID.IsEmpty() ? "[Not Set]" : "[Missing] " + data.referencePath;
					attributeIndex = 0;
					attributeNames.Insert(0,message);
					attributeIDs.Insert(0,"0");
				}
				if(drawOperator != null){
					this.DrawOperator(line,data,(bool)!drawOperator);
					line = line.Add(81,0,-81,0);
				}
				if(drawSpecial != null){
					this.DrawSpecial(line,data);
					line = line.Add(61,0,-61,0);
				}
				Rect attributeRect = line.Add(18,0,-18,0);
				int previousIndex = attributeIndex;
				attributeIndex = attributeNames.Draw(attributeRect,attributeIndex);
				string name = attributeNames[attributeIndex];
				string id = attributeIDs[attributeIndex];
				if(attributeIndex != previousIndex){
					data.referencePath = name;
					data.referenceID = id;
				}
			}
			else{
				Rect warningRect = this.valueRect.Add(18,0,-18,0);
				string targetName = target.direct == null ? "Target" : target.direct.ToString().Strip("(UnityEngine.GameObject)").Trim();
				string typeName = data.GetVariableType("value").Name.Replace("Single","Float").Replace("Int32","Int");
				string message = "<b>" + targetName.Truncate(16) + "</b> has no <b>"+typeName+"</b> attributes.";
				message.DrawLabel(warningRect,GUI.skin.GetStyle("WarningLabel"));
			}
		}
		public void DrawSpecial(Rect area,AttributeData data){
			Rect specialRect = area.Add(18,0,0,0).SetWidth(60);
			List<string> specialList = new List<string>();
			int specialIndex = 0;
			Enum special = data.GetVariable<Enum>("special");
			Type specialType = special.GetType();
			string specialName = Enum.GetName(specialType,special);
			specialList = Enum.GetNames(specialType).ToList();
			specialIndex = specialList.IndexOf(specialName);
			if(specialIndex == -1){specialIndex = 0;}
			specialIndex = specialList.Draw(specialRect,specialIndex);
			data.SetVariable("special",Enum.GetValues(specialType).GetValue(specialIndex));
		}
		public void DrawOperator(Rect area,AttributeData data,bool disabled=false){
			Rect operatorRect = area.Add(18,0,0,0).SetWidth(80);
			EditorGUIUtility.AddCursorRect(operatorRect,MouseCursor.Arrow);
			GUIStyle style = new GUIStyle(EditorStyles.popup);
			style.alignment = TextAnchor.MiddleRight;
			style.contentOffset = new Vector2(-3,0);
			style.fontStyle = FontStyle.Bold;
			List<string> operatorList = new List<string>();
			if(disabled){
				GUI.enabled = false;
				operatorList.Add("=");
				operatorList.Draw(operatorRect,0,style);
				GUI.enabled = true;
				return;
			}
			var operatorCollection = typeof(AttributeType).GetVariable<Dictionary<Type,string[]>>("compare");
			operatorList = operatorCollection[data.GetType()].ToList();
			int operatorIndex = Mathf.Clamp(data.sign,0,operatorList.Count-1);
			data.sign = operatorList.Draw(operatorRect,operatorIndex,style);
		}
		public void DrawFormula(GUIContent label){
			Rect labelRect = this.labelRect.AddX(12);
			EditorGUIUtility.AddCursorRect(this.fullRect,MouseCursor.ArrowPlus);
			bool formulaExpanded = EditorPrefs.GetBool(this.attribute.info.path+"FormulaExpanded");
			if(this.labelRect.AddX(16).Clicked() || this.valueRect.Clicked()){
				GUI.changed = true;
				formulaExpanded = !formulaExpanded;
			}
			formulaExpanded = EditorGUI.Foldout(labelRect,formulaExpanded,label,GUI.skin.GetStyle("IconFormula"));
			EditorPrefs.SetBool(this.attribute.info.path+"FormulaExpanded",formulaExpanded);
			if(formulaExpanded){
				float lineHeight = EditorGUIUtility.singleLineHeight+2;
				this.fullRect = this.fullRect.SetX(45).AddWidth(-55);
				this.labelRect = this.labelRect.SetX(45).SetWidth(25);
				this.valueRect = this.valueRect.SetX(70).SetWidth(this.fullRect.width);
				for(int index=0;index<this.attribute.data.Length;++index){
					AttributeData currentData = this.attribute.data[index];
					if(currentData == null){continue;}
					SerializedObject currentProperty = new SerializedObject(currentData);
					GUIContent formulaLabel = new GUIContent("#"+(index+1));
					this.fullRect = this.fullRect.AddY(lineHeight);
					this.labelRect = this.labelRect.AddY(lineHeight);
					this.valueRect.y += lineHeight;
					this.drawer.overallHeight += lineHeight;
					bool? operatorState = index == 0 ? (bool?)false : (bool?)true;
					if(currentData.usage == AttributeUsage.Direct){
						this.fullRect = this.fullRect.AddWidth(25);
						this.DrawDirect(currentData,formulaLabel,true,operatorState);
						this.fullRect = this.fullRect.AddWidth(-25);
					}
					else if(currentData.usage == AttributeUsage.Shaped){
						this.DrawShaped(currentProperty,formulaLabel,true,operatorState);
					}
					if(GUI.changed){Utility.SetDirty(currentData);}
					this.DrawContext(currentData,false,index!=0);
				}
				this.labelRect.y += lineHeight;
				this.drawer.overallHeight += lineHeight;
				if(GUI.Button(this.labelRect.SetWidth(100),"Add Attribute")){
					if(this.attribute.GetFormulaTypes().Length > 1){
						this.DrawAddMenu();
						return;
					}
					this.attribute.Add<DataType>();
					GUI.changed = true;
				}
			}
			else{
				string message = "[expand for details]";
				message.DrawLabel(this.valueRect,GUI.skin.GetStyle("WarningLabel"));
			}
		}
		public void DrawAddMenu(){
			GenericMenu menu = new GenericMenu();
			foreach(Type AttributeType in this.attribute.GetFormulaTypes()){
				string name = AttributeType.Name.Strip("Attribute","Data");
				MethodInfo generic = this.attribute.GetType().GetMethod("Add",new Type[]{}).MakeGenericMethod(AttributeType);
				MenuFunction method = ()=>{generic.Invoke(attribute,null);};
				menu.AddItem(new GUIContent(name),false,method);
			}
			menu.ShowAsContext();
		}
		public void DrawContext(AttributeData data,bool showMode=true,bool showRemove=false){
			if(this.labelRect.Clicked(1)){
				this.contextOpen = true;
				GenericMenu menu = new GenericMenu();
				AttributeMode mode = this.attribute.info.mode;
				AttributeUsage usage = data.usage;
				MenuFunction removeAttribute = ()=>{this.attribute.Remove(data);};
				MenuFunction modeNormal  = ()=>{this.attribute.info.mode = AttributeMode.Normal;};
				MenuFunction modeLinked  = ()=>{this.attribute.info.mode = AttributeMode.Linked;};
				MenuFunction modeFormula = ()=>{this.attribute.info.mode = AttributeMode.Formula;};
				MenuFunction usageDirect = ()=>{data.usage = AttributeUsage.Direct;};
				MenuFunction usageShaped = ()=>{data.usage = AttributeUsage.Shaped;};
				bool normal = this.attribute.info.mode == AttributeMode.Normal;
				if(this.attribute.locked){
					menu.AddDisabledItem(new GUIContent("Attribute Locked"));
					menu.ShowAsContext();
					return;
				}
				if(showMode){
					string directPath = this.attribute.canShape ? "Normal/Direct" : "Direct";
					string shapedPath = this.attribute.canDirect ? "Normal/Shaped" : "Shaped";
					if(this.attribute.canDirect){menu.AddItem(new GUIContent(directPath),normal&&(usage==AttributeUsage.Direct),modeNormal+usageDirect);}
					if(this.attribute.canShape){menu.AddItem(new GUIContent(shapedPath),normal&&(usage==AttributeUsage.Shaped),modeNormal+usageShaped);}
					if(this.attribute.canLink){menu.AddItem(new GUIContent("Linked"),(mode==AttributeMode.Linked),modeLinked+usageShaped);}
					if(this.attribute.canFormula){menu.AddItem(new GUIContent("Formula"),(mode==AttributeMode.Formula),modeFormula);}
				}
				else{
					if(this.attribute.canDirect){menu.AddItem(new GUIContent("Direct"),normal&&(usage==AttributeUsage.Direct),usageDirect);}
					if(this.attribute.canShape){menu.AddItem(new GUIContent("Shaped"),normal&&(usage==AttributeUsage.Shaped),usageShaped);}
				}
				if(showRemove){
					menu.AddItem(new GUIContent("Remove"),false,removeAttribute);	
				}
				menu.ShowAsContext();
			}
			if(this.contextOpen && Event.current.button == 0){
				GUI.changed = true;
				this.ForceUpdate();
				this.contextOpen = false;
			}
		}
		public void ForceUpdate(){
			SerializedProperty forceUpdate = property.FindPropertyRelative("info").FindPropertyRelative("path");
			string path = forceUpdate.stringValue;
			forceUpdate.stringValue = "";
			forceUpdate.stringValue = path;
		}
	}
}