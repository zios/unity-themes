using Zios;
using System;
using UnityEngine;
using System.Collections.Generic;
using Attribute = Zios.Attribute;
using Action = Zios.Action;
[AddComponentMenu("Zios/Singleton/Attribute Manager")][ExecuteInEditMode]
public class AttributeManager : MonoBehaviour{
	private float nextStep;
	private bool initialSearch;
	public List<GameObject> builtTargets = new List<GameObject>();
	public List<AttributeVector3> builtPosition = new List<AttributeVector3>();
	public List<AttributeVector3> builtRotation = new List<AttributeVector3>();
	public List<AttributeVector3> builtScale = new List<AttributeVector3>();
	public void OnApplicationQuit(){
		Utility.EditorUpdate(this.Start,true);
	}
	public void OnDestroy(){
		Utility.RemoveAssetUpdate(this.FullRefresh);
		Utility.RemoveHierarchyUpdate(this.SceneRefresh);
		Utility.RemoveEditorUpdate(this.Start);
	}
	public void Update(){
		Utility.AssetUpdate(this.FullRefresh);
		Utility.HierarchyUpdate(this.SceneRefresh);
		Utility.EditorUpdate(this.Start,true);
		//Utility.EditorCall(this.Start);
	}
	public void FullRefresh(){
		this.SceneRefresh(true);
	}
	public void SceneRefresh(bool full=false){
		AttributeBox[] boxes = Locate.GetSceneObjects<AttributeBox>();
		ActionPart[] parts = Locate.GetSceneObjects<ActionPart>();
		Action[] actions = Locate.GetSceneObjects<Action>();
		foreach(AttributeBox box in boxes){
			if(full || !box.gameObject.activeInHierarchy || !box.enabled){box.Awake();}
		}
		foreach(ActionPart part in parts){
			if(full || !part.gameObject.activeInHierarchy || !part.enabled){part.Awake();}
		}
		foreach(Action action in actions){
			if(full || !action.gameObject.activeInHierarchy || !action.enabled){action.Awake();}
		}
		this.Setup();
	}
	public void Start(){
		if(Application.isPlaying || Time.realtimeSinceStartup > this.nextStep){
			this.nextStep = Time.realtimeSinceStartup + 1;
			if(!this.initialSearch){
				this.SceneRefresh(!Application.isPlaying);
				this.initialSearch = true;
				return;
			}
			this.Setup();
		}
	}
	public void Setup(){
		foreach(GameObject target in this.builtTargets.Copy()){
			if(target.IsNull() || target.IsPrefab()){
				int index = this.builtTargets.IndexOf(target);
				this.builtTargets.RemoveAt(index);
				this.builtPosition.RemoveAt(index);
				this.builtRotation.RemoveAt(index);
				this.builtScale.RemoveAt(index);
			}
		}
		for(int index=0;index<this.builtTargets.Count;++index){
			GameObject target = this.builtTargets[index];
			AttributeVector3 position = this.builtPosition[index];
			AttributeVector3 rotation = this.builtRotation[index];
			AttributeVector3 scale = this.builtScale[index];
			this.SetupExtra(target,position,rotation,scale);
		}
		foreach(var attribute in Attribute.all.Copy()){
			if(attribute.parent.IsNull()){
				Attribute.all.Remove(attribute);
				continue;
			}
			GameObject target = attribute.parent.gameObject;
			this.Build(target);
			foreach(var data in attribute.GetData()){
				if(data.usage != AttributeUsage.Direct){
					target = data.target.Get();
					this.Build(target);
				}
			}
		}
		foreach(var attribute in Attribute.all){attribute.SetupTable();}
		foreach(var attribute in Attribute.all){attribute.SetupData();}
		Attribute.ready = true;
	}
	public void Build(GameObject target){
		if(this.builtTargets.Contains(target) || target.IsNull() || target.IsPrefab() || target.name.IsEmpty()){return;}
		this.SetupExtra(target,null,null,null);
	}
	public void SetupExtra(GameObject target,AttributeVector3 position,AttributeVector3 rotation,AttributeVector3 scale){
		if(position.IsNull()){position = new AttributeVector3(target.transform.position);}
		if(rotation.IsNull()){rotation = new AttributeVector3(target.transform.eulerAngles);}
		if(scale.IsNull()){scale = new AttributeVector3(target.transform.localScale);}
		if(!this.builtTargets.Contains(target)){this.builtTargets.Add(target);}
		if(!this.builtPosition.Contains(position)){this.builtPosition.Add(position);}
		if(!this.builtRotation.Contains(rotation)){this.builtRotation.Add(rotation);}
		if(!this.builtScale.Contains(scale)){this.builtScale.Add(scale);}
		position.getMethod = ()=>target.transform.position;
		position.setMethod = value=>target.transform.position = value;
		rotation.getMethod = ()=>target.transform.eulerAngles;
		rotation.setMethod = value=>target.transform.eulerAngles = value;
		scale.getMethod = ()=>target.transform.localScale;
		scale.setMethod = value=>target.transform.localScale = value;
		position.Setup("Position",target.transform);
		rotation.Setup("Rotation",target.transform);
		scale.Setup("Scale",target.transform);
	}
}