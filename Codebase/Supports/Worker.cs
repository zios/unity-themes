using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
namespace Zios{
	[Serializable]
	public class Worker{
		public List<Thread> threads = new List<Thread>();
		public int progress;
		public int size;
		public virtual void Perform(){
			foreach(var thread in this.threads){thread.Start();}
			foreach(var thread in this.threads){thread.Join();}
		}
		public virtual void Step<Type>(List<Type> part,int position=0){}
		public void Build<Type>(List<Type> items,int threads){
			this.size = items.Count;
			if(threads==1){
				this.Step(items);
				this.Perform();
				return;
			}
			var index = 0;
			foreach(var item in items.DivideInto(threads)){
				if(item.Count == 0){break;}
				var part = item;
				var position = index;
				var chunk = new ThreadStart(()=>this.Step(part,position));
				var thread = new Thread(chunk);
				this.threads.Add(thread);
				index += part.Count;
			}
			var perform = new ThreadStart(this.Perform);
			new Thread(perform).Start();
		}
		public static Worker<int> Create(int size,MethodStepIndex<int> onStep=null,MethodEndIndex<int> onEnd=null,int threads=16){
			var data = Enumerable.Range(0,++size).ToList();
			return Worker.Create<int>(data,onStep,onEnd,threads);
		}
		public static Worker<Value> Create<Value>(List<Value> collection,MethodStepIndex<Value> onStep=null,MethodEndIndex<Value> onEnd=null,int threads=16){
			var worker = new Worker<Value>();
			worker.onStep = onStep ?? worker.onStep;
			worker.onEnd = onEnd ?? worker.onEnd;
			worker.collection = collection;
			worker.Build(collection,threads);
			return worker;
		}
		public static Worker<Key,Value> Create<Key,Value>(Dictionary<Key,Value> collection,MethodStepKey<Key,Value> onStep=null,MethodEndKey<Key,Value> onEnd=null,int threads=16){
			var worker = new Worker<Key,Value>();
			worker.onStep = onStep ?? worker.onStep;
			worker.onEnd = onEnd ?? worker.onEnd;
			worker.collection = collection;
			worker.Build(collection.Keys.ToList(),threads);
			return worker;
		}
	}
	public class Worker<Value> : Worker{
		public MethodEndIndex<Value> onEnd;
		public MethodStepIndex<Value> onStep;
		public List<Value> collection;
		public override void Perform(){
			base.Perform();
			this.onEnd(this);
		}
		public override void Step<Data>(List<Data> data,int position=0){
			var index = 0;
			while(true){
				if(index>=data.Count){break;}
				if(this.onStep(this,position+index)){
					lock(this){this.progress += 1;}
					index += 1;
				}
			}
		}
	}
	public class Worker<Key,Value> : Worker{
		public MethodEndKey<Key,Value> onEnd;
		public MethodStepKey<Key,Value> onStep;
		public Dictionary<Key,Value> collection;
		public override void Perform(){
			base.Perform();
			this.onEnd(this);
		}
		public override void Step<Data>(List<Data> data,int position=0){
			var index = 0;
			var part = data.As<List<Key>>();
			while(true){
				if(index>=part.Count){break;}
				if(this.onStep(this,part[position+index])){
					lock(this){this.progress += 1;}
					index += 1;
				}
			}
		}
	}
	public delegate bool MethodStepKey<Key,Value>(Worker<Key,Value> worker,Key key);
	public delegate void MethodEndKey<Key,Value>(Worker<Key,Value> worker);
	public delegate bool MethodStepIndex<Value>(Worker<Value> worker,int index);
	public delegate void MethodEndIndex<Value>(Worker<Value> worker);
}