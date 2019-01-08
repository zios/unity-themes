using System;
namespace Zios.Supports.Accessor{
	using Zios.Reflection;
	public class Accessor{
		public object scope;
		public int index;
		public string name;
		public Type type;
		public Accessor(){}
		public Accessor(object scope,string name) : this(scope,name,-1){}
		public Accessor(object scope,string name,int index){
			this.scope = scope;
			this.name = name;
			this.index = index;
			this.type = this.scope.GetVariableType(name,index);
		}
		public Type Get<Type>(int index=-1){
			if(index==-1){index = this.index;}
			return this.scope.GetVariable<Type>(this.name,index);
		}
		public object Get(int index=-1){
			return this.Get<object>(index);
		}
		public void Set<Type>(Type value,int index=-1){
			if(index==-1){index = this.index;}
			this.scope.SetVariable<Type>(this.name,value,index);
		}
	}
}