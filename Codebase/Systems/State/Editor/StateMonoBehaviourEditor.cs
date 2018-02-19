using System.Linq;
using UnityEditor;
using UnityEngine;
using MenuFunction = UnityEditor.GenericMenu.MenuFunction;
namespace Zios.Unity.Editor.State{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.State;
	using Zios.Unity.Editor.Inspectors.DataBehaviour;
	using Zios.Unity.Editor.Pref;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.EditorUI;
	using Zios.Unity.Extensions;
	using Zios.Unity.Extensions.Convert;
	using Zios.Unity.Proxy;
	//asm Zios.Unity.Components.DataBehaviour;
	//asm Zios.Unity.Components.ManagedBehaviour;
	//asm Zios.Unity.Editor.Inspectors;
	//asm Zios.Unity.Editor.MonoBehaviourEditor;
	[CustomEditor(typeof(StateBehaviour),true)]
	public class StateBehaviourEditor : DataBehaviourEditor{
		public static bool isVisible = true;
		public Rect breakdownArea;
		public bool breakdownVisible = true;
		private string nameColor;
		private string onColor;
		private string offColor;
		private string usedColor;
		private float height = 0;
		private GUIStyle labelStyle;
		public virtual StateTable GetTable(){
			var script = (StateBehaviour)this.target;
			return script.controller;
		}
		public override void OnInspectorGUI(){
			if(!Event.current.IsUseful()){return;}
			EditorUI.Reset();
			this.SetupColors();
			this.DrawBreakdown();
			base.OnInspectorGUI();
		}
		public void SetupColors(){
			this.nameColor = EditorStyles.label.normal.textColor.ToHex();
			this.offColor = GUI.skin.GetStyle("CN EntryError").normal.textColor.ToHex();
			this.onColor = EditorStyles.boldLabel.normal.textColor.ToHex();
			this.usedColor = EditorStyles.whiteLabel.normal.textColor.ToHex();
			this.labelStyle = GUI.skin.label.RichText(true);
		}
		public void DrawBreakdown(){
			string breakdown = "StateBehaviourEditor-ToggleBreakdown";
			var alias = "State-"+this.target.As<StateBehaviour>().alias;
			if(EditorPref.Has(breakdown)){
				StateBehaviourEditor.isVisible = !StateBehaviourEditor.isVisible;
				EditorPrefs.DeleteKey(breakdown);
			}
			StateTable table = this.GetTable();
			if((this.showAll || StateBehaviourEditor.isVisible) && table != null){
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
				EditorGUILayout.BeginHorizontal();
				if(hasOnData){
					var container = EditorStyles.helpBox.FixedWidth(Screen.width/2-23).Padding(20,20,8,8);
					EditorGUILayout.BeginVertical(container,GUILayout.MinHeight(this.height));
					for(int index=0;index<onRows.Length;++index){
						string title = index < 1 ? "<b>ENABLED</b> if" : "<b>OR</b> if";
						this.DrawState(onRows,index,title);
					}
					EditorGUILayout.EndVertical();
					this.height = EditorUI.foldoutChanged ? 0 : this.height.Max(GUILayoutUtility.GetLastRect().height);
					EditorGUILayout.BeginVertical(container,GUILayout.MinHeight(this.height));
					if(table.advanced){
						bool hasOffData = offRows.Select(x=>x.data).First().Where(x=>x.requireOn||x.requireOff).FirstOrDefault() != null;
						if(!hasOffData){
							string phrase = "Never turns off!".ToUpper();
							phrase.DrawHelp("Warning");
						}
						else{
							for(int index=0;index<offRows.Length;++index){
								string title = index < 1 ? "<b>DISABLED</b> if" : "<b>OR</b> if";
								this.DrawState(offRows,index,title);
							}
						}
					}
					else{
						string header = "<b>DISABLED</b> if";
						if(header.ToLabel().DrawFoldout(alias+"-Disabled",EditorStyles.foldout.RichText(true))){
							string phrase = "<color="+this.nameColor+">@EXTERNAL</color><i> is </i><color="+this.offColor+"><b>OFF</b></color>";
							phrase.ToLabel().DrawLabel(this.labelStyle);
							if(onRows.SelectMany(x=>x.data).ToList().Exists(x=>x.name!="@External"&&(x.requireOn||x.requireOff))){
								for(int index=0;index<onRows.Length;++index){
									string title = "<b>OR</b> if";
									this.DrawState(onRows,index,title,true);
								}
							}
						}
					}
					EditorGUILayout.EndVertical();
					this.height = EditorUI.foldoutChanged ? 0 : this.height.Max(GUILayoutUtility.GetLastRect().height);
				}
				else{
					EditorGUILayout.BeginVertical(EditorStyles.helpBox);
					string phrase = ("Always <b><color="+this.onColor+">ENABLED</color></b>").ToUpper();
					phrase.ToLabel().DrawLabel(this.labelStyle.Alignment("MiddleCenter"));
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndHorizontal();
				Rect area = GUILayoutUtility.GetLastRect();
				if(!area.IsEmpty()){
					if(Proxy.IsRepainting()){this.breakdownArea = area;}
					if(area.Clicked(1)){this.DrawBreakdownMenu();}
					if(Event.current.shift && area.Clicked(0)){
						StateBehaviourEditor.isVisible = !StateBehaviourEditor.isVisible;
					}
				}
				if(Proxy.IsRepainting() && !this.breakdownArea.IsEmpty()){
					this.breakdownVisible = this.breakdownArea.InInspectorWindow();
				}
			}
		}
		public void DrawBreakdownMenu(){
			GenericMenu menu = new GenericMenu();
			MenuFunction hideBreakdown = ()=>{StateBehaviourEditor.isVisible = false;};
			menu.AddItem(new GUIContent("Settings/Hide"),false,hideBreakdown);
			menu.ShowAsContext();
		}
		public string DrawState(StateRowData[] rowData,int rowIndex,string title,bool flip=false){
			StateRowData row = rowData[rowIndex];
			string phrase = "";
			var alias = "State-"+this.target.As<StateBehaviour>().alias;
			if(title.ToLabel().DrawFoldout(alias+"-"+title+rowIndex.ToString()+flip.Serialize(),EditorStyles.foldout.RichText(true))){
				EditorGUILayout.BeginVertical();
				bool hasDrawn = false;
				for(int index=0;index<row.data.Length;++index){
					StateRequirement requirement = row.data[index];
					if(flip && requirement.name == "@External"){continue;}
					if(!requirement.requireOn && !requirement.requireOff && !requirement.requireUsed){continue;}
					string name = "</i><color="+this.nameColor+">"+requirement.name+"</color><i>";
					string stateName = "ON";
					string stateColor = this.onColor;
					if(requirement.requireOff || (flip && requirement.requireOn)){
						stateName = "OFF";
						stateColor = this.offColor;
					}
					if(requirement.requireUsed){
						stateName = "USED";
						stateColor = this.usedColor;
					}
					string state = "</i><color="+stateColor+"><b>"+stateName+"</b></color><i>";
					phrase = name + " " + state;
					var extraColor = "<color=" + this.nameColor.ToColor().SetAlpha(0.3f).ToHex() + ">";
					if(hasDrawn){phrase = extraColor + (flip ? "or " : "and ") + "</color>" + phrase;}
					phrase = "<i>"+phrase.ToUpper()+"</i>";
					phrase.ToLabel().DrawLabel(this.labelStyle);
					hasDrawn = true;
				}
				EditorGUILayout.EndVertical();
			}
			return phrase;
		}
	}
}