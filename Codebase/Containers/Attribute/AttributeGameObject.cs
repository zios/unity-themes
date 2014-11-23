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
		public AttributeGameObject(GameObject value){this.Add(value);}
		public static implicit operator AttributeGameObject(GameObject current){return new AttributeGameObject(current);}
		public static implicit operator GameObject(AttributeGameObject current){return current.Get();}
		public override GameObject Get(){
			if(this.getMethod != null){return this.getMethod();}
			AttributeGameObjectData first = this.data[0];
			if(this.usage == AttributeUsage.Shaped && first.reference.IsNull()){
				return first.target.Get();
			}
			return this.GetValue(first);
		}
		public override void Setup(string path,Component component){
			this.canFormula = false;
			base.Setup(path,component);
		}
		public override GameObject HandleSpecial(SpecialGameObject special,GameObject value){
			if(special == SpecialGameObject.Parent){return value.GetParent();}
			return value;
		}
	}
	[Serializable]
	public class AttributeGameObjectData : AttributeData<GameObject,AttributeGameObject,OperatorGameObject,SpecialGameObject>{}
}
