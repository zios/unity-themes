using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
public abstract class TableTemplate{
	public UnityEngine.Object target;
	public List<TableRow> tableItems;
	public List<string> headers;
	public bool shouldRepaint;
	public float labelSize = 0;
	public float labelWidth;
	public GUIStyle customStyle = new GUIStyle();
	public TableTemplate(UnityEngine.Object target){
		this.target = target;
		this.headers = new List<string>();
		this.tableItems = new List<TableRow>();
		this.CreateHeaders();
		this.customStyle.wordWrap = true;
		this.customStyle.fixedWidth = 1;
		this.customStyle.alignment = TextAnchor.LowerRight;
		this.customStyle.fixedHeight = this.labelSize * 13;
		this.labelWidth = this.labelSize * 8;
	}
	public void Draw(){
		this.shouldRepaint = false;
		this.CreateItems();
		EditorGUILayout.BeginVertical();
		EditorGUILayout.BeginHorizontal();
		foreach(string header in this.headers){
			this.CreateHeader(header);
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();
		EditorGUILayout.BeginVertical();
		foreach(TableRow item in this.tableItems){
			EditorGUILayout.BeginHorizontal();
			item.Draw(headers,this.labelWidth);
			if(item.shouldRepaint){
				this.shouldRepaint = true;
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();
		if(GUI.changed){
			EditorUtility.SetDirty(target);
		}
	}
	private void CreateHeader(string header){
		if(string.IsNullOrEmpty(header)){
			EditorGUILayout.LabelField(new GUIContent(header),GUILayout.Width(this.labelWidth));
		}
		else{
			EditorGUILayout.LabelField(new GUIContent(header),this.customStyle);
		}
	}
	public abstract void CreateHeaders();
	public abstract void CreateItems();
}
public abstract class TableRow{
	public string label;
	public List<string> positiveChecks;
	public List<string> negativeChecks;
	public object target;
	public bool allowNegative;
	public bool shouldRepaint;
	public TableRow(string label,bool allowNegative,object target){
		this.label = label;
		this.positiveChecks = new List<string>();
		this.negativeChecks = new List<string>(); 
		this.allowNegative = allowNegative;
		this.target = target;
		this.PopulateChecks();
	}
	public void Draw(List<string> headers,float labelWidth){
		EditorGUILayout.LabelField(new GUIContent(label),GUILayout.Width(labelWidth - 11));
		if(GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && Event.current.type == EventType.ContextClick){
			this.CheckContext();
			this.shouldRepaint = true;
		}
		foreach(string state in headers){
			if(!string.IsNullOrEmpty(state)){
				string symbol = " ";
				if(positiveChecks.Contains(state)){
					symbol = "✓";
				}
				else if(negativeChecks.Contains(state)){
					symbol = "x";
				}
				if(GUILayout.Button(new GUIContent(symbol),GUILayout.Width(24))){
					this.Toogle(state);
					this.shouldRepaint = true;
				}
			}
		}
	}
	public abstract void PopulateChecks();
	public abstract void Toogle(string state);
	public abstract void CheckContext();
}
