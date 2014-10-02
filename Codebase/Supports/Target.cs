using System;
using System.Collections.Generic;
using UnityEngine;
using Zios;
[Serializable]
public class Target{
	public string search;
	public GameObject direct;
	private Dictionary<string,GameObject> special = new Dictionary<string,GameObject>();
	public static implicit operator GameObject(Target value){
		return value.direct;
	}
	public Target(){
		Events.Add("OnStart",this.Start);
		Events.Add("SetTarget",this.OnSetTarget);
	}
	public void AddSpecial(string name,GameObject target){
		this.special[name] = target;
	}
	public void OnSetTarget(string search){
		this.search = search;
		this.direct = null;
		this.Start();
	}
	public void OnSetTarget(GameObject target){
		this.direct = target;
	}
	public void SetDefault(GameObject target){
		this.Start();
		if(this.direct == null){
			if(!this.search.IsEmpty()){
				this.search = "<" + this.search + " Not Found>";
			}
			this.direct = target;
		}
	}
	public void Start(){
		if(this.direct == null){
			if(this.special.ContainsKey(this.search)){
				this.direct = this.special[this.search];
			}
			else if(!this.search.IsEmpty()){
				this.direct = GameObject.Find(this.search);
			}
		}
	}
}