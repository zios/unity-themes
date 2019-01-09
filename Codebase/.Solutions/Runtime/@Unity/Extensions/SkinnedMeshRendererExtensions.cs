using System.Collections.Generic;
using UnityEngine;
namespace Zios.Unity.Extensions{
	using Zios.Extensions;
	using Zios.Unity.Extensions.Convert;
	public static class SkinnedMeshRendererExtension{
		public static void ResetBlendShapes(this SkinnedMeshRenderer renderer){
			var shapeCount = renderer.sharedMesh.blendShapeCount;
			for(int index=0;index<shapeCount;++index){
				renderer.SetBlendShapeWeight(index,0);
			}
		}
	}
}