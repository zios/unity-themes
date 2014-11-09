using System;
using System.Collections.Generic;
using UnityEngine;
using Zios;
using Action = Zios.Action;
[Serializable]
public class Target{
	public string search = "";
	public GameObject direct;
	private MonoBehaviour script;
	private bool hasSearched;
	private bool hasWarned;
	private string lastSearch = "";
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
		this.AddSpecial("[This]",this.script.gameObject);
		this.AddSpecial("[Self]",this.script.gameObject);
		foreach(MonoBehaviour script in scripts){
			if(script is ActionPart || script is Action){
				ActionPart part = script is ActionPart ? (ActionPart)script : null;
				Action action = script is Action ? (Action)script : part.action;
				GameObject actionObject = action != null ? action.gameObject : part.gameObject;
				GameObject ownerObject = action != null ? action.owner : part.gameObject;
				this.AddSpecial("[Owner]",ownerObject);
				this.AddSpecial("[Action]",actionObject);
			}
		}
	}
	public void Setup(string name,MonoBehaviour script,string defaultSearch="[Self]"){
		this.Setup(name,new MonoBehaviour[]{script});
		this.DefaultSearch(defaultSearch);
	}
	public GameObject Get(){
		this.Prepare();
		return this.direct;
	}
	public void AddSpecial(string name,GameObject target){
		this.special[name.ToLower()] = target;
	}
	public void SkipWarning(){this.hasWarned = true;}
	public void DefaultSearch(){this.DefaultSearch(this.fallbackSearch);}
	public void DefaultSearch(string target){
		this.fallbackSearch = target;
		if(this.search != this.lastSearch){
			if(this.search.IsEmpty()){
				this.search = target;
			}
			this.Prepare();
		}
	}
	public void DefaultTarget(GameObject target){
		if(this.direct.IsNull()){
			this.direct = target;
		}
	}
	public GameObject FindTarget(string search){
		foreach(var item in this.special){
			string special = item.Key;
			if(search.ToLower().Contains(special)){
				string specialPath = this.special[special].GetPath();
				search = search.Replace(special,specialPath,true);
			}
		}
		if(search.Contains("/")){
			string[] parts = search.Split("/");
			string total = "";
			for(int index=0;index<parts.Length;++index){
				string part = parts[index];
				if(part == ".." || part == "." || part.IsEmpty()){
					if(total.IsEmpty()){
						GameObject current = this.special["[this]"];
						if(!current.IsNull()){
							if(part == ".."){
								total = current.GetParent().IsNull() ? "" :current.GetParent().GetPath();
							}
							else{total = current.GetPath();}
						}
						continue;
					}
					GameObject path = GameObject.Find(total);
					if(!path.IsNull()){
						if(part == ".."){
							total = path.GetParent().IsNull() ? "" :path.GetParent().GetPath();
						}
						continue;
					}
				}
				total += part + "/";
			}
			search = total.Trim("/");
		}
		return GameObject.Find(search);
	}
	public void Prepare(){
		bool editorMode = !Application.isPlaying;
		this.search = this.search.Replace("\\","/");
		if((editorMode || !this.hasSearched) && !this.search.IsEmpty()){
			this.direct = this.FindTarget(this.search);
			this.lastSearch = this.search;
			this.hasSearched = true;
		}
		if(!editorMode && this.direct.IsNull() && !this.hasWarned){
			Debug.LogWarning("Target : No gameObject was found for " + this.script.name,this.script);
			if(!search.IsEmpty() && !search.Contains("Not Found")){
				this.search = "<" + this.search + " Not Found>";
			}
			this.hasWarned = true;
		}
	}
}
