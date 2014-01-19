using UnityEngine;
using System;
using System.Linq;
using System.Collections;
[ExecuteInEditMode]
[AddComponentMenu("Zios/Component/Overlay/Text")]
public class OverlayText: OverlayBase{
	public static OverlayText Get(string name){return Overlay.Get<OverlayText>(name);}
	public Texture2D background;
	public string text = "Example Text";
	public Color textColor = Color.white;
	public Color shadowColor = Color.black;
	public Vector2 shadowOffset = new Vector2(2,2);
	public Font font;
	public int fontSize = 18;
	public FontStyle fontStyle;
	public int letterSpacing;
	public GUIStyle style;
	[NonSerialized] public string trueText;
	[NonSerialized] public string shadowText;
	[NonSerialized] public GUIStyle shadowStyle;
	public override void OnGUI(){
		base.OnGUI();
		if(Event.current.type == EventType.Repaint && this.visible){
			if(this.shadowOffset != Vector2.zero){
				GUI.Label(this.area,this.shadowText,this.shadowStyle);
			}
			GUI.Label(this.area,this.trueText,this.style);
		}
	}
	public override void UpdateRender(){
		base.UpdateRender();
		this.style.fixedWidth = this.size.x;
		this.style.fixedHeight = this.size.y;
		this.style.normal.background = this.background;
		this.style.normal.textColor = this.textColor;
		if(this.letterSpacing != 0){
			this.style.richText = true;
		}
		this.style.font = this.font;
		this.style.fontSize = this.fontSize;
		this.style.fontStyle = this.fontStyle;
		this.shadowStyle = new GUIStyle(this.style);
		this.shadowStyle.normal.textColor = this.shadowColor;
		this.shadowStyle.contentOffset = this.style.contentOffset + this.shadowOffset;
		string separator = "<size=" + (this.fontSize/8) * this.letterSpacing + "> </size>";
		this.trueText = this.letterSpacing != 0 ? this.text.Implode(separator) : this.text;
		this.shadowText = this.letterSpacing != 0 ? this.text.StripMarkup().Implode(separator) : this.text.StripMarkup();
		if(this.shadowOffset != Vector2.zero){
			this.style.normal.background = null;	
		}
	}
	public void UpdateText(string text,object color=null,int size=-1){
		this.text = text;
		if(color != null){this.textColor = (Color)color;}
		if(size != -1){this.fontSize = size;}
		this.UpdateRender();
	}
}
