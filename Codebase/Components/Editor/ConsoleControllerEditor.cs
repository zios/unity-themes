using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Zios.Editor;
using ConsoleCallback = Zios.ConsoleCallback;
using Cvar = Zios.Cvar;
[CustomEditor(typeof(ConsoleController))]
public class ConsoleControllerEditor : Editor{
	private TableGUI cvarTable;
	private TableGUI shortcutTable;
	private TableGUI keywordTable;
	private ConsoleController controller ;
	private TableField selectedField;
	private int selectedColumn;
	public string removeRow;
	public void OnEnable(){
		this.cvarTable = new TableGUI();
		this.cvarTable.SetHeader(false,true,this.CompareRows);
		this.cvarTable.header.spaced = false;
		this.cvarTable.showEmptiesButton = false;
		this.cvarTable.tableTitle = "Cvars";
		shortcutTable = new TableGUI();
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
		this.controller.populateTest();
		if(!Application.isPlaying){
			this.BuildTables();
		}
	}
	public override void OnInspectorGUI(){
		this.cvarTable.Draw();
		if(GUILayout.Button("New Cvar")){
			CreateCvar();
		}
		this.shortcutTable.Draw();
		if(GUILayout.Button("New Shortcut")){
			CreateShortcut();
		}
		this.keywordTable.Draw();
		if(GUILayout.Button("New Keyword")){
			CreateKeyword();
		}
		this.Repaint();
		if(GUI.changed){
			EditorUtility.SetDirty(this.target);
		}
	}
	public void BuildTables(){
		this.cvarTable.AddHeader("Cvar",this.DisplayHeader);
		this.cvarTable.AddHeader("Alias",this.DisplayHeader);
		this.cvarTable.AddHeader("Target",this.DisplayHeader);
		this.cvarTable.AddHeader("Label",this.DisplayHeader);
		this.cvarTable.AddHeader("Help",this.DisplayHeader);
		this.cvarTable.AddHeader("Target Method",this.DisplayHeader);
		foreach(string cvarName in controller.cvars.Keys){
			TableRow tableRow = this.cvarTable.AddRow();
			tableRow.AddField(cvarName,this.OnDisplayCvar,this.OnClickCvar);
		}
		this.shortcutTable.AddHeader("Shortcut",this.DisplayHeader);
		this.shortcutTable.AddHeader("Replaced Command",this.DisplayHeader);
		foreach(string shortcut in controller.shortcuts.Keys){
			TableRow tableRow = this.shortcutTable.AddRow();
			tableRow.AddField(shortcut,this.OnDisplayShortcut,this.OnClickShortcut);
		}
		this.keywordTable.AddHeader("Keyword",this.DisplayHeader);
		this.keywordTable.AddHeader("Method",this.DisplayHeader);
		this.keywordTable.AddHeader("Cvar Method",this.DisplayHeader);
		this.keywordTable.AddHeader("Minimum Parameters",this.DisplayHeader);
		this.keywordTable.AddHeader("Help",this.DisplayHeader);
		foreach(string keyword in controller.keywords.Keys){
			TableRow tableRow = this.keywordTable.AddRow();
			tableRow.AddField(keyword,this.OnDisplayKeyword,this.OnClickKeyword);
		}
	}
	public void DisplayHeader(TableHeaderItem headerField){
		GUIStyle style = GUI.skin.customStyles[2];
		GUIContent content = new GUIContent(headerField.label + headerField.orientation);
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
	public void OnDisplayCvar(TableField field){
		if(field.target is string){
			string name = (string)field.target;
			string newName = name;
			int columnNumber = 0;
			if(this.selectedField == field && selectedColumn == columnNumber){
				newName = GUILayout.TextField(name);
				if(!name.Equals(newName)){
					Cvar oldCvar = controller.cvars[name];
					controller.cvars.Remove(name);
					controller.cvars.Add(newName,oldCvar);
					field.target = newName;
				}
				CheckKeyboard();
			}
			else{
				OnDisplayField(field,name,columnNumber);
			}
			Cvar cvar = controller.cvars[newName];
			columnNumber = 1;
			if(this.selectedField == field && selectedColumn == columnNumber){
				cvar.name = GUILayout.TextField(cvar.name);
				CheckKeyboard();
			}
			else{
				OnDisplayField(field,cvar.name,columnNumber);
			}
			columnNumber = 2;
			string scopeLabel = cvar.scope.ToString();
			if(cvar.scope is Component){
				scopeLabel = cvar.scope.GetType().Name;
			}
			if(this.selectedField == field && selectedColumn == columnNumber){
				scopeLabel = GUILayout.TextField(scopeLabel);
				Type staticType = this.LoadType(scopeLabel);
				Component component = controller.GetComponent(scopeLabel);
				if(staticType != null){
					cvar.scope = staticType;
				}
				else if(component != null){
					cvar.scope = component;
				}
				else{
					cvar.scope = scopeLabel;
				}
				CheckKeyboard();
			}
			else{
				GUIStyle style = GUI.skin.label;
				if(cvar.scope is string){
					style = GUI.skin.GetStyle("labelError");
				}
				else{
					style = GUI.skin.GetStyle("labelSuccess");
				}
				OnDisplayField(field,scopeLabel,columnNumber,style);
			}
			columnNumber = 3;
			if(this.selectedField == field && selectedColumn == columnNumber){
				cvar.fullName = GUILayout.TextField(cvar.fullName);
				CheckKeyboard();
			}
			else{
				OnDisplayField(field,cvar.fullName,columnNumber);
			}
			columnNumber = 4;
			if(this.selectedField == field && selectedColumn == columnNumber){
				cvar.help = GUILayout.TextField(cvar.help);
				CheckKeyboard();
			}
			else{
				OnDisplayField(field,cvar.help,columnNumber);
			}
			columnNumber = 5;
			this.OnDisplayMethod(cvar.method,field,columnNumber);
			controller.cvars[newName] = cvar;
		}

	}
	private void OnDisplayMethod(ConsoleCallback callback,TableField field,int columnNumber){
		if(this.selectedField == field && selectedColumn == columnNumber){
			/*if(callback != null && callback.basic != null){
					callback.basic = GUILayout.TextField(callback.basic);
				}
				else if(callback != null && callback.full != null){
					callback.full = GUILayout.TextField(callback.full);
				}
				else{
					//TODO: Ajustar
					callback.full = "";
					callback.full = GUILayout.TextField(callback.full);
				}*/
		}
		else{
			string method = "";
			if(callback.simple != null){
				method = callback.simple.Method.Name;
			}
			else if(callback.basic != null){
				method = callback.basic.Method.Name;
			}
			else if(callback.full != null){
				method = callback.full.Method.Name;
			}
			if(method == "HandleCvar"){
				method = "<Default>";
			}
			OnDisplayField(field,method,columnNumber);
		}
	}
	public void OnDisplayShortcut(TableField field){
		if(field.target is string){
			string name = (string)field.target;
			string newName = name;
			int columnNumber = 0;
			if(this.selectedField == field && selectedColumn == columnNumber){
				newName = GUILayout.TextField(name);
				if(!name.Equals(newName)){
					string shortcutReplacement = controller.shortcuts[name];
					controller.shortcuts.Remove(name);
					controller.shortcuts.Add(newName,shortcutReplacement);
					field.target = newName;
				}
				CheckKeyboard();
			}
			else{
				OnDisplayField(field,name,columnNumber);
			}
			columnNumber = 1;
			if(this.selectedField == field && selectedColumn == columnNumber){
				controller.shortcuts[newName] = GUILayout.TextField(controller.shortcuts[newName]);
				CheckKeyboard();
			}
			else{
				OnDisplayField(field,controller.shortcuts[newName],columnNumber);
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
					ConsoleCallback oldCallback = controller.keywords[name];
					controller.keywords.Remove(name);
					controller.keywords.Add(newName,oldCallback);
					field.target = newName;
				}
				CheckKeyboard();
			}
			else{
				OnDisplayField(field,name,columnNumber);
			}
			ConsoleCallback callback = controller.keywords[newName];
			columnNumber = 1;
			ConsoleCallback newCallback = new ConsoleCallback();
			newCallback.simple = callback.simple;
			this.OnDisplayMethod(newCallback,field,columnNumber);
			callback.simple = newCallback.simple;
			columnNumber = 2;
			newCallback.simple = null;
			newCallback.basic = callback.basic;
			newCallback.full = callback.full;
			this.OnDisplayMethod(newCallback,field,columnNumber);
			callback.basic = newCallback.basic;
			callback.full = newCallback.full;
			columnNumber = 3;
			if(this.selectedField == field && selectedColumn == columnNumber){
				callback.minimumParameters = EditorGUILayout.IntField(callback.minimumParameters);
				CheckKeyboard();
			}
			else{
				OnDisplayField(field,callback.minimumParameters.ToString(),columnNumber);
			}
			columnNumber = 4;
			if(callback.help == null){
				callback.help = "";
			}
			if(this.selectedField == field && selectedColumn == columnNumber){
				callback.help = GUILayout.TextField(callback.help);
				CheckKeyboard();
			}
			else{
				OnDisplayField(field,callback.help,columnNumber);
			}
			controller.keywords[newName] = callback;
		}
	}
	public void CreateCvar(){
		string cvarName = "NEW";
		if(!controller.cvars.ContainsKey(cvarName)){
			Cvar cvar = new Cvar();
			cvar.fullName = "";
			cvar.help = "";
			cvar.name = "";
			cvar.scope = "";
			cvar.method = new ConsoleCallback();
			controller.cvars.Add(cvarName,cvar);
			TableRow tableRow = this.cvarTable.AddRow();
			tableRow.AddField(cvarName,this.OnDisplayCvar,this.OnClickCvar);
		}
	}
	public void CreateShortcut(){
		string shortcutName = "NEW";
		if(!controller.shortcuts.ContainsKey(shortcutName)){
			controller.shortcuts.Add(shortcutName,"");
			TableRow tableRow = this.shortcutTable.AddRow();
			tableRow.AddField(shortcutName,this.OnDisplayShortcut,this.OnClickShortcut);
		}
	}
	public void CreateKeyword(){
		string keywordName = "NEW";
		if(!controller.keywords.ContainsKey(keywordName)){
			ConsoleCallback callback = new ConsoleCallback();
			callback.help = "";
			controller.keywords.Add(keywordName,callback);
			TableRow tableRow = this.keywordTable.AddRow();
			tableRow.AddField(keywordName,this.OnDisplayKeyword,this.OnClickKeyword);
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
		IDictionary collection = null;
		TableGUI table = null;
		if(this.removeRow == "cvar"){
			collection = controller.cvars;
			table = this.cvarTable;
		}
		else if(this.removeRow == "shortcut"){
			collection = controller.shortcuts;
			table = this.shortcutTable;
		}
		else if(this.removeRow == "keyword"){
			collection = controller.keywords;
			table = this.keywordTable;
		}
		TableField field = (TableField)target;
		string name = (string)field.target;
		collection.Remove(name);
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
