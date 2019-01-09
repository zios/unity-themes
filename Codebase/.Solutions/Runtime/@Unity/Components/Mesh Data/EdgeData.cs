using System.Collections.Generic;
using UnityEngine;
namespace Zios.Unity.Components.MeshData{
	using Zios.Unity.Supports.MeshInfo;
	//asm Zios.Unity.Supports.MeshWrap;
	[ExecuteInEditMode]
	public class EdgeData : MonoBehaviour{
		public List<Edge> edges;
		public void Awake(){
			this.edges = Edge.Get(this,this.edges);
		}
	}
}