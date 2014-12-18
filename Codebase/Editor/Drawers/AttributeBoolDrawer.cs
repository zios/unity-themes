using Zios;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
namespace Zios{
	[CustomPropertyDrawer(typeof(AttributeBool),true)]
	public class AttributeBoolDrawer : AttributeDrawer{
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
			if(this.isPrefab){return;}
			if(!Attribute.ready || !area.InspectorValid()){return;}
			this.overallHeight = this.GetBaseHeight(property,label);
			if(this.access == null){
				this.access = new AttributeBoolAccess();
			}
			this.access.Setup(this,area,property,label);
		}
	}
	public class AttributeBoolAccess : AttributeAccess<bool,AttributeBool,AttributeBoolData>{
		public override void DrawFormulaRow(AttributeData data,int index){
			float lineHeight = EditorGUIUtility.singleLineHeight+2;
			Rect original = this.fullRect;
			this.operatorOverride = null;
			this.SetupAreas(this.fullRect.AddY(lineHeight).Scale(0.48f,1));
			this.labelRect = this.labelRect.SetWidth(1);
			this.valueRect = this.fullRect.Add(this.labelRect.width,0,-labelRect.width,0);
			if(data.usage == AttributeUsage.Direct){
				data.usage = AttributeUsage.Shaped;
			}
			this.attribute.canDirect = false;
			this.DrawFormulaPart(data,index);
			int dataIndex = this.attributeCast.data.IndexOf(data);
			AttributeData[] dataB = this.attribute.info.dataB;
			this.SetupAreas(this.fullRect.AddX(this.fullRect.width+3));
			this.labelRect = this.labelRect.SetWidth(1);
			this.valueRect = this.fullRect.Add(this.labelRect.width,0,-labelRect.width,0);
			this.activeDataset = dataB;
			this.attribute.defaultSet = "B";
			if(dataIndex < dataB.Length && dataB[dataIndex] != null){
				string dataType = data.GetType().Name.Strip("Data","Attribute").Replace("Int","Number").Replace("Float","Number");
				string compareType = dataB[dataIndex].GetType().Name.Strip("Data","Attribute").Replace("Int","Number").Replace("Float","Number");
				if(!AttributeBool.comparers.ContainsKey(dataType+compareType)){
					string warning = "Cannot compare <b>" + dataType + "</b> and <b>" + compareType + "</b>.";
					if(this.valueRect.Clicked(0) || this.valueRect.Clicked(1)){
						this.DrawTypeMenu(dataB[dataIndex]);
					}
					warning.Draw(this.valueRect,GUI.skin.GetStyle("WarningLabel"));
				}
				else{
					this.operatorOverride = AttributeBool.comparers[dataType+compareType].ToList();
					if(AttributeBool.comparers.ContainsKey(dataType+compareType)){
						this.attribute.canDirect = true;
						this.DrawFormulaPart(dataB[dataIndex],index+1);
					}
				}
			}
			else if(GUI.Button(this.valueRect.SetWidth(120),"Add Comparison")){
				this.DrawAddMenu();
			}
			this.attribute.defaultSet = "A";
			this.activeDataset = this.attribute.info.data;
			this.fullRect = original.AddY(lineHeight);
			this.drawer.overallHeight += lineHeight;
		}
		public void DrawFormulaPart(AttributeData data,int index){
			//string name = emptyLabel ? " " : ((char)('A'+index)).ToString();
			SerializedObject currentProperty = new SerializedObject(data);
			GUIContent formulaLabel = new GUIContent(" ");
			bool? operatorState = index == 0 ? (bool?)false : (bool?)true;
			if(data.usage == AttributeUsage.Direct){
				GUI.Box(this.labelRect.AddX(2),"",GUI.skin.GetStyle("IconDirect"));
				this.DrawDirect(this.fullRect,this.valueRect,data,formulaLabel,true,operatorState);
			}
			else if(data.usage == AttributeUsage.Shaped){
				this.DrawShaped(this.valueRect,currentProperty,formulaLabel,true,operatorState);
			}
			if(GUI.changed){Utility.SetDirty(data);}
			this.DrawContext(data,index!=0,false);
		}
	}
}