using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.Drawers.Table{
	using Zios.Extensions;
	using Zios.File;
	using Zios.Supports.Mutant;
	using Zios.Unity.Editor.Pref;
	using Zios.Unity.Extensions;
	public class Table{
		public string skinDark = "Table-Dark";
		public string skinLight = "Table-Light";
		public List<TableRow> rows = new List<TableRow>();
		public Mutant<Vector2> scroll = Vector2.zero;
		public Rect region;
		public TableRow AppendRow(TableRow row){
			row.table = this;
			row.order = this.rows.Count;
			this.rows.Add(row);
			return row;
		}
		public GUISkin GetSkin(){
			string skin = EditorGUIUtility.isProSkin || EditorPref.Get<bool>("EditorTheme-Dark",false) ? this.skinDark : this.skinLight;
			return File.GetAsset<GUISkin>(skin+".guiskin");
		}
		public TableRow AddRow(object target=null){
			TableRow row = new TableRow(target,this);
			this.rows.Add(row);
			row.order = this.rows.Count-1;
			return row;
		}
		public void ShowAll(){
			foreach(var row in this.rows){
				row.hidden = false;
				foreach(var field in row.fields){
					field.hidden = false;
				}
			}
		}
		public void Reorder(){
			int rowIndex = 0;
			foreach(var row in this.rows){
				int columnIndex = 0;
				row.order = rowIndex;
				foreach(var column in row.fields){
					column.order = columnIndex;
					columnIndex += 1;
				}
				rowIndex += 1;
			}
		}
		public virtual void Draw(){
			GUI.skin = this.GetSkin();
			var region = new Rect(0,0,Screen.width,Screen.height);
			if(this.region != region || this.scroll.HasChanged()){
				this.region = region;
				this.scroll.Morph();
				this.ShowAll();
			}
			foreach(var row in this.rows){
				if(row.hidden){
					GUILayout.Space(row.last.height+1);
					continue;
				}
				if(!row.disabled){row.Draw();}
			}
			if(this.rows.Count < 1){
				EditorGUILayout.HelpBox("Please add components to generate table.",MessageType.Info,true);
			}
		}
	}
	public class TableRow{
		public bool hidden;
		public bool selected;
		public bool disabled;
		public object target;
		public Table table;
		public int order;
		public List<TableField> fields = new List<TableField>();
		public Rect last;
		public TableRow(object target=null,Table table=null){
			this.table = table;
			this.target = target;
		}
		public TableField AppendField(TableField field){
			field.row = this;
			field.order = this.fields.Count;
			this.fields.Add(field);
			return field;
		}
		public TableField AddField(object target=null){
			if(target == null){target = this.target;}
			var field = new TableField(target,this);
			field.order = this.fields.Count;
			this.fields.Add(field);
			return field;
		}
		public bool IsVisible(Rect area,Rect region,Vector2 scroll){
			bool fixedX  = area.AddX(-scroll.x).Overlaps(region);
			bool fixedY  = area.AddY(-scroll.y).Overlaps(region);
			bool fixedXY = area.AddXY(-scroll).Overlaps(region);
			bool normal  = area.Overlaps(region);
			return normal || fixedX || fixedY || fixedXY;
		}
		public virtual void Draw(){
			GUILayout.BeginHorizontal();
			foreach(TableField field in this.fields){
				if(field.hidden){
					GUILayout.Space(field.last.width+1);
					continue;
				}
				if(!field.disabled){
					field.Draw();
					field.last = GUILayoutUtility.GetLastRect();
					field.hidden = field.last.width > 1 && !this.table.IsNull() && !this.IsVisible(field.last,this.table.region,this.table.scroll);
				}
			}
			GUILayout.EndHorizontal();
			this.last = GUILayoutUtility.GetLastRect();
			this.hidden = this.last.height > 1 && !this.table.IsNull() && !this.IsVisible(this.last,this.table.region,this.table.scroll);
		}
	}
	public class TableField{
		public bool hidden;
		public bool hovered;
		public bool disabled;
		public bool selected;
		public TableRow row;
		public object target;
		public int order;
		public Rect last;
		public TableField(object target=null,TableRow row=null){
			this.row = row;
			this.target = target;
		}
		public virtual void Draw(){}
		public virtual void Clicked(int button){}
		public void CheckHovered(float xAdjust=0,float yAdjust=0){
			if(Event.current.type == EventType.MouseMove){
				this.hovered = GUILayoutUtility.GetLastRect().AddXY(xAdjust,yAdjust).Hovered();
			}
		}
		public void CheckClicked(float xAdjust=0,float yAdjust=0){
			if(GUILayoutUtility.GetLastRect().AddXY(xAdjust,yAdjust).Clicked()){
				this.Clicked(Event.current.button);
				Event.current.Use();
			}
		}
	}
}