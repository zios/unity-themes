using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
[AddComponentMenu("Zios/Component/Overlay/Image")]
public class Overlay2D: OverlayBase{
	public static Overlay2D Get(string name){return Overlay.Get<Overlay2D>(name);}
	public Vector2 tiling = Vector2.one;
	public Texture2D texture;
	public Material material;
	private Vector2 lastTiling = Vector2.one;
	public override void OnGUI(){
		base.OnGUI();
		if(Event.current.type == EventType.Repaint && this.visible){
			if(this.lastTiling != this.tiling){
				this.UpdateRender();
				this.lastTiling = this.tiling;
			}
			Graphics.DrawTexture(this.area,this.texture,this.material);
		}
	}
	public override void UpdateRender(){
		base.UpdateRender();
		bool atlasTile = this.material.HasProperty("atlasUVScale");
		if(atlasTile){this.material.SetVector("atlasUVScale",this.tiling);}
		else{this.material.SetTextureScale("diffuseMap",this.tiling);}
		if(this.texture == null){
			this.texture = (Texture2D)Resources.LoadAssetAtPath("Assets/Interface/Patterns/Transparency.png",typeof(Texture2D));
		}
		this.area = new Rect(this.position.x,this.position.y,this.size.x*this.tiling.x,this.size.y*this.tiling.y);
	}
}