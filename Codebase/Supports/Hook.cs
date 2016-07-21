using System;
using UnityEngine;
namespace Zios{
	using Events;
	#if UNITY_EDITOR
	using UnityEditor;
	using CallbackFunction = UnityEditor.EditorApplication.CallbackFunction;
	#endif
	public static class Hook{
		public static bool disabled;
		public static bool temporary;
		public static bool hidden;
		public static bool debug;
		public static GameObject main;
		static Hook(){
			Hook.SetState();
		}
		public static void LoadSettings(){
			#if UNITY_THEMES
			Hook.hidden = true;
			Hook.temporary = true;
			#elif UNITY_EDITOR
			Hook.hidden = EditorPrefs.GetBool("EditorSettings-HideHooks",false);
			Hook.disabled = EditorPrefs.GetBool("EditorSettings-DisableHooks",false);
			Hook.temporary = EditorPrefs.GetBool("EditorSettings-TemporaryHooks",false);
			#endif
		}
		public static void SetState(){
			Locate.SetDirty();
			Hook.LoadSettings();
			foreach(var current in Locate.GetSceneObjects()){
				if(current.name != "@Main"){continue;}
				current.hideFlags = Hook.hidden ? HideFlags.HideInHierarchy : HideFlags.None;
				if(Hook.temporary){
					current.hideFlags = Hook.hidden ? HideFlags.HideAndDontSave : HideFlags.DontSave;
				}
			}
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
			Locate.SetDirty();
			Func<Singleton> getInstance = ()=>typeof(Singleton).GetVariable<Singleton>("instance");
			Action<Singleton> setInstance = (Singleton value)=>typeof(Singleton).SetVariable("instance",value);
			Hook.main = Hook.main ?? Locate.Find("@Main");
			Hook.main = Hook.main.IsNull() ? Locate.GetScenePath("@Main") : Hook.main;
			if(getInstance().IsNull()){
				setInstance(Hook.main.GetComponent<Singleton>());
				if(getInstance().IsNull()){
					if(!Hook.hidden && Hook.debug){Debug.Log("[Hook] : Auto-creating " + typeof(Singleton).Name + " Singleton.");}
					setInstance(Hook.main.AddComponent<Singleton>());
				}
			}
			Hook.SetState();
		}
	}
}