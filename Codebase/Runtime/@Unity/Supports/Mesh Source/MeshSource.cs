using System;
using System.Linq;
using UnityEngine;
namespace Zios.Unity.Supports.MeshSource{
	using System.Collections.Generic;
	using Zios.Extensions;
	using Zios.Unity.Log;
	[Serializable]
	public class MeshSource{
		public static Dictionary<GameObject,MeshSource> cache = new Dictionary<GameObject,MeshSource>();
		public static Dictionary<GameObject,MeshSource[]> cacheAll = new Dictionary<GameObject,MeshSource[]>();
		private bool exists;
		public GameObject target;
		public MeshFilter meshFilter;
		public MeshRenderer meshRenderer;
		public SkinnedMeshRenderer skinnedRenderer;
		public MeshSource(){}
		public MeshSource(Component source){this.Setup(source.gameObject);}
		public MeshSource(GameObject source){this.Setup(source);}
		public void Setup(GameObject source){
			if(source.IsNull()){return;}
			this.meshFilter = source.GetComponentInChildren<MeshFilter>();
			this.meshRenderer = source.GetComponentInChildren<MeshRenderer>();
			this.skinnedRenderer = source.GetComponentInChildren<SkinnedMeshRenderer>();
			if(this.meshFilter.IsNull() && this.skinnedRenderer.IsNull()){
				Log.Warning("[MeshSource] : Gameobject has no SkinnedMeshRenderer or MeshFilter.",source);
				return;
			}
			this.exists = true;
			this.target = this.meshFilter ? this.meshFilter.gameObject : this.skinnedRenderer.gameObject;
			MeshSource.cache[source] = this;
			MeshSource.cache[this.target] = this;
		}
		public Mesh GetMesh(bool baked=false){
			if(!this.meshFilter.IsNull()){return this.meshFilter.sharedMesh;}
			if(!this.skinnedRenderer.IsNull()){
				if(baked){
					var mesh = new Mesh();
					this.skinnedRenderer.BakeMesh(mesh);
					return mesh;
				}
				return this.skinnedRenderer.sharedMesh;
			}
			return null;
		}
		public void SetMesh(Mesh mesh){
			if(!this.meshFilter.IsNull()){this.meshFilter.sharedMesh = mesh;}
			if(!this.skinnedRenderer.IsNull()){this.skinnedRenderer.sharedMesh = mesh;}
		}
		public Renderer GetRenderer(){
			if(!this.meshRenderer.IsNull()){return this.meshRenderer;}
			if(!this.skinnedRenderer.IsNull()){return this.skinnedRenderer;}
			return null;
		}
		public static MeshSource Get(GameObject target){
			if(target.IsNull()){return null;}
			if(MeshSource.cache.ContainsKey(target)){return MeshSource.cache[target];}
			var result = new MeshSource(target);
			return result.exists ? result : null;
		}
		public static MeshSource Get(Component target){return MeshSource.Get(target.gameObject);}
		public static MeshSource[] GetAll(GameObject target){
			if(target.IsNull()){return null;}
			if(MeshSource.cacheAll.ContainsKey(target)){return MeshSource.cacheAll[target];}
			var components = new List<Component>(target.GetComponentsInChildren<MeshFilter>());
			components.AddRange(target.GetComponentsInChildren<SkinnedMeshRenderer>());
			MeshSource.cacheAll[target] = components.Select(x=>MeshSource.Get(x)).ToArray();
			return MeshSource.cacheAll[target];
		}
		public static MeshSource[] GetAll(Component target){return MeshSource.GetAll(target.gameObject);}
	}
	public static class MeshSourceExtensions{
		public static MeshSource GetMeshSource(this GameObject current){return MeshSource.Get(current);}
		public static MeshSource GetMeshSource(this Component current){return MeshSource.Get(current.gameObject);}
		public static MeshSource[] GetMeshSources(this GameObject current){return MeshSource.GetAll(current);}
		public static MeshSource[] GetMeshSources(this Component current){return MeshSource.GetAll(current.gameObject);}
	}
}