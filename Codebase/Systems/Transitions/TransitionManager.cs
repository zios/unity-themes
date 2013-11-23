using UnityEngine;
using System.Collections;
public struct TransitionEffect{
	public const byte solid = 0;
	public const byte keyhole = 1;
}
[AddComponentMenu("Zios/Singleton/Transition")]
public class TransitionManager : MonoBehaviour{
	public Texture[] textures = new Texture[8];
	private Method transitionOut;
	private Method transitionIn;
	private int timer = 0;
	private int duration = 0;
	private string type = "fade";
	public void Begin(string type,int duration,Method transitionOut,Method transitionIn){
		this.duration = duration;
		this.timer = duration/2;
		this.type = type;
		this.transitionOut = transitionOut;
		this.transitionIn = transitionIn;
	}
	public void Awake(){
		Global.Transition = this;
		DontDestroyOnLoad(this.gameObject);
	}
	public void Begin(string type,int duration){
		this.Begin(type,duration,null,null);
	}
	public void OnGUI(){
		if(this.timer != 0){
			Rect screen = new Rect(0,0,Screen.width,Screen.height);
			int fadeTime = this.duration/4;
			int elapsed = (int)(Time.deltaTime*1000);
			float fadePercent = (Mathf.Abs(this.timer)-fadeTime) / (float)(fadeTime);
			if(this.timer > 0){
				fadePercent = 1-fadePercent;
				this.timer -= elapsed;
				if(fadePercent >= 1 && this.transitionOut != null){
					this.transitionOut();
					this.transitionOut = null;
				}
				if(this.timer <= 0){
					this.timer = -(this.duration/2);
				}
			}
			else if(this.timer < 0){
				this.timer += elapsed;
				if(this.timer >= 0){
					this.timer = 0;
					this.transitionIn();
					this.transitionIn = null;
				}
			}
			if(type == "fade"){
				GUI.color = new Color(0,0,0,Mathf.Lerp(0,1,fadePercent));
				GUI.DrawTexture(screen,this.textures[TransitionEffect.solid]);
			}
			else if(type == "keyhole"){
				if(fadePercent >= 1){
					GUI.DrawTexture(screen,this.textures[TransitionEffect.solid]);
				}
				else{
					fadePercent = 1-fadePercent;
					float width = Mathf.Clamp(Screen.width*(fadePercent*16),Screen.width,Screen.width*16);
					float height = Mathf.Clamp(Screen.height*(fadePercent*16),Screen.height,Screen.height*16);
					screen = new Rect((Screen.width/2)-width/2,(Screen.height/2)-height/2,width,height);
					GUI.DrawTexture(screen,this.textures[TransitionEffect.keyhole]);
				}
			}
		}
	}
}