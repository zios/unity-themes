using Zios;
using UnityEngine;
using System;
public enum UpdateRate{Default,FixedUpdate,Update,LateUpdate,None};
[Serializable]
public class ManagedMonoBehaviour : MonoBehaviour{
	public string alias;
	public UpdateRate rate = UpdateRate.Default;
	public float deltaTime{get{return this.GetTime();}}
	public virtual void OnApplicationQuit(){this.Awake();}
	public virtual void Reset(){this.Awake();}
	public virtual void Awake(){
		if(this.alias.IsEmpty()){
			this.alias = this.GetType().ToString().ToTitle();
		}
	}
	public float GetTime(){
		if(this.rate == UpdateRate.FixedUpdate){
			return Time.fixedDeltaTime;
		}
		return Time.deltaTime;
	}
	public void DefaultRate(string rate){
		if(this.rate == UpdateRate.Default){
			if(rate == "FixedUpdate"){this.rate = UpdateRate.FixedUpdate;}
			if(rate == "LateUpdate"){this.rate = UpdateRate.LateUpdate;}
		}
	}
	public virtual void FixedUpdate(){
		if(this.rate == UpdateRate.FixedUpdate){
			this.Step();
		}
	}
	public virtual void Update(){
		if(this.rate == UpdateRate.Update || this.rate == UpdateRate.Default){
			this.Step();
		}
	}
	public virtual void LateUpdate(){
		if(this.rate == UpdateRate.LateUpdate){
			this.Step();
		}
	}
	public virtual void Step(){}
}