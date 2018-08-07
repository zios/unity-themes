using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
namespace Zios.Supports.LogicController{
	using Zios.Supports.State;
	using Zios.Extensions;
	public class LogicController : MonoBehaviour{
		public Dictionary<string,State> states = new Dictionary<string,State>();
		public List<Block> blocks = new List<Block>();
		public static void Parse(Block block){
			foreach(var condition in block.conditions){
				//var data = condition.data;
			}
		}
		public virtual void Awake(){
			this.states = this.GetComponents<State>().ToDictionary(x=>x.name,x=>x);
		}
		public virtual void Update(){
			foreach(var block in this.blocks){
				block.Perform();
			}
		}
	}
	[Serializable]
	public class Block{
		public List<Condition> conditions = new List<Condition>();
		public List<Reaction> reactions = new List<Reaction>();
		private bool ready;
		public void Perform(){
			if(!this.ready){
				LogicController.Parse(this);
				this.ready = true;
			}
			foreach(var condition in this.conditions){
				if(!condition.method()){
					return;
				}
			}
			foreach(var reaction in this.reactions){
				reaction.method();
			}
		}
	}
	[Serializable]
	public class Condition{
		public string data;
		public Func<bool> method = ()=>false;
	}
	[Serializable]
	public class Reaction{
		public string data;
		public Action method = ()=>{};
	}
}