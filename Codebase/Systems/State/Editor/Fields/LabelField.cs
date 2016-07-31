using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEvent = UnityEngine.Event;
using MenuFunction2 = UnityEditor.GenericMenu.MenuFunction2;
namespace Zios.Editors.StateEditors{
	using Actions;
	public class LabelField : TableField{
		public bool delayedContext;
		public LabelField(object target=null,TableRow row=null) : base(target,row){}
		public override void Draw(){
			var window = StateWindow.Get();
			this.DrawStyle();
			if(this.delayedContext){
				this.Clicked(1);
				this.delayedContext = false;
			}
			this.CheckHovered(window.scroll.x);
			this.CheckClicked(window.scroll.x);
		}
		public virtual void DrawStyle(){
			var window = StateWindow.Get();
			var stateRow = (StateRow)this.row.target;
			var row = this.row.target.As<StateRow>();
			var script = row.target;
			bool darkSkin = EditorGUIUtility.isProSkin || EditorPrefs.GetBool("EditorTheme-Dark",false);
			string name = this.target is string ? (string)this.target : this.target.As<StateRow>().name;
			string background = darkSkin ? "BoxBlackA30" : "BoxWhiteBWarm";
			Color textColor = darkSkin ? Colors.Get("Silver") : Colors.Get("Black");
			string prefixColor = Colors.Get("DarkOrange").ToHex();
			GUIStyle style = new GUIStyle(GUI.skin.label);
			style.margin.left = 5;
			bool fieldHovered = window.row == this.row.order || this.hovered;
			if(fieldHovered){
				prefixColor = Colors.Get("ZestyOrange").ToHex();
				textColor = darkSkin ? Colors.Get("ZestyBlue") : Colors.Get("White");
				background = darkSkin ? "BoxBlackHighlightBlueAWarm" : "BoxBlackHighlightBlueDWarm";
			}
			if(this.row.selected){
				prefixColor = Colors.Get("ZestyOrange").ToHex();
				textColor = darkSkin ? Colors.Get("White") : Colors.Get("White");
				background = darkSkin ? "BoxBlackHighlightCyanA" : "BoxBlackHighlightCyanCWarm";
			}
			if(Application.isPlaying){
				textColor = Colors.Get("Gray");
				background = darkSkin ? "BoxBlackAWarm30" : "BoxWhiteBWarm50";
				bool usable = row.target is StateTable && row.target != window.target ? row.target.As<StateTable>().external : script.usable;
				if(usable){
					textColor = darkSkin ? Colors.Get("Silver") : Colors.Get("Black");
					background = darkSkin ? "BoxBlackA30" : "BoxWhiteBWarm";
				}
				if(script.used){
					textColor = darkSkin ? Colors.Get("White") : Colors.Get("White");
					background = darkSkin ? "BoxBlackHighlightYellowAWarm" : "BoxBlackHighlightYellowDWarm";
				}
				if(script.active){
					textColor = darkSkin ? Colors.Get("White") : Colors.Get("White");
					background = darkSkin ? "BoxBlackHighlightPurpleAWarm" : "BoxBlackHighlightPurpleDWarm";
				}
			}
			if(!row.section.IsEmpty()){
				style.margin.left = 33;
				style.fixedWidth -= 28;
			}
			style.normal.textColor = textColor;
			style.normal.background = FileManager.GetAsset<Texture2D>(background);
			if(this.row.selected){style.hover = style.normal;}
			int currentRow = window.rowIndex[stateRow]+1;
			int totalRows = stateRow.requirements.Length;
			var prefix = stateRow.requirements.Length > 1 ? "<color="+prefixColor+"><i>["+currentRow+"/"+totalRows+"]</i></color>  " : "";
			GUIContent content = new GUIContent(prefix+name);
			StateWindow.Clip(content,style,-1,window.headerSize);
		}
		public override void Clicked(int button){
			var window = StateWindow.Get();
			var stateRow = (StateRow)this.row.target;
			int rowIndex = window.rowIndex[stateRow];
			var selected = this.row.table.rows.Where(x=>x.selected).ToArray();
			if(UnityEvent.current.alt && stateRow.requirements.Length > 1){
				int length = stateRow.requirements.Length;
				rowIndex += button == 1 ? -1 : 1;
				if(rowIndex < 0){rowIndex = length-1;}
				if(rowIndex >= length){rowIndex = 0;}
				window.rowIndex[stateRow] = rowIndex;
				window.BuildTable();
				return;
			}
			if(!this.row.selected && button == 1){this.delayedContext = true;}
			if(button == 0 || !this.row.selected){
				if(UnityEvent.current.shift){
					var allRows = this.row.table.rows;
					int firstIndex = selected.Length < 1 ? allRows.Count-1 : allRows.FindIndex(x=>x==selected.First());
					int lastIndex = selected.Length < 1 ? 0 : allRows.FindIndex(x=>x==selected.Last());
					int current = allRows.FindIndex(x=>x==this.row);
					int closest = current.Closest(firstIndex,lastIndex);
					foreach(var row in allRows.Skip(current.Min(closest)).Take(current.Distance(closest)+1)){
						row.selected = !row.disabled;
					}
				}
				else{
					bool state = !this.row.selected;
					if(!UnityEvent.current.control){window.DeselectAll();}
					this.row.selected = state;
				}
			}
			else if(button == 1){
				var menu = new GenericMenu();
				string term = selected.Any(x=>x.target is StateRow && !x.target.As<StateRow>().section.IsEmpty()) ? "Regroup" : "Group";
				menu.AddItem(term+" Selected",false,window.GroupSelected);
				if(selected.Count(x=>!x.target.As<StateRow>().section.IsEmpty()) > 0){
					menu.AddItem("Ungroup Selected",false,window.UngroupSelected);
				}
				menu.AddItem("Selection/Invert",false,window.InvertSelection);
				menu.AddItem("Selection/Deselect All",false,window.DeselectAll);
				if(selected.Length == 1){
					menu.AddItem("Add Alternate Row",false,new MenuFunction2(this.AddAlternativeRow),stateRow);
					if(rowIndex != 0){
						menu.AddItem("Remove Alternative Row",false,new MenuFunction2(this.RemoveAlternativeRow),stateRow);
					}
				}
				menu.ShowAsContext();
			}
			window.Repaint();
		}
		public void AddAlternativeRow(object target){
			var window = StateWindow.Get();
			StateRow row = (StateRow)target;
			List<StateRowData> data = new List<StateRowData>(row.requirements);
			data.Add(new StateRowData());
			row.requirements = data.ToArray();
			window.target.Refresh();
			window.rowIndex[row] = row.requirements.Length-1;
			window.BuildTable();
		}
		public void RemoveAlternativeRow(object target){
			var window = StateWindow.Get();
			StateRow row = (StateRow)target;
			int rowIndex = window.rowIndex[row];
			List<StateRowData> data = new List<StateRowData>(row.requirements);
			data.RemoveAt(rowIndex);
			row.requirements = data.ToArray();
			window.rowIndex[row] = rowIndex-1;
			window.BuildTable();
		}
	}
}