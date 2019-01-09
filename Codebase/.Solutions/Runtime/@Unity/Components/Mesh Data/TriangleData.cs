using System.Collections.Generic;
using UnityEngine;
namespace Zios.Unity.Components.MeshData{
	using Zios.Unity.Supports.MeshInfo;
	//asm Zios.Unity.Supports.MeshWrap;
	[ExecuteInEditMode]
	public class TriangleData : MonoBehaviour{
		public List<Triangle> triangles;
		public void Awake(){
			this.triangles = Triangle.Get(this,this.triangles);
		}
	}
}