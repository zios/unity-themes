using Zios;
using System;
using UnityEngine;
using System.Collections.Generic;
namespace Zios{
	[Serializable]
	public class AttributeGameObject : Attribute<GameObject,AttributeGameObject,AttributeGameObjectData>{
		public static string[] specialList = new string[]{"Copy","Parent"};
		public AttributeGameObject() : this(default(GameObject)){}
		public AttributeGameObject(GameObject value){this.delayedValue = value;}
		public static implicit operator AttributeGameObject(GameObject current){return new AttributeGameObject(current);}
		public static implicit operator GameObject(AttributeGameObject current){return current.Get();}
		public override GameObject Get(){
			if(this.getMethod != null){return this.getMethod();}
			AttributeGameObjectData data = this.GetFirstRaw();
			return data.target.Get();
		}
		public override void Setup(string path,Component component){
			base.Setup(path,component);
			this.canDirect = this.canFormula = this.canAdvanced = false;
			this.canGroup = true;
			this.usage = AttributeUsage.Shaped;
		}
		public void DefaultSearch(){this.GetFirst().target.DefaultSearch();}
		public void DefaultSearch(string target){this.GetFirst().target.DefaultSearch(target);}
	}
}
