using System;
namespace Zios.Supports.Data{
	public class Data{
		public object value;
		public Func<object> Get;
		public Action<object> Set;
		public Data(object value){
			this.value = value;
			this.Get = ()=>this.value;
			this.Set = (x)=>this.value = x;
		}
	}
}