using System;
using UnityEngine;
namespace Zios{
	[Serializable]
	public class EventListener{
		public object target;
		public object method;
		public string name;
		public int occurrences;
		public bool paused;
		public bool permanent;
		public bool unique;
		public bool isStatic;
		public float cooldown;
		private float resting;
		private bool warned;
		public bool IsValid(){
			var method = this.method.As<Delegate>();
			bool nullTarget = this.target.IsNull() || (!this.isStatic && method.Target.IsNull());
			if(nullTarget && !this.warned && Events.debug.Has("Call")){
				Debug.LogWarning("[Events] Null call attempted -- " + this.name + " -- " + this.target + " -- " + Events.GetMethodName(method));
				this.warned = true;
			}
			return !nullTarget;
		}
		public bool IsResting(){return Time.realtimeSinceStartup < this.resting;}
		public void Rest(float seconds=1){this.resting = Time.realtimeSinceStartup+seconds;}
		public void SetCooldown(float seconds=1){this.cooldown = seconds;}
		public void SetPaused(bool state=true){this.paused = state;}
		public void SetPermanent(bool state=true){this.permanent = state;}
		public void SetUnique(bool state=true){
			if(state){Events.unique.AddNew(this.target)[this.name] = this;}
			this.unique = state;
		}
		public void Remove(){
			if(Events.cache.ContainsKey(this.target) && Events.cache[this.target].ContainsKey(this.name)){
				Events.cache[this.target][this.name].Remove(this.method);
			}
			if(Events.cache.AddNew(Events.all).ContainsKey(this.name)){
				Events.cache[Events.all][this.name].Remove(this.method);
			}
			Events.listeners.Remove(this);
			this.paused = true;
		}
		public void Call(object[] values){
			if(Utility.IsPaused()){return;}
			if(!this.IsValid()){return;}
			if(this.cooldown > 0){this.Rest(this.cooldown);}
			if(this.occurrences > 0){this.occurrences -= 1;}
			if(this.occurrences == 0){this.Remove();}
			if(values.Length < 1 || this.method is Method){
				((Method)this.method)();
				return;
			}
			object value = values[0];
			if(this.method is MethodFull){((MethodFull)this.method)(values);}
			else if(value is object && this.method is MethodObject){((MethodObject)this.method)((object)value);}
			else if(value is int && this.method is MethodInt){((MethodInt)this.method)((int)value);}
			else if(value is float && this.method is MethodFloat){((MethodFloat)this.method)((float)value);}
			else if(value is string && this.method is MethodString){((MethodString)this.method)((string)value);}
			else if(value is bool && this.method is MethodBool){((MethodBool)this.method)((bool)value);}
			else if(value is Vector2 && this.method is MethodVector2){((MethodVector2)this.method)((Vector2)value);}
			else if(value is Vector3 && this.method is MethodVector3){((MethodVector3)this.method)((Vector3)value);}
		}
	}
}