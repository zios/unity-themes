using System;
using System.Collections.Generic;
using UnityEngine;
namespace Zios.Unity.Supports.MeshWrap{
	using Zios.Extensions;
	using Zios.Unity.Extensions;
	using Mesh = UnityEngine.Mesh;
	[Serializable]
	public class MeshWrap{
		public static Dictionary<Mesh,MeshWrap> cache = new Dictionary<Mesh,MeshWrap>();
		public string name;
		public int vertexCount;
		public Mesh source;
		public int[] triangles;
		public Vector3[] positions;
		public Vector3[] normals;
		public Vector4[] tangents;
		public Color32[] colors;
		public Vector2[] uv;
		public Vector2[] uv2;
		public Vector2[] uv3;
		public Vector2[] uv4;
		public Dictionary<string,BlendShape> blendShapes = new Dictionary<string,BlendShape>();
		public static implicit operator MeshWrap(MonoBehaviour current){return MeshWrap.Get(current);}
		public static implicit operator MeshWrap(Mesh current){return MeshWrap.Get(current);}
		public MeshWrap(Mesh mesh){
			MeshWrap.cache[mesh] = this;
			this.source = mesh;
			this.triangles = mesh.triangles;
			this.name = mesh.name;
			this.vertexCount = mesh.vertexCount;
			this.positions = mesh.vertices;
			this.tangents = mesh.tangents;
			this.normals = mesh.normals;
			this.colors = mesh.colors32;
			this.uv = mesh.uv;
			this.uv2 = mesh.uv2;
			this.uv3 = mesh.uv3;
			this.uv4 = mesh.uv4;
			for(var shapeIndex=0;shapeIndex<mesh.blendShapeCount;++shapeIndex){
				var blendShape = new BlendShape();
				blendShape.name = mesh.GetBlendShapeName(shapeIndex);
				blendShape.index = shapeIndex;
				var frames = new List<BlendFrame>();
				for(var frameIndex=0;frameIndex<mesh.GetBlendShapeFrameCount(shapeIndex);++frameIndex){
					var shapeWeight = mesh.GetBlendShapeFrameWeight(shapeIndex,frameIndex);
					var positions = new Vector3[mesh.vertexCount];
					var normals = new Vector3[mesh.vertexCount];
					var tangents = new Vector3[mesh.vertexCount];
					mesh.GetBlendShapeFrameVertices(shapeIndex,frameIndex,positions,normals,tangents);
					frames.Add(new BlendFrame(shapeWeight,positions,normals,tangents));
				}
				blendShape.frames = frames.ToArray();
				this.blendShapes[blendShape.name] = blendShape;
			}
		}
		public static MeshWrap Get(Mesh mesh){
			if(mesh.IsNull()){return null;}
			if(MeshWrap.cache.ContainsKey(mesh)){return MeshWrap.cache[mesh];}
			return new MeshWrap(mesh);
		}
		public static MeshWrap Get(GameObject target){return MeshWrap.Get(target.GetMesh());}
		public static MeshWrap Get(Component target){return MeshWrap.Get(target.GetMesh());}
	}
	public class BlendShape{
		public int index;
		public string name;
		public BlendFrame[] frames;
	}
	public class BlendFrame{
		public float weight;
		public Vector3[] positions;
		public Vector3[] normals;
		public Vector3[] tangents;
		public BlendFrame(float weight,Vector3[] positions,Vector3[] normals=null,Vector3[] tangents=null){
			this.weight = weight;
			this.positions = positions;
			this.normals = normals;
			this.tangents = tangents;
		}
	}
	public static class MeshWrapExtensions{
		public static MeshWrap GetMeshWrap(this Mesh current){return MeshWrap.Get(current);}
		public static MeshWrap GetMeshWrap(this GameObject current){return MeshWrap.Get(current.GetMesh());}
		public static MeshWrap GetMeshWrap(this Component current){return MeshWrap.Get(current.GetMesh());}
	}
}