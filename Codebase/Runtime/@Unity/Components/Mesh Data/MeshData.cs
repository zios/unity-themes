using System.Collections.Generic;
using System;
using UnityEngine;
namespace Zios.Unity.Components.MeshData{
	using Zios.Unity.Supports.MeshInfo;
	//asm Zios.Unity.Supports.MeshWrap;
	[ExecuteInEditMode]
	public class MeshData : MonoBehaviour{
		public List<Vertex> vertexes = new List<Vertex>();
		public List<Triangle> triangles = new List<Triangle>();
		public List<Edge> edges = new List<Edge>();
		public void Awake(){
			this.vertexes = Vertex.Get(this,this.vertexes);
			this.triangles = Triangle.Get(this,this.triangles);
			this.edges = Edge.Get(this,this.edges);
		}
	}
}