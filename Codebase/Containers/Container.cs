using System;
using System.Collections.Generic;
using System.Linq;
namespace Zios{
	[Serializable]
	public class Container<TKey,TValue>{
		public List<TKey> keys = new List<TKey>();
		public List<TValue> values = new List<TValue>();
		public int Count = 0;
		private Dictionary<TKey,TValue> iterator = new Dictionary<TKey,TValue>();
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
					this.iterator = this.keys.ToDictionary(x => x,x => this.values[this.keys.IndexOf(x)]);
					this.Count = this.keys.Count;
				}
				else{
					this.values[index] = value;
				}
			}
		}
		public void Clear(){
			this.keys.Clear();
			this.values.Clear();
			this.iterator.Clear();
		}
		public IEnumerator<KeyValuePair<TKey,TValue>> GetEnumerator(){
			return this.iterator.GetEnumerator();
		}
	}
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