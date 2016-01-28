using System;
using UnityEngine;
namespace Zios{
	#if UNITY_EDITOR
	using UnityEditor;
	using CallbackFunction = UnityEditor.EditorApplication.CallbackFunction;
	#endif
	public class Hook<Singleton> where Singleton : Component{
		private bool setup;
		public CallbackFunction resetMethod;
		public CallbackFunction createMethod;
		public Hook(CallbackFunction reset=null,CallbackFunction create=null){
			if(Application.isPlaying){return;}
			this.resetMethod = reset ?? this.Reset;
			this.createMethod = create ?? this.Create;
			#if UNITY_EDITOR
			EditorApplication.delayCall += this.resetMethod;
			#endif
		}
		public void Reset(){
			Events.Add("On Scene Loaded",new Method(this.resetMethod)).SetPermanent();
			Events.Add("On Hierarchy Changed",new Method(this.resetMethod)).SetPermanent();
			Events.Add("On Exit Play",new Method(this.resetMethod)).SetPermanent();
			this.setup = false;
			this.createMethod();
		}
		public void Create(){
			if(this.setup || Application.isPlaying){return;}
			this.setup = true;
			Func<Singleton> getInstance = ()=>typeof(Singleton).GetVariable<Singleton>("instance");
			Action<Singleton> setInstance = (Singleton value)=>typeof(Singleton).SetVariable("instance",value);
			if(getInstance().IsNull()){
				var path = Locate.GetScenePath("@Main");
				setInstance(path.GetComponent<Singleton>());
				if(getInstance().IsNull()){
					Debug.Log("[Hook] : Auto-creating " + typeof(Singleton).Name + " Manager GameObject.");
					setInstance(path.AddComponent<Singleton>());
				}
			}
		}
	}
}