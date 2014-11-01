using System;
using System.Collections.Generic;
using UnityEngine;
using Zios;
[Serializable]
public class Target{
	public string search;
	public GameObject direct;
	private MonoBehaviour script;
	private bool hasSearched;
	private bool hasWarned;
	private string fallbackSearch;
	private Dictionary<string,GameObject> special = new Dictionary<string,GameObject>();
	public static implicit operator Transform(Target value){
		value.Prepare();
		return value.direct.transform;
	}
	public static implicit operator GameObject(Target value){
		value.Prepare();
		return value.direct;
	}
	public void Setup(params MonoBehaviour[] scripts){this.Setup("",scripts);}
	public void Setup(string name="",params MonoBehaviour[] scripts){
		this.script = scripts[0];
		foreach(MonoBehaviour script in scripts){
			if(script is ActionPart){
				ActionPart part = (ActionPart)script;
				if(part.action){this.AddSpecial("[Owner]",part.action.owner);}
				this.AddSpecial("[Action]",part.gameObject);
				this.DefaultSearch(part.action ? "[Owner]" : "[Action]");
			}
		}
		//this.DefaultSearch();
	}
	public GameObject Get(){
		this.Prepare();
		return this.direct;
	}
	public void AddSpecial(string name,GameObject target){
		this.special[name] = target;
	}
	public void DefaultSearch(){this.DefaultSearch(this.fallbackSearch);}
	public void DefaultSearch(string target){
		if(this.search.IsEmpty() && this.direct == null){
			this.search = this.fallbackSearch = target;
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
		string search = this.search.IsEmpty() ? "" : this.search;
		if(editorMode || !this.hasSearched){
			if(this.special.ContainsKey(search)){
				this.direct = this.special[search];
			}
			else if(!search.IsEmpty()){
				this.direct = GameObject.Find(search);
			}
			this.hasSearched = true;
		}
		if(!editorMode && this.direct == null && !this.hasWarned){
			Debug.LogWarning("Target : No gameObject was found for " + this.script.name,this.script);
			if(!search.IsEmpty() && !search.Contains("Not Found")){
				this.search = "<" + this.search + " Not Found>";
			}
			this.hasWarned = true;
		}
	}
}
