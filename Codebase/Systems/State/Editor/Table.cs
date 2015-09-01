using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
namespace Zios.UI{
	public class Table{
		public string skinDark = "Table-Dark";
		public string skinLight = "Table-Light";
		public List<TableRow> rows = new List<TableRow>();
		public TableRow AppendRow(TableRow row){
			row.table = this;
			row.order = this.rows.Count;
			this.rows.Add(row);
			return row;
		}
		public GUISkin GetSkin(){
			string skin = EditorGUIUtility.isProSkin ? this.skinDark : this.skinLight;
			return FileManager.GetAsset<GUISkin>(skin+".guiskin");
		}
		public TableRow AddRow(object target=null){
			TableRow row = new TableRow(target,this);
			this.rows.Add(row);
			row.order = this.rows.Count-1;
			return row;
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
			foreach(var row in this.rows){
				if(!row.disabled){row.Draw();}
			}
			if(this.rows.Count < 1){
				EditorGUILayout.HelpBox("Please add components to generate table.",MessageType.Info,true);
			}
		}
	}
	public class TableRow{
		public bool selected;
		public bool disabled;
		public object target;
		public Table table;
		public int order;
		public List<TableField> fields = new List<TableField>();
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
		public virtual void Draw(){
			GUILayout.BeginHorizontal();
			foreach(TableField field in this.fields){
				if(!field.disabled){field.Draw();}
			}
			GUILayout.EndHorizontal();
		}
	}
	public class TableField{
		public bool disabled;
		public bool selected;
		public TableRow row;
		public object target;
		public int order;
		public TableField(object target=null,TableRow row=null){
			this.row = row;
			this.target = target;
		}
		public virtual void Draw(){}
		public virtual void Clicked(int button){}
		public void CheckClicked(float xAdjust=0,float yAdjust=0){
			if(GUILayoutUtility.GetLastRect().AddXY(xAdjust,yAdjust).Clicked()){
				this.Clicked(Event.current.button);
				Event.current.Use();
			}
		}
	}
}
