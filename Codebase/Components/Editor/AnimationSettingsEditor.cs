using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Zios;
namespace Zios.UI{
    [CustomEditor(typeof(AnimationSettings))]
    public class AnimationSettingsEditor : Editor{
	    private CustomListElement listElement;
	    public override void OnInspectorGUI(){
		    if(this.listElement == null){
			    this.listElement = new CustomListElement(target);
		    }
		    this.listElement.Draw();
		    if(this.listElement.shouldRepaint){
			    this.Repaint();
		    }
			if(GUI.changed){
				Utility.SetDirty(this.target);
			}
	    }
	    public class ApplyChangesAction : ListAction{
		    public override void OnAction(UnityEngine.Object target,object targetItem){
			    ((AnimationConfiguration)targetItem).Apply(((AnimationSettings)target).gameObject.GetComponent<Animation>());
		    }
	    }
	    public class PlayAnimationAction : ListAction{
		    public AnimationConfiguration activeAnimation;
		    public float animationTime = 0;
		    public override void OnAction(UnityEngine.Object target,object targetItem){
			    AnimationConfiguration configuration = (AnimationConfiguration)targetItem;
			    bool isPlaying = this.activeAnimation == configuration;
			    if(isPlaying && GUILayout.Button("Stop")){
				    this.activeAnimation = null;
					Events.Resume("On Hierarchy Changed");
			    }
			    else if(!isPlaying && GUILayout.Button("Play")){
				    this.animationTime = 0;
				    this.activeAnimation = configuration;
					Events.Pause("On Hierarchy Changed");
			    }
		    }
		    public override void OnGlobalAction(UnityEngine.Object target){
			    AnimationSettings settings = (AnimationSettings)target;
			    GameObject gameObject = settings.gameObject;
			    if(this.activeAnimation != null && gameObject.GetComponent<Animation>()[this.activeAnimation.name] != null){
				    AnimationState animationState = gameObject.GetComponent<Animation>()[this.activeAnimation.name];
				    bool loop = animationState.clip.wrapMode == WrapMode.Loop;
				    float animationTime = animationState.clip.length * (animationState.clip.frameRate);
				    float framerate = 10000.0f / (animationState.clip.frameRate * animationState.speed);
				    this.animationTime += (animationTime / framerate);
				    if(this.animationTime >= animationTime){
					    this.animationTime = loop ? 0 : animationTime;
				    }
				    animationState.clip.SampleAnimation(gameObject, Time.realtimeSinceStartup);
				    this.shouldRepaint = true;
			    }
			    else{
				    this.shouldRepaint = false;
			    }
		    }
	    }
	    public class CustomListElement : ListElementsTemplate{
		    public CustomListElement(UnityEngine.Object target):base(target){}
		    public override void CreateActions(){
			    this.actions.Add(new ApplyChangesAction());
			    this.actions.Add(new PlayAnimationAction());
		    }
		    public override void CreateItems(){
			    float labelWidth = 100;
			    float defaultWidth = 120;
			    float fpsWidth = 50;
			    float blendWidth = 80;
			    this.listItems.Add(new ListItem("Name","name",labelWidth,ItemTypes.Label));
			    this.listItems.Add(new ListItem("FPS","fps",fpsWidth,ItemTypes.Float));
			    this.listItems.Add(new ListItem("Blend","blendMode",blendWidth,ItemTypes.Enumeration));
			    this.listItems.Add(new ListItem("Wrap","wrapMode",defaultWidth,ItemTypes.Enumeration));
		    }
		    public override List<object> GetList(){
			    List<object> elements = new List<object>();
			    foreach(AnimationConfiguration configuration in ((AnimationSettings)target).animations){
				    elements.Add(configuration);
			    }
			    return elements;
		    }
	    }
    }
}