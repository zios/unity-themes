﻿using UnityEngine;
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
		public virtual void Draw(){
			GUI.skin = this.GetSkin();
			foreach(var row in this.rows){row.Draw();}
			if(this.rows.Count < 1){
				EditorGUILayout.HelpBox("Please add components to generate table.",MessageType.Info,true);
			}
		}
	}
	public class TableRow{
		public bool selected;
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
				field.Draw();
			}
			GUILayout.EndHorizontal();
		}
	}
	public class TableField{
		public bool selected;
		public TableRow row;
		public object target;
		public int order;
		public string style = "Text Field";
		public TableField(object target=null,TableRow row=null){
			this.row = row;
			this.target = target;
		}
		public virtual void Draw(){
			var style = new GUIStyle(Style.Get(this.style));
			if(this.selected){style.normal = style.active;}
			if(this.target is string || this.target.HasVariable("name")){
				string name = this.target is string ? (string)this.target : this.target.GetVariable<string>("name");
				name.DrawLabel(style);
			}
			this.CheckClicked();
		}
		public virtual void Clicked(int button){}
		public void CheckClicked(){
			if(GUILayoutUtility.GetLastRect().Clicked()){
				this.Clicked(Event.current.button);
			}
		}
	}
}