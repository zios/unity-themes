using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
namespace Zios.Editor{
	public delegate void OnEvent(TableField field);
	public class TableGUI{
		public GUISkin tableSkin;
		public GUISkin tableHeaderSkin;
		public int width = -1;
		public int height = -1;
		public TableHeader header = new TableHeader(false,true);
		public List<TableRow> rows = new List<TableRow>();
		public TableGUI(){
			string skin = EditorGUIUtility.isProSkin ? "Dark" : "Light";
			this.tableSkin = FileManager.GetAsset<GUISkin>("Table-" + skin + ".guiskin");
			this.tableHeaderSkin = FileManager.GetAsset<GUISkin>("TableHeader-" + skin + ".guiskin");
		}
		public void SetHeader(bool vertical=false,bool sortable=true){
			this.header = new TableHeader(vertical,sortable);
		}
		public void AddHeader(string label){
			this.header.items.Add(new TableHeaderItem(label));
		}
		public TableRow AddRow(){
			TableRow row = new TableRow();
			this.rows.Add(row);
			return row;
		}
		public void Draw(){
			GUI.skin = this.tableHeaderSkin;
			EditorGUILayout.BeginVertical();
			this.header.Draw();
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
		public void Draw(){
			EditorGUILayout.BeginHorizontal();
			foreach(TableHeaderItem item in this.items){
				item.Draw();
			}
			EditorGUILayout.EndHorizontal();
		}
	}
	public class TableHeaderItem{
		public string label;
		public static int defaultWidth = 8;
		public TableHeaderItem(string label){
			this.label = label;
			if(label.Length * 8 > defaultWidth){
				TableHeaderItem.defaultWidth = label.Length * 8;
			}
		}
		public void Draw(){
			if(label == ""){
				GUILayout.Space(TableHeaderItem.defaultWidth);
				return;
			}
			//float xOffset = (-225) + GUILayoutUtility.GetLastRect().x;
			//float yOffset = (-100) + GUILayoutUtility.GetLastRect().y;
			//GUIUtility.RotateAroundPivot(90,new Vector2(xOffset,yOffset));
			GUILayout.Label(label,GUI.skin.label);
			//GUIUtility.RotateAroundPivot(-90,new Vector2(xOffset,yOffset));
		}
	}
	public class TableRow{
		public bool selected;
		public object target;
		public List<TableField> fields = new List<TableField>();
		public void AddField(object target,OnEvent onDisplay,OnEvent onClick){
			this.fields.Add(new TableField(target,onDisplay,onClick));
		}
		public void AddField(string value,object target,TableFieldType type,OnEvent onClick){
			this.fields.Add(new TableField(value,target,type,onClick));
		}
		public void Draw(){
			EditorGUILayout.BeginHorizontal();
			foreach(TableField field in this.fields){
				field.Draw();
			}
			EditorGUILayout.EndHorizontal();
		}
	}
	public class TableField{
		public bool selected;
		public TableFieldType type;
		public object target;
		public string value;
		public OnEvent onDisplay;
		public OnEvent onClick;
		public GUIStyle style;
		public TableField(string value,object target,TableFieldType type,OnEvent onClick){
			this.value = value;
			this.target = target;
			this.type = type;
			this.onClick = onClick;
		}
		public TableField(object target,OnEvent onDisplay,OnEvent onClick){
			this.target = target;
			this.onDisplay = onDisplay;
			this.onClick = onClick;
		}
		public void Draw(){
			if(this.onDisplay != null){
				this.onDisplay(this);
			}
			else{
				if(this.type == TableFieldType.String){
					GUILayout.Label(this.value);
					if(GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && Event.current.type == EventType.ContextClick && this.onClick != null){
						this.onClick(this);
					}
				}
			}
		}
	}
	public enum TableFieldType{String,Checkbox,Switch,Color};
}
