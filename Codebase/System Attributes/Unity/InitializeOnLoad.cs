using UnityEngine;
namespace Zios{
	#if UNITY_EDITOR
	using UnityEditor;
	public class InitializeOnLoadAttribute : UnityEditor.InitializeOnLoadAttribute{}
	#else
	public class InitializeOnLoadAttribute : System.Attribute{}
	#endif
}