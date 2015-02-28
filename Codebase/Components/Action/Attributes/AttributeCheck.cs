using Zios;
using System;
using UnityEngine;
namespace Zios{
    [AddComponentMenu("Zios/Component/Action/Attribute/Attribute Check")]
    public class AttributeCheck : ActionLink{
	    public AttributeBool value = false;
	    public override void Awake(){
		    base.Awake();
		    this.value.Setup("",this);
		    this.value.usage = AttributeUsage.Shaped;
	    }
	    public override void Use(){
		    bool active = this.value.Get();
		    if(active){base.Use();}
		    else{base.End();}
	    }
    }
}