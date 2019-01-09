using System;
using System.Collections.Generic;
using UnityEngine;
namespace Zios.Unity.Components.MeshSmooth{
	using Zios.Extensions;
	using Zios.Unity.Extensions;
	using mattatz.MeshSmoothingSystem;
	using Zios.Unity.Supports.MeshSource;
	[ExecuteInEditMode][AddComponentMenu("Zios/Component/Mesh/Mesh Smooth")]
	public class MeshSmooth : MonoBehaviour{
		private bool needsUpdate;
		private MeshSource source;
		public Mesh original;
		public Mesh current;
		public MeshSmoothType type;
		[Range(0,20)] public int iterations = 1;
		[Range(0,1)] public float intensity = 0.5f;
		[Range(0,1)] public float hcAlpha = 0.5f;
		[Range(0,1)] public float hcBeta = 0.5f;
		public void OnValidate(){this.needsUpdate = true;}
		public void Update(){
			if(this.needsUpdate && !this.original.IsNull()){
				if(this.type == MeshSmoothType.Laplacian){
					//this.current = MeshSmoothing.LaplacianFilter(this.original.Copy(),this.iterations);
					this.current = this.original.Copy();
					for(int index=0;index<this.iterations;++index){
						this.current.vertices = SmoothFilter.laplacianFilter(this.current.vertices,this.current.triangles);
					}
				}
				if(this.type == MeshSmoothType.HCLaplacian){
					//this.current = MeshSmoothing.HCFilter(this.original.Copy(),this.iterations,this.hcAlpha,this.hcBeta);
					this.current = this.original.Copy();
					for(int index=0;index<this.iterations;++index){
						this.current.vertices = SmoothFilter.hcFilter(this.original.vertices,this.current.vertices,this.current.triangles,this.hcAlpha,this.hcBeta);
					}
				}
				this.source.SetMesh(this.current);
				this.needsUpdate = false;
			}
		}
		public void OnEnable(){
			this.source = this.GetMeshSource();
			if(this.original.IsNull()){
				this.original = this.GetMesh();
			}
			this.needsUpdate = true;
		}
		public void OnDisable(){
			if(!this.original.IsNull()){
				this.source.SetMesh(this.original);
			}
		}
	}
	public enum MeshSmoothType{Laplacian,HCLaplacian}
}