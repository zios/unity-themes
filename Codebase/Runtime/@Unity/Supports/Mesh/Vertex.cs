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
	public class Vertex : MeshInfo<Vertex,VertexBuild>{
		public Vector3 position;
		public Color32 color;
		public Vector2 texCoord0;
		public Vector2 texCoord1;
		public Vector2 texCoord2;
		public Vector2 texCoord3;
		public Vector2 uv{get{return this.texCoord0;}set{this.texCoord0=value;}}
		public Vector2 uv0{get{return this.texCoord0;}set{this.texCoord0=value;}}
		public Vector2 uv1{get{return this.texCoord1;}set{this.texCoord1=value;}}
		public Vector2 uv2{get{return this.texCoord2;}set{this.texCoord2=value;}}
		public Vector2 uv3{get{return this.texCoord3;}set{this.texCoord3=value;}}
		public Vector4 tangent;
		public Vertex[] adjacent;
		public Triangle[] triangles;
		public Edge[] edges;
	}
	//============================
	// Build
	//============================
	public class VertexBuild : MeshBuild{
		public override void Start(){
			var data = this.mesh.positions.ToList();
			this.worker = Worker.Create(data).OnStep(this.Step).OnEnd(this.End).Async().Build();
		}
		public bool Step(int index){
			var mesh = this.mesh;
			Vertex vertex;
			lock(Vertex.cache){vertex = Vertex.cache[mesh].AddNew();}
			vertex.index = index;
			vertex.position = mesh.positions[index];
			vertex.normal = index < mesh.normals.Length-1 ? mesh.normals[index] : default(Vector3);
			vertex.tangent = index < mesh.tangents.Length-1 ? mesh.tangents[index] : default(Vector4);
			vertex.color = index < mesh.colors.Length-1 ? mesh.colors[index] : default(Color32);
			vertex.texCoord0 = index < mesh.uv.Length-1 ? mesh.uv[index] : default(Vector2);
			vertex.texCoord1 = index < mesh.uv2.Length-1 ? mesh.uv2[index] : default(Vector2);
			vertex.texCoord2 = index < mesh.uv3.Length-1 ? mesh.uv3[index] : default(Vector2);
			vertex.texCoord3 = index < mesh.uv4.Length-1 ? mesh.uv4[index] : default(Vector2);
			return true;
		}
		public void Extra(Vertex[] edges,MeshWrap mesh){}
		public void Adjacent(Vertex[] vertexes){}
	}
}