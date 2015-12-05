using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;
using MenuFunction = UnityEditor.GenericMenu.MenuFunction;
using Class = Zios.UI.StateMonoBehaviourEditor;
namespace Zios.UI{
	[CustomEditor(typeof(StateMonoBehaviour),true)]
	public class StateMonoBehaviourEditor : DataMonoBehaviourEditor{
		public static bool isVisible = true;
		public static bool isFixed;
		public static bool isOneLine;
		public GUISkin skin;
		public Rect breakdownArea;
		public bool breakdownVisible = true;
		public virtual StateTable GetTable(){
			var script = (StateMonoBehaviour)this.target;
			return script.controller;
		}
		public override void OnInspectorGUI(){
			if(!Event.current.IsUseful()){return;}
			this.DrawBreakdown();
			base.OnInspectorGUI();
		}
		public void DrawBreakdown(){
			string breakdown = "StateMonoBehaviourEditor-ToggleBreakdown";
			if(EditorPrefs.HasKey(breakdown)){
				Class.isVisible = !Class.isVisible;
				EditorPrefs.DeleteKey(breakdown);
			}
			StateTable table = this.GetTable();
			bool showBreakdown = Class.isVisible;
			bool isFixed = Class.isFixed;
			bool isOneLine = Class.isOneLine;
			if((this.showAll || showBreakdown) && table != null){
				string skinName = EditorGUIUtility.isProSkin ? "Dark" : "Light";
				if(this.skin == null || !this.skin.name.Contains(skinName)){
					this.skin = FileManager.GetAsset<GUISkin>("Gentleface-" + skinName + ".guiskin");
				}
				GUI.skin = this.skin;
				var matchingOnRows = table.table.Where(x=>x.target==this.target).FirstOrDefault();
				var matchingOffRows = table.tableOff.Where(x=>x.target==this.target).FirstOrDefault();
				StateRowData[] onRows = new StateRowData[0];
				StateRowData[] offRows = new StateRowData[0];
				bool hasOnData = matchingOnRows != null;
				if(hasOnData){
					onRows = matchingOnRows.requirements;
					offRows = matchingOffRows.requirements;
					hasOnData = onRows.Select(x=>x.data).First().Where(x=>x.requireOn||x.requireOff).FirstOrDefault() != null;
				}
				this.BeginArea();
				bool fastInspector = EditorPrefs.GetBool("MonoBehaviourEditor-FastInspector");
				if(fastInspector && !this.breakdownVisible){
					GUILayout.Space(this.breakdownArea.height);
				}
				else{
					EditorGUILayout.BeginHorizontal();
					if(hasOnData){
						int fixedWidth = isFixed ? 170 : (Screen.width/2)-18;
						var columnStyle = GUI.skin.GetStyle("Box").FixedWidth(fixedWidth).Background("");
						EditorGUILayout.BeginVertical(columnStyle);
						for(int index=0;index<onRows.Length;++index){
							string title = index < 1 ? "<b>ON</b> When" : "<b>OR</b> When";
							this.DrawState(onRows,index,title);
						}
						EditorGUILayout.EndVertical();
						EditorGUILayout.BeginVertical(columnStyle);
						if(table.advanced){
							bool hasOffData = offRows.Select(x=>x.data).First().Where(x=>x.requireOn||x.requireOff).FirstOrDefault() != null;
							if(!hasOffData){
								GUIStyle boxStyle = GUI.skin.GetStyle("Box").Background("BoxWhiteHighlightRedA50.png");
								EditorGUILayout.BeginVertical(boxStyle);
								string phraseColor = EditorGUIUtility.isProSkin ? "#FF6666" : "#770000";
								string phrase = "<color="+phraseColor+">Never turns off!</color>".ToUpper();
								phrase.DrawLabel(GUI.skin.GetStyle("Label").Alignment("MiddleCenter"));
								EditorGUILayout.EndVertical();
							}
							else{
								for(int index=0;index<offRows.Length;++index){
									string title = index < 1 ? "<b>OFF</b> When" : "<b>OR</b> When";
									this.DrawState(offRows,index,title);
								}
							}
						}
						else{
							for(int index=0;index<onRows.Length;++index){
								string title = index < 1 ? "<b>OFF</b> When" : "<b>AND</b>";
								this.DrawState(onRows,index,title,true);
							}
						}
						EditorGUILayout.EndVertical();
					}
					else{
						int fixedWidth = isFixed ? 305 : Screen.width-37;
						var columnStyle = GUI.skin.GetStyle("Box").FixedWidth(fixedWidth);
						EditorGUILayout.BeginVertical(columnStyle.Background(""));
						EditorGUILayout.BeginVertical(columnStyle);
						string onColor = EditorGUIUtility.isProSkin ? "#95e032" : "#0000AA99";
						string phrase = ("Always <b><color="+onColor+">On</color></b>").ToUpper();
						phrase.DrawLabel(GUI.skin.GetStyle("FixedLabel").Alignment("MiddleCenter"));
						EditorGUILayout.EndVertical();
						EditorGUILayout.EndVertical();
					}
					EditorGUILayout.EndHorizontal();
					Rect area = GUILayoutUtility.GetLastRect();
					if(!area.IsEmpty()){
						if(Event.current.type == EventType.Repaint){this.breakdownArea = area;}
						if(area.Clicked(1)){this.DrawBreakdownMenu();}
						if(Event.current.shift && area.Clicked(0)){
							Class.isVisible = !Class.isVisible;
						}
					}
				}
				if(Event.current.type == EventType.Repaint && !this.breakdownArea.IsEmpty()){
					this.breakdownVisible = this.breakdownArea.InInspectorWindow();
				}
			}
		}
		public void DrawBreakdownMenu(){
			GenericMenu menu = new GenericMenu();
			MenuFunction hideBreakdown = ()=>{Class.isVisible = false;};
			MenuFunction toggleFixed = ()=>{Class.isFixed = !Class.isFixed;};
			MenuFunction toggleOneLine = ()=>{Class.isOneLine = !Class.isOneLine;};
			menu.AddItem(new GUIContent("Fixed Layout"),Class.isFixed,toggleFixed);
			//menu.AddItem(new GUIContent("One Line Layout"),Class.isOneLine,toggleOneLine);
			menu.AddItem(new GUIContent("Hide Breakdown"),false,hideBreakdown);
			menu.ShowAsContext();
		}
		public string DrawState(StateRowData[] rowData,int rowIndex,string title,bool flip=false){
			StateRowData row = rowData[rowIndex];
			GUIStyle boxStyle = GUI.skin.GetStyle("Box");
			if(rowIndex > 0){
				string background = EditorGUIUtility.isProSkin ? "solidBlack10.png" : "solidWhite10.png";
				boxStyle = boxStyle.Background(background);
			}
			EditorGUILayout.BeginVertical(boxStyle);
			string phrase = "";
			string headerColor = EditorGUIUtility.isProSkin ? "#AAAAAA" : "#555555";
			string header = "<color="+headerColor+">"+title+"</color>";
			header.DrawLabel(GUI.skin.GetStyle("FixedLabel").Alignment("MiddleRight"));
			bool hasDrawn = false;
			for(int index=0;index<row.data.Length;++index){
				StateRequirement requirement = row.data[index];
				if(!requirement.requireOn && !requirement.requireOff){continue;}
				string nameColor = EditorGUIUtility.isProSkin ? "#CCCCCC" : "#000000";
				string name = "</i><color="+nameColor+">"+requirement.name+"</color><i>";
				bool stateOn = flip ? !requirement.requireOn : requirement.requireOn;
				string stateName = stateOn ? "ON" : "OFF";
				string stateColor = EditorGUIUtility.isProSkin ? "#95e032" : "#0000AA99";
				if(!stateOn){stateColor = EditorGUIUtility.isProSkin ? "#e03232" : "#a22e2e";}
				string state = "</i><color="+stateColor+"><b>"+stateName+"</b></color><i>";
				phrase = name + " is " + state;
				if(hasDrawn){phrase = "and " + phrase;}
				phrase = "<i>"+phrase.ToUpper()+"</i>";
				if(!Class.isOneLine){
					phrase.DrawLabel(GUI.skin.GetStyle("FixedLabel").Alignment("MiddleRight"));
				}
				hasDrawn = true;
			}
			EditorGUILayout.EndVertical();
			return phrase;
		}
	}
}