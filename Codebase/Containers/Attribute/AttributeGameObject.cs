using Zios;
using System;
using UnityEngine;
using System.Collections.Generic;
namespace Zios{
	public enum OperatorGameObject{}
	public enum SpecialGameObject{Copy,Parent};
	[Serializable]
	public class AttributeGameObject : Attribute<GameObject,AttributeGameObject,AttributeGameObjectData,OperatorGameObject,SpecialGameObject>{
		public AttributeGameObject() : this(null){}
		public AttributeGameObject(GameObject value){this.delayedValue = value;}
		public static implicit operator AttributeGameObject(GameObject current){return new AttributeGameObject(current);}
		public static implicit operator GameObject(AttributeGameObject current){return current.Get();}
		public override GameObject Get(){
			if(this.getMethod != null){return this.getMethod();}
			AttributeGameObjectData data = this.GetFirst();
			if(this.usage == AttributeUsage.Shaped && data.reference.IsNull()){
				return data.target.Get();
			}
			return data.Get();
		}
		public override void Setup(string path,Component component){
			this.canDirect = this.canFormula = false;
			this.usage = AttributeUsage.Shaped;
			base.Setup(path,component);
		}
		public void DefaultSearch(string target){
			this.GetFirst().target.DefaultSearch(target);
		}
	}
}
