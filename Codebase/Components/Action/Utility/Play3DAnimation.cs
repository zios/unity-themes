using UnityEngine;
using System;
using Zios;
namespace Zios{
    [AddComponentMenu("Zios/Component/Action/Play Animation (3D)")]
    public class Play3DAnimation : ActionLink{
	    public AttributeString animationName = "";
	    public AttributeFloat speed = 1;
	    public AttributeFloat weight = 1;
	    public AttributeGameObject target = new AttributeGameObject();
	    public override void Awake(){
		    base.Awake();
		    this.AddDependent<AnimationController>(this.target);
		    this.animationName.Setup("Animation Name",this);
		    this.speed.Setup("Speed",this);
		    this.weight.Setup("Weight",this);
		    this.target.Setup("Target",this);
	    }
	    public override void Use(){
		    base.Use();
		    string name = this.animationName.Get();
		    this.target.Get().Call("Set Animation Speed",name,this.speed.Get());
            this.target.Get().Call("Set Animation Weight", name, this.weight.Get());
            this.target.Get().Call("Play Animation", name);
	    }
	    public override void End(){
		    base.End();
		    GameObject target = this.target.Get();
		    if(!target.IsNull()){
			    target.Call("Stop Animation",this.animationName.Get());
		    }
	    }
    }
}