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
		public TableHeader header = new TableHeader(false,true);
		public bool showEmpty = true;
		public List<int> emptyRows = new List<int>();
		public List<int> emptyColumns = new List<int>();
		public List<TableRow> rows = new List<TableRow>();
		public OnCompareEvent onCompare;
		public SortOptions sortOptions;
		public TableGUI(){
			string skin = EditorGUIUtility.isProSkin ? "Dark" : "Light";
			this.tableSkin = FileManager.GetAsset<GUISkin>("Table-" + skin + ".guiskin");
			this.sortOptions = new SortOptions();
		}
		public static bool CheckRegion(){
			bool onHover = GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition);
			bool onMouseDown = Event.current.type == EventType.MouseDown;
			return onHover && onMouseDown;
		}
		public void SetHeader(bool vertical=false,bool sortable=true,OnCompareEvent onCompare = null){
			this.header = new TableHeader(vertical,sortable);
			this.onCompare = onCompare;
		}
		public void AddHeader(string label,OnHeaderEvent onDisplay=null,OnHeaderEvent onClick=null){
			this.header.items.Add(new TableHeaderItem(label,onDisplay,onClick));
		}
		public TableRow AddRow(OnRowEvent onDisplay=null,OnRowEvent onClick=null){
			TableRow row = new TableRow(onDisplay,onClick);
			this.rows.Add(row);
			return row;
		}
		public static RectOffset RotateOffset(RectOffset rectOffset){
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
		public static GUIStyle RotateStyle(GUIStyle style){
			float width = style.fixedWidth;
			float height = style.fixedHeight;
			style.fixedWidth = height;
			style.fixedHeight = width;
			style.margin = TableGUI.RotateOffset(style.margin);
			style.padding = TableGUI.RotateOffset(style.padding);
			return style;
		}
		public int CompareRows(object row1,object row2){
			object target1 = ((TableRow)row1).fields[this.sortOptions.fieldNumber].target;
			object target2 = ((TableRow)row2).fields[this.sortOptions.fieldNumber].target;
			int result = onCompare(target1,target2);
			return result;
		}
		public void Draw(){
			GUI.skin = this.tableSkin;
			EditorGUILayout.BeginVertical();
			this.header.emptyColumns = this.emptyColumns;
			this.header.showEmpty = this.showEmpty;
			this.header.Draw();
			if(onCompare != null && this.header.sortColumn > -1 && !this.emptyColumns.Contains(this.header.sortColumn - 1)){
				this.sortOptions.Setup(this.header.sortColumn);
				this.rows.Sort(this.sortOptions,CompareRows);
				this.header.sortColumn = -1;
			}
			foreach(TableRow row in this.rows){
				row.emptyColumns = this.emptyColumns;
				row.showEmpty = this.showEmpty;
				int rowIndex = this.rows.IndexOf(row);
				if(!this.emptyRows.Contains(rowIndex) || this.showEmpty){
					row.Draw();
				}
			}
			EditorGUILayout.EndVertical();
			string prefix = this.showEmpty ? "☑ " : "☐ ";
			GUIStyle buttonStyle = new GUIStyle(GUI.skin.toggle);
			buttonStyle.normal = this.showEmpty ? GUI.skin.toggle.active : buttonStyle.normal;
			buttonStyle.hover = this.showEmpty ? GUI.skin.toggle.active : buttonStyle.hover;
			if(GUILayout.Button(prefix + "Show empty rows/columns",buttonStyle)){
				this.showEmpty = !this.showEmpty;
			}
			this.CalculateEmpty();
		}
		public void CalculateEmpty(){
			this.emptyColumns.Clear();
			this.emptyRows.Clear();
			List<int> emptyColumnCount = new List<int>();
			for(int i = 0;i < this.rows.Count;i++){
				emptyColumnCount.Add(0);
			}
			foreach(TableRow row in this.rows){
				int rowIndex = this.rows.IndexOf(row);
				bool emptyRow = true;
				for(int i = 1;i < row.fields.Count;i++){
					TableField field = row.fields[i];
					if(field.empty){
						emptyColumnCount[i - 1] = emptyColumnCount[i - 1] + 1;
					}
					else{
						emptyRow = false;
					}
				}
				if(emptyRow){
					this.emptyRows.Add(rowIndex);
				}
			}
			for(int i = 0;i < emptyColumnCount.Count;i++){
				if(emptyColumnCount[i] == this.rows.Count){
					this.emptyColumns.Add(i);
				}
			}
		}
	}
	public class TableHeader{
		public List<TableHeaderItem> items;
		public bool vertical;
		public bool sortable;
		public bool showEmpty = true;
		public int sortColumn = -1;
		public List<int> emptyColumns = new List<int>();
		public TableHeader(bool vertical,bool sortable){
			this.vertical = vertical;
			this.sortable = sortable;
			this.items = new List<TableHeaderItem>();
		}
		public void Draw(){
			GUIStyle style = new GUIStyle(GUI.skin.label);
			if(!this.vertical){style = TableGUI.RotateStyle(style);}
			EditorGUILayout.BeginHorizontal();
			foreach(TableHeaderItem item in this.items){
				int headerIndex = this.items.IndexOf(item);
				if(headerIndex == 0 || (!this.emptyColumns.Contains(headerIndex - 1) || this.showEmpty)){
					item.onSort = SortItems;
					item.Draw(style,vertical);
				}
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(-(100-style.fixedWidth)/2);
		}
		public void SortItems(TableHeaderItem headerField){
			this.sortColumn = this.items.IndexOf(headerField);
		}
	}
	public class TableHeaderItem{
		public string label;
		public OnHeaderEvent onDisplay;
		public OnHeaderEvent onClick;
		public OnHeaderEvent onSort;
		public TableHeaderItem(string label,OnHeaderEvent onDisplay=null,OnHeaderEvent onClick=null){
			this.onDisplay = onDisplay;
			this.onClick = onClick;
			this.label = label;
		}
		public void Draw(GUIStyle style,bool verticalHeader){
			if(this.onDisplay != null){
				this.onDisplay(this);
				return;
			}
			float halfWidth = style.fixedHeight/2;
			float halfHeight = style.fixedWidth/2;
			if(label == ""){
				GUILayout.Space(style.fixedWidth + halfWidth + 1);
				return;
			}
			if(verticalHeader){
				GUIStyle empty = new GUIStyle(GUI.skin.label);
				TableGUI.RotateStyle(empty);
				empty.normal.background = null;
				GUILayout.Label("",empty);
				Rect last = GUILayoutUtility.GetLastRect();
				Vector2 pivotPoint = last.center;
				GUIUtility.RotateAroundPivot(90,pivotPoint);
				Rect position = new Rect(last.x-last.width-halfWidth,last.y+last.height-halfHeight,0,0);
				GUI.Label(position,label,style);
				GUIUtility.RotateAroundPivot(-90,pivotPoint); 
			}
			else{
				GUILayout.Label(label,style);
			}
			if(TableGUI.CheckRegion()){
				this.onSort(this);
				if(this.onClick != null){
					this.onClick(this);
				}
			}
		}
	}
	public class TableRow{
		public bool selected;
		public object target;
		public OnRowEvent onDisplay;
		public OnRowEvent onClick;
		public bool showEmpty = true;
		public List<int> emptyColumns = new List<int>();
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
				int fieldIndex = this.fields.IndexOf(field);
				if(fieldIndex == 0 || (!this.emptyColumns.Contains(fieldIndex - 1) || this.showEmpty)){
					field.Draw();
				}
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
		public bool empty;
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
			if(this.selected && this.style != null){
				this.style = new GUIStyle(this.style);
				this.style.normal = this.style.active;	
			} 
			if(this.target is string || this.target.HasAttribute("name")){
				string name = this.target is string ? (string)this.target : this.target.GetAttribute<string>("name");
				GUILayout.Label(name);
				this.CheckClick();
			}
			if(this.target is bool){/*Checkbox;*/
			}
			if(this.target.GetType().IsEnum){/*Dropdown*/
			}
			if(this.target is Color){/*Color*/
			}
			//if(this.target is Switch){}
		}
		public void CheckClick(){
			if(this.onClick == null){
				return;
			}
			if(TableGUI.CheckRegion()){
				this.onClick(this);
			}
		}
	}
}
