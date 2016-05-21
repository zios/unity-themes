using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEvent = UnityEngine.Event;
using MenuFunction = UnityEditor.GenericMenu.MenuFunction;
namespace Zios.Editors.StateEditors{
	using Interface;
	using Actions;
	using Class = StateMonoBehaviourEditor;
	[CustomEditor(typeof(StateMonoBehaviour),true)]
	public class StateMonoBehaviourEditor : DataMonoBehaviourEditor{
		public static bool isVisible = true;
		public static bool isFixed;
		public static bool isOneLine;
		public GUISkin skin;
		public Rect breakdownArea;
		public bool breakdownVisible = true;
		private string nameColor;
		private string warningColor;
		private string headerColor;
		private string onColor;
		private string offColor;
		private string usedColor;
		private GUIStyle labelStyle;
		private GUIStyle warningStyle;
		private GUIStyle boxStyle;
		private GUIStyle columnStyle;
		private GUIStyle containerStyle;
		public virtual StateTable GetTable(){
			var script = (StateMonoBehaviour)this.target;
			return script.controller;
		}
		public override void OnInspectorGUI(){
			if(!UnityEvent.current.IsUseful()){return;}
			this.SetupStyles();
			this.DrawBreakdown();
			base.OnInspectorGUI();
		}
		public void SetupStyles(){
			string skinName = EditorGUIUtility.isProSkin ? "Dark" : "Light";
			if(this.skin == null || !this.skin.name.Contains(skinName)){
				this.skin = FileManager.GetAsset<GUISkin>("Gentleface-" + skinName + ".guiskin");
			}
			GUI.skin = this.skin;
			int fixedWidth = Class.isFixed ? 200 : (Screen.width/2)-18;
			this.headerColor = EditorGUIUtility.isProSkin ? "#AAAAAA" : "#555555";
			this.nameColor = EditorGUIUtility.isProSkin ? "#CCCCCC" : "#000000";
			this.warningColor = EditorGUIUtility.isProSkin ? "#FF6666" : "#770000";
			this.onColor = EditorGUIUtility.isProSkin ? "#95e032" : "#0000AA99";
			this.offColor = EditorGUIUtility.isProSkin ? "#e03232" : "#a22e2e";
			this.usedColor = EditorGUIUtility.isProSkin ? "#E0C532" : "#8F7E21";
			this.boxStyle = GUI.skin.GetStyle("Box");
			this.columnStyle = GUI.skin.GetStyle("Box").FixedWidth(fixedWidth);
			this.containerStyle = GUI.skin.GetStyle("Box").FixedWidth(fixedWidth).Background("");
			this.warningStyle = GUI.skin.GetStyle("Label").Alignment("MiddleCenter");
			this.labelStyle = GUI.skin.GetStyle("FixedLabel").Alignment("MiddleRight");
		}
		public void DrawBreakdown(){
			string breakdown = "StateMonoBehaviourEditor-ToggleBreakdown";
			if(EditorPrefs.HasKey(breakdown)){
				Class.isVisible = !Class.isVisible;
				EditorPrefs.DeleteKey(breakdown);
			}
			StateTable table = this.GetTable();
			if((this.showAll || Class.isVisible) && table != null){
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
						EditorGUILayout.BeginVertical(this.containerStyle);
						for(int index=0;index<onRows.Length;++index){
							string title = index < 1 ? "<b>ON</b> When" : "<b>OR</b> When";
							this.DrawState(onRows,index,title);
						}
						EditorGUILayout.EndVertical();
						EditorGUILayout.BeginVertical(this.containerStyle);
						if(table.advanced){
							bool hasOffData = offRows.Select(x=>x.data).First().Where(x=>x.requireOn||x.requireOff).FirstOrDefault() != null;
							if(!hasOffData){
								GUIStyle boxStyle = this.columnStyle.Background("BoxWhiteHighlightRedA50.png");
								EditorGUILayout.BeginVertical(boxStyle);
								string phrase = "<color="+this.warningColor+">Never turns off!</color>".ToUpper();
								phrase.ToLabel().DrawLabel(this.warningStyle);
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
							EditorGUILayout.BeginVertical(this.columnStyle);
							string header = "<color="+this.headerColor+"><b>OFF</b> When</color>";
							string phrase = "<color="+this.nameColor+">@EXTERNAL</color><i> is </i><color="+this.offColor+"><b>OFF</b></color>";
							header.ToLabel().DrawLabel(this.labelStyle);
							phrase.ToLabel().DrawLabel(this.labelStyle);
							EditorGUILayout.EndVertical();
							if(onRows.SelectMany(x=>x.data).ToList().Exists(x=>x.name!="@External"&&(x.requireOn||x.requireOff))){
								for(int index=0;index<onRows.Length;++index){
									string title = "<b>OR</b> When";
									this.DrawState(onRows,index,title,true);
								}
							}
						}
						EditorGUILayout.EndVertical();
					}
					else{
						int size = Screen.width-37;
						EditorGUILayout.BeginVertical(this.containerStyle);
						EditorGUILayout.BeginVertical(this.columnStyle.FixedWidth(size));
						string phrase = ("Always <b><color="+this.onColor+">On</color></b>").ToUpper();
						phrase.ToLabel().DrawLabel(this.labelStyle.Alignment("MiddleCenter"));
						EditorGUILayout.EndVertical();
						EditorGUILayout.EndVertical();
					}
					EditorGUILayout.EndHorizontal();
					Rect area = GUILayoutUtility.GetLastRect();
					if(!area.IsEmpty()){
						if(UnityEvent.current.type == EventType.Repaint){this.breakdownArea = area;}
						if(area.Clicked(1)){this.DrawBreakdownMenu();}
						if(UnityEvent.current.shift && area.Clicked(0)){
							Class.isVisible = !Class.isVisible;
						}
					}
				}
				if(UnityEvent.current.type == EventType.Repaint && !this.breakdownArea.IsEmpty()){
					this.breakdownVisible = this.breakdownArea.InInspectorWindow();
				}
			}
		}
		public void DrawBreakdownMenu(){
			GenericMenu menu = new GenericMenu();
			MenuFunction hideBreakdown = ()=>{Class.isVisible = false;};
			MenuFunction toggleFixed = ()=>{Class.isFixed = !Class.isFixed;};
			menu.AddItem(new GUIContent("Settings/Fixed"),Class.isFixed,toggleFixed);
			menu.AddItem(new GUIContent("Settings/Hide"),false,hideBreakdown);
			menu.ShowAsContext();
		}
		public string DrawState(StateRowData[] rowData,int rowIndex,string title,bool flip=false){
			StateRowData row = rowData[rowIndex];
			EditorGUILayout.BeginVertical(this.boxStyle);
			string phrase = "";
			string header = "<color="+this.headerColor+">"+title+"</color>";
			header.ToLabel().DrawLabel(this.labelStyle);
			bool hasDrawn = false;
			for(int index=0;index<row.data.Length;++index){
				StateRequirement requirement = row.data[index];
				if(flip && requirement.name == "@External"){continue;}
				if(!requirement.requireOn && !requirement.requireOff && !requirement.requireUsed){continue;}
				string name = "</i><color="+nameColor+">"+requirement.name+"</color><i>";
				string stateName = "ON";
				string stateColor = this.onColor;
				string term = "is";
				if(requirement.requireOff || (flip && requirement.requireOn)){
					stateName = "OFF";
					stateColor = this.offColor;
				}
				if(requirement.requireUsed){
					stateName = "USED";
					stateColor = this.usedColor;
					term = "has";
				}
				string state = "</i><color="+stateColor+"><b>"+stateName+"</b></color><i>";
				phrase = name + " " + term + " " + state;
				if(hasDrawn){phrase = (flip ? "or " : "and ") + phrase;}
				phrase = "<i>"+phrase.ToUpper()+"</i>";
				if(!Class.isOneLine){
					phrase.ToLabel().DrawLabel(this.labelStyle);
				}
				hasDrawn = true;
			}
			EditorGUILayout.EndVertical();
			return phrase;
		}
	}
}