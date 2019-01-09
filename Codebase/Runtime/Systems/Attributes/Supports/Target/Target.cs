using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios.Attributes.Supports{
	using Zios.Events;
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.Extensions;
	using Zios.Unity.Locate;
	using Zios.Unity.Log;
	//asm Zios.Shortcuts;
	//asm Zios.Unity.Shortcuts;
	public enum TargetMode{Search,Direct};
	[Serializable]
	public class Target{
		public static string defaultSearch = "[Self]";
		private List<GameObject> special = new List<GameObject>();
		private List<string> specialNames = new List<string>();
		public bool disabled;
		public string search = "";
		public GameObject directObject;
		public GameObject searchObject;
		public UnityObject parent;
		public TargetMode mode = TargetMode.Search;
		public string path;
		private bool verified;
		private string fallbackSearch = "";
		public static implicit operator Transform(Target value){return value.Get().transform;}
		public static implicit operator GameObject(Target value){return value.Get();}
		public static implicit operator UnityObject(Target value){return value.Get();}
		public GameObject Get(){
			this.Verify();
			GameObject result = this.mode == TargetMode.Search ? this.searchObject : this.directObject;
			if(result.IsNull() && Application.isPlaying){
				Log.Warning(this.path+"Missing","[Target] No target found for : " + this.path,this.parent,1);
			}
			return result;
		}
		public void Verify(){
			if(!this.verified && this.mode == TargetMode.Search && this.searchObject.IsNull()){
				this.Search();
				this.verified = false;
			}
		}
		public void Clear(){
			if(!this.disabled){
				Events.Remove("On Validate",this.Search,this.parent);
				Events.Remove("On Components Changed",this.Search,this.parent);
				this.disabled = true;
			}
		}
		public void Setup(string path,UnityObject parent){
			this.disabled = false;
			if(parent.Is<Component>()){
				var component = parent.As<Component>();
				this.path = component.GetPath() + "/" + path;
				this.parent = parent;
				if(!Application.isPlaying){
					this.AddSpecial("[This]",component.gameObject);
					this.AddSpecial("[Self]",component.gameObject);
					this.AddSpecial("[Next]",component.gameObject.GetNextSibling(true));
					this.AddSpecial("[Previous]",component.gameObject.GetPreviousSibling(true));
					this.AddSpecial("[NextEnabled]",component.gameObject.GetNextSibling());
					this.AddSpecial("[PreviousEnabled]",component.gameObject.GetPreviousSibling());
					this.AddSpecial("[Root]",component.gameObject.GetPrefabRoot());
					Events.Add("On Validate",this.Search,parent);
					Events.Add("On Components Changed",this.Search,parent);
					/*if(parent is StateBehaviour){
						var state = (StateBehaviour)parent;
						GameObject stateObject = state.gameObject;
						GameObject parentObject = state.gameObject;
						if(state.controller != null){
							stateObject = state.controller.gameObject;
							parentObject = state.controller.gameObject;
							if(state.controller.controller != null){
								parentObject = state.controller.controller.gameObject;
							}
						}
						this.AddSpecial("[ParentController]",parentObject);
						this.AddSpecial("[Controller]",stateObject);
						this.AddSpecial("[State]",state.gameObject);
					}*/
					this.SetFallback(Target.defaultSearch);
					if(this.searchObject.IsNull()){
						this.Search();
					}
				}
			}
		}
		public void SetFallback(string name){this.fallbackSearch = name;}
		public void AddSpecial(string name,GameObject target){
			if(target.IsNull()){target = this.parent.Is<Component>() ? this.parent.As<Component>().gameObject : null;}
			if(!this.specialNames.Any(x=>x.Contains(name,true))){
				this.specialNames.Add(name);
				this.special.Add(target);
			}
			else{
				int index = this.specialNames.FindIndex(x=>x.Contains(name,true));
				this.special[index] = target;
			}
		}
		public void Search(){
			if(this.mode != TargetMode.Search){return;}
			if(this.search.IsEmpty()){this.search = this.fallbackSearch;}
			if(this.search.IsEmpty()){return;}
			string search = this.search.Replace("\\","/");
			if(!search.IsEmpty()){
				for(int index=0;index<this.special.Count;++index){
					string specialName = this.specialNames[index];
					GameObject special = this.special[index];
					if(!special.IsNull() && search.Contains(specialName,true)){
						string specialPath = special.GetPath();
						search = search.Replace(specialName,specialPath,true);
					}
				}
				if(search.ContainsAny("/",".")){
					string[] parts = search.Split("/");
					string total = "";
					GameObject current = null;
					for(int index=0;index<parts.Length;++index){
						string part = parts[index];
						current = GameObject.Find(total);
						if(part.IsEmpty()){continue;}
						if(part == ".." || part == "."){
							if(total.IsEmpty()){
								int specialIndex = this.specialNames.FindIndex(x=>x.Contains("[this]",true));
								current = specialIndex != -1 ? this.special[index] : null;
								if(!current.IsNull()){
									if(part == ".."){
										total = current.GetParent().IsNull() ? "" : current.GetParent().GetPath();
									}
									else{total = current.GetPath();}
								}
								continue;
							}
							current = GameObject.Find(total);
							if(!current.IsNull()){
								if(part == ".."){
									total = current.GetParent().IsNull() ? "" : current.GetParent().GetPath();
								}
								continue;
							}
						}
						GameObject next = GameObject.Find(total+part+"/");
						if(next.IsNull() && !current.IsNull() && Attribute.lookup.ContainsKey(current)){
							var match = Attribute.lookup[current].Where(x=>x.Value.info.name.Matches(part)).FirstOrDefault().Value;
							if(match is AttributeGameObject){
								next = match.As<AttributeGameObject>().Get();
								if(!next.IsNull()){
									total = next.GetPath();
								}
								continue;
							}
						}
						total += part + "/";
					}
					search = total;
				}
				this.searchObject = Locate.Find(search);
			}
		}
	}
}