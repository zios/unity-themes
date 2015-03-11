using UnityEngine;
using UnityEditor;
using System.Collections;
using Zios.FPS;
namespace Zios{
    [CustomEditor(typeof(EditorFPS))]
    public class EditorFPSEditor : Editor{
	    public string text;
	    private int frames = 0;
	    private float nextUpdate;
	    private GUISkin skin;
	    public override void OnInspectorGUI(){
			if(!Event.current.IsUseful()){return;}
		    Utility.AddEditorUpdate(this.Step,true);	
		    string skinName = EditorGUIUtility.isProSkin ? "Dark" : "Light";
		    if(this.skin == null || !this.skin.name.Contains(skinName)){
			    this.skin = FileManager.GetAsset<GUISkin>("Gentleface-" + skinName + ".guiskin");
		    }
		    GUI.skin = this.skin;
		    this.text.DrawLabel(GUI.skin.GetStyle("LargeLabel"));
	    }
	    public void Step(){
		    if(this.target.IsNull()){
			    Utility.RemoveEditorUpdate(this.Step);
			    return;
		    }
		    this.frames += 1;
		    if(Time.realtimeSinceStartup >= this.nextUpdate){
			    string color = EditorGUIUtility.isProSkin ? "white" : "black";
			    string frameText = "<color="+color+">" + this.frames.ToString() + "</color>";
			    this.nextUpdate = Time.realtimeSinceStartup + 0.5f;
			    this.text = frameText + " fps";
			    this.frames = 0;
			    this.Repaint();
		    }
	    }
    }
}