using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
[AddComponentMenu("Zios/Component/Overlay/Image")]
public class Overlay2D: OverlayBase{
	public static Texture2D placeholder;
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
			Debug.Log(this.area);
			Graphics.DrawTexture(this.area,this.texture,this.material);
		}
	}
	public override void UpdateRender(){
		if(Overlay2D.placeholder == null){
			Overlay2D.placeholder = (Texture2D)Resources.LoadAssetAtPath("Assets/Interface/Patterns/Transparency.png",typeof(Texture2D));
		}
		base.UpdateRender();
		Vector2 size = this.size;
		bool atlasTile = this.material.HasProperty("atlasUVScale");
		string targetMap = this.material.HasProperty("diffuseMap") ? "diffuseMap" : "_MainTex";
		if(this.texture == null){
			this.texture = Overlay2D.placeholder;
		}
		this.material.SetTexture(targetMap,this.texture);
		if(atlasTile){
			this.material.SetVector("atlasUVScale",this.tiling);
			size.x *= this.tiling.x;
			size.y *= this.tiling.y;
		}
		else{
			this.material.SetTextureScale(targetMap,this.tiling);
		}
		this.area = new Rect(this.position.x,this.position.y,size.x,size.y);
	}
}