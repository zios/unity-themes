using UnityEngine;
namespace Zios{
	public static class AnimationExtension{
		public static void Set(this Animation current,string name,bool state){
			if(state){current.Play(name);}
			else{current.Stop(name);}
		}
	}
}