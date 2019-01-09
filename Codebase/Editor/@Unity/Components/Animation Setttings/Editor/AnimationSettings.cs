using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.Components.AnimationSettings{
	using Zios.Events;
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Reflection;
	using Zios.Unity.Components.AnimationSettings;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.EditorUI;
	using Zios.Unity.Extensions;
	using Zios.Unity.Log;
	using Zios.Unity.Supports.MeshWrap;
	using Zios.Unity.Time;
	//asm Zios.Shortcuts;
	//asm Zios.Unity.Shortcuts;
	//asm Zios.Unity.Supports.Storage;
	[CustomEditor(typeof(AnimationSettings))]
	public class AnimationSettingsEditor : UnityEditor.Editor{
		public static float lastTime;
		private SkinnedMeshRenderer renderer;
		private string newState;
		private BlendState blendState;
		private Dictionary<string,BlendState> blendStates = new Dictionary<string,BlendState>();
		private List<AnimationConfiguration> active = new List<AnimationConfiguration>();
		public void OnEnable(){
			this.blendStates.Clear();
			this.renderer = this.target.As<AnimationSettings>().Get<SkinnedMeshRenderer>();
			this.blendState = this.blendStates.AddNew("Default").Set("Default",this.renderer.sharedMesh);
		}
		public override void OnInspectorGUI(){
			EditorUI.Reset();
			Events.Add("On Editor Update",this.EditorUpdate);
			if(this.renderer.sharedMesh != this.blendState.mesh){this.OnEnable();}
			var blendValues = this.blendState.values;
			var id = this.renderer.GetInstanceID();
			EditorUI.SetFieldSize(-1,150,false);
			var hasSkeletal = !this.target.As<AnimationSettings>().Get<Animation>().IsNull();
			var hasBlendshapes = blendValues.Count > 0;
			if(hasBlendshapes && (!hasSkeletal || EditorUI.DrawFoldout("Blend Shapes",id+"Blend"))){
				EditorGUI.indentLevel += 1;
				var wrap = this.renderer.gameObject.GetMeshWrap();
				var names = blendValues.Keys.ToList();
				var values = this.blendStates.Values.ToList();
				var active = values.IndexOf(this.blendState);
				this.blendState = values[this.blendStates.Keys.ToList().Draw(active,"State")];
				if(EditorUI.lastChanged){
					this.SetBlendState(this.blendState.name);
					this.Repaint();
				}
				for(var index=0;index<blendValues.Count;++index){
					if(index > names.Count-1){break;}
					var shapeName = names[index];
					var shapeValue = this.renderer.GetBlendShapeWeight(index);
					var separated = blendValues.ContainsKey(shapeName+"-Left") && blendValues.ContainsKey(shapeName+"-Right");
					if(shapeName.EndsWith("-")){continue;}
					if(shapeName.EndsWith("+")){
						shapeName = shapeName.TrimRight("+");
						if(!wrap.blendShapes.ContainsKey(shapeName+"-")){
							Log.Warning("[AnimationSettings] Matching blendshape ("+shapeName+"-) does not exist. Skipping.");
							continue;
						}
						var negativeIndex = wrap.blendShapes[shapeName+"-"].index;
						var positiveIndex = wrap.blendShapes[shapeName+"+"].index;
						var negativeValue = this.renderer.GetBlendShapeWeight(negativeIndex);
						var displayValue = shapeValue > 0 ? (shapeValue/100f).Lerp(50,100) : 50f;
						displayValue = negativeValue > 0 ? (negativeValue/100f).Lerp(50,0) : displayValue;
						displayValue = displayValue.DrawSlider(0,100,shapeName);
						if(EditorUI.lastChanged){
							blendValues[shapeName+"+"] = 100*displayValue.InverseLerp(50,100);
							blendValues[shapeName+"-"] = 100*displayValue.InverseLerp(50,0);
							this.renderer.SetBlendShapeWeight(positiveIndex,blendValues[shapeName+"+"]);
							this.renderer.SetBlendShapeWeight(negativeIndex,blendValues[shapeName+"-"]);
						}
						index += 1;
						continue;
					}
					var expanded = false;
					if(separated){
						EditorGUILayout.BeginHorizontal();
						expanded = EditorUI.DrawFoldout(shapeName,id+shapeName+"Individual",EditorStyles.toggle);
						EditorUI.SetFieldSize(-1,96);
						this.DrawBlendState(index,shapeName,shapeValue,null);
						EditorGUILayout.EndHorizontal();
						if(expanded){
							EditorGUI.indentLevel += 1;
							EditorUI.SetFieldSize(-1,150,false);
							this.DrawBlendState(index+1,shapeName+"-Left",this.renderer.GetBlendShapeWeight(index+1),"Left");
							this.DrawBlendState(index+2,shapeName+"-Right",this.renderer.GetBlendShapeWeight(index+2),"Right");
							EditorGUI.indentLevel -= 1;
						}
						index += 2;
						continue;
					}
					this.DrawBlendState(index,shapeName,shapeValue);
				}
				EditorGUILayout.BeginHorizontal();
				this.newState = this.newState.Layout(150,18).Draw();
				if("Add State".ToLabel().Layout(100,19).DrawButton() && !this.newState.IsEmpty() && !this.blendStates.ContainsKey(this.newState)){
					var mesh = Mesh.Instantiate(this.renderer.sharedMesh);
					this.renderer.BakeMesh(mesh);
					this.blendStates.AddNew(this.newState).Set(this.newState,mesh);
					this.SetBlendState(this.newState);
					this.newState = "";
				}
				if("Randomize".ToLabel().Layout(100,19).DrawButton()){
					foreach(var item in wrap.blendShapes){
						var shape = item.Value;
						this.renderer.SetBlendShapeWeight(shape.index,Random.Range(0,100));
					}
				}
				if("Reset".ToLabel().Layout(100,19).DrawButton()){
					foreach(var item in wrap.blendShapes){
						var shape = item.Value;
						this.renderer.SetBlendShapeWeight(shape.index,0);
					}
				}
				EditorGUILayout.EndHorizontal();
				EditorGUI.indentLevel -= 1;
			}
			EditorUI.allowIndention = false;
			if(hasSkeletal && (!hasBlendshapes || EditorUI.DrawFoldout("Skeletal",id+"Skeleton"))){
				EditorGUILayout.BeginHorizontal();
				"Name".Layout(150).DrawLabel();
				"Rate •".Layout(50).DrawLabel();
				if(GUILayoutUtility.GetLastRect().Clicked()){
					AnimationConfiguration.rateMode.Get().GetNames().DrawMenu(this.SetRateMode,AnimationConfiguration.rateMode.Get().ToName().AsList());
				}
				"Speed •".Layout(50).DrawLabel();
				if(GUILayoutUtility.GetLastRect().Clicked()){
					AnimationConfiguration.speedMode.Get().GetNames().DrawMenu(this.SetSpeedMode,AnimationConfiguration.speedMode.Get().ToName().AsList());
				}
				"Blend".Layout(80).DrawLabel();
				"Wrap".Layout(115).DrawLabel();
				EditorGUILayout.EndHorizontal();
				foreach(var config in this.target.As<AnimationSettings>().animations){
					EditorGUILayout.BeginHorizontal();
					bool isPlaying = this.active.Contains(config);
					config.name.Layout(150).DrawLabel();
					config.rate = config.rate.Layout(50).Draw();
					config.speed = config.speed.Layout(50).Draw();
					config.blendMode = config.blendMode.Layout(80).Draw().As<AnimationBlendMode>();
					config.wrapMode = config.wrapMode.Layout(115).Draw().As<WrapMode>();
					if(isPlaying && "Stop".ToLabel().Layout(0,17).DrawButton()){this.Stop(config);}
					if(!isPlaying && "Play".ToLabel().Layout(0,17).DrawButton()){
						this.StopAll();
						this.active.AddNew(config);
						Events.Pause("On Hierarchy Changed");
					}
					if(GUI.changed){
						ProxyEditor.RecordObject(this.target,"Animation Settings Changed");
						config.Apply();
						ProxyEditor.SetDirty(this.target);
					}
					EditorGUILayout.EndHorizontal();
				}
			}
		}
		public void SetBlendState(string state){
			this.blendState = this.blendStates[state];
			this.renderer.sharedMesh = this.blendState.mesh;
			var values = this.blendState.values.Values.ToList();
			for(var index=0;index<this.blendState.values.Count;++index){
				this.renderer.SetBlendShapeWeight(index,values[index]);
			}
		}
		public void DrawBlendState(int index,string name,float shapeValue,string label=""){
			if(label == ""){label = name;}
			shapeValue = shapeValue.DrawSlider(0,100,label);
			if(EditorUI.lastChanged){
				this.blendState.values[name] = shapeValue;
				this.renderer.SetBlendShapeWeight(index,shapeValue);
			}
		}
		public void SetRateMode(object index){
			if(index.As<SpeedUnit>() == AnimationConfiguration.rateMode){return;}
			ProxyEditor.RecordObject(this.target,"Animation Rate Mode Changed");
			AnimationConfiguration.rateMode.Set(index.As<SpeedUnit>());
			foreach(var config in this.target.As<AnimationSettings>().animations){
				config.rate = AnimationConfiguration.rateMode == SpeedUnit.Framerate ? config.rate*config.originalSpeed : config.rate/config.originalSpeed;
			}
			ProxyEditor.SetDirty(this.target);
		}
		public void SetSpeedMode(object index){
			if(index.As<SpeedUnit>() == AnimationConfiguration.speedMode){return;}
			ProxyEditor.RecordObject(this.target,"Animation Speed Mode Changed");
			AnimationConfiguration.speedMode.Set(index.As<SpeedUnit>());
			foreach(var config in this.target.As<AnimationSettings>().animations){
				config.speed = AnimationConfiguration.speedMode == SpeedUnit.Framerate ? config.speed*config.originalSpeed : config.speed/config.originalSpeed;
			}
			ProxyEditor.SetDirty(this.target);
		}
		public void StopAll(){
			foreach(var active in this.active.Copy()){
				this.Stop(active);
			}
		}
		public void Stop(AnimationConfiguration config){
			config.time = 0;
			this.active.Remove(config);
			Events.Resume("On Hierarchy Changed");
			ProxyEditor.RepaintInspectors();
		}
		public void EditorUpdate(){
			var delta = Time.Get()-AnimationSettingsEditor.lastTime;
			var weight = 1.0f/this.active.Count;
			foreach(var config in this.active.Copy()){
				if(config.IsNull() || config.name.IsEmpty()){continue;}
				Events.Pause("On Hierarchy Changed");
				var state = config.parent[config.name];
				state.weight = weight;
				config.parent.Blend(config.name,weight);
				config.time += delta*state.speed;
				var settings = this.target.As<AnimationSettings>();
				if(config.time >= state.clip.length){
					if(state.wrapMode == WrapMode.ClampForever){
						config.lastFrame = state.clip.length;
						state.clip.SampleAnimation(settings.gameObject,config.lastFrame.ToFloat());
						continue;
					}
					else if(state.wrapMode == WrapMode.Default || state.wrapMode == WrapMode.Once){
						this.Stop(config);
					}
				}
				var time = config.time%state.clip.length;
				var tick = 1.0d/state.clip.frameRate;
				time = time.ClampStep(tick);
				if(state.wrapMode == WrapMode.PingPong){
					if(config.time >= state.clip.length){
						time = state.clip.length-time;
						if(time <= 0.05f){config.time = 0;}
					}
				}
				if(time != config.lastFrame){
					config.lastFrame = time;
					state.clip.SampleAnimation(settings.gameObject,time.ToFloat());
				}
			}
			AnimationSettingsEditor.lastTime = Time.Get();
		}
	}
	public class BlendState{
		public string name;
		public Mesh mesh;
		public Dictionary<string,float> values = new Dictionary<string,float>();
		public BlendState Set(string name,Mesh mesh){
			if(mesh.IsNull()){return this;}
			this.name = name;
			this.mesh = mesh;
			for(var index=0;index<mesh.blendShapeCount;++index){
				this.values.Add(mesh.GetBlendShapeName(index).Replace(" ",""),0);
			}
			return this;
		}
	}
}