using UnityEngine;
using System.Linq;
using System.Collections.Generic;
namespace Zios.Supports.DataController{
	using Zios.Supports.State;
	using Zios.Supports.Data;
	using Zios.Reflection;
	using Zios.Extensions;
	public class DataController : MonoBehaviour{
		public Dictionary<string,Data> data = new Dictionary<string,Data>();
		public void Add<Type>(string key,Type value){
			this.data[key] = new Data(value);
		}
		public Type Get<Type>(string key,Type fallback=default(Type)){
			Data result;
			if(this.data.TryGetValue(key,out result)){
				return (Type)result.Get();
			}
			return fallback;
		}
		public void Set<Type>(string key,Type value){
			this.data[key].Set(value);
		}
		public virtual void Awake(){
			var states = this.GetComponents<State>().ToDictionary(x=>x.name,x=>x);
			foreach(var state in states){
				foreach(var field in state.Value.GetVariables()){
					this.Add(field.Key,field.Value);
				}
			}
		}
	}
}