using System;
using System.Collections.Generic;
using UnityEngine;
using Zios;
[Serializable]
public class Target{
	public string search;
	public GameObject direct;
	private bool hasSearched;
	private bool hasWarned;
	private Dictionary<string,GameObject> special = new Dictionary<string,GameObject>();
	public static implicit operator Transform(Target value){
		value.Prepare();
		return value.direct.transform;
	}
	public static implicit operator GameObject(Target value){
		value.Prepare();
		return value.direct;
	}
	public void Update(MonoBehaviour script){
		if(script is ActionPart){
			ActionPart part = (ActionPart)script;
			if(part.action){this.AddSpecial("[Owner]",part.action.owner);}
			this.AddSpecial("[Action]",part.gameObject);
			this.DefaultSearch(part.action ? "[Owner]" : "[Action]");
		}
	}
	public void Setup(MonoBehaviour script,string eventName=""){
		this.Update(script);
		Events.AddScope("Set"+eventName+"Target",this.OnSetTarget,script.gameObject);
	}
	public GameObject Get(){
		this.Prepare();
		return this.direct;
	}
	public void OnSetTarget(string search){
		this.hasSearched = false;
		this.direct = null;
		this.Search(search);
	}
	public void AddSpecial(string name,GameObject target){
		this.special[name] = target;
	}
	public void DefaultSearch(string target){
		if(this.search.IsEmpty() && this.direct == null){
			this.search = target;
		}
		this.Prepare();
	}
	public void DefaultTarget(GameObject target){
		if(this.direct == null){
			this.direct = target;
		}
	}
	public void Search(string search){
		this.search = search;
		this.Prepare();
	}
	public void Prepare(){
		bool editorMode = !Application.isPlaying;
		if(editorMode || !this.hasSearched){
			if(this.special.ContainsKey(this.search)){
				this.direct = this.special[this.search];
			}
			else if(!this.search.IsEmpty()){
				this.direct = GameObject.Find(this.search);
			}
			this.hasSearched = true;
		}
		if(!editorMode && this.direct == null && !this.hasWarned){
			Debug.LogWarning("Target : No gameObject was found!");
			if(!this.search.IsEmpty() && !this.search.Contains("Not Found")){
				this.search = "<" + this.search + " Not Found>";
			}
			this.hasWarned = true;
		}
	}
}
