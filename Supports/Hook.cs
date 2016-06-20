using System;
using System.Linq;
using UnityEngine;
namespace Zios{
	using Events;
	#if UNITY_EDITOR
	using UnityEditor;
	using CallbackFunction = UnityEditor.EditorApplication.CallbackFunction;
	#endif
	public static class Hook{
		public static bool disabled;
		public static bool hidden;
		#if UNITY_EDITOR
		static Hook(){
			Hook.hidden = EditorPrefs.GetBool("EditorSettings-HideHooks",false);
		}
		#endif
		public static void SetHidden(bool state){
			Locate.SetDirty();
			foreach(var current in Locate.GetSceneObjects()){
				if(current.name != "@Main"){continue;}
				current.hideFlags = state ? HideFlags.HideInHierarchy : HideFlags.None;
				foreach(var component in Locate.GetObjectComponents<Component>(current)){
					component.hideFlags = current.hideFlags;
				}
			}
			Hook.hidden = state;
			Utility.Destroy(new GameObject("@*#&"));
		}
	}
	public class Hook<Singleton> where Singleton : Component{
		private bool setup;
		public bool disabled;
		public CallbackFunction resetMethod;
		public CallbackFunction createMethod;
		public Hook(CallbackFunction reset=null,CallbackFunction create=null){
			if(this.disabled || Hook.disabled || Application.isPlaying){return;}
			this.resetMethod = reset ?? this.Reset;
			this.createMethod = create ?? this.Create;
			#if UNITY_EDITOR
			EditorApplication.delayCall += this.resetMethod;
			#endif
		}
		public void Reset(){
			if(this.disabled || Hook.disabled){return;}
			Event.Add("On Level Was Loaded",new Method(this.resetMethod)).SetPermanent();
			Event.Add("On Components Changed",new Method(this.resetMethod)).SetPermanent();
			this.setup = false;
			this.createMethod();
		}
		public void Create(){
			if(this.disabled || Hook.disabled || this.setup || Application.isPlaying){return;}
			this.setup = true;
			Func<Singleton> getInstance = ()=>typeof(Singleton).GetVariable<Singleton>("instance");
			Action<Singleton> setInstance = (Singleton value)=>typeof(Singleton).SetVariable("instance",value);
			var rootObjects = Locate.GetSceneObjects().Where(x=>!x.IsNull()&&x.transform.parent.IsNull());
			rootObjects.Where(x=>x.name!="@Main"&&x.name.Contains("@Main")).ToList().ForEach(x=>Utility.Destroy(x));
			if(getInstance().IsNull()){
				var path = Locate.GetScenePath("@Main");
				setInstance(path.GetComponent<Singleton>());
				if(getInstance().IsNull() && !Hook.hidden){
					Debug.Log("[Hook] : Auto-creating " + typeof(Singleton).Name + " Singleton.");
					setInstance(path.AddComponent<Singleton>());
				}
			}
			Hook.SetHidden(Hook.hidden);
		}
	}
}