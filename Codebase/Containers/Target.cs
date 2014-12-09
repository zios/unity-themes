using Zios;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Action = Zios.Action;
using UnityObject = UnityEngine.Object;
[Serializable]
public class Target{
	public List<GameObject> special = new List<GameObject>();
	public List<string> specialNames = new List<string>();
	public string search = "";
	public GameObject direct;
	public Component parent;
	private bool hasSearched;
	private bool hasWarned;
	private int siblingCount;
	private Component lastParent;
	private string lastSearch = "";
	private string fallbackSearch = "";
	public static implicit operator Transform(Target value){
		value.Prepare();
		return value.direct.transform;
	}
	public static implicit operator GameObject(Target value){
		value.Prepare();
		return value.direct;
	}
	public static implicit operator UnityObject(Target value){
		value.Prepare();
		return value.direct;
	}
	public void Setup(string path,Component parent,string defaultSearch="[Self]"){
		this.parent = parent;
		if(!Application.isPlaying){
			this.AddSpecial("[This]",parent.gameObject);
			this.AddSpecial("[Self]",parent.gameObject);
			this.AddSpecial("[Next]",parent.gameObject.GetNextSibling(true));
			this.AddSpecial("[Previous]",parent.gameObject.GetPreviousSibling(true));
			this.AddSpecial("[NextEnabled]",parent.gameObject.GetNextSibling());
			this.AddSpecial("[PreviousEnabled]",parent.gameObject.GetPreviousSibling());
			this.AddSpecial("[Root]",parent.gameObject.GetPrefabRoot());
		}
		if(parent is ActionPart || parent is Action){
			ActionPart part = parent is ActionPart ? (ActionPart)parent : null;
			Action action = parent is Action ? (Action)parent : part.action;
			GameObject actionObject = action != null ? action.gameObject : part.gameObject;
			GameObject ownerObject = action != null ? action.owner : part.gameObject;
			this.AddSpecial("[Owner]",ownerObject);
			this.AddSpecial("[Action]",actionObject);
		}
		this.DefaultSearch(defaultSearch);
	}
	public GameObject Get(){
		this.Prepare();
		return this.direct;
	}
	public void AddSpecial(string name,GameObject target){
		if(target.IsNull()){target = this.parent.gameObject;}
		if(!this.specialNames.Any(x=>x.Contains(name,true))){
			this.specialNames.Add(name);
			this.special.Add(target);
		}
		else{
			int index = this.specialNames.FindIndex(x=>x.Contains(name,true));
			this.special[index] = target;
		}
	}
	public void SkipWarning(){this.hasWarned = true;}
	public void DefaultSearch(){this.DefaultSearch(this.fallbackSearch);}
	public void DefaultSearch(string target){
		int siblingCount = this.parent.IsNull() ? -1 : this.parent.gameObject.GetSiblingCount(true);
		this.fallbackSearch = target;
		bool searchChange = this.search != this.lastSearch;
		bool parentChange = this.parent != this.lastParent;
		bool siblingChange = this.siblingCount != siblingCount;
		if(parentChange || searchChange || siblingChange || this.direct == null){
			this.hasSearched = false;
			this.lastParent = this.parent;
			this.siblingCount = siblingCount;
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
		for(int index=0;index<this.special.Count;++index){
			string specialName = this.specialNames[index];
			GameObject special = this.special[index];
			if(!special.IsNull() && search.Contains(specialName,true)){
				string specialPath = special.GetPath();
				search = search.Replace(specialName,specialPath,true);
			}
		}
		if(search.Contains("/")){
			string[] parts = search.Split("/");
			string total = "";
			for(int index=0;index<parts.Length;++index){
				string part = parts[index];
				if(part == ".." || part == "." || part.IsEmpty()){
					if(part.IsEmpty()){continue;}
					if(total.IsEmpty()){
						int specialIndex = this.specialNames.FindIndex(x=>x.Contains("[this]",true));
						GameObject current = specialIndex != -1 ? this.special[index] : null;
						if(!current.IsNull()){
							if(part == ".."){
								total = current.GetParent().IsNull() ? "" : current.GetParent().GetPath();
							}
							else{total = current.GetPath();}
						}
						continue;
					}
					GameObject path = GameObject.Find(total);
					if(!path.IsNull()){
						if(part == ".."){
							total = path.GetParent().IsNull() ? "" : path.GetParent().GetPath();
						}
						continue;
					}
				}
				total += part + "/";
			}
			search = total;
		}
		return Locate.Find(search);
	}
	public void Prepare(){
		bool editorMode = !Application.isPlaying;
		this.search = this.search.Replace("\\","/");
		if(!this.hasSearched && !this.search.IsEmpty()){
			this.direct = this.FindTarget(this.search);
			this.lastSearch = this.search;
			this.hasSearched = true;
		}
		if(!editorMode && this.direct.IsNull() && !this.hasWarned){
			Debug.LogWarning("Target : No gameObject was found for " + this.parent.name,this.parent);
			if(!search.IsEmpty() && !search.Contains("Not Found")){
				this.search = "<" + this.search + " Not Found>";
			}
			this.hasWarned = true;
		}
	}
}
