using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
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
			EditorUtility.SetDirty(target);
		}
	}
	public class ApplyChangesAction : ListAction{
		public override void OnAction(UnityEngine.Object target,object targetItem){
			((AnimationConfig)targetItem).Apply();
		}
	}
	public class PlayAnimationAction : ListAction{
		public AnimationConfig activeAnimation;
		public float animationTime = 0;
		public override void OnAction(UnityEngine.Object target,object targetItem){
			AnimationConfig configuration = (AnimationConfig)targetItem;
			bool isPlaying = this.activeAnimation == configuration;
			if(isPlaying && GUILayout.Button("Stop")){
				this.activeAnimation = null;
			}
			else if(!isPlaying && GUILayout.Button("Play")){
				this.animationTime = 0;
				this.activeAnimation = configuration;
			}
		}
		public override void OnGlobalAction(UnityEngine.Object target){
			GameObject gameObject = ((AnimationSettings)target).gameObject;
			if(this.activeAnimation != null && gameObject.animation[this.activeAnimation.name] != null){
				AnimationState animationState = gameObject.animation[this.activeAnimation.name];
				bool loop = animationState.clip.wrapMode == WrapMode.Loop;
				float length = animationState.length;
				float framerate = 1000.0f / (animationState.clip.frameRate * animationState.speed);
				this.animationTime += (length / framerate);
				if(this.animationTime >= length){
					this.animationTime = loop ? 0 : length;
				}
				gameObject.SampleAnimation(animationState.clip,this.animationTime);
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
			foreach(AnimationConfig configuration in ((AnimationSettings)target).configs){
				elements.Add(configuration);
			}
			return elements;
		}
	}
}