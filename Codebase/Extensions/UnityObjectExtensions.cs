using System;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios{
    public static class UnityObjectExtension{
		public static bool IsExpanded(this UnityObject current){
			if(current is Component){
				Component component = (Component)current;
				return component.IsExpanded();
			}
			return false;
		}
	}
}