using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Zios;
namespace Zios.UI{
	public enum HeaderMode{Vertical,Horizontal,HorizontalFit}
	public class StateWindow : EditorWindow{
		//===================================
		// Data
		//===================================
		public static StateWindow Get(){return StateWindow.instance;}
		public static StateWindow instance;
		public Table tableGUI = new Table();
		public Dictionary<StateRow,int> rowIndex = new Dictionary<StateRow,int>();
		public List<string> setupSections = new List<string>();
		public Action repaintHooks = ()=>{};
		//===================================
		// Selection
		//===================================
		public StateTable target;
		//===================================
		// State
		//===================================
		public int tableIndex = 0;
		public int row = -1;
		public int column = -1;
		public bool hovered;
		public bool prompted;
		//===================================
		// Visual
		//===================================
		public Vector2 scroll = Vector2.zero;
		public float cellSize;
		public float headerSize;
		public string newSection;
		//===================================
		// Unity-Specific
		//===================================
		public void Update(){
			StateWindow.instance = this;
			this.wantsMouseMove = true;
			this.CheckTarget();
			if(this.target.IsNull()){return;}
			Events.Add("On State Refreshed",this.BuildTable,this.target);
			if(Application.isPlaying){
				Events.Add("On State Updated",this.Repaint,this.target);
				this.row = -1;
				this.column = -1;
			}
		}
		public void OnGUI(){
			if(this.target.IsNull()){return;}
			this.tableGUI.scroll.Set(this.scroll);
			//if(!Event.current.IsUseful()){return;}
			if(Event.current.type == EventType.ScrollWheel){
				this.scroll += Event.current.delta*5;
				Event.current.Use();
				this.Repaint();
			}
			if(!this.prompted){
				this.hovered = false;
				this.scroll = GUILayout.BeginScrollView(this.scroll);
				this.FitLabels();
				this.tableGUI.Draw();
				GUILayout.Space(10);
				GUILayout.EndScrollView();
			}
			this.CheckHotkeys();
			if(Event.current.type == EventType.MouseDown && !Event.current.control && !Event.current.shift){this.DeselectAll();}
			if(Event.current.type == EventType.MouseMove){this.Repaint();}
			if(Event.current.type == EventType.Repaint){
				this.repaintHooks();
				this.repaintHooks = ()=>{};
				if(!this.hovered){
					this.row = -1;
					this.column = -1;
				}
			}
		}
		//===================================
		// Checks
		//===================================
		public void ClearTarget(){this.target = null;}
		public void CheckTarget(){
			var target = Selection.activeGameObject;
			if(!target.IsNull()){
				var table = target.GetComponent<StateTable>();
				bool changed = table != this.target || this.target.IsNull();
				if(changed && !table.IsNull()){
					if(!this.target.IsNull()){
						Events.Remove("On State Updated",this.Repaint,this.target);
						Events.Remove("On State Refreshed",this.BuildTable,this.target);
						Events.Remove("On Components Changed",this.BuildTable,this.target.gameObject);
					}
					Events.Add("On Components Changed",this.BuildTable,table.gameObject);
					this.target = table;
					this.tableIndex = 0;
					this.BuildTable();
				}
			}
			if(this.tableGUI.rows.Count < 1){
				this.target = null;
			}
		}
		public void CheckHotkeys(){
			if(prompted){
				int state = "Group Name?".DrawPrompt(ref this.newSection);
				if(state > 0){
					Utility.RecordObject(this.target,"State Window - Group Assignment");
					var selected = this.tableGUI.rows.Where(x=>x.selected).ToArray();
					foreach(var row in selected){
						row.target.As<StateRow>().section = this.newSection;
					}
					Utility.SetDirty(this.target,false,true);
				}
				if(state != 0){
					GUIUtility.keyboardControl = 0;
					this.prompted = false;
					this.BuildTable();
				}
				return;
			}
			if(Button.KeyUp("A")){this.SelectAll();}
			if(Button.KeyUp("I")){this.InvertSelection();}
			if(Button.KeyUp("G")){this.GroupSelected();}
			if(Button.KeyUp("Escape")){this.DeselectAll();}
		}
		//===================================
		// Operations
		//===================================
		[MenuItem ("Zios/Window/State")]
		public static void Begin(){
			var window = EditorWindow.GetWindow<StateWindow>();
			if(StateWindow.instance == null){
				window.position = new Rect(100,150,600,500);
			}
			window.titleContent = new GUIContent("State");
		}
		public void SelectAll(){
			foreach(var row in this.tableGUI.rows.Skip(2)){
				row.selected = true;
			}
			this.Repaint();
		}
		public void DeselectAll(){
			foreach(var row in this.tableGUI.rows){
				row.selected = false;
			}
			this.Repaint();
		}
		public void InvertSelection(){
			foreach(var row in this.tableGUI.rows.Skip(2).Where(x=>!x.disabled)){
				row.selected = !row.selected;
			}
			this.Repaint();
		}
		public void GroupSelected(){
			var selected = this.tableGUI.rows.Where(x=>x.selected).ToArray();
			if(selected.Length > 0){
				string section = selected[0].target.As<StateRow>().section;
				bool sameSection = selected.Count(x=>section==x.target.As<StateRow>().section) == selected.Length;
				this.newSection = sameSection ? section : "";
				this.prompted = true;
				this.Repaint();
			}
		}
		public void UngroupSelected(){
			var selected = this.tableGUI.rows.Where(x=>x.selected).ToArray();
			Utility.RecordObject(this.target,"State Window - Group Assignment");
			foreach(var row in selected){
				var stateRow = (StateRow)row.target;
				stateRow.section = "";
			}
			Utility.SetDirty(this.target,false,true);
			this.BuildTable();
		}
		public void FitLabels(){
			if(this.target.tables.Count-1 < this.tableIndex){return;}
			StateRow[] activeTable = this.target.tables[this.tableIndex];
			if(activeTable.Length > 0){
				this.tableGUI.GetSkin().label.fixedWidth = 0;
				foreach(StateRow stateRow in activeTable){
					int size = (int)(GUI.skin.label.CalcSize(new GUIContent(stateRow.name)).x) + 28;
					size = (size / 8) * 8 + 1;
					if(size > this.tableGUI.GetSkin().label.fixedWidth){
						this.tableGUI.GetSkin().label.fixedWidth = size+36;
					}
				}
			}
		}
		public virtual void BuildTable(){
			if(this.target.IsNull()){return;}
			StateTable stateTable = this.target;
			stateTable.UpdateTableList();
			StateRow[] activeTable = stateTable.tables[this.tableIndex];
			this.tableGUI = new Table();
			TableRow tableRow = this.tableGUI.AddRow();
			tableRow.AppendField(new TitleField(stateTable));
			if(activeTable.Length > 0){
				tableRow = this.tableGUI.AddRow();
				tableRow.AppendField(new HeaderField(""));
				foreach(var stateRequirement in activeTable[0].requirements[0].data){
					var field = new HeaderField(stateRequirement);
					field.disabled = !stateRequirement.target.IsEnabled();
					tableRow.AppendField(field);
				}
				foreach(StateRow stateRow in activeTable){
					if(!this.rowIndex.ContainsKey(stateRow)){
						this.rowIndex[stateRow] = 0;
					}
					int rowIndex = this.rowIndex[stateRow];
					tableRow = this.tableGUI.AddRow(stateRow);
					tableRow.disabled = !stateRow.target.IsEnabled();
					tableRow.AppendField(new LabelField(stateRow));
					foreach(StateRequirement requirement in stateRow.requirements[rowIndex].data){
						var tableField = new StateField(requirement);
						tableField.disabled = tableRow.disabled || !requirement.target.IsEnabled();
						tableRow.AppendField(tableField);
					}
				}
				this.setupSections.Clear();
				var tableRows = this.tableGUI.rows.Skip(2).ToList();
				foreach(TableRow row in tableRows){
					if(row.disabled){continue;}
					var stateRow = row.target.As<StateRow>();
					string section = stateRow.section;
					if(!section.IsEmpty() && !this.setupSections.Contains(section)){
						bool open = EditorPrefs.GetBool("StateWindow-GroupRow-"+section,false);
						var groupRow = new TableRow(stateRow,this.tableGUI);
						var groupLabel = new GroupLabelField(section);
						var groupRows = tableRows.Where(x=>x.target.As<StateRow>().section==section).ToArray();
						groupLabel.groupRows = groupRows;
						groupRow.disabled = groupLabel.groupRows.Count(x=>!x.disabled) == 0;
						groupRow.AppendField(groupLabel);
						foreach(TableField field in row.fields){
							var groupField = new GroupField(field.target);
							var columnFields = groupRows.SelectMany(x=>x.fields).Where(x=>x is StateField && x.order==field.order).Cast<StateField>().ToArray();
							groupField.disabled = groupRow.disabled || field.disabled;
							groupField.columnFields = columnFields;
							groupRow.AppendField(groupField);
						}
						foreach(var item in groupRows){item.disabled = !open;}
						int insertIndex = tableRows.FindIndex(x=>x.target==stateRow);
						this.tableGUI.rows.Insert(insertIndex+2,groupRow);
						this.setupSections.Add(section);
					}
				}
				var ordered = this.tableGUI.rows.Skip(2).Where(x=>x.target is StateRow).OrderBy(x=>{
					var row = x.target.As<StateRow>();
					if(!row.section.IsEmpty()){return row.section;}
					return row.name;
				});
				this.tableGUI.rows = this.tableGUI.rows.Take(2).Concat(ordered).ToList();
				this.tableGUI.Reorder();
			}
			this.Repaint();
		}
		//===================================
		// Utility
		//===================================
		public static void Clip(UnityLabel label,GUIStyle style,float xClip=0,float yClip=0){
			Rect next = GUILayoutUtility.GetRect(label,style);
			StateWindow.Clip(next,label,style,xClip,yClip);
		}
		public static void Clip(Rect next,UnityLabel label,GUIStyle style,float xClip=0,float yClip=0){
			Vector2 scroll = StateWindow.Get().scroll;
			float x = next.x - scroll.x;
			float y = next.y - scroll.y;
			if(xClip == -1){next.x += scroll.x;}
			if(yClip == -1){next.y += scroll.y;}
			if(xClip > 0){style.overflow.left = (int)Mathf.Min(x-xClip,0);}
			if(yClip > 0){style.overflow.top  = (int)Mathf.Min(y-yClip,0);}
			bool xPass = xClip == -1 || (x + next.width  > xClip);
			bool yPass = yClip == -1 || (y + next.height > yClip);
			label.value.text = style.overflow.left >= -(next.width/4) ? label.value.text : "";
			label.value.text = style.overflow.top >= -(next.height/4) ? label.value.text : "";
			if(xPass && yPass){label.DrawLabel(next,style);}
		}
	}
}