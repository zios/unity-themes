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
	public float labelSize;
	public GUISkin tableSkin;
	public GUISkin tableHeaderSkin;
	public TableTemplate(UnityEngine.Object target){
		string skin = EditorGUIUtility.isProSkin ? "Dark" : "Light";
		this.target = target;
		this.headers = new List<string>();
		this.tableItems = new List<TableRow>(); 
		this.tableSkin = FileManager.GetAsset<GUISkin>("Table-"+skin+".guiskin");
		this.tableHeaderSkin = FileManager.GetAsset<GUISkin>("TableHeader-"+skin+".guiskin");
		this.CreateHeaders();
	}
	public void Draw(){
		this.shouldRepaint = false;
		this.CreateItems();
		EditorGUILayout.BeginVertical();
		EditorGUILayout.BeginHorizontal();
		GUI.skin = this.tableHeaderSkin;
		for(int index=0;index<this.headers.Count;++index){
			this.CreateHeader(this.headers[index],index);
		}
		EditorGUILayout.EndHorizontal();
		GUI.skin = this.tableSkin;
		foreach(TableRow item in this.tableItems){
			EditorGUILayout.BeginHorizontal();
			item.Draw(headers,this.labelSize);
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
	private void CreateHeader(string header,int offset=0){
		if(header == ""){
			GUILayout.Space(this.labelSize);
			return;
		}
		//float xOffset = (-225) + GUILayoutUtility.GetLastRect().x;
		//float yOffset = (-100) + GUILayoutUtility.GetLastRect().y;
		//GUIUtility.RotateAroundPivot(90,new Vector2(xOffset,yOffset));
		GUILayout.Label(header,GUI.skin.label);
		//GUIUtility.RotateAroundPivot(-90,new Vector2(xOffset,yOffset));
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
	public void Draw(List<string> headers,float labelSize){
		GUILayout.Label(label);
		if(GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && Event.current.type == EventType.ContextClick){
			this.CheckContext();
			this.shouldRepaint = true;
		}
		foreach(string state in headers){
			if(!string.IsNullOrEmpty(state)){
				string symbol = " ";
				GUIStyle style = GUI.skin.button;
				if(positiveChecks.Contains(state)){
					symbol = "✓";
					if(GUI.skin.FindStyle("buttonOn") != null){
						style = GUI.skin.GetStyle("buttonOn");
					}
				}
				else if(negativeChecks.Contains(state)){
					symbol = "X";
					if(GUI.skin.FindStyle("buttonOff") != null){
						style = GUI.skin.GetStyle("buttonOff");

					}
				}
				if(GUILayout.Button(new GUIContent(symbol),style)){
					this.Toggle(state);
					this.shouldRepaint = true;
				}
			}
		}
	}
	public abstract void PopulateChecks();
	public abstract void Toggle(string state);
	public abstract void CheckContext();
}
