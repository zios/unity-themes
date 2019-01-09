using System;
using System.Linq;
using System.Collections.Generic;
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
	public partial class MeshInfo{
		public int index;
		public Vector3 normal;
	}
	public partial class MeshBuild{
		public Worker worker;
		public MeshWrap mesh;
		public virtual void Start(){}
		public virtual void End(){cache.Remove(this);}
	}
	//============================
	// Operations
	//============================
	public partial class MeshInfo<Type,Build> : MeshInfo where Build : MeshBuild,new(){
		public static Dictionary<MeshWrap,List<Type>> cache = new Dictionary<MeshWrap,List<Type>>();
		public static List<Type> Get(MeshWrap mesh,List<Type> existing=null){
			var cache = MeshInfo<Type,Build>.cache;
			if(mesh.IsNull()){
				Debug.LogWarning("["+typeof(Type).Name+"] No mesh found on gameObject.  Please insert a MeshFilter or SkinnedMeshRenderer.");
				return null;
			}
			if(!existing.IsNull() && existing.Count > 1){
				cache[mesh] = existing;
			}
			if(cache.ContainsKey(mesh)){return cache[mesh];}
			var entry = cache.AddNew(mesh);
			new Build().Setup<Type>(mesh);
			return entry;
		}
		public static bool Ready(MeshWrap mesh){
			Get(mesh);
			return MeshBuild.Get<Type>(mesh).IsNull();
		}
	}
	public partial class MeshBuild{
		public static List<MeshBuild> cache = new List<MeshBuild>();
		public void Setup<Type>(MeshWrap mesh){
			MeshBuild.cache.AddNew(this);
			this.mesh = mesh;
			this.Start();
		}
		public static MeshBuild Get<Type>(MeshWrap mesh){
			return MeshBuild.cache.Where(x=>x.GetType()==typeof(Type)&&x.mesh==mesh).FirstOrDefault();
		}
	}
	//============================
	// Shared
	//============================
	public delegate bool MeshStep(MeshWrap mesh,int index);
	public static class MeshExtensions{
		/*public static List<Edge> GetEdges(this Mesh current,bool extended=false){return Edge.Get(MeshWrap.Get(current),extended);}
		public static List<Vertex> GetVertexes(this Mesh current,bool extended=false){return Vertex.Get(MeshWrap.Get(current),extended);}
		public static List<Triangle> GetTriangles(this Mesh current,bool extended=false){return Triangle.Get(MeshWrap.Get(current),extended);}*/
	}
}