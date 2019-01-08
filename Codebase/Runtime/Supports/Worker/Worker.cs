using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
namespace Zios.Supports.Worker{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.Log;
	public class Worker{
		public static Dictionary<Thread,Worker> all = new Dictionary<Thread,Worker>();
		public static Thread mainThread;
		public List<Thread> threads = new List<Thread>();
		public List<Thread> remaining = new List<Thread>();
		public List<Action> sourceCallback = new List<Action>();
		public MethodEnd onEnd;
		public Thread manager;
		public bool finalize;
		public object group;
		public bool stepper;
		public bool monitor;
		public bool async;
		public bool quit;
		public int threadCount;
		public int progress;
		public int size;
		//=================
		// Interface
		//=================
		static Worker(){
			Worker.mainThread = Thread.CurrentThread;
		}
		public static Worker<Type> Get<Type>(){
			return (Worker<Type>)Worker.Get();
		}
		public static Worker Get(){
			var thread = Thread.CurrentThread;
			//var managerThread = Worker.all.Where(x=>x.Value.manager==thread).ToArray();
			//if(managerThread.Length > 0){return managerThread[0].Value;}
			return Worker.all.TryGet(thread);
		}
		public static Worker Create(Func<bool> method){
			var worker = new WorkerCall().Async();
			worker.method = method;
			var perform = new ThreadStart(worker.Step);
			var thread = new Thread(perform);
			lock(Worker.all){Worker.all[thread] = worker;}
			worker.threads.Add(thread);
			worker.MonitorStart();
			return worker;
		}
		public static Worker<int> Create(int size){
			var data = Enumerable.Range(0,++size).ToList();
			return Worker.Create(data);
		}
		public static Worker<Value> Create<Value>(List<Value> collection){
			return new Worker<Value>(collection);
		}
		public static WorkerDictionary<Key,Value> Create<Key,Value>(Dictionary<Key,Value> collection){
			return new WorkerDictionary<Key,Value>(collection);
		}
		public static Worker<Value,Output> Create<Output,Value>(List<Value> collection,MethodStep<Value,Output> onStep){
			return new Worker<Value,Output>(collection).OnStep(onStep);
		}
		public static void Quit(object group,bool finish=true){
			if(group.IsNull()){return;}
			lock(Worker.all){
				foreach(var worker in Worker.all.Select(x=>x.Value).Where(x=>x.group==group)){
					lock(worker){worker.quit = true;}
				}
			}
		}
		public static void MainThread(Action method){
			var worker = Worker.Get();
			if(worker.IsNull()){
				method();
				return;
			}
			worker.Handoff(method);
		}
		//=================
		// Internal
		//=================
		public void Handoff(Action method){
			lock(this.sourceCallback){this.sourceCallback.Add(method);}
			if(!this.async && this.threads.Count<1){this.CheckHandoff();}
			while(true){
				lock(this.sourceCallback){
					if(!this.sourceCallback.Contains(method)){break;}
				}
			}
		}
		public void MonitorStart(){
			this.ThreadStart();
			ProxyEditor.AddUpdate(Worker.MonitorCheck);
		}
		public static void MonitorCheck(){
			var workers = new Worker[0];
			lock(Worker.all){workers = Worker.all.Copy().Values.Where(x=>x.monitor).ToArray();}
			foreach(var worker in workers){
				lock(worker.remaining){
					foreach(var thread in worker.remaining){
						worker.CheckHandoff();
					}
				}
			}
		}
		public virtual void ThreadStart(){
			this.remaining = this.threads.Copy();
			foreach(var thread in this.threads){thread.Start();}
		}
		public virtual void ThreadCheck(){
			while(this.remaining.Count > 0){
				lock(this.remaining){
					foreach(var thread in this.remaining){
						this.CheckHandoff();
					}
				}
			}
		}
		public virtual void ThreadEnd(){
			if(this.onEnd != null && (!this.quit || this.finalize)){
				if(this.stepper){Worker.MainThread(()=>this.onEnd());}
				else{this.onEnd();}
			}
			//foreach(var thread in this.remaining){thread.Abort();}
			lock(Worker.all){Worker.all.Remove(x=>x.Value==this);}
		}
		public virtual void StepEnd(){
			lock(this.remaining){
				this.remaining.Remove(Thread.CurrentThread);
				if(this.remaining.Count()<1){
					this.ThreadEnd();
				}
			}
		}
		public void CheckHandoff(){
			while(true){
				lock(this.sourceCallback){
					if(this.sourceCallback.Count < 1){break;}
					try{this.sourceCallback[0]();}
					catch(Exception exception){
						var message = "[Worker-Handoff] Uncaught exception. \n";
						message += exception.Message + "\n" + exception.StackTrace;
						Log.Error(message);
					}
					this.sourceCallback.RemoveAt(0);
				}
			}
		}
		public virtual void Step<Data>(List<Data> data,int position=0){this.StepEnd();}
		public virtual int Execute(Func<bool> step,int index,object element){
			bool success = false;
			try{
				if(this.stepper){Worker.MainThread(()=>success = step());}
				else{success = step();}
			}
			catch(Exception exception){
				var message = "[Worker] Uncaught exception for ("+element.GetType().Name + ") on " + index + "/" + this.size + "\n";
				message += element.GetType().Name + "\n" + exception.Message + "\n" + exception.StackTrace;
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
	public class WorkerCall : Worker{
		public Func<bool> method;
		public void Step(){
			while(this.method()){}
			this.StepEnd();
		}
	}
	public class Worker<Value> : Worker{
		public MethodStep<Value> onStep;
		public MethodStepIndex onStepIndex;
		public List<Value> collection;
		public Worker(List<Value> collection){this.collection = collection;}
		public Worker<Value> Build(){return this.Build(this.collection);}
		public override void Step<Data>(List<Data> data,int position=0){
			var index = 0;
			var useIndex = !this.onStepIndex.IsNull();
			Func<bool> step = ()=>false;
			while(index < data.Count && index >= 0){
				if(this.quit){break;}
				if(useIndex){
					step = ()=>this.onStepIndex(position+index);
				}
				else{
					var element = data[index].As<Value>();
					step = ()=>this.onStep(element);
				}
				index = this.Execute(step,index,data[index]);
			}
			this.StepEnd();
		}
	}
	public class Worker<Value,Output> : Worker<Value>{
		public new MethodStep<Value,Output> onStep;
		public new MethodStepIndex<Output> onStepIndex;
		public List<Output>[] result;
		public bool skip;
		public Worker(List<Value> collection) : base(collection){}
		public new Worker<Value,Output> Build(){return this.Build(this.collection);}
		public override void ThreadStart(){
			this.result = new List<Output>[this.threads.Count];
			base.ThreadStart();
		}
		public override void Step<Data>(List<Data> data,int position=0){
			var index = 0;
			var result = default(Output);
			var output = new List<Output>();
			var response = new WorkerResponse();
			var useIndex = !this.onStepIndex.IsNull();
			Func<bool> step = ()=>false;
			while(index < data.Count && index >= 0){
				if(this.quit){break;}
				if(useIndex){
					step = ()=>{
						result = this.onStepIndex(response,position+index);
						return true;
					};
				}
				else{
					var element = data[index].As<Value>();
					step = ()=>{
						result = this.onStep(response,element);
						return true;
					};
				}
				index = this.Execute(step,index,data[index]);
				if(index == -1){break;}
				if(!response.skip){output.Add(result);}
				response.skip = false;
			}
			lock(this.result){this.result[this.threads.IndexOf(Thread.CurrentThread)] = output;}
			this.StepEnd();
		}
	}
	public class WorkerDictionary<Key,Value> : Worker{
		public MethodStep<Value> onStep;
		public MethodStep<Key> onStepKey;
		public Dictionary<Key,Value> collection;
		public WorkerDictionary(Dictionary<Key,Value> collection){this.collection = collection;}
		public WorkerDictionary<Key,Value> Build(){return this.Build(this.collection.Keys.ToList());}
		public override void Step<Data>(List<Data> data,int position=0){
			var index = 0;
			var part = data.As<List<Key>>();
			var useKey = !this.onStepKey.IsNull();
			Func<bool> step = ()=>false;
			while(index < data.Count && index >= 0){
				if(this.quit){break;}
				if(useKey){
					var key = part[index];
					step = ()=>this.onStepKey(key);
					index = this.Execute(step,index,key);
				}
				else{
					var value = this.collection[part[index]];
					step = ()=>this.onStep(value);
					index = this.Execute(step,index,value);
				}
			}
			this.StepEnd();
		}
	}
	public class WorkerResponse{
		public bool skip;
	}
}
namespace Zios.Supports.Worker{
	using Zios.Unity.Log;
	using Zios.Extensions;
	using Zios.Reflection;
	public static class IEnumerableExtensions{
		public static IEnumerable<Output> ThreadedSelect<Type,Output>(this IEnumerable<Type> source,Func<Type,Output> method){
			MethodStep<Type,Output> Select = (response,value)=>method(value);
			return Worker.Create(source.ToList(),Select).Build().result.SelectMany(x=>x);
		}
		public static IEnumerable<Type> ThreadedWhere<Type>(this IEnumerable<Type> source,Func<Type,bool> method){
			MethodStep<Type,Type> Where = (response,value)=>{
				response.skip = !method(value);
				return value;
			};
			return Worker.Create(source.ToList(),Where).Build().result.SelectMany(x=>x);
		}
		public static int ThreadedCount<Type>(this IEnumerable<Type> source,Func<Type,bool> method){
			return source.ThreadedWhere(method).Count();
		}
	}
	public static class WorkerExtensions{
		public static Worker<Value> OnStep<Value>(this Worker<Value> current,MethodStepIndex method){
			current.onStepIndex = method;
			return current;
		}
		public static Worker<int,Output> OnStep<Output>(this Worker<int,Output> current,MethodStepIndex<Output> method){
			current.onStepIndex = method;
			return current;
		}
		public static Worker<Value> OnStep<Value>(this Worker<Value> current,MethodStep<Value> method){
			current.onStep = method;
			return current;
		}
		public static Worker<Value,Output> OnStep<Value,Output>(this Worker<Value,Output> current,MethodStep<Value,Output> method){
			current.onStep = method;
			return current;
		}
		public static WorkerDictionary<Key,Value> OnStep<Key,Value>(this WorkerDictionary<Key,Value> current,MethodStep<Value> method){
			current.onStep = method;
			return current;
		}
		public static WorkerDictionary<Key,Value> OnStep<Key,Value>(this WorkerDictionary<Key,Value> current,MethodStep<Key> method){
			current.onStepKey = method;
			return current;
		}
		public static Type OnEnd<Type>(this Type current,MethodEnd method) where Type : Worker{
			current.onEnd = method;
			return current;
		}
		public static Type Threads<Type>(this Type current,int count) where Type : Worker{
			current.threadCount = count;
			return current;
		}
		public static Type Group<Type>(this Type current,object group) where Type : Worker{
			current.group = group;
			return current;
		}
		public static Type Async<Type>(this Type current,bool state=true) where Type : Worker{
			current.async = state;
			return current;
		}
		public static Type Finalize<Type>(this Type current,bool state=true) where Type : Worker{
			current.finalize = state;
			return current;
		}
		public static Type Monitor<Type>(this Type current,bool state=true) where Type : Worker{
			current.monitor = state;
			current.Async(state);
			return current;
		}
		public static Type Stepper<Type>(this Type current,bool state=true) where Type : Worker{
			current.stepper = state;
			current.Monitor(state);
			current.Threads(1);
			return current;
		}
		public static Type Quit<Type>(this Type current) where Type : Worker{
			current.quit = true;
			return current;
		}
		public static Type Build<Type,Data>(this Type current,List<Data> items) where Type : Worker{
			if(current.threadCount <= 0){
				var active = 1+Worker.all.Where(x=>x.Key!=Worker.mainThread).Select(x=>x.Value).Where(x=>!x.quit).Select(x=>x.remaining.Count).DefaultIfEmpty(0).Sum();
				var limit = current.threadCount;
				//Log.Show("[Worker] Creating dynamic worker -- " + (Environment.ProcessorCount-active-limit) + " threads.");
				current.threadCount = (Environment.ProcessorCount-active-limit).Max(1);
			}
			current.size = items.Count;
			if(!current.async && current.threadCount <= 0){
				Worker.all[Thread.CurrentThread] = current;
				current.Step(items,0);
				return current;
			}
			if(current.threadCount > 0){
				var index = 0;
				foreach(var item in items.DivideInto(current.threadCount)){
					if(item.Count == 0){break;}
					var part = item;
					var position = index;
					var chunk = new ThreadStart(()=>current.Step(part,position));
					var thread = new Thread(chunk);
					current.threads.Add(thread);
					lock(Worker.all){Worker.all[thread] = current;}
					index += part.Count;
				}
			}
			if(!current.async){
				current.ThreadStart();
				current.ThreadCheck();
				return current;
			}
			if(current.monitor){
				current.MonitorStart();
				return current;
			}
			current.ThreadStart();
			return current;
		}
	}
}
namespace Zios.Supports.Worker{
	public delegate void MethodEnd();
	public delegate bool MethodStep<Value>(Value item);
	public delegate bool MethodStepIndex(int index);
	public delegate Output MethodStep<Value,Output>(WorkerResponse response,Value item);
	public delegate Output MethodStepIndex<Output>(WorkerResponse response,int index);
}