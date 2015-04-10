using Zios;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios{
    public enum TargetMode{Search,Direct};
    [Serializable]
    public class Target{
	    public List<GameObject> special = new List<GameObject>();
	    public List<string> specialNames = new List<string>();
	    public string search = "";
	    public GameObject directObject;
	    public GameObject searchObject;
	    public Component parent;
	    public TargetMode mode = TargetMode.Search;
	    private bool hasSearched;
	    private bool hasWarned;
	    private int siblingCount;
	    private Component lastParent;
	    private string lastSearch = "";
	    private string fallbackSearch = "";
	    public static implicit operator Transform(Target value){return value.Get().transform;}
	    public static implicit operator GameObject(Target value){return value.Get();}
	    public static implicit operator UnityObject(Target value){return value.Get();}
	    public GameObject Get(){
		    GameObject result = this.directObject;
		    if(this.mode == TargetMode.Search){
			    this.PerformSearch();
			    return this.searchObject;
		    }
		    if(result == null && Application.isPlaying){
			    Debug.LogWarning("[Target] No target found for : " + this.parent.name);
		    }
		    return result;
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
				Events.Add("On Validate",(Method)this.DefaultSearch,parent);
		    }
		    if(parent is ActionLink || parent is StateLink){
			    ActionLink actionLink = parent is ActionLink ? (ActionLink)parent : null;
			    StateLink stateLink = parent is StateLink ? (StateLink)parent : actionLink.stateLink;
			    GameObject linkObject = stateLink != null ? stateLink.gameObject : actionLink.gameObject;
			    GameObject ownerObject = stateLink != null ? stateLink.owner : actionLink.gameObject;
			    this.AddSpecial("[Owner]",ownerObject);
			    this.AddSpecial("[Action]",linkObject);
			    this.AddSpecial("[ActionLink]",linkObject);
			    this.AddSpecial("[StateLink]",linkObject);
		    }
		    this.DefaultSearch(defaultSearch);
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
		    if(parentChange || searchChange || siblingChange || this.searchObject.IsNull()){
			    this.hasSearched = false;
			    this.lastParent = this.parent;
			    this.siblingCount = siblingCount;
			    if(this.search.IsEmpty()){
				    this.search = target;
			    }
			    this.PerformSearch();
		    }
	    }
	    public void DefaultTarget(GameObject target){
		    if(this.searchObject.IsNull()){
			    this.searchObject = target;
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
	    public void PerformSearch(){
		    bool editorMode = !Application.isPlaying;
		    this.search = this.search.Replace("\\","/");
		    if(!this.hasSearched && !this.search.IsEmpty()){
			    this.searchObject = this.FindTarget(this.search);
			    this.lastSearch = this.search;
			    this.hasSearched = true;
		    }
		    if(!editorMode && this.searchObject.IsNull() && !this.parent.IsNull() && !this.hasWarned){
			    Debug.LogWarning("[Target] No gameObject was found for search " + this.parent.GetPath(),this.parent);
			    if(!search.IsEmpty() && !search.Contains("Not Found")){
				    this.search = "<" + this.search + " Not Found>";
			    }
			    this.hasWarned = true;
		    }
	    }
    }
}