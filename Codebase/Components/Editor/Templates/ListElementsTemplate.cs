using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
namespace Zios{
    public abstract class ListElementsTemplate{
	    public UnityEngine.Object target;
	    public List<ListItem> listItems;
	    public List<ListAction> actions;
	    public SortOptions sortOptions = new SortOptions();
	    public bool shouldRepaint;
	    private List<object> currentObjects = new List<object>();
	    public ListElementsTemplate(UnityEngine.Object target){
		    this.target = target;
		    this.listItems = new List<ListItem>();
		    this.actions = new List<ListAction>();
		    this.CreateItems();
		    this.CreateActions();
	    }
	    public void Draw(){
		    this.shouldRepaint = false;
		    List<object> newObjects = this.GetList();
		    if(this.currentObjects.Count != newObjects.Count){
			    this.currentObjects = newObjects;
		    }
		    EditorGUILayout.BeginVertical();
		    EditorGUILayout.BeginHorizontal();
		    foreach(ListItem item in this.listItems){
			    if(item.title != null){
				    this.CreateHeader(item.title,item.width,item.fieldName);
			    }
		    }
		    EditorGUILayout.EndHorizontal();
		    EditorGUILayout.EndVertical();
		    EditorGUILayout.BeginVertical();
		    foreach(object targetObject in this.currentObjects){
			    EditorGUILayout.BeginHorizontal();
			    foreach(ListItem item in this.listItems){
				    item.Draw(targetObject);
			    }
			    foreach(ListAction action in this.actions){
				    action.OnAction(this.target,targetObject);
			    }
			    EditorGUILayout.EndHorizontal();
		    }
		    EditorGUILayout.EndVertical();
		    foreach(ListAction action in this.actions){
			    action.OnGlobalAction(this.target);
			    this.shouldRepaint = this.shouldRepaint || action.shouldRepaint;
		    }
	    }
	    private void CreateHeader(string title,float width,string field){
		    if(this.sortOptions.field != null && this.sortOptions.field.Equals(field)){
			    string direction = this.sortOptions.orientation == SortOrientation.Ascending ? "↑" : "↓";
			    title += string.Format(" {0}",direction);
		    }
		    EditorGUILayout.LabelField(new GUIContent(title),GUILayout.Width(width));
		    if(GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && Event.current.type == EventType.mouseDown){
			    this.sortOptions.Setup(field);
			    this.currentObjects.Sort(this.sortOptions);
			    shouldRepaint = true;
		    }
	    }
	    public abstract void CreateItems();
	    public abstract void CreateActions();
	    public abstract List<object> GetList();
    }
    public enum ItemTypes{
	    Label,
	    Float,
	    Enumeration}
    ;
    public class ListItem{
	    public string title;
	    public string fieldName;
	    public float width;
	    public ItemTypes type;
	    public ListItem(string title,string fieldName,float width,ItemTypes type){
		    this.title = title;
		    this.fieldName = fieldName;
		    this.width = width;
		    this.type = type;
	    }
	    public void Draw(object target){
		    FieldInfo field = target.GetType().GetField(this.fieldName);
		    switch(this.type){
			    case ItemTypes.Float:
				    field.SetValue(target,EditorGUILayout.FloatField((float)field.GetValue(target),GUILayout.Width(this.width)));
				    break;
			    case ItemTypes.Enumeration:
				    field.SetValue(target,EditorGUILayout.EnumPopup((System.Enum)field.GetValue(target),GUILayout.Width(this.width)));
				    break;
			    default:
				    EditorGUILayout.LabelField(new GUIContent((string)field.GetValue(target)),GUILayout.Width(this.width));
				    break;
		    }
	    }
    }
    public abstract class ListAction{
	    public bool shouldRepaint;
	    public abstract void OnAction(UnityEngine.Object target,object targetItem);
	    public virtual void OnGlobalAction(UnityEngine.Object target){
	    }
    }
}