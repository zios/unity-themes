using System;
using System.Linq;
using UnityEngine;
namespace Zios.Sprites{
	using Containers.Math;
	[AddComponentMenu("Zios/Component/Rendering/Sprite After Image")]
	public class SpriteAfterImage : MonoBehaviour{
		public bool active = false;
		public int amount = 5;
		public float delay = 0.05f;
		public float lifetime = 0.2f;
		public bool matchChanges = true;
		public bool randomColors = false;
		public float randomIntensity = 1.0f;
		public Color[] startColor;
		public Color[] endColor;
		public float startAlpha = 1;
		public float endAlpha = 0;
		public SpriteController source;
		private SpriteController[] sprites;
		private int nextIndex = 0;
		private float nextSpawn;
		private float[] nextDeath;
		public void Start(){
			if(this.source == null){
				this.source = this.gameObject.GetComponentInChildren<SpriteController>();
			}
			this.BuildImages();
		}
		public void BuildImages(){
			this.sprites = new SpriteController[this.amount];
			this.nextSpawn = Time.time + this.delay;
			this.nextDeath = new float[this.amount];
			Transform parent = Locate.GetScenePath("Effects").transform;
			for(int index=0;index<this.amount;++index){
				GameObject image = (GameObject)Instantiate(source.gameObject);
				Component[] components = image.GetComponentsInChildren<Component>();
				Type[] safe = new Type[]{typeof(Transform),typeof(SpriteController),typeof(MeshRenderer),typeof(MeshFilter),typeof(Renderer)};
				foreach(Component current in components){
					Type type = current.GetType();
					if(type == typeof(Transform)){
						Transform active = (Transform)current;
						bool child = active.parent == image.transform;
						if(child){Destroy(active.gameObject);}
					}
					else if(!safe.Contains(type)){Destroy(current);}
				}
				foreach(Transform current in image.transform){Destroy(current);}
				this.sprites[index] = image.GetComponentInChildren<SpriteController>();
				image.name = source.name + "-AfterImage-" + (index+1);
				image.transform.parent = parent;
				image.GetComponent<Renderer>().material = new Material(image.GetComponent<Renderer>().material);
				image.GetComponent<Renderer>().material.shader = Shader.Find("Zios/Olio/Sprite + Particle + Lerp");
				image.GetComponent<Renderer>().material.SetFloat("lerpCutoff",0);
				image.SetActive(false);
				this.sprites[index].Start();
			}
		}
		public void Update(){
			bool ready = (this.matchChanges && this.source.frameChanged) || (!this.matchChanges && Time.time > this.nextSpawn);
			if(ready && this.active){
				SpriteController sprite = this.sprites[this.nextIndex];
				sprite.transform.position = this.source.transform.position;
				sprite.transform.rotation = this.source.transform.rotation;
				sprite.transform.localScale = this.source.transform.localScale;
				sprite.transform.Translate(new Vector3(0,0,-1));
				sprite.gameObject.SetActive(true);
				sprite.spriteAnimation = this.source.spriteAnimation;
				sprite.Load();
				sprite.instance.SetFrame((int)this.source.frame);
				sprite.spriteActive = sprite.instance.active = false;
				sprite.UpdateFrame();
				if(this.randomColors){
					sprite.gameObject.GetComponent<Renderer>().material.SetColor("lerpColor",Color.red.Random(this.randomIntensity));
				}
				this.nextDeath[this.nextIndex] = Time.time + this.lifetime;
				this.nextSpawn = Time.time + this.delay;
				this.nextIndex = (this.nextIndex+1)%this.sprites.Length;
			}
			for(int index=0;index<this.sprites.Length;++index){
				SpriteController sprite = this.sprites[index];
				if(sprite.gameObject.activeSelf){
					float deathTime = this.nextDeath[index];
					float createTime = deathTime - this.lifetime;
					float progress = (Time.time - createTime) / (deathTime - createTime);
					if(!this.randomColors){
						Color startColor = this.startColor[index % this.startColor.Length];
						Color endColor = this.endColor[index % this.endColor.Length];
						Color mixColor = Color.Lerp(startColor,endColor,progress);
						sprite.gameObject.GetComponent<Renderer>().material.SetColor("lerpColor",mixColor);
					}
					float mixAlpha = new Bezier(this.startAlpha,this.endAlpha).Curve(progress);
					sprite.gameObject.GetComponent<Renderer>().material.SetFloat("alpha",mixAlpha);
					if(Time.time > deathTime){sprite.gameObject.SetActive(false);}
				}
			}
		}
	}
}