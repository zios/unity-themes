using Zios;
using System.Linq;
using UnityEngine;
using UnityEditor;
namespace Zios{
    [CustomEditor(typeof(ActionLink),true)]
    public class ActionLinkEditor : MonoBehaviourEditor{
		public GUISkin skin;
	    public override void OnInspectorGUI(){
			if(!Event.current.IsUseful()){return;}
		    ActionLink script = (ActionLink)this.target;
			if(script.actionTable != null){
				string skinName = EditorGUIUtility.isProSkin ? "Dark" : "Light";
				if(this.skin == null || !this.skin.name.Contains(skinName)){
					this.skin = FileManager.GetAsset<GUISkin>("Gentleface-" + skinName + ".guiskin");
				}
				GUI.skin = this.skin;
				StateRowData[] onRows = script.actionTable.table.Where(x=>x.target==script).FirstOrDefault().requirements;
				StateRowData[] offRows = script.actionTable.tableOff.Where(x=>x.target==script).FirstOrDefault().requirements;
				bool hasOnData = onRows.Select(x=>x.data).First().Where(x=>x.requireOn||x.requireOff).FirstOrDefault() != null;
				if(hasOnData){
					GUILayout.BeginHorizontal();
					for(int index=0;index<onRows.Length;++index){
						string title = index < 1 ? "<b>ON</b> When" : "<b>OR</b> When";
						this.DrawState(onRows,index,title);
					}
					if(script.actionTable.advanced){
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
							this.DrawState(onRows,index,title,true);}
						}
					GUILayout.EndHorizontal();
				}
				else{
					GUILayout.BeginVertical(GUI.skin.GetStyle("Box").FixedWidth(305));
					string onColor = EditorGUIUtility.isProSkin ? "#95e032" : "#0000AA99";
					string phrase = ("Always <b><color="+onColor+">On</color></b>").ToUpper();
					phrase.DrawLabel(GUI.skin.GetStyle("FixedLabel").Alignment("MiddleCenter"));
					GUILayout.EndVertical();
				}
			}
		    base.OnInspectorGUI();
	    }
		public void DrawState(StateRowData[] rowData,int rowIndex,string title,bool flip=false){
			StateRowData row = rowData[rowIndex];
			GUIStyle boxStyle = GUI.skin.GetStyle("Box").FixedWidth(150);
			if(rowIndex > 0){
				string background = EditorGUIUtility.isProSkin ? "solidBlack10.png" : "solidWhite10.png";
				boxStyle.Background(background,false);
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