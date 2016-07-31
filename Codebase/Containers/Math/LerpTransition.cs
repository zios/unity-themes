#pragma warning disable 0169
using UnityEngine;
namespace Zios.Containers.Math{
	using Attributes;
	using Actions;
	using Events;
	public class LerpTransition{
		public AttributeBool isAngle = true;
		public AttributeBool isResetOnChange = true;
		public AttributeFloat speed = 0;
		public Transition transition = new Transition();
		protected Component parent;
		protected string path;
		protected bool fixedTime;
		protected bool active;
		public void Reset(){
			this.active = false;
		}
		public virtual void Setup(string path,Component parent){
			this.path = (parent.GetAlias() + "/" + path).Trim("/");
			this.parent = parent;
			if(this.parent is ManagedMonoBehaviour){
				var script = (ManagedMonoBehaviour)parent;
				this.fixedTime = script.rate == UpdateRate.FixedUpdate;
			}
			if(this.parent is StateMonoBehaviour){
				var script = (StateMonoBehaviour)parent;
				Event.Add(script.alias+"/On End",this.Reset,parent.gameObject);
			}
			Event.Add(path+"/Transition/On Reset",this.Reset,parent.gameObject);
			this.isAngle.Setup(path+"/Is Angle",parent);
			this.isResetOnChange.Setup(path+"/Is Reset On Change",parent);
			this.speed.Setup(path+"/Transition/Speed",parent);
			this.transition.Setup(path+"/Transition",parent);
		}
		public virtual Vector3 FixedStep(Vector3 current,Vector3 end,float size,bool[] useAxes=null){
			if(useAxes == null){useAxes = new bool[3]{true,true,true};}
			Vector3 value = Vector3.MoveTowards(current,end,size);
			if(useAxes[0]){current.x = value.x;}
			if(useAxes[1]){current.y = value.y;}
			if(useAxes[2]){current.z = value.z;}
			return current;
		}
		protected float Lerp(float start,float end,float percent){
			if(this.isAngle){return Mathf.LerpAngle(start,end,percent);}
			else{return Mathf.Lerp(start,end,percent);}
		}
		protected void CheckActive(){
			if(!this.active){
				this.active = true;
				this.transition.Reset();
			}
		}
	}
}