using Zios;
using UnityEngine;
namespace Zios{
    [AddComponentMenu("Zios/Component/Action/Move/Link Objects")]
    public class LinkObject : StateMonoBehaviour{
	    public LinkMode mode;
	    [EnumMask] public LinkOptions options = (LinkOptions)(-1);
	    public AttributeGameObject target = new AttributeGameObject();
	    public AttributeGameObject linkTo = new AttributeGameObject();
	    [Internal] public AttributeBool isLinked = false;
	    private Transform previousLink;
	    private Quaternion preserveRotation;
	    private Vector3 preservePosition;
	    private Vector3 preserveScale;
	    public override void Awake(){
		    base.Awake();
		    this.isLinked.Setup("Is linked",this);
		    this.target.Setup("Target",this);
		    this.linkTo.Setup("Link To",this);
	    }
	    public override void Use(){
		    GameObject target = this.target.Get();
		    GameObject linkTo = this.linkTo.Get();
		    if(linkTo.IsNull() || target.IsNull()){return;}
		    if(!this.isLinked){
			    if(!this.options.Contains(LinkOptions.Position)){this.preservePosition = target.transform.position;}
			    if(!this.options.Contains(LinkOptions.Rotation)){this.preserveRotation = target.transform.rotation;}
			    //if(!this.options.Contains(LinkOptions.Scale)){this.preserveScale = target.transform.localScale;}
			    this.previousLink = target.transform.parent;
			    this.isLinked.Set(true);
		    }
		    target.transform.parent = (this.mode != LinkMode.Unlink) ? linkTo.transform : this.previousLink;
		    if(!this.options.Contains(LinkOptions.Position)){target.transform.position = this.preservePosition;}
		    if(!this.options.Contains(LinkOptions.Rotation)){target.transform.rotation = this.preserveRotation;}
		    base.Use();
	    }
	    public override void End(){
		    if(this.mode == LinkMode.Automatic){
			    GameObject target = this.target.Get();
			    target.transform.parent = this.previousLink;
			    this.previousLink = null;
			    this.isLinked.Set(false);
		    }
		    base.End();
	    }
    }
    public enum LinkOptions : int{
	    Position  = 0x000,
	    Rotation  = 0x001,
	    //Scale    = 0x002,
    }
    public enum LinkMode{Automatic,Link,Unlink}
}
