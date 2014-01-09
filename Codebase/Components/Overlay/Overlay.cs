using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
[AddComponentMenu("")]
public class Overlay: MonoBehaviour{
	public static Overlay Get(string name){return Overlay.instances.ContainsKey(name) ? Overlay.instances[name] : null;}
	public static T Get<T>(string name){
		Overlay overlay = Overlay.instances.ContainsKey(name) ? Overlay.instances[name] : null;
		return (T)Convert.ChangeType(overlay,typeof(T));
	}
	public static Dictionary<string,Overlay> instances = new Dictionary<string,Overlay>();
	public bool visible = true;
	public new string name;
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
			GUI.matrix = Global.Scene.guiMatrix;
		}
	}
	public virtual void UpdateRender(){
		if(Global.Scene != null){Global.Scene.CalculateGUIMatrix();}
		this.area = new Rect(this.position.x,this.position.y,this.size.x,this.size.y);
	}
	public void OnDrawGizmosSelected(){this.UpdateRender();}
}
