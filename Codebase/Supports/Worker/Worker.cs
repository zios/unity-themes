using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
namespace Zios.Supports.Worker{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.Log;
	public class Worker{
		public static Dictionary<Thread,Worker> all = new Dictionary<Thread,Worker>();
		public static bool monitored = true;
		public List<Thread> threads = new List<Thread>();
		public List<Thread> remaining = new List<Thread>();
		public List<Action> sourceCallback = new List<Action>();
		public Func<bool> executeStep = ()=>true;
		public Thread manager;
		public bool async;
		public bool quit;
		public int progress;
		public int size;
		//=================
		// Interface
		//=================
		public static Worker<int> Create(int size,MethodStepIndex<int> onStep=null,MethodEndIndex<int> onEnd=null,bool async=false,int threads=-1){
			var data = Enumerable.Range(0,++size).ToList();
			return Worker.Create<int>(data,onStep,onEnd,async,threads);
		}
		public static Worker Create(Func<bool> method){
			var worker = new Worker();
			worker.executeStep = method;
			worker.async = true;
			var perform = new ThreadStart(worker.Step);
			var thread = new Thread(perform);
			Worker.all[thread] = worker;
			worker.threads.Add(thread);
			worker.Build();
			return worker;
		}
		public static Worker<Value> Create<Value>(List<Value> collection,MethodStepIndex<Value> onStep=null,MethodEndIndex<Value> onEnd=null,bool async=false,int threads=-1){
			var worker = new Worker<Value>();
			worker.onStep = onStep;
			worker.onEnd = onEnd ?? worker.onEnd;
			worker.collection = collection;
			worker.Build(collection,async,threads);
			return worker;
		}
		public static Worker<Value,Output> Create<Value,Output>(List<Value> collection,MethodStepIndex<Value,Output> onStep=null,MethodEndIndex<Value> onEnd=null,bool async=false,int threads=-1){
			var worker = new Worker<Value,Output>();
			worker.onStep = onStep;
			worker.onEnd = onEnd ?? worker.onEnd;
			worker.collection = collection;
			worker.Build(collection,async,threads);
			return worker;
		}
		public static WorkerDictionary<Key,Value> CreateKeyed<Key,Value>(Dictionary<Key,Value> collection,MethodStepKey<Key,Value> onStep=null,MethodEndKey<Key,Value> onEnd=null,bool async=false,int threads=-1){
			var worker = new WorkerDictionary<Key,Value>();
			worker.onStep = onStep;
			worker.onEnd = onEnd ?? worker.onEnd;
			worker.collection = collection;
			worker.Build(collection.Keys.ToList(),async,threads);
			return worker;
		}
		public static WorkerDictionary<Key,Value> CreateAsyncKeyed<Key,Value>(Dictionary<Key,Value> collection,MethodStepKey<Key,Value> onStep=null,MethodEndKey<Key,Value> onEnd=null,int threads=-1){
			return Worker.CreateKeyed(collection,onStep,onEnd,true,threads);
		}
		public static Worker<int> CreateAsync<Value>(int size,MethodStepIndex<int> onStep=null,MethodEndIndex<int> onEnd=null,int threads=-1){
			return Worker.Create(size,onStep,onEnd,true,threads);
		}
		public static Worker<Value> CreateAsync<Value>(List<Value> collection,MethodStepIndex<Value> onStep=null,MethodEndIndex<Value> onEnd=null,int threads=-1){
			return Worker.Create(collection,onStep,onEnd,true,threads);
		}
		public static Worker<Value> CreateAsync<Value>(List<Value> collection,MethodStepItem<Value> onStep=null,MethodEndIndex<Value> onEnd=null,int threads=-1){
			return Worker.Create(collection,onStep.AsIndexed(),onEnd,true,threads);
		}
		public static Worker<Value> CreateAsync<Value>(List<Value> collection,MethodStepSimple<Value> onStep=null,MethodEndIndex<Value> onEnd=null,int threads=-1){
			return Worker.Create(collection,onStep.AsIndexed(),onEnd,true,threads);
		}
		public static Worker<Value,Output> CreateAsync<Value,Output>(List<Value> collection,MethodStepIndex<Value,Output> onStep=null,MethodEndIndex<Value> onEnd=null,int threads=-1){
			return Worker.Create(collection,onStep,onEnd,true,threads);
		}
		public static Worker<Value,Output> CreateAsync<Value,Output>(List<Value> collection,MethodStepItem<Value,Output> onStep=null,MethodEndIndex<Value> onEnd=null,int threads=-1){
			return Worker.Create(collection,onStep.AsIndexedOut(),onEnd,true,threads);
		}
		public static Worker<Type> Get<Type>(){return (Worker<Type>)Worker.all[Thread.CurrentThread];}
		public static Worker Get(){return Worker.all.TryGet(Thread.CurrentThread);}
		public static void MainThread(Action method){
			var worker = Worker.Get();
			if(worker.IsNull()){
				method();
				return;
			}
			worker.Handoff(method);
		}
		public void Handoff(Action method){
			lock(this.sourceCallback){this.sourceCallback.Add(method);}
			if(this.threads.Count<1){this.CheckHandoff();}
			while(true){
				lock(this.sourceCallback){
					if(!this.sourceCallback.Contains(method)){break;}
				}
			}
		}
		//=================
		// Internal
		//=================
		public static void Check(){
			var workers = Worker.all.Copy().Values;
			foreach(var worker in workers){
				if(worker.async && worker.manager.IsNull()){
					foreach(var thread in worker.remaining.Copy()){
						worker.CheckHandoff();
						if(thread.Join(0)){worker.remaining.Remove(thread);}
					}
					if(worker.remaining.Count()<1){worker.End();}
				}
			}
		}
		public void Build(){
			this.Start();
			ProxyEditor.AddUpdate(Worker.Check);
		}
		public void Build<Type>(List<Type> items,bool async,int threads){
			if(threads==-1){threads = Environment.ProcessorCount;}
			threads -= 1;
			this.async = async;
			this.size = items.Count;
			if(threads <= 0){
				this.Step(items,0);
				this.Main();
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
				Worker.all[thread] = this;
				index += part.Count;
			}
			if(!async){
				this.Main();
				return;
			}
			if(Worker.monitored){
				this.Build();
				return;
			}
			var perform = new ThreadStart(this.Main);
			this.manager = new Thread(perform);
		}
		public virtual void Main(){
			this.Start();
			this.Monitor();
			this.End();
		}
		public virtual void Start(){
			this.remaining = this.threads.Copy();
			foreach(var thread in this.threads){thread.Start();}
		}
		public virtual void Monitor(){
			while(this.remaining.Count > 0){
				foreach(var thread in this.remaining.Copy()){
					this.CheckHandoff();
					if(thread.Join(0)){this.remaining.Remove(thread);}
				}
			}
		}
		public virtual void End(){
			foreach(var thread in this.remaining){thread.Abort();}
			Worker.all.Remove(x=>x.Value==this);
		}
		public void CheckHandoff(){
			while(true){
				lock(this.sourceCallback){
					if(this.sourceCallback.Count < 1){break;}
					try{this.sourceCallback[0]();}
					catch(Exception exception){
						var message = "[Worker-Handoff] Uncaught exception. \n";
						message += exception.Message;
						Log.Error(message);
					}
					this.sourceCallback.RemoveAt(0);
				}
			}
		}
		public virtual void Step(){
			while(this.executeStep()){}
		}
		public virtual void Step<Data>(List<Data> data,int position=0){}
		public virtual int Execute(int index,int size,object element){
			bool success = false;
			try{success = this.executeStep();}
			catch(Exception exception){
				var message = "[Worker] Uncaught exception for ("+element.GetType().Name + ") on " + index + "/" + size + "\n";
				message += element.ToString() + "\n" + exception.Message;
				Log.Error(message);
				return -1;
			}
			if(success){
				Interlocked.Increment(ref this.progress);
				index += 1;
			}
			return index;
		}
	}
	public class Worker<Value> : Worker{
		public MethodEndIndex<Value> onEnd = (x)=>{};
		public MethodStepIndex<Value> onStep;
		public List<Value> collection;
		public override void End(){
			this.onEnd(this);
			base.End();
		}
		public override void Step<Data>(List<Data> data,int position=0){
			var index = 0;
			while(index < data.Count && index >= 0){
				if(this.quit){break;}
				this.executeStep = ()=>this.onStep(this,position+index);
				index = this.Execute(index,this.collection.Count,data[index]);
			}
		}
	}
	public class Worker<Value,Output> : Worker<Value>{
		public new MethodStepIndex<Value,Output> onStep;
		public List<Output>[] result;
		public bool skip;
		public override void Start(){
			this.result = new List<Output>[this.threads.Count];
			base.Start();
		}
		public override void Step<Data>(List<Data> data,int position=0){
			var index = 0;
			var result = default(Output);
			var output = new List<Output>();
			var response = new WorkerResponse();
			while(index < data.Count && index >= 0){
				if(this.quit){break;}
				try{result = this.onStep(this,response,position+index);}
				catch(Exception exception){
					var message = "[Worker] Uncaught exception for ("+data[index].GetType().Name + ") on " + index + "/" + this.collection.Count + "\n";
					message += data[index].ToString() + "\n" + exception.Message;
					Log.Error(message);
					break;
				}
				if(!response.skip){output.Add(result);}
				response.skip = false;
				Interlocked.Increment(ref this.progress);
				index += 1;
			}
			lock(this.result){this.result[this.threads.IndexOf(Thread.CurrentThread)] = output;}
		}
	}
	public class WorkerDictionary<Key,Value> : Worker{
		public MethodEndKey<Key,Value> onEnd = (x)=>{};
		public MethodStepKey<Key,Value> onStep;
		public Dictionary<Key,Value> collection;
		public override void End(){
			this.onEnd(this);
			base.End();
		}
		public override void Step<Data>(List<Data> data,int position=0){
			var index = 0;
			var part = data.As<List<Key>>();
			while(index < data.Count && index >= 0){
				if(this.quit){break;}
				var element = part[position+index];
				this.executeStep = ()=>this.onStep(this,element);
				index = this.Execute(index,this.collection.Count,this.collection[element]);
			}
		}
	}
	public class WorkerResponse{
		public bool skip;
	}
	public delegate void MethodEndKey<Key,Value>(WorkerDictionary<Key,Value> worker);
	public delegate void MethodEndIndex<Value>(Worker<Value> worker);
	public delegate bool MethodStepSimple<Value>(Value item);
	public delegate bool MethodStepKey<Key,Value>(WorkerDictionary<Key,Value> worker,Key key);
	public delegate bool MethodStepIndex<Value>(Worker<Value> info,int index);
	public delegate bool MethodStepItem<Value>(Worker<Value> info,Value item);
	public delegate Output MethodStepIndex<Value,Output>(Worker<Value> info,WorkerResponse response,int index);
	public delegate Output MethodStepItem<Value,Output>(Worker<Value> info,WorkerResponse response,Value item);
	public static class MethodStepExtensions{
		public static MethodStepIndex<Value> AsIndexed<Value>(this MethodStepItem<Value> current){
			return (worker,index)=>current(worker,worker.collection[index]);
		}
		public static MethodStepIndex<Value> AsIndexed<Value>(this MethodStepSimple<Value> current){
			return (worker,index)=>current(worker.collection[index]);
		}
		public static MethodStepIndex<Value,Output> AsIndexedOut<Value,Output>(this MethodStepItem<Value,Output> current){
			return (worker,response,index)=>current(worker,response,worker.collection[index]);
		}
	}
}
namespace Zios.Supports.Worker{
	public static class IEnumerableExtensions{
		public static IEnumerable<Output> ThreadedSelect<Type,Output>(this IEnumerable<Type> source,Func<Type,Output> method){
			MethodStepItem<Type,Output> Select = (worker,response,value)=>method(value);
			return Worker.Create(source.ToList(),Select.AsIndexedOut()).result.SelectMany(x=>x);
		}
		public static IEnumerable<Type> ThreadedWhere<Type>(this IEnumerable<Type> source,Func<Type,bool> method){
			MethodStepItem<Type,Type> Where = (worker,response,value)=>{
				response.skip = !method(value);
				return value;
			};
			return Worker.Create(source.ToList(),Where.AsIndexedOut()).result.SelectMany(x=>x);
		}
		public static int ThreadedCount<Type>(this IEnumerable<Type> source,Func<Type,bool> method){
			return source.ThreadedWhere(method).Count();
		}
	}
}