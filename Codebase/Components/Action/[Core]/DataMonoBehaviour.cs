using Zios;
using UnityEngine;
using System;
[Serializable]
public class DataMonoBehaviour : MonoBehaviour{
	public string alias;
	public virtual void OnApplicationQuit(){this.Awake();}
	public virtual void Reset(){this.Awake();}
	public virtual void Awake(){
		string name = this.GetType().ToString().ToTitle();
		this.alias = this.alias.SetDefault(name);
	}
}