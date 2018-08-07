using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Zios.Supports.Reference{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Reflection;
	using Zios.Unity.Supports.MeshInfo;
	public class Reference<Type> : IEnumerable<Type> where Type:MeshInfo{
		[HideInInspector] public Mesh mesh;
		[HideInInspector] public int[] ids = new int[0];
		[NonSerialized] public Type[] values = new Type[0];
		public void Set(Mesh mesh,int[] ids){
			this.mesh = mesh;
			this.ids = ids;
		}
		public Type[] Get(){
			this.Restore();
			return this.values.As<Type[]>();
		}
		public void Restore(){
			if(this.mesh.IsNull()){return;}
			var data = typeof(Type).CallMethod<Type[]>("Get",this.mesh,false);
			if(this.values.Length < 1 && this.ids.Length > 1){
				this.values = data.Where(x=>this.ids.Contains(x.index)).ToArray();
			}
		}
		public Type this[int key]{
			get{return this.Get()[key].As<Type>();}
			set{this.values[key] = value;}
		}
		public IEnumerator<Type> GetEnumerator(){return this.Get().GetEnumerator().As<IEnumerator<Type>>();}
		IEnumerator IEnumerable.GetEnumerator(){return GetEnumerator();}
	}
}