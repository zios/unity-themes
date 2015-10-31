#pragma warning disable 0618
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
namespace Zios{
	[Serializable]
	public class EventStepper{
		public MethodStep method;
		public string eventName;
		public IList collection;
		public int index;
		public bool complete;
		public List<Method> passes = new List<Method>();
		public void Step(){
			bool canceled = false;
			if(this.index != -1){
				this.method(this.collection,this.index);
				float percent = ((float)this.index)/this.collection.Count;
				canceled = Utility.DisplayCancelableProgressBar(Events.stepperTitle,Events.stepperMessage,percent);
				this.index += 1;
			}
			bool loading = Application.isLoadingLevel || EventDetector.loading || this.passes.Count < 1;
			bool ended = (this.index > this.collection.Count-1) || this.index == -1;
			if((loading || canceled || ended) && !this.complete){
				this.index = -1;
				foreach(var pass in this.passes){
					Events.Remove(this.eventName,pass);
				}
				this.complete = true;
				this.passes.Clear();
				Events.steppers.Remove(this.method);
				Utility.ClearProgressBar();
			}
		}
	}
}