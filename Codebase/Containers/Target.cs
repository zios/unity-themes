using System;
using System.Collections.Generic;
using UnityEngine;
using Zios;
[Serializable]
public class Target{
	public string search;
	public GameObject direct;
	private bool hasSearched;
	private Dictionary<string,GameObject> special = new Dictionary<string,GameObject>();
	public static implicit operator GameObject(Target value){
		value.Setup();
		return value.direct;
	}
	public Target(){
		Events.Add("SetTarget",this.OnSetTarget);
	}
	public GameObject Get(){
		this.Setup();
		return this.direct;
	}
	public void OnSetTarget(string search){
		this.search = search;
		this.direct = null;
	}
	public void OnSetTarget(GameObject target){
		this.direct = target;
	}
	public void AddSpecial(string name,GameObject target){
		this.special[name] = target;
	}
	public void DefaultSearch(string target){
		if(this.search.IsEmpty() && this.direct == null){
			this.search = target;
		}
		this.Setup();
	}
	public void DefaultTarget(GameObject target){
		if(this.direct == null){
			this.direct = target;
		}
	}
	public void Setup(){
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
		if(!editorMode && this.direct == null){
			Debug.LogWarning("Target : No gameObject was found!");
			if(!this.search.IsEmpty() && !this.search.Contains("Not Found")){
				this.search = "<" + this.search + " Not Found>";
			}
		}
	}
}