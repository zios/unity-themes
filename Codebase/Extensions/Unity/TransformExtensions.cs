using UnityEngine;
using System;
using System.Collections.Generic;
namespace Zios{
	public static class TransformExtension{
		public static string GetPath(this Transform current){
			return current.gameObject.GetPath();
		}
		public static Vector3 Localize(this Transform current,Vector3 value){
			Vector3 local = current.right * value.x;
			local += current.up * value.y;
			local += current.forward * value.z;
			return local;
		}
	}
}