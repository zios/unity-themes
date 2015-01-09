using System;
using UnityEngine;
public static class LayerMaskExtension{
	public static bool Contains(this LayerMask current,LayerMask mask){
		return (current.value | (1<<mask.value)) == current.value;
	}
	public static bool Contains(this LayerMask current,int mask){
		return (current.value | (1<<mask)) == current.value;
	}
}