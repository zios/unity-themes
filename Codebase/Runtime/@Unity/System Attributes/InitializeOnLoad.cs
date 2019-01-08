namespace Zios.Unity.SystemAttributes{
	using Zios.Reflection;
	#if UNITY_EDITOR
	[UnityEditor.InitializeOnLoad]
	#endif
	public static class Initializer{
		static Initializer(){
			foreach(var type in Reflection.GetTypes()){
				if(type.HasAttribute(typeof(InitializeOnLoadAttribute))){
					System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
				}
			}
		}
	}
	public class InitializeOnLoadAttribute : System.Attribute{}
}