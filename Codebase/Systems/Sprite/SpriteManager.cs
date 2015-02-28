using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Xml;
using System.IO;
using System;
namespace Zios{
    public class SpriteSheet{
	    public string name;
	    public string imagePath;
	    public Texture2D image;
	    public TextAsset xml;
	    public Dictionary<string,Sprite> sequences = new Dictionary<string,Sprite>();
	    public Dictionary<string,List<Sprite>> families = new Dictionary<string,List<Sprite>>();
	    public int width;
	    public int height;
	    public SpriteSheet(){}
	    public SpriteSheet(Texture2D image,TextAsset xml){
		    XmlReader xmlData = XmlReader.Create(new StringReader(xml.text));
		    this.xml = xml;
		    this.image = image;
		    this.LoadXML(xmlData);
	    }
	    public Sprite GetSprite(string name){
		    if(this.sequences.ContainsKey(name)){
			    return this.sequences[name];
		    }
		    return null;
	    }
	    public Sprite GetSpriteInstance(string name){
		    Sprite sprite = this.GetSprite(name);
		    if(sprite != null){
			    sprite = sprite.Copy();
		    }
		    return sprite;
	    }
	    public List<Sprite> GetFamily(string name){
		    if(this.families.ContainsKey(name)){
			    return this.families[name];
		    }
		    return null;
	    }
	    public void LoadXML(XmlReader xml){
		    using(xml){
			    while(xml.Read()){
				    if(xml.IsStartElement()){
					    if(xml.Name == "TextureAtlas"){
						    this.imagePath = xml["imagePath"];
						    this.name = this.imagePath.Split('.')[0].Replace("Atlas","");
						    if(this.image == null){
							    this.image = FileManager.GetAsset<Texture2D>(this.imagePath);
							    if(this.image == null){
								    Debug.Log("[SpriteManager] -- Could not locate " + this.imagePath);
								    break;
							    }
						    }
						    this.width = this.image.width;
						    this.height = this.image.height;
					    }
					    if(xml.Name == "SubTexture"){
						    Sprite sprite;
						    string[] nameParts = xml["name"].Split('-');
						    string name = nameParts[0].Replace("&amp;","&");
						    string sequence = nameParts.Length > 1 ? nameParts[1] : "";
						    string fullName = sequence != "" ? name + "-" + sequence : name;
						    if(!this.sequences.ContainsKey(fullName)){
							    sprite = new Sprite(name,sequence,this);
							    this.sequences[fullName] = sprite;
						    }
						    else{
							    sprite = this.sequences[fullName];
						    }
						    if(!this.families.ContainsKey(name)){
							    this.families[name] = new List<Sprite>();
						    }
						    this.families[name].Add(sprite);
						    bool animated = sequence != "";
						    int x = Convert.ToInt16(xml["x"]);
						    int y = Convert.ToInt16(xml["y"]);
						    int width = Convert.ToInt16(xml["width"]);
						    int height = Convert.ToInt16(xml["height"]);
						    Frame frame = sprite.AddFrame(x,y,width,height,this.width,this.height,animated);
						    if(xml["frameWidth"] != null){
							    x = Math.Abs(Convert.ToInt16(xml["frameX"]));
							    y = Math.Abs(Convert.ToInt16(xml["frameY"]));
							    width = Convert.ToInt16(xml["frameWidth"]);
							    height = Convert.ToInt16(xml["frameHeight"]);
							    frame.AddPadding(x,y,width,height,this.width,this.height);
							    sprite.padded = true;
						    }
						    sprite.family = this.families[name];
					    }
				    }
			    }
		    }
	    }
    }
    public delegate void SpriteEvent(Sprite sprite);
    public class Sprite{
	    public SpriteSheet parent;
	    public string name;
	    public string sequence;
	    public string fullName;
	    public float currentFrame = 0;
	    public float lastPlayedFrame = 0;
	    public bool active = true;
	    public bool reverse = false;
	    public bool animated = false;
	    public bool padded = false;
	    public bool loop = true;
	    public Frame current;
	    public List<Sprite> family;
	    public List<Frame> frames = new List<Frame>();
	    public SpriteEvent onStart;
	    public SpriteEvent onFirst;
	    public SpriteEvent onStop;
	    public SpriteEvent onLast;
	    public Dictionary<int,SpriteEvent> frameEvents = new Dictionary<int,SpriteEvent>();
	    public Sprite(){}
	    public Sprite(string name,string sequence,SpriteSheet parent){
		    this.name = name;
		    this.sequence = sequence;
		    this.fullName = sequence != "" ? name + "-" + sequence : name;
		    this.parent = parent;
	    }
	    public Sprite Copy(){
		    Sprite copy = new Sprite(this.name,this.sequence,this.parent);
		    copy.family = this.family;
		    copy.frames = this.frames;
		    copy.animated = this.animated;
		    copy.current = this.current;
		    copy.reverse = this.reverse;
		    copy.padded = this.padded;
		    copy.loop = this.loop;
		    copy.onStart = this.onStart;
		    copy.onFirst = this.onFirst;
		    copy.onStop = this.onStop;
		    copy.onLast = this.onLast;
		    copy.frameEvents = new Dictionary<int,SpriteEvent>(this.frameEvents);
		    return copy;
	    }
	    public void Clear(){
		    this.active = false;
		    this.frameEvents.Clear();
		    this.frames.Clear();
		    this.current = null;
		    this.onStart = null;
		    this.onFirst = null;
		    this.onStop = null;
		    this.onLast = null;
	    }
	    public void AddEvent(string name,int frame,SpriteEvent method){
		    foreach(Sprite sprite in this.family){
			    if(sprite.sequence == name){
				    sprite.AddEvent(frame,method);
			    }
		    }
	    }
	    public void AddEvent(int frame,SpriteEvent method){
		    if(frame > 0 && frame <= this.frames.Count){
			    this.frameEvents[frame-1] = method;
		    }
	    }
	    public Frame AddFrame(Frame frame,bool animation=false){
		    this.frames.Add(frame);
		    if(this.current == null){this.current = frame;}
		    if(this.frames.Count > 1 || animation){this.animated = true;}
		    return frame;
	    }
	    public Frame AddFrame(int x,int y,int width,int height,int sheetWidth,int sheetHeight,bool animation=false){
		    Frame newFrame = new Frame(x,y,width,height,sheetWidth,sheetHeight);
		    return this.AddFrame(newFrame,animation);
	    }
	    public void Play(bool reset=true){
		    if(!this.animated){return;}
		    if(this.onStart != null){this.onStart(this);}
		    if(reset){
			    int frame = this.reverse ? this.frames.Count-1 : 0;
			    this.current = this.frames[frame];
			    this.currentFrame = frame;
		    }
		    this.active = true;
	    }
	    public void Stop(){
		    if(this.onStop != null){this.onStop(this);}
		    this.active = false;
	    }
	    public void NextFrame(){
		    this.currentFrame += this.reverse ? 2 : 0;
		    this.Step();
	    }
	    public void PreviousFrame(){
		    this.currentFrame -= this.reverse ? 0 : 2;
		    this.Step();
	    }
	    public void SetFrame(int index){
		    this.currentFrame = this.reverse ? index+1 : index-1;
		    this.Step(true);
	    }
	    public void Step(bool force = false){
		    if(this.active || force){
			    float advance = this.reverse ? -1 : 1;
			    this.currentFrame += advance;
			    if(this.currentFrame >= this.frames.Count){
				    if(this.onLast != null){this.onLast(this);}
				    if(this.loop){this.currentFrame = 0;}
				    else{
					    this.Stop();
					    this.currentFrame = this.frames.Count-1;
				    }
			    }
			    else if(this.currentFrame < 0){
				    if(this.onFirst != null){this.onFirst(this);}
				    if(this.loop){this.currentFrame = this.frames.Count-1;}
				    else{
					    this.Stop();
					    this.currentFrame = 0;
				    }
			    }
			    int indexFrame = (int)this.currentFrame;
			    int sweepFrame = (int)this.lastPlayedFrame;
			    this.current = this.frames[indexFrame];
			    while(sweepFrame != indexFrame){
				    if(this.frameEvents.ContainsKey(sweepFrame)){
					    this.frameEvents[sweepFrame](this);
					    break;
				    }
				    if(!this.reverse){sweepFrame = sweepFrame+1 >= this.frames.Count ? 0 : sweepFrame+1;}
				    else{sweepFrame = sweepFrame-1 < 0 ? this.frames.Count-1 : sweepFrame-1;}
			    }
			    this.lastPlayedFrame = this.currentFrame;
		    }
	    }
    }
    public class Frame{
	    public Rect bounds;
	    public Rect fullBounds;
	    public Vector4 uv = new Vector4(0,0,0,0);
	    public Vector4 uvPadding = new Vector4(0,0,1,1);
	    public Frame(int x,int y,int width,int height,int sheetWidth,int sheetHeight){
		    this.bounds = this.fullBounds = new Rect(x,y,width,height);
		    this.uv.x = (float)x / (float)sheetWidth;
		    this.uv.y = 1.0f - (((float)y+height) / (float)sheetHeight);
		    this.uv.z = this.uv.x + ((float)width / (float)sheetWidth);
		    this.uv.w = 1.0f - ((float)y / (float)sheetHeight);
	    }
	    public void AddPadding(int x,int y,int width,int height,int sheetWidth,int sheetHeight){
		    this.fullBounds = new Rect(x,y,width,height);
		    this.fullBounds.x = width-(x+this.bounds.width);
		    this.fullBounds.y = height-(y+this.bounds.height);
		    this.uvPadding.x = ((float)x / (float)width);
		    this.uvPadding.y = (float)this.fullBounds.y / (float)height;
		    this.uvPadding.z = 1-((float)this.fullBounds.x / (float)width);
		    this.uvPadding.w = 1-((float)y / (float)height);
	    }
	    public int[] GetIntBounds(){
		    return new int[4]{(int)this.bounds.x,(int)this.bounds.y,(int)this.bounds.width,(int)this.bounds.height}; 
	    }
	    public int[] GetIntFullBounds(){
		    return new int[4]{(int)this.fullBounds.x,(int)this.fullBounds.y,(int)this.fullBounds.width,(int)this.fullBounds.height}; 
	    }
    }
    public static class SpriteManager{
	    public static Dictionary<string,SpriteSheet> spriteSheets = new Dictionary<string,SpriteSheet>();
	    public static Dictionary<string,Sprite> sequences = new Dictionary<string,Sprite>();
	    public static Dictionary<TextAsset,SpriteSheet> cached = new Dictionary<TextAsset,SpriteSheet>();
	    public static void Clear(){
		    spriteSheets.Clear();
		    sequences.Clear();
		    cached.Clear();
	    }
	    public static SpriteSheet Add(TextAsset asset,Texture2D image=null){
		    if(asset == null){return null;}
		    bool exists = false;
		    SpriteSheet spriteSheet = new SpriteSheet();
		    if(cached.ContainsKey(asset)){
			    exists = true;
			    spriteSheet = cached[asset];
		    }
		    if(!exists){
			    spriteSheet = new SpriteSheet(image,asset);
			    spriteSheets[spriteSheet.imagePath] = spriteSheet;
			    foreach(var item in spriteSheet.sequences){
				    sequences[item.Key] = item.Value;
			    }
			    cached[asset] = spriteSheet;
		    }
		    return spriteSheet;
	    }
	    public static Sprite GetSprite(string name){
		    if(sequences.ContainsKey(name)){
			    return sequences[name];
		    }
		    return null;
	    }
	    public static Sprite GetSpriteInstance(string name){
		    Sprite sprite = GetSprite(name);
		    if(sprite != null){
			    sprite = sprite.Copy();
		    }
		    return sprite;
	    }
    }
}