using System;
using System.Collections;
using System.Collections.Generic;
namespace Zios.Supports.Stepper{
	using Zios.Extensions;
	using Zios.Shortcuts;
	using Zios.Unity.EditorUI;
	using Zios.Unity.Proxy;
	[Serializable]
	public class Stepper{
		public static List<Stepper> instances = new List<Stepper>();
		public static Stepper active;
		public static string title;
		public static string message;
		public Method onEnd = ()=>{};
		public MethodStep onStep;
		public IList collection;
		public int index;
		public int passes = 1;
		public bool complete;
		public bool inline;
		public Stepper(MethodStep onStep,Method onEnd,IList collection,int passCount=1,bool inline=false){
			this.onStep = onStep;
			this.collection = collection;
			this.passes = passCount;
			this.onEnd = onEnd ?? this.onEnd;
			this.inline = inline;
			Stepper.instances.AddNew(this);
		}
		public void Step(){
			Stepper.active = this;
			var count = this.passes;
			float percent = 0;
			while(count > 0){
				count -= 1;
				bool canceled = false;
				if(this.index != -1){
					this.onStep(this.collection,this.index);
					percent = ((float)this.index)/this.collection.Count;
					if(!this.inline){canceled = EditorUI.DrawProgressBar(Stepper.title,Stepper.message,percent,this.inline);}
					this.index += 1;
				}
				bool loading = Proxy.IsLoading();
				bool ended = (this.index > this.collection.Count-1) || this.index == -1;
				if((loading || canceled || ended) && !this.complete){
					this.End();
					break;
				}
			}
			if(this.inline){EditorUI.DrawProgressBar(Stepper.title,Stepper.message,percent,this.inline);}
			Stepper.active = null;
		}
		public void End(){
			this.index = -1;
			this.complete = true;
			this.onEnd();
			Stepper.instances.Remove(this);
			EditorUI.ClearProgressBar();
		}
	}
}