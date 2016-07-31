using System;
using UnityEngine;
namespace Zios.Animations{
	[Serializable]
	public class MeshSource{
		public MeshFilter meshFilter;
		public MeshRenderer meshRenderer;
		public SkinnedMeshRenderer skinnedRenderer;
		public MeshSource(GameObject source=null){this.Setup(source);}
		public void Setup(GameObject source=null){
			if(source.IsNull()){return;}
			this.meshFilter = source.GetComponent<MeshFilter>();
			this.meshRenderer = source.GetComponent<MeshRenderer>();
			this.skinnedRenderer = source.GetComponent<SkinnedMeshRenderer>();
			if(this.meshFilter.IsNull() && this.skinnedRenderer.IsNull()){
				Debug.LogWarning("[MeshSource] : Gameobject has no SkinnedMeshRenderer or MeshFilter.",source);
			}
		}
		public Mesh GetMesh(){
			if(this.meshFilter != null){return this.meshFilter.sharedMesh;}
			if(this.skinnedRenderer != null){return this.skinnedRenderer.sharedMesh;}
			return null;
		}
		public void SetMesh(Mesh mesh){
			if(this.meshFilter != null){this.meshFilter.sharedMesh = mesh;}
			if(this.skinnedRenderer != null){this.skinnedRenderer.sharedMesh = mesh;}
		}
		public Renderer GetRenderer(){
			if(this.meshRenderer != null){return this.meshRenderer;}
			if(this.skinnedRenderer != null){return this.skinnedRenderer;}
			return null;
		}
	}
}