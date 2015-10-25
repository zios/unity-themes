using System.Collections.Generic;
namespace Zios{
	public class FixedList<T> : List<T>{
		public int maxSize = 0;
		public FixedList(int size) : base(size){
			this.maxSize = size;
		}
		public new void Add(T item){
			if(this.Count >= this.maxSize){
				this.RemoveAt(0);
			}
			base.Add(item);
		}
	}
}
