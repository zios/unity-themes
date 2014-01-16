#if UNITY_EDITOR 
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public static class Overlay{
	public static OverlayBase Get(string name){return Overlay.instances.ContainsKey(name) ? Overlay.instances[name] : null;}
	public static Dictionary<string,OverlayBase> instances = new Dictionary<string,OverlayBase>();
	public static Matrix4x4 guiMatrix;
	public static float[] guiScale = new float[2]{1,1};
	public static float[] defaultResolution;
	static Overlay(){
		Overlay.defaultResolution = new float[2]{Screen.width,Screen.height};
		#if UNITY_EDITOR 
		Overlay.defaultResolution = new float[2]{PlayerSettings.defaultScreenWidth,PlayerSettings.defaultScreenHeight};
		#endif
		Events.Add("OnResolutionChange",Overlay.CalculateGUIMatrix);
		Overlay.CalculateGUIMatrix();
	}
	public static T Get<T>(string name){
		OverlayBase overlay = Overlay.instances.ContainsKey(name) ? Overlay.instances[name] : null;
		return (T)Convert.ChangeType(overlay,typeof(T));
	}
	public static void CalculateGUIMatrix(){
		float xScale = Overlay.guiScale[0] = Screen.width / Overlay.defaultResolution[0];
		float yScale = Overlay.guiScale[1] =  Screen.height / Overlay.defaultResolution[1];
		Overlay.guiMatrix = Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(xScale,yScale,1));
	}
}
[AddComponentMenu("")]
public class OverlayBase : MonoBehaviour{
	public bool visible = true;
	public new string name;
	public bool autoScale = true;
	public int depth = 1000;
	public Vector2 position;
	public Vector2 size;
	[NonSerialized] public Rect area;
	public virtual void Start(){
		this.OnDrawGizmosSelected();
		if(this.name != ""){Overlay.instances[this.name] = this;}
	}
	public virtual void OnDestroy(){
		if(Overlay.Get(this.name) != null){
			Overlay.instances.Remove(this.name);
		}
	}
	public virtual void OnGUI(){
		GUI.depth = this.depth;
		if(Event.current.type == EventType.Repaint && this.visible){
			if(!Application.isPlaying){this.UpdateRender();}
			GUI.matrix = Overlay.guiMatrix;
		}
	}
	public virtual void UpdateRender(){
		Rect area = new Rect(this.position.x,this.position.y,this.size.x,this.size.y);
		if(this.autoScale){
			area.width = (int)(area.width * Overlay.guiScale[0]);
			area.height = (int)(area.height * Overlay.guiScale[1]);
		}
		this.area = area;
	}
	public void OnDrawGizmosSelected(){
		this.UpdateRender();
	}
}
