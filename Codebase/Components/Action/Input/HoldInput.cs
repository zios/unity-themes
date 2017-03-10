using System;
using UnityEngine;
namespace Zios.Actions.InputComponents{
	using Attributes;
	using Inputs;
	using Event;
	[AddComponentMenu("Zios/Component/Action/Input/Hold Input")]
	public class HoldInput : StateMonoBehaviour{
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