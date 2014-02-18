using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Zios.Editor{
	public delegate void OnRowEvent(TableRow row);
	public delegate void OnFieldEvent(TableField field);
	public delegate void OnHeaderEvent(TableHeaderItem headerField);
	public class TableGUI{
		public GUISkin tableSkin;
		public GUISkin tableHeaderSkin;
		public bool verticalHeader;
		public int width = -1;
		public int height = -1;
		public TableHeader header = new TableHeader(false,true);
		public List<TableRow> rows = new List<TableRow>();
		public TableGUI(){
			string skin = EditorGUIUtility.isProSkin ? "Dark" : "Light";
			this.tableSkin = FileManager.GetAsset<GUISkin>("Table-" + skin + ".guiskin");
			this.tableHeaderSkin = FileManager.GetAsset<GUISkin>("TableHeader-" + skin + ".guiskin");
		}
		public static bool CheckRegion(){
			bool onHover = GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition);
			bool onMouseDown = Event.current.type == EventType.MouseDown;
			return onHover && onMouseDown;
		}
		public void SetHeader(bool vertical=false,bool sortable=true){
			this.header = new TableHeader(vertical,sortable);
		}
		public void AddHeader(string label,OnHeaderEvent onDisplay=null,OnHeaderEvent onClick=null){
			this.header.items.Add(new TableHeaderItem(label,onDisplay,onClick));
		}
		public TableRow AddRow(OnRowEvent onDisplay=null,OnRowEvent onClick=null){
			TableRow row = new TableRow(onDisplay,onClick);
			this.rows.Add(row);
			return row;
		}
		public static RectOffset rotateOffset(RectOffset rectOffset){
			int left = rectOffset.left;
			int right = rectOffset.right;
			int top = rectOffset.top;
			int bottom = rectOffset.bottom;
			rectOffset.left = top;
			rectOffset.right = bottom;
			rectOffset.top = left;
			rectOffset.bottom = right;
			return rectOffset;
		}
		public void Draw(){
			EditorGUILayout.BeginVertical();
			GUI.skin = this.tableHeaderSkin;
			this.header.Draw(this.verticalHeader);
			GUI.skin = this.tableSkin;
			foreach(TableRow row in this.rows){
				row.Draw();
			}
			EditorGUILayout.EndVertical();
		}
	}
	public class TableHeader{
		public List<TableHeaderItem> items;
		public bool vertical;
		public bool sortable;
		public TableHeader(bool vertical,bool sortable){
			this.vertical = vertical;
			this.sortable = sortable;
			this.items = new List<TableHeaderItem>();
		}
		public void Draw(bool isVertical){
			GUIStyle style = GUI.skin.label;
			if(isVertical){
				style = new GUIStyle(style);
				float width = style.fixedWidth;
				float height = style.fixedHeight;
				style.fixedWidth = height;
				style.fixedHeight = width;
				style.margin = TableGUI.rotateOffset(style.margin);
				style.padding = TableGUI.rotateOffset(style.padding);
				style.wordWrap = false;
				style.alignment = TextAnchor.MiddleRight;
			}
			EditorGUILayout.BeginHorizontal();
			foreach(TableHeaderItem item in this.items){
				item.Draw(style, isVertical);
			}
			EditorGUILayout.EndHorizontal();
		}
	}
	public class TableHeaderItem{
		public static int defaultWidth = 125;
		public string label;
		public OnHeaderEvent onDisplay;
		public OnHeaderEvent onClick;
		public TableHeaderItem(string label,OnHeaderEvent onDisplay=null,OnHeaderEvent onClick=null){
			this.onDisplay = onDisplay;
			this.onClick = onClick;
			this.label = label;
		}
		public void Draw(GUIStyle style, bool verticalHeader){
			if(this.onDisplay != null){
				this.onDisplay(this);
				return;
			}
			if(label == ""){
				GUILayout.Space(TableHeaderItem.defaultWidth);
				GUILayout.Space(3);
				return;
			}
			if(verticalHeader){
				GUILayout.Label("",GUI.skin.label);
				Rect last = GUILayoutUtility.GetLastRect();
				Vector2 pivotPoint = last.center;
				GUIUtility.RotateAroundPivot(90,pivotPoint);
				Rect position = new Rect(last.x - (7*last.width/2)+1,last.y + (7*last.height/16)+2,0,0);
				GUI.Label(position, label, style);
				GUIUtility.RotateAroundPivot(-90,pivotPoint); 
			}else{
				GUILayout.Label(label, style);
			}
		}
	}
	public class TableRow{
		public bool selected;
		public object target;
		public OnRowEvent onDisplay;
		public OnRowEvent onClick;
		public List<TableField> fields = new List<TableField>();
		public TableRow(OnRowEvent onDisplay=null,OnRowEvent onClick=null){
			this.onDisplay = onDisplay;
			this.onClick = onClick;
		}
		public void AddField(object target,OnFieldEvent onDisplay=null,OnFieldEvent onClick=null){
			this.fields.Add(new TableField(target,onDisplay,onClick));
		}
		public void Draw(){
			if(this.onDisplay != null){
				this.onDisplay(this);
				return;
			}
			EditorGUILayout.BeginHorizontal();
			this.selected = false;
			foreach(TableField field in this.fields){
				field.Draw();
			}
			EditorGUILayout.EndHorizontal();
		}
		public void DeselectFields(){
			foreach(TableField field in this.fields){
				field.selected = false;
			}
			this.selected = false;
		}
	}
	public class TableField{
		public bool selected;
		public object target;
		public OnFieldEvent onDisplay;
		public OnFieldEvent onClick;
		public GUIStyle style;
		public TableField(object target,OnFieldEvent onDisplay=null,OnFieldEvent onClick=null){
			this.target = target;
			this.onDisplay = onDisplay;
			this.onClick = onClick;
		}
		public void Draw(){
			if(this.onDisplay != null){
				this.onDisplay(this);
				return;
			}
			if(this.selected && this.style != null ){
				this.style = new GUIStyle(this.style);
				this.style.normal = this.style.active;	
			} 
			if(this.target is string || this.target.HasAttribute("name")){
				string name = this.target is string ? (string)this.target : this.target.GetAttribute<string>("name");
				GUILayout.Label(name);
				this.CheckClick();
			}
			if(this.target is bool){/*Checkbox;*/}
			if(this.target.GetType().IsEnum){/*Dropdown*/}
			if(this.target is Color){/*Color*/}
			//if(this.target is Switch){}
		}
		public void CheckClick(){
			if(this.onClick == null){return;}
			if(TableGUI.CheckRegion()){
				this.onClick(this);
			}
		}
	}
}
