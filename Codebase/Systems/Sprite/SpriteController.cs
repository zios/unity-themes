using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Zios{
    [AddComponentMenu("Zios/Component/Rendering/Sprite Controller")]
    public class SpriteController : MonoBehaviour{
	    public Texture2D spriteTexture;
	    public TextAsset spriteXML;
	    public string spriteName;
	    public string spriteAnimation;
	    public string spriteFallback;
	    public SpriteSheet spriteSheet;
	    public Renderer targetRenderer;
	    public float spriteSpeed = 6;
	    public bool spriteActive = true;
	    public bool spriteUnique = true;
	    public bool spriteLoop = false;
	    public bool spriteReverse = false;
	    public bool spriteRandomStart = false;
	    public bool spriteDelayStart = true;
	    [Internal] public bool forceUpdate = false;
	    [Internal] public bool frameChanged = false;
		public string[] shaderTextureNames = new string[]{"indexMap","textureMap"};
	    [System.NonSerialized] public bool visible = true;
	    [System.NonSerialized] public Sprite instance;
	    [System.NonSerialized] public float frame = -1;
	    private Dictionary<string,Sprite> sequences = new Dictionary<string,Sprite>();
	    private Material activeMaterial;
	    private Renderer[] renderers;
	    private float nextUpdate;
	    public void Start(){
		    this.renderers = this.transform.GetComponentsInChildren<Renderer>();
		    if(Application.isPlaying && this.instance == null && this.spriteXML != null){
			    this.spriteSheet = SpriteManager.Add(this.spriteXML,this.spriteTexture);	
			    Renderer targetRenderer = this.targetRenderer ?? this.gameObject.GetComponent<Renderer>();
			    if(this.activeMaterial == null && targetRenderer != null){
				    if(this.spriteUnique){
					    targetRenderer.material = targetRenderer.material;
				    }
				    this.activeMaterial = targetRenderer.sharedMaterial;
			    }
			    this.Load();
		    }
	    }
	    public void CheckVisible(){
		    foreach(Renderer renderer in this.renderers){
			    if(renderer == null){
				    this.visible = false;
				    break;
			    }
			    this.visible = renderer.isVisible;
			    if(this.visible){break;}
		    }
	    }
	    public void Load(){
		    string fullName = this.spriteName+"-"+this.spriteAnimation;
		    this.frame = -1;
		    if(this.instance != null){
			    if(this.instance.sequence == this.spriteAnimation){return;}
			    this.instance.active = false;
		    }
		    if(this.sequences.ContainsKey(fullName)){
			    this.instance = this.sequences[fullName];
			    this.Begin();
		    }
		    else if(this.spriteSheet.GetSprite(fullName) != null){
			    this.instance = this.sequences[fullName] = this.spriteSheet.GetSpriteInstance(fullName);
			    if(this.instance == null){
				    Debug.Log("[SpriteController] ^2Error loading sprite -- ^7" + this.spriteName + "-^4" + this.spriteAnimation);
				    return;
			    }
			    this.Begin();
		    }
		    else{
			    Debug.Log("[SpriteController] No sprite named " + fullName + " was found.");
		    }
	    }
	    public void AddEvent(string name,int frame,SpriteEvent method){
		    string fullName = this.name+"-"+name;
		    if(this.sequences.ContainsKey(fullName)){
			    this.sequences[fullName].AddEvent(frame,method);
		    }
	    }
	    public void Reset(){
		    if(this.instance != null){
			    this.instance.Play(true);
			    this.frame = this.instance.currentFrame;
			    if(this.spriteRandomStart){
				    int random = Random.Range(0,this.instance.frames.Count-1);
				    this.instance.currentFrame = random;
			    }
			    if(this.spriteDelayStart){
			    }
		    }
	    }
	    public void NextFrame(){
		    this.instance.NextFrame();
		    this.Update();
	    }
	    public void PreviousFrame(){
		    this.instance.PreviousFrame();
		    this.Update();
	    }
	    public void Begin(){
		    this.spriteActive = true;
		    this.Update();
		    this.Reset();
		    this.Update();
	    }
	    public bool IsPlaying(string name){
		    return this.spriteAnimation == name;
	    }
	    public void Play(string name){
		    this.spriteActive = true;
		    this.spriteAnimation = name;
	    }
	    public void Stop(){
		    this.spriteActive = false;
		    this.Update();
	    }
	    public void Update(){
		    if(Application.isPlaying && this.instance != null && this.spriteAnimation != ""){
			    this.frameChanged = false;
			    if((!this.visible && !this.forceUpdate) || !this.spriteActive){
				    this.instance.active = false;
				    return;
			    }
			    if(this.instance.sequence != this.spriteAnimation){
				    this.frameChanged = true;
				    this.Load();
			    }
			    if(this.instance.active != this.spriteActive){
				    this.instance.active = this.spriteActive;
			    }
			    if(this.instance.loop != this.spriteLoop){
				    this.instance.loop = this.spriteLoop;
				    if(this.spriteLoop){this.instance.active = true;}
			    }
			    if(this.instance.reverse != this.spriteReverse){
				    this.instance.reverse = this.spriteReverse;
			    }
			    if(!this.spriteLoop && this.spriteFallback != "" && this.spriteAnimation != this.spriteFallback){
				    float frame = this.instance.currentFrame;
				    if((this.spriteReverse && frame == 0) || (!this.spriteReverse && frame == this.instance.frames.Count-1)){
					    this.spriteAnimation = this.spriteFallback;
					    this.Load();
				    }
			    }
			    if(Time.time > this.nextUpdate){
				    this.UpdateFrame();
			    }
		    }
	    }
	    public void UpdateFrame(){
		    this.instance.Step();
		    if(this.activeMaterial != null){
			    this.activeMaterial.SetVector("atlasUV",this.instance.current.uv);
			    this.activeMaterial.SetVector("paddingUV",this.instance.current.uvPadding);
				foreach(var name in this.shaderTextureNames){
					this.activeMaterial.SetTexture(name,this.spriteTexture);
				}
		    }
		    if(this.frame != this.instance.currentFrame){this.frameChanged = true;}
		    this.frame = this.instance.currentFrame;
		    this.nextUpdate = Time.time + ((1000.0f / this.spriteSpeed) / 1000.0f);
	    }
	    public void LateUpdate(){
		    this.CheckVisible();
	    }
	    public SpriteController Copy(){return (SpriteController)this.MemberwiseClone();}
    }
}
