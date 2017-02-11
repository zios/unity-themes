using UnityEngine;
namespace Zios{
	#if !UNITY_EDITOR
	public class MenuItemAttribute : System.Attribute{
		public MenuItemAttribute(){}
		public MenuItemAttribute(string term){}
	}
	#endif
}