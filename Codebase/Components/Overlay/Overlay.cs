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
	public static Matrix4x4 guiMatrix = Matrix4x4.zero;
	public static Vector2 guiScale = new Vector2(1,1);
	public static Vector2 defaultResolution;
	static Overlay(){
		if(Overlay.defaultResolution == Vector2.zero){
			Overlay.defaultResolution = new Vector2(Screen.width,Screen.height);
			#if UNITY_EDITOR 
			Overlay.defaultResolution = new Vector2(PlayerSettings.defaultScreenWidth,PlayerSettings.defaultScreenHeight);
			#endif
		}
		Events.Add("Resolution Change",Overlay.CalculateGUIMatrix);
		Overlay.CalculateGUIMatrix();
	}
	public static T Get<T>(string name){
		OverlayBase overlay = Overlay.instances.ContainsKey(name) ? Overlay.instances[name] : null;
		return (T)Convert.ChangeType(overlay,typeof(T));
	}
	public static void CalculateGUIMatrix(){
		if(Overlay.defaultResolution != Vector2.zero){
			float xScale = Overlay.guiScale.x = Screen.width / Overlay.defaultResolution.x;
			float yScale = Overlay.guiScale.y =  Screen.height / Overlay.defaultResolution.y;
			Overlay.guiMatrix = Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(xScale,yScale,1));
		}
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
			if(Overlay.guiMatrix != Matrix4x4.zero){
				GUI.matrix = Overlay.guiMatrix;
			}
		}
	}
	public virtual void UpdateRender(){
		Rect area = new Rect(this.position.x,this.position.y,this.size.x,this.size.y);
		if(this.autoScale){
			area.width = (int)(area.width * Overlay.guiScale.x);
			area.height = (int)(area.height * Overlay.guiScale.y);
		}
		this.area = area;
	}
	public void OnDrawGizmosSelected(){
		this.UpdateRender();
	}
}
