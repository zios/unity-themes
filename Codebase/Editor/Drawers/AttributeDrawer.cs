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
		public float overallHeight;
		public override float GetPropertyHeight(SerializedProperty property,GUIContent label){
			this.OnGUI(new Rect(-10000,-10000,0,0),property,label);
			return this.overallHeight;
		}
		public override void OnGUI(Rect area,SerializedProperty property,GUIContent label){
			if(!property.GetObject<Attribute>().showInEditor){
				this.overallHeight = -2;
				return;
			}
			if(!Attribute.ready || !area.HierarchyValid()){return;}
			this.overallHeight = base.GetPropertyHeight(property,label);
			if(this.access == null){
				object generic = property.GetObject<object>();
				if(generic is AttributeFloat){this.access = new AttributeAccess<float,AttributeFloat,AttributeFloatData,OperatorNumeral,SpecialNumeral>();}
				if(generic is AttributeInt){this.access = new AttributeAccess<int,AttributeInt,AttributeIntData,OperatorNumeral,SpecialNumeral>();}
				if(generic is AttributeString){this.access = new AttributeAccess<string,AttributeString,AttributeStringData,OperatorString,SpecialString>();}
				if(generic is AttributeBool){this.access = new AttributeAccess<bool,AttributeBool,AttributeBoolData,OperatorBool,SpecialBool>();}
				if(generic is AttributeVector3){this.access = new AttributeAccess<Vector3,AttributeVector3,AttributeVector3Data,OperatorVector3,SpecialVector3>();}
				if(generic is AttributeGameObject){this.access = new AttributeAccess<GameObject,AttributeGameObject,AttributeGameObjectData,OperatorGameObject,SpecialGameObject>();}
			}
			if(this.access != null){
				this.access.Setup(this,area,property,label);
			}
		}
	}
	public interface IAttributeAccess{
		void Setup(AttributeDrawer drawer,Rect area,SerializedProperty property,GUIContent label);
	}
	public class AttributeAccess<BaseType,AttributeType,DataType,Operator,Special> : IAttributeAccess
		where Operator : struct
		where Special  : struct
		where AttributeType     : Attribute<BaseType,AttributeType,DataType,Operator,Special>,new()
		where DataType : AttributeData<BaseType,AttributeType,DataType,Operator,Special>,new(){
		public AttributeDrawer drawer;
		public SerializedProperty property;
		public GUIContent label;
		public Rect fullRect;
		public Rect labelRect;
		public Rect valueRect;
		public Rect iconRect;
		public bool contextOpen;
		public Dictionary<AttributeData,bool> targetMode = new Dictionary<AttributeData,bool>();
		public void Setup(AttributeDrawer drawer,Rect area,SerializedProperty property,GUIContent label){
			string skinName = EditorGUIUtility.isProSkin ? "Dark" : "Light";
			GUI.skin = FileManager.GetAsset<GUISkin>("Gentleface-" + skinName + ".guiskin");
			this.drawer = drawer;
			this.property = property;
			this.label = label;
			this.fullRect = area.SetHeight(EditorGUIUtility.singleLineHeight);
			this.iconRect = this.fullRect.SetSize(14,14);
			this.labelRect = this.fullRect.SetWidth(EditorGUIUtility.labelWidth);
			this.valueRect = this.fullRect.Add(labelRect.width,0,-labelRect.width,0);
			this.iconRect = this.fullRect.SetSize(14,14);
			List<UnityObject> sources = new List<UnityObject>(){property.serializedObject.targetObject};
			foreach(var data in this.property.GetObject<Attribute>().data){
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
			AttributeType attribute = this.property.GetObject<AttributeType>();
			DataType firstData = attribute.GetFirst();
			if(firstData.IsNull()){return;}
			SerializedObject firstProperty = new SerializedObject(firstData);
			this.DrawContext(attribute,firstData);
			if(attribute.mode == AttributeMode.Normal){
				if(firstData.usage == AttributeUsage.Direct){
					this.DrawDirect(firstData,this.label);
				}
				if(firstData.usage == AttributeUsage.Shaped){
					GUI.Box(this.iconRect,"",GUI.skin.GetStyle("IconShaped"));
					this.labelRect = this.labelRect.AddX(16);
					this.DrawShaped(attribute,firstProperty,this.label,true);
				}
				if(GUI.changed){Utility.SetDirty(firstData);}
			}
			if(attribute.mode == AttributeMode.Linked){
				attribute.usage = AttributeUsage.Shaped;
				GUI.Box(this.iconRect,"",GUI.skin.GetStyle("IconLinked"));
				this.labelRect = this.labelRect.AddX(16);
				this.DrawShaped(attribute,firstProperty,this.label);
				if(GUI.changed){Utility.SetDirty(firstData);}
			}
			if(attribute.mode == AttributeMode.Formula){
				this.DrawFormula(attribute,this.label);
			}
		}
		public void DrawDirect(AttributeData data,GUIContent label,bool? drawSpecial=null,bool? drawOperator=null){
			float labelSize = EditorGUIUtility.labelWidth;
			Rect comboRect = this.fullRect;
			Rect extraRect = this.valueRect;
			if(drawOperator != null){EditorGUIUtility.labelWidth -= 24;}
			if(drawSpecial != null){EditorGUIUtility.labelWidth -= 24;}
			//label.DrawLabel(this.labelRect);
			if(data is AttributeFloatData){
				AttributeFloatData floatData = (AttributeFloatData)data;
				//floatData.value = floatData.value.Draw(comboRect);
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
			if(drawOperator != null){
				this.DrawOperator(extraRect,data,(bool)!drawOperator);
				extraRect = extraRect.Add(81,0,-81,0);
			}
			if(drawSpecial != null){
				this.DrawSpecial(extraRect,data);
			}
			EditorGUIUtility.labelWidth = labelSize;
		}
		public void DrawShaped(AttributeType attribute,SerializedObject property,GUIContent label,bool? drawSpecial=null,bool? drawOperator=null){
			label.DrawLabel(labelRect);
			AttributeData data = (AttributeData)property.targetObject;
			Target target = data.target;
			Rect toggleRect = this.valueRect.SetWidth(16);
			bool toggleActive = this.targetMode.ContainsKey(data) ? this.targetMode[data] : !data.referenceID.IsEmpty();
			this.targetMode[data] = toggleActive.Draw(toggleRect,GUI.skin.GetStyle("CheckmarkToggle"));
			if(toggleActive != this.targetMode[data]){
				if(attribute is AttributeGameObject){
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
					if(!item.Value.dataType.IsType(data.GetType())){continue;}
					bool feedback = item.Value.id == attribute.id || item.Value.data[0].referenceID == attribute.id;
					if(!feedback){
						attributeNames.Add(item.Value.path);
					}
				}
				attributeNames = attributeNames.Order().OrderBy(item=>item.Contains("/")).ToList();
				foreach(string name in attributeNames){
					string id = lookup.Values.ToList().Find(x=>x.path == name).id;
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
			Enum sign = data.GetVariable<Enum>("sign");	
			Type signType = sign.GetType();
			operatorList = Enum.GetNames(signType).ToList();
			string operatorName = Enum.GetName(signType,sign);
			int operatorIndex = operatorList.IndexOf(operatorName);
			if(operatorIndex == -1){operatorIndex = 0;}
			for(int index=0;index<operatorList.Count;++index){
				string operatorAlias = operatorList[index];
				if(operatorAlias.Contains("Add")){operatorAlias="+";}
				if(operatorAlias.Contains("Sub")){operatorAlias="-";}
				if(operatorAlias.Contains("Mul")){operatorAlias="×";}
				if(operatorAlias.Contains("Div")){operatorAlias="÷";}
				operatorList[index] = operatorAlias;
			}
			operatorIndex = operatorList.Draw(operatorRect,operatorIndex,style);
			data.SetVariable("sign",Enum.GetValues(signType).GetValue(operatorIndex));
		}
		public void DrawFormula(AttributeType attribute,GUIContent label){
			Rect labelRect = this.labelRect.AddX(12);
			EditorGUIUtility.AddCursorRect(this.fullRect,MouseCursor.ArrowPlus);
			bool formulaExpanded = EditorPrefs.GetBool(attribute.path+"FormulaExpanded");
			if(this.labelRect.AddX(16).Clicked() || this.valueRect.Clicked()){
				GUI.changed = true;
				formulaExpanded = !formulaExpanded;
			}
			formulaExpanded = EditorGUI.Foldout(labelRect,formulaExpanded,label,GUI.skin.GetStyle("IconFormula"));
			EditorPrefs.SetBool(attribute.path+"FormulaExpanded",formulaExpanded);
			if(formulaExpanded){
				float lineHeight = EditorGUIUtility.singleLineHeight+2;
				this.fullRect = this.fullRect.SetX(45).AddWidth(-55);
				this.labelRect = this.labelRect.SetX(45).SetWidth(25);
				this.valueRect = this.valueRect.SetX(70).SetWidth(this.fullRect.width);
				for(int index=0;index<attribute.data.Length;++index){
					AttributeData currentData = attribute.data[index];
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
						this.DrawShaped(attribute,currentProperty,formulaLabel,true,operatorState);
					}
					if(GUI.changed){Utility.SetDirty(currentData);}
					this.DrawContext(attribute,currentData,false,index!=0);
				}
				this.labelRect.y += lineHeight;
				this.drawer.overallHeight += lineHeight;
				if(GUI.Button(this.labelRect.SetWidth(100),"Add Attribute")){
					if(attribute.GetFormulaTypes().Length > 1){
						this.DrawAddMenu(attribute);
						return;
					}
					attribute.Add<DataType>();
					GUI.changed = true;
				}
			}
			else{
				string message = "[expand for details]";
				message.DrawLabel(this.valueRect,GUI.skin.GetStyle("WarningLabel"));
			}
		}
		public void DrawAddMenu(Attribute attribute){
			GenericMenu menu = new GenericMenu();
			foreach(Type AttributeType in attribute.GetFormulaTypes()){
				string name = AttributeType.Name.Strip("Attribute","Data");
				MethodInfo generic = attribute.GetType().GetMethod("Add",new Type[]{}).MakeGenericMethod(AttributeType);
				MenuFunction method = ()=>{generic.Invoke(attribute,null);};
				menu.AddItem(new GUIContent(name),false,method);
			}
			menu.ShowAsContext();
		}
		public void DrawContext(Attribute attribute,AttributeData data,bool showMode=true,bool showRemove=false){
			if(this.labelRect.Clicked(1)){
				this.contextOpen = true;
				GenericMenu menu = new GenericMenu();
				AttributeMode mode = attribute.mode;
				AttributeUsage usage = data.usage;
				MenuFunction removeAttribute = ()=>{attribute.Remove(data);};
				MenuFunction modeNormal  = ()=>{attribute.mode = AttributeMode.Normal;};
				MenuFunction modeLinked  = ()=>{attribute.mode = AttributeMode.Linked;};
				MenuFunction modeFormula = ()=>{attribute.mode = AttributeMode.Formula;};
				MenuFunction usageDirect = ()=>{data.usage = AttributeUsage.Direct;};
				MenuFunction usageShaped = ()=>{data.usage = AttributeUsage.Shaped;};
				bool normal = attribute.mode == AttributeMode.Normal;
				if(attribute.locked){
					menu.AddDisabledItem(new GUIContent("Attribute Locked"));
					menu.ShowAsContext();
					return;
				}
				if(showMode){
					string directPath = attribute.canShape ? "Normal/Direct" : "Direct";
					string shapedPath = attribute.canDirect ? "Normal/Shaped" : "Shaped";
					if(attribute.canDirect){menu.AddItem(new GUIContent(directPath),normal&&(usage==AttributeUsage.Direct),modeNormal+usageDirect);}
					if(attribute.canShape){menu.AddItem(new GUIContent(shapedPath),normal&&(usage==AttributeUsage.Shaped),modeNormal+usageShaped);}
					if(attribute.canLink){menu.AddItem(new GUIContent("Linked"),(mode==AttributeMode.Linked),modeLinked+usageShaped);}
					if(attribute.canFormula){menu.AddItem(new GUIContent("Formula"),(mode==AttributeMode.Formula),modeFormula);}
				}
				else{
					if(attribute.canDirect){menu.AddItem(new GUIContent("Direct"),normal&&(usage==AttributeUsage.Direct),usageDirect);}
					if(attribute.canShape){menu.AddItem(new GUIContent("Shaped"),normal&&(usage==AttributeUsage.Shaped),usageShaped);}
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
			SerializedProperty forceUpdate = property.FindPropertyRelative("path");
			string path = forceUpdate.stringValue;
			forceUpdate.stringValue = "";
			forceUpdate.stringValue = path;
		}
	}
}