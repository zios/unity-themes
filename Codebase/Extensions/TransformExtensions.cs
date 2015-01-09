using UnityEngine;
using System;
using System.Collections.Generic;
public static class TransformExtension{
	public static string GetPath(this Transform current){	
		return current.gameObject.GetPath();
	}
}