using Zios;
using System.Linq;
using UnityEngine;
using UnityEditor;
using MenuFunction = UnityEditor.GenericMenu.MenuFunction;
namespace Zios{
    [CustomEditor(typeof(StateLink),true)]
    public class StateLinkEditor : MonoBehaviourEditor{
		public GUISkin skin;
		public Rect breakdownArea;
		public bool breakdownVisible = true;
		public virtual StateTable GetTable(){
		    StateLink script = (StateLink)this.target;
			return script.stateTable;
		}
	    public override void OnInspectorGUI(){
			if(!Event.current.IsUseful()){return;}
			StateTable table = this.GetTable();
			bool showBreakdown = EditorPrefs.GetBool("StateLinkBreakdownVisible",true);
			bool showFixed = EditorPrefs.GetBool("StateLinkBreakdownFixed");
			if((this.showAll || showBreakdown) && table != null){
				string skinName = EditorGUIUtility.isProSkin ? "Dark" : "Light";
				if(this.skin == null || !this.skin.name.Contains(skinName)){
					this.skin = FileManager.GetAsset<GUISkin>("Gentleface-" + skinName + ".guiskin");
				}
				GUI.skin = this.skin;
				StateRowData[] onRows = table.table.Where(x=>x.target==this.target).FirstOrDefault().requirements;
				StateRowData[] offRows = table.tableOff.Where(x=>x.target==this.target).FirstOrDefault().requirements;
				bool hasOnData = onRows.Select(x=>x.data).First().Where(x=>x.requireOn||x.requireOff).FirstOrDefault() != null;
				this.BeginArea();
				bool fastInspector = EditorPrefs.GetBool("MonoBehaviourEditor-FastInspector",true);
				if(fastInspector && !this.breakdownVisible){
					GUILayout.Space(this.breakdownArea.height);
				}
				else{
					GUILayout.BeginHorizontal();
					if(hasOnData){
						int fixedWidth = showFixed ? 150 : (Screen.width/2)-18;
						var columnStyle = GUI.skin.GetStyle("Box").FixedWidth(fixedWidth).Background("");
						GUILayout.BeginVertical(columnStyle);
						for(int index=0;index<onRows.Length;++index){
							string title = index < 1 ? "<b>ON</b> When" : "<b>OR</b> When";
							this.DrawState(onRows,index,title);
						}
						GUILayout.EndVertical();
						GUILayout.BeginVertical(columnStyle);
						if(table.advanced){
							bool hasOffData = offRows.Select(x=>x.data).First().Where(x=>x.requireOn||x.requireOff).FirstOrDefault() != null;
							if(!hasOffData){
								GUIStyle boxStyle = GUI.skin.GetStyle("Box").FixedWidth(150).Background("SolidRed50.png");
								GUILayout.BeginVertical(boxStyle);
								string phraseColor = EditorGUIUtility.isProSkin ? "#FF6666" : "#770000";
								string phrase = "<color="+phraseColor+">Never turns off!</color>".ToUpper();
								phrase.DrawLabel(GUI.skin.GetStyle("FixedLabel").Alignment("MiddleCenter"));
								GUILayout.EndVertical();
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
						GUILayout.EndVertical();
					}
					else{
						int fixedWidth = showFixed ? 305 : Screen.width-37;
						var columnStyle = GUI.skin.GetStyle("Box").FixedWidth(fixedWidth);
						GUILayout.BeginVertical(columnStyle.Background(""));
						GUILayout.BeginVertical(columnStyle);
						string onColor = EditorGUIUtility.isProSkin ? "#95e032" : "#0000AA99";
						string phrase = ("Always <b><color="+onColor+">On</color></b>").ToUpper();
						phrase.DrawLabel(GUI.skin.GetStyle("FixedLabel").Alignment("MiddleCenter"));
						GUILayout.EndVertical();
						GUILayout.EndVertical();
					}
					GUILayout.EndHorizontal();
					Rect area = GUILayoutUtility.GetLastRect();
					if(!area.IsEmpty()){
						if(Event.current.type == EventType.Repaint){this.breakdownArea = area;}
						if(area.Clicked(1)){this.DrawBreakdownMenu();}
						if(Event.current.shift && area.Clicked(0)){
							Utility.ToggleEditorPref("StateLinkBreakdownVisible");
						}
					}
				}
				if(Event.current.type == EventType.Repaint && !this.breakdownArea.IsEmpty()){
					this.breakdownVisible = this.breakdownArea.InInspectorWindow();
				}
			}
		    base.OnInspectorGUI();
	    }
		public void DrawBreakdownMenu(){
			GenericMenu menu = new GenericMenu();
			MenuFunction hideBreakdown = ()=>{Utility.ToggleEditorPref("StateLinkBreakdownVisible");};
			MenuFunction toggleFixed = ()=>{Utility.ToggleEditorPref("StateLinkBreakdownFixed");};
			menu.AddItem(new GUIContent("Fixed Layout"),EditorPrefs.GetBool("StateLinkBreakdownFixed"),toggleFixed);
			menu.AddItem(new GUIContent("Hide Breakdown"),false,hideBreakdown);
			menu.ShowAsContext();
		}
		public void DrawState(StateRowData[] rowData,int rowIndex,string title,bool flip=false){
			StateRowData row = rowData[rowIndex];
			GUIStyle boxStyle = GUI.skin.GetStyle("Box");
			if(rowIndex > 0){
				string background = EditorGUIUtility.isProSkin ? "solidBlack10.png" : "solidWhite10.png";
				boxStyle = boxStyle.Background(background);
			}
			GUILayout.BeginVertical(boxStyle);
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
				string phrase = name + " is " + state;
				if(hasDrawn){phrase = "and " + phrase;}
				phrase = "<i>"+phrase.ToUpper()+"</i>";
				phrase.DrawLabel(GUI.skin.GetStyle("FixedLabel").Alignment("MiddleRight"));
				hasDrawn = true;
			}
			GUILayout.EndVertical();
		}
    }
}