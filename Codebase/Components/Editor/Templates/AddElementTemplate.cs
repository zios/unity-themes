using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
namespace Zios{
    public abstract class AddElementTemplate{
	    public UnityEngine.Object target;
	    public string title;
	    public List<Selectbox> selectboxes;
	    public ListElementsTemplate list;
	    public AddElementTemplate(string title,UnityEngine.Object target,ListElementsTemplate list){
		    this.title = title;
		    this.target = target;
		    this.list = list;
		    this.selectboxes = new List<Selectbox>();
		    this.CreateSelectboxes();
	    }
	    public void Draw(){
		    this.UpdateSelectboxes();
		    EditorGUILayout.LabelField(new GUIContent(this.title));
		    float labelWidth = 142f;
		    this.list.Draw();
		    EditorGUILayout.BeginHorizontal();
		    if(GUILayout.Button("Add",GUILayout.Width(labelWidth))){
			    this.AddElement();
		    }
		    foreach(Selectbox box in this.selectboxes){
			    box.Draw();
		    }
		    DrawCustomElements();
		    EditorGUILayout.EndHorizontal();
	    }
	    public abstract void CreateSelectboxes();
	    public virtual void DrawCustomElements(){
	    }
	    public abstract void AddElement();
	    public abstract void UpdateSelectboxes();
    }
    public class Selectbox{
	    public int index;
	    public int previousIndex;
	    public string[] options;
	    public float width;
	    public Selectbox(float width){
		    this.width = width;
		    this.options = new string[0];
		    this.previousIndex = -1;
	    }
	    public bool Changed(){
		    return this.index != this.previousIndex;
	    }
	    public void Draw(){
		    this.previousIndex = this.index;
		    this.index = EditorGUILayout.Popup(this.index,this.options,GUILayout.Width(this.width));
		    if(GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && Event.current.type == EventType.mouseDown){
			    this.OnClick();
		    }
	    }
	    public virtual void OnClick(){
	    }
    }
}