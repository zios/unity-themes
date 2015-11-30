using UnityEngine;
using Zios;
namespace Zios{
	[AddComponentMenu("")]
	public class AttributeGameObjectData : AttributeData<GameObject,AttributeGameObject,AttributeGameObjectData>{
		public override GameObject HandleSpecial(){
			GameObject value = this.value;
			string special = AttributeGameObject.specialList[this.special];
			if(special == "Parent"){return value.GetParent();}
			return value;
		}
		public override GameObject Get(){
			AttributeInfo attribute = this.attribute;
			if(!Attribute.ready && Application.isPlaying){
				if(Attribute.debug.Has("Issue")){Debug.LogWarning("[AttributeData] Get attempt before attribute data built : " + attribute.fullPath,attribute.parent);}
				return default(GameObject);
			}
			else if(this.reference.IsNull()){
				GameObject target = this.target.Get();
				if(target.IsNull() && !Attribute.getWarning.ContainsKey(this)){
					string source = "("+attribute.fullPath+")";
					string goal = (target.GetPath() + this.referencePath).Trim("/");
					if(Attribute.debug.Has("Issue")){Debug.LogWarning("[AttributeData] Get : No reference found for " + source + " to " + goal,attribute.parent);}
					Attribute.getWarning[this] = true;
				}
				return target;
			}
			this.value = ((AttributeGameObject)this.reference).Get();
			if(attribute.mode == AttributeMode.Linked){return this.value;}
			return this.HandleSpecial();
		}
	}
}