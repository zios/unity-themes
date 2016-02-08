using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Zios.Containers{
	[Serializable]
	public class Container<TKey,TValue>{
		public List<TKey> keys = new List<TKey>();
		public List<TValue> values = new List<TValue>();
		public Dictionary<TKey,TValue> collection = new Dictionary<TKey,TValue>();
		public int Count{
			get{return this.collection.Count;}
			set{}
		}
		public TValue this[TKey key]{
			get{
				int index = this.keys.IndexOf(key);
				return this.values.ElementAt(index);
			}
			set{
				int index = this.keys.IndexOf(key);
				if(index == -1){
					this.keys.Add(key);
					this.values.Add(value);
					this.collection = this.keys.ToDictionary(x => x,x => this.values[this.keys.IndexOf(x)]);
				}
				else{
					this.values[index] = value;
				}
			}
		}
		public void Clear(){
			this.keys.Clear();
			this.values.Clear();
			this.collection.Clear();
		}
		public IEnumerator<KeyValuePair<TKey,TValue>> GetEnumerator(){
			return this.collection.GetEnumerator();
		}
	}
	[Serializable] public class IntContainer : Container<string,int>{}
	[Serializable] public class StringContainer : Container<string,string>{}
	[Serializable] public class FloatContainer : Container<string,float>{}
	[Serializable] public class BoolContainer : Container<string,bool>{}
	[Serializable] public class GameObjectContainer : Container<string,GameObject>{}
	[Serializable]
	public class FixedContainer<TKey,TValue>{
		public TKey[] keys;
		public TValue[] values;
		private int nextIndex = 0;
		public FixedContainer(int size){
			this.keys = new TKey[size];
			this.values = new TValue[size];
		}
		public TValue this[TKey key]{
			get{
				int index = Array.IndexOf(this.keys,key);
				return this.values[index];
			}
			set{
				int index = Array.IndexOf(this.keys,key);
				if(index == -1){
					this.keys[this.nextIndex] = key;
					this.values[this.nextIndex] = value;
					++this.nextIndex;
				}
				else{
					this.values[index] = value;
				}
			}
		}
	}
}