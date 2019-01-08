using System;
using System.Collections;
using System.Collections.Generic;
namespace Zios.Supports.NonGeneric{
	[Serializable] public class FloatList : List<float>{}
	[Serializable] public class IntList : List<int>{}
	[Serializable] public class BoolList: List<bool>{}
	[Serializable] public class StringList: List<string>{}
	[Serializable] public class ListFloat : ListBox<float>{}
	[Serializable] public class ListInt : ListBox<int>{}
	[Serializable] public class ListBool : ListBox<bool>{}
	//[Serializable] public class ListString : ListBox<string>{}
	[Serializable] public class ListBox<Type> : IEnumerable where Type : new(){
		public List<Type> value = new List<Type>();
		public static implicit operator List<Type>(ListBox<Type> current){return current.value;}
		public ListBox(){}
		public ListBox(params Type[] values){
			this.value = new List<Type>(values);
		}
		public Type this[int index]{
			get{return this.value[index];}
			set{this.value[index] = value;}
		}
		public void Add(Type value){this.value.Add(value);}
		public IEnumerator GetEnumerator(){
			return this.value.GetEnumerator();
		}
	}
}