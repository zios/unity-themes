using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Zios.Editor;
using ConsoleData = ConsoleController.ConsoleData;
using ConsoleMethod = Zios.ConsoleMethod;
using ConsoleMethodFull = Zios.ConsoleMethodFull;
[CustomEditor(typeof(ConsoleController))]
public class ConsoleControllerEditor : Editor{
	private TableGUI cvarTable;
	private TableGUI shortcutTable;
	private TableGUI keywordTable;
	private ConsoleController controller;
	private TableField selectedField;
	private int selectedColumn;
	public string removeRow;
	public void OnEnable(){
		this.cvarTable = new TableGUI();
		this.cvarTable.SetHeader(false,true,this.CompareRows);
		this.cvarTable.header.spaced = false;
		this.cvarTable.showEmptiesButton = false;
		this.cvarTable.tableTitle = "Cvars";
		this.shortcutTable = new TableGUI();
		this.shortcutTable.SetHeader(false,true,this.CompareRows);
		this.shortcutTable.header.spaced = false;
		this.shortcutTable.showEmptiesButton = false;
		this.shortcutTable.tableTitle = "Shortcuts";
		this.keywordTable = new TableGUI();
		this.keywordTable.SetHeader(false,true,this.CompareRows);
		this.keywordTable.header.spaced = false;
		this.keywordTable.showEmptiesButton = false;
		this.keywordTable.tableTitle = "Keywords";
		this.controller = (ConsoleController)target;
		this.BuildTables();
	}
	public override void OnInspectorGUI(){
		if(this.cvarTable.rows.Count > 0){
			this.cvarTable.Draw();
		}
		GUIStyle style = GUI.skin.GetStyle("addButton");     
		if(GUILayout.Button("New Cvar",style)){
			this.CreateCvar();
		}
		if(this.shortcutTable.rows.Count > 0){
		this.shortcutTable.Draw();
		}
		if(GUILayout.Button("New Shortcut",style)){
			this.CreateShortcut();
		}
		if(this.keywordTable.rows.Count > 0){
		this.keywordTable.Draw();
		}
		if(GUILayout.Button("New Keyword",style)){
			this.CreateKeyword();
		}
		this.Repaint();
		if(GUI.changed){
			EditorUtility.SetDirty(this.target);
		}
	}
	public void BuildTables(){
		this.cvarTable.AddHeader("Name","The term typed into console.",this.DisplayHeader);
		this.cvarTable.AddHeader("Full Name","The spaced, pretty full name.",this.DisplayHeader);
		this.cvarTable.AddHeader("Target Object","The target object scope where attribute is.",this.DisplayHeader);
		this.cvarTable.AddHeader("Target Attribute","The target attribute from the scope.",this.DisplayHeader);
		this.cvarTable.AddHeader("Callback","A callback method that occurs after cvar has been get/set.",this.DisplayHeader);
		this.cvarTable.AddHeader("Help","The help string.",this.DisplayHeader);
		foreach(ConsoleData data in this.controller.cvars){
			data.ValidateScope(this.controller);
			data.ValidateAttribute();
			TableRow tableRow = this.cvarTable.AddRow();
			tableRow.AddField(data.key,this.OnDisplayCvar,this.OnClickCvar);
		}
		this.shortcutTable.AddHeader("Replace","The original command.",this.DisplayHeader);
		this.shortcutTable.AddHeader("With","The shortcut for the command.",this.DisplayHeader);
		foreach(ConsoleData data in this.controller.shortcuts){
			TableRow tableRow = this.shortcutTable.AddRow();
			tableRow.AddField(data.key,this.OnDisplayShortcut,this.OnClickShortcut);
		}
		this.keywordTable.AddHeader("Name","",this.DisplayHeader);
		this.keywordTable.AddHeader("Target Object","The target object scope where attribute is.",this.DisplayHeader);
		this.keywordTable.AddHeader("Callback","A callback method that occurs after cvar has been get/set.",this.DisplayHeader);
		this.keywordTable.AddHeader("Minimum Parameters","",this.DisplayHeader);
		this.keywordTable.AddHeader("Help","The help string.",this.DisplayHeader);
		foreach(ConsoleData data in this.controller.keywords){
			data.ValidateScope(this.controller);
			data.ValidateAttribute();
			data.ValidateMethod();
			TableRow tableRow = this.keywordTable.AddRow();
			tableRow.AddField(data.key,this.OnDisplayKeyword,this.OnClickKeyword);
		}
	}
	public void DisplayHeader(TableHeaderItem headerField){
		GUIStyle style = GUI.skin.GetStyle("header");
		GUIContent content = new GUIContent(headerField.label + headerField.orientation, headerField.tooltip);
		GUILayout.Label(content,style);
		if(TableGUI.CheckRegion()){
			headerField.onSort(headerField);
			if(headerField.onClick != null){
				headerField.onClick(headerField);
			}
		}
	}
	private void OnDisplayField(TableField field,string label,int columnNumber,GUIStyle style = null){
		if(style == null){
			GUILayout.Label(label);
		}
		else{
			GUILayout.Label(label,style);
		}
		if(TableGUI.CheckRegion()){
			if(Event.current.button == 0){
				this.selectedField = field;
				this.selectedColumn = columnNumber;
			}
		}
		field.CheckClick();
	}
	private void CheckKeyboard(){
		if(Event.current.type == EventType.keyUp){
			if(Event.current.keyCode == KeyCode.Return){
				this.selectedColumn = -1;
			}
		}
	}
	public ConsoleData FindData(string key,List<ConsoleData> entities){
		foreach(ConsoleData data in entities){
			if(data.key == key){
				return data;
			}
		}
		return null;
	}
	public void OnDisplayCvar(TableField field){
		if(field.target is string){
			string name = (string)field.target;
			string newName = name;
			int columnNumber = 0;
			if(this.selectedField == field && selectedColumn == columnNumber){
				newName = GUILayout.TextField(name);
				if(!name.Equals(newName)){
					ConsoleData data = this.FindData(name,this.controller.cvars);
					data.key = newName;
					field.target = newName;
				}
				this.CheckKeyboard();
			}
			else{
				this.OnDisplayField(field,name,columnNumber);
			}
			ConsoleData cvarData = this.FindData(newName,this.controller.cvars);
			columnNumber = 1;
			if(this.selectedField == field && selectedColumn == columnNumber){
				cvarData.fullName = GUILayout.TextField(cvarData.fullName);
				this.CheckKeyboard();
			}
			else{
				this.OnDisplayField(field,cvarData.fullName,columnNumber);
			}
			columnNumber = 2;
			if(this.selectedField == field && selectedColumn == columnNumber){
				cvarData.scopeName = GUILayout.TextField(cvarData.scopeName);
				this.CheckKeyboard();
				cvarData.ValidateScope(this.controller);
			}
			else{
				GUIStyle style = GUI.skin.label;
				if(cvarData.scope == null){
					style = GUI.skin.GetStyle("labelError");
				}
				else{
					style = GUI.skin.GetStyle("labelSuccess");
				}
				this.OnDisplayField(field,cvarData.scopeName,columnNumber,style);
			}
			columnNumber = 3;
			if(this.selectedField == field && selectedColumn == columnNumber){
				cvarData.name = GUILayout.TextField(cvarData.name);
				this.CheckKeyboard();
				cvarData.ValidateAttribute();
			}
			else{
				GUIStyle style = GUI.skin.label;
				if(!cvarData.validName){
					style = GUI.skin.GetStyle("labelError");
				}
				else{
					style = GUI.skin.GetStyle("labelSuccess");
				}
				this.OnDisplayField(field,cvarData.name,columnNumber,style);
			}
			columnNumber = 4;
			this.OnDisplayMethod(cvarData,field,columnNumber);
			columnNumber = 5;
			if(this.selectedField == field && selectedColumn == columnNumber){
				cvarData.help = GUILayout.TextField(cvarData.help);
				this.CheckKeyboard();
			}
			else{
				this.OnDisplayField(field,cvarData.help,columnNumber);
			}
		}

	}
	private void OnDisplayMethod(ConsoleData data,TableField field,int columnNumber){
		if(this.selectedField == field && selectedColumn == columnNumber){
			if(data.methodName == null){
				data.methodName = "";
			}
			data.methodName = GUILayout.TextField(data.methodName);
			this.CheckKeyboard();
			data.ValidateMethod();
		}
		else{
			string method = data.methodName;
			if(method == "HandleCvar"){
				method = "<Default>";
			}
			GUIStyle style = GUI.skin.label;
			if(data.methodName != null && data.methodName.Trim() != "" && (data.basic == null && data.simple == null && data.full == null)){
				style = GUI.skin.GetStyle("labelError");
			}
			else{
				style = GUI.skin.GetStyle("labelSuccess");
			}
			this.OnDisplayField(field,method,columnNumber,style);
		}
	}
	public void OnDisplayShortcut(TableField field){
		if(field.target is string){
			string name = (string)field.target;
			ConsoleData shortcutData = this.FindData(name,this.controller.shortcuts);
			int columnNumber = 0;
			if(this.selectedField == field && selectedColumn == columnNumber){
				shortcutData.replace = GUILayout.TextField(shortcutData.replace);
				this.CheckKeyboard();
			}
			else{
				this.OnDisplayField(field,shortcutData.replace,columnNumber);
			}
			string newName = name;
			columnNumber = 1;
			if(this.selectedField == field && selectedColumn == columnNumber){
				newName = GUILayout.TextField(name);
				if(!name.Equals(newName)){
					ConsoleData data = this.FindData(name,this.controller.shortcuts);
					data.key = newName;
					field.target = newName;
				}
				this.CheckKeyboard();
			}
			else{
				this.OnDisplayField(field,name,columnNumber);
			}

		}
	}
	public void OnDisplayKeyword(TableField field){
		if(field.target is string){
			string name = (string)field.target;
			string newName = name;
			int columnNumber = 0;
			if(this.selectedField == field && selectedColumn == columnNumber){
				newName = GUILayout.TextField(name);
				if(!name.Equals(newName)){
					ConsoleData data = this.FindData(name,this.controller.keywords);
					data.key = newName;
					field.target = newName;
				}
				this.CheckKeyboard();
			}
			else{
				this.OnDisplayField(field,name,columnNumber);
			}
			ConsoleData keywordData = this.FindData(newName,this.controller.keywords);
			columnNumber = 1;
			if(this.selectedField == field && selectedColumn == columnNumber){
				keywordData.scopeName = GUILayout.TextField(keywordData.scopeName);
				this.CheckKeyboard();
				keywordData.ValidateScope(this.controller);
			}
			else{
				GUIStyle style = GUI.skin.label;
				if(keywordData.scope == null){
					style = GUI.skin.GetStyle("labelError");
				}
				else{
					style = GUI.skin.GetStyle("labelSuccess");
				}
				this.OnDisplayField(field,keywordData.scopeName,columnNumber,style);
			}
			columnNumber = 2;
			this.OnDisplayMethod(keywordData,field,columnNumber);
			columnNumber = 3;
			if(this.selectedField == field && selectedColumn == columnNumber){
				keywordData.minimumParameters = EditorGUILayout.IntField(keywordData.minimumParameters);
				this.CheckKeyboard();
			}
			else{
				this.OnDisplayField(field,keywordData.minimumParameters.ToString(),columnNumber);
			}
			columnNumber = 4;
			if(keywordData.help == null){
				keywordData.help = "";
			}
			if(this.selectedField == field && selectedColumn == columnNumber){
				keywordData.help = GUILayout.TextField(keywordData.help);
				this.CheckKeyboard();
			}
			else{
				this.OnDisplayField(field,keywordData.help,columnNumber);
			}
		}
	}
	public void CreateCvar(){
		string key = "NEW";
		if(this.FindData(key,controller.cvars) == null){
			ConsoleData data = new ConsoleData();
			data.key = key;
			data.fullName = "";
			data.help = "";
			data.name = "";
			data.scopeName = "";
			data.methodName = "";
			this.controller.cvars.Add(data);
			TableRow tableRow = this.cvarTable.AddRow();
			tableRow.AddField(key,this.OnDisplayCvar,this.OnClickCvar);
		}
	}
	public void CreateShortcut(){
		string key = "NEW";
		if(this.FindData(key,controller.shortcuts) == null){
			ConsoleData data = new ConsoleData();
			data.key = key;
			data.replace = "NEW";
			this.controller.shortcuts.Add(data);
			TableRow tableRow = this.shortcutTable.AddRow();
			tableRow.AddField(key,this.OnDisplayShortcut,this.OnClickShortcut);
		}
	}
	public void CreateKeyword(){
		string key = "NEW";
		if(this.FindData(key,controller.keywords) == null){
			ConsoleData data = new ConsoleData();
			data.key = key;
			data.help = "";
			data.scopeName = "";
			data.methodName = "";
			this.controller.keywords.Add(data);
			TableRow tableRow = this.keywordTable.AddRow();
			tableRow.AddField(key,this.OnDisplayKeyword,this.OnClickKeyword);
		}
	}
	public void OnClickCvar(TableField field){
		this.removeRow = "cvar";
		this.OnClickRow(field);
	}
	public void OnClickShortcut(TableField field){
		this.removeRow = "shortcut";
		this.OnClickRow(field);
	}
	public void OnClickKeyword(TableField field){
		this.removeRow = "keyword";
		this.OnClickRow(field);
	}
	public void OnClickRow(TableField field){
		if(Event.current.button == 1){
			GenericMenu menu = new GenericMenu();
			GUIContent remove = new GUIContent("Remove '" + field.target.ToString() + "'");
			menu.AddItem(remove,false,new GenericMenu.MenuFunction2(this.RemoveRow),field);
			menu.ShowAsContext();
		}
		Event.current.Use();
	}
	public void RemoveRow(object target){
		List<ConsoleData> collection = null;
		TableGUI table = null;
		if(this.removeRow == "cvar"){
			collection = this.controller.cvars;
			table = this.cvarTable;
		}
		else if(this.removeRow == "shortcut"){
			collection = this.controller.shortcuts;
			table = this.shortcutTable;
		}
		else if(this.removeRow == "keyword"){
			collection = this.controller.keywords;
			table = this.keywordTable;
		}
		TableField field = (TableField)target;
		string name = (string)field.target;
		collection.Remove(this.FindData(name,collection));
		int totalRows = table.rows.Count;
		for(int i = 0;i < totalRows;i++){
			TableRow row = table.rows[i];
			if(row.fields[0] == field){
				table.rows.Remove(row);
				break;
			}
		}
	}
	public int CompareRows(object target1,object target2){
		string row1 = (string)target1;
		string row2 = (string)target2;
		return row1.CompareTo(row2);
	}
}
