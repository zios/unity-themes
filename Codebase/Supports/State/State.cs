using UnityEngine;
namespace Zios.Supports.State{
	public class State : MonoBehaviour{
		public bool active;
		public bool usable;
		public virtual void Begin(){this.active = this.usable;}
		public virtual void End(){this.active = false;}
		public virtual void On(){}
		public virtual void Off(){}
	}
}