using System;
namespace Zios.Unity.EditorUI{
	public class EditorAction{
		public Action action;
		public bool active;
		public static implicit operator EditorAction(Action current){return new EditorAction(current);}
		public static implicit operator EditorAction(Delegate current){return new EditorAction((Action)current);}
		public EditorAction(Action action){
			this.action = action;
		}
		public EditorAction(Action action,bool active){
			this.action = action;
			this.active = active;
		}
	}
}