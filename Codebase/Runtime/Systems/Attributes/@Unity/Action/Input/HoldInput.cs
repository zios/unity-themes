using System;
using UnityEngine;
namespace Zios.Attributes.Actions{
	using Zios.Attributes.Supports;
	using Zios.Events;
	using Zios.Extensions;
	using Zios.Inputs;
	using Zios.State;
	using Zios.Unity.SystemAttributes;
	//asm Zios.Shortcuts;
	//asm Zios.Unity.Components.DataBehaviour;
	//asm Zios.Unity.Components.ManagedBehaviour;
	//asm Zios.Unity.Shortcuts;
	[AddComponentMenu("Zios/Component/Action/Input/Hold Input")]
	public class HoldInput : StateBehaviour{
		public AttributeGameObject target;
		[InputName] public AttributeString inputName = "";
		[NonSerialized] public InputInstance instance;
		public override void Awake(){
			base.Awake();
			this.inputName.Setup("Input Name",this);
			this.target.Setup("Input Target",this);
			this.AddDependent<InputInstance>(target);
			this.SetInstance();
			Events.Add("On Validate",this.SetInstance,this);
		}
		public void SetInstance(){
			this.instance = this.target.Get() ? this.target.Get().GetComponent<InputInstance>() : null;
		}
		public override void Use(){
			if(this.instance.IsNull()){return;}
			this.instance.CallEvent("Hold Input",this.inputName.Get());
		}
		public override void End(){
			if(this.instance.IsNull()){return;}
			this.instance.CallEvent("Release Input",this.inputName.Get());
		}
	}
}