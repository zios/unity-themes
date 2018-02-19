using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.State{
	using Zios.Extensions.Convert;
	using Zios.File;
	using Zios.State;
	using Zios.Unity.Colors;
	using Zios.Unity.Editor.Drawers.Table;
	using Zios.Unity.Editor.Pref;
	using Zios.Unity.Extensions;
	using Zios.Unity.Editor.Extensions;
	using Zios.Unity.Style;
	public class GroupLabelField : LabelField{
		public TableRow[] groupRows = new TableRow[0];
		public GroupLabelField(object target=null,TableRow row=null) : base(target,row){}
		public override void Draw(){
			Vector2 scroll = StateWindow.Get().scroll;
			this.DrawStyle();
			this.CheckClicked(scroll.x);
		}
		public override void DrawStyle(){
			var window = StateWindow.Get();
			var row = this.row.target.As<StateRow>();
			var script = row.target;
			bool darkSkin = EditorGUIUtility.isProSkin || EditorPref.Get<bool>("EditorTheme-Dark",false);
			string name = this.target is string ? (string)this.target : script.alias;
			string background = darkSkin ? "BoxBlackA30" : "BoxWhiteBWarm";
			Color textColor = darkSkin? Colors.Get("Silver") : Colors.Get("Black");
			GUIStyle style = new GUIStyle(GUI.skin.label);
			GUIStyle expand = Style.Get("buttonExpand",true);
			bool fieldHovered = window.row == this.row.order;
			if(fieldHovered){
				textColor = darkSkin ? Colors.Get("ZestyBlue") : Colors.Get("White");
				background = darkSkin ? "BoxBlackHighlightBlueAWarm" : "BoxBlackHighlightBlueDWarm";
			}
			if(this.row.selected){
				textColor = darkSkin ? Colors.Get("White") : Colors.Get("White");
				background = darkSkin ? "BoxBlackHighlightCyanA" : "BoxBlackHighlightCyanCWarm";
			}
			style.fixedWidth -= 28;
			style.margin.left = 0;
			style.normal.textColor = textColor;
			style.normal.background = File.GetAsset<Texture2D>(background);
			if(this.row.selected){
				expand.normal.background = style.normal.background;
				expand.normal.textColor = textColor;
				expand.hover = expand.normal;
				style.hover = style.normal;
			}
			bool open = EditorPref.Get<bool>("StateWindow-GroupRow-"+row.section,false);
			string symbol = open ? "-" : "+";
			StateWindow.Clip(symbol,expand,-1,window.headerSize);
			if(GUILayoutUtility.GetLastRect().AddX(window.scroll.x).Clicked()){
				EditorPref.Toggle("StateWindow-GroupRow-"+row.section);
				foreach(var groupRow in this.groupRows){groupRow.disabled = !groupRow.disabled;}
				Event.current.Use();
				window.tableGUI.ShowAll();
				window.Repaint();
			}
			StateWindow.Clip(name,style,-1,window.headerSize);
		}
		public void SelectGroup(bool toggle=false){
			foreach(var row in this.groupRows){
				row.selected = toggle ? !row.selected : true;
			}
		}
		public void Ungroup(){
			this.SelectGroup();
			StateWindow.Get().UngroupSelected();
		}
		public override void Clicked(int button){
			var window = StateWindow.Get();
			if(button == 0){
				var multiple = window.tableGUI.rows.Count(x=>x.selected && !this.groupRows.Contains(x)) > 0;
				if(!multiple && !Event.current.control){window.DeselectAll();}
				this.SelectGroup(true);
				this.row.selected = this.groupRows.Count(x=>x.selected) > 0;
			}
			if(button == 1){
				var menu = new GenericMenu();
				menu.AddItem("Ungroup",false,this.Ungroup);
				if(this.row.table.rows.Count(x=>x.selected) > 0){
					menu.AddItem("Selection/Invert",false,window.InvertSelection);
					menu.AddItem("Selection/Deselect All",false,window.DeselectAll);
				}
				//menu.AddItem("Rename",false,this.row.target.As<StateRow>().PromptRename());
				menu.ShowAsContext();
			}
			window.Repaint();
		}
	}
}