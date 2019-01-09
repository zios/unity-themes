using System.Linq;
using System.Collections.Generic;
namespace Zios.Unity.Supports.MeshInfo{
	using Zios.Extensions;
	using Zios.Serializer;
	using Zios.Supports.Worker;
	using Zios.Unity.Supports.MeshWrap;
	//============================
	// Container
	//============================
	[Store]
	public class Edge : MeshInfo<Edge,EdgeBuild>{
		public bool degenerate;
		public Edge[] adjacent;
		public Vertex[] vertexes;
		public Triangle[] triangles;
	}
	//============================
	// Build
	//============================
	public partial class EdgeBuild : MeshBuild{
		public static Dictionary<Vertex,List<Vertex>> matches = new Dictionary<Vertex,List<Vertex>>();
		public override void Start(){
			var data = this.mesh.triangles.ToList().DivideEvery(3);
			this.worker = Worker.Create(data).OnStep(this.Step).OnEnd(this.FilterStart).Async().Build();
		}
		public bool Step(int index){
			if(!Triangle.Ready(this.mesh)){return false;}
			var mesh = this.mesh;
			var matches = EdgeBuild.matches;
			var triangles = Triangle.Get(mesh);
			var vertexes = triangles[index].vertexes.OrderBy(x=>x.index).ToArray();
			//UnityEngine.Debug.Log(vertexes.Length);
			lock(EdgeBuild.matches){
				matches.AddNew(vertexes[0]).AddNew(vertexes[1]);
				matches.AddNew(vertexes[0]).AddNew(vertexes[2]);
				matches.AddNew(vertexes[1]).AddNew(vertexes[2]);
			}
			return true;
		}
		public void FilterStart(){
			this.worker = Worker.Create(EdgeBuild.matches).OnStep(this.FilterStep).OnEnd(this.End).Async().Build();
		}
		public bool FilterStep(Vertex pointA){
			var mesh = this.mesh;
			var matches = EdgeBuild.matches[pointA];
			foreach(var pointB in matches){
				Edge edge;
				lock(Edge.cache){edge = Edge.cache[mesh].AddNew();}
				edge.index = Edge.cache[mesh].IndexOf(edge);
				edge.normal = (pointA.normal + pointB.normal) /2;
				edge.vertexes = new Vertex[2]{pointA,pointB};
			}
			return true;
		}
		public static void Extra(List<Edge> edges,MeshWrap mesh){
			/*var triangles = Triangle.Get(mesh,true);
			foreach(var edge in edges){
				if(edge.triangles.ids.Length < 1){
					edge.triangles.Set(mesh,triangles.Where(x=>x.edges.Get().Contains(edge)).Select(x=>x.index).ToArray());
				}
				edge.degenerate = edge.triangles.ids.Length == 1;
			}*/
		}
		public static void Adjacent(List<Edge> edges,MeshWrap mesh){
			foreach(var edge in edges){
				/*if(edge.adjacent.ids.Length < 1){
					var vertexA = edge.vertexes[0];
					var vertexB = edge.vertexes[1];
					var ids = edges.Where(x=>x!=edge&&x.vertexes.Get().ContainsAny(vertexA,vertexB)).Select(x=>x.index).ToArray();
					edge.adjacent.Set(mesh.source,ids);
				}*/
			}
		}
	}
}