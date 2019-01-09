using System.Collections.Generic;
using UnityEngine;
namespace Zios.Unity.Components.MeshData{
	using Zios.Unity.Supports.MeshInfo;
	//asm Zios.Unity.Supports.MeshWrap;
	[ExecuteInEditMode]
	public class VertexData : MonoBehaviour{
		public List<Vertex> vertexes = new List<Vertex>();
		public void Awake(){
			this.vertexes = Vertex.Get(this,this.vertexes);
		}
	}
}