using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Zios.Unity.Supports.MeshInfo{
	using Zios.Extensions;
	using Zios.Serializer;
	using Zios.Supports.Worker;
	using Zios.Unity.Supports.MeshWrap;
	//============================
	// Container
	//============================
	[Store]
	public class Triangle : MeshInfo<Triangle,TriangleBuild>{
		public Vertex[] vertexes;
		public Triangle[] adjacent;
		public Edge[] edges;
	}
	//============================
	// Build
	//============================
	public class TriangleBuild : MeshBuild{
		public override void Start(){
			var data = this.mesh.triangles.ToList().DivideEvery(3);
			this.worker = Worker.Create(data).OnStep(this.Step).OnEnd(this.End).Async().Build();
		}
		public bool Step(int index){
			if(!Vertex.Ready(this.mesh)){return false;}
			var mesh = this.mesh;
			Triangle triangle;
			var ids = Worker.Get<List<int>>().collection[index];
			lock(Triangle.cache){triangle = Triangle.cache[mesh].AddNew();}
			var vertexes = Vertex.Get(mesh);
			var vertexA = vertexes[ids[0]];
			var vertexB = vertexes[ids[1]];
			var vertexC = vertexes[ids[2]];
			var distanceA = vertexB.position - vertexA.position;
			var distanceB = vertexC.position - vertexA.position;
			triangle.index = index;
			triangle.vertexes = new Vertex[3]{vertexA,vertexB,vertexC};
			triangle.normal = Vector3.Cross(distanceA,distanceB).normalized;
			return true;
		}
		public static void Extra(Triangle[] triangles,MeshWrap mesh){
			/*foreach(var triangle in triangles){
				if(triangle.edges.Get().Length < 1){
					var ids = Edge.Get(mesh).Where(x=>x.vertexes.Get().ContainsAmount(2,triangle.vertexes.Get())).Select(x=>x.index).ToArray();
					triangle.edges.Set(mesh,ids);
				}
			}*/
		}
		public static void Adjacent(Triangle[] triangles,MeshWrap mesh){}
	}
}