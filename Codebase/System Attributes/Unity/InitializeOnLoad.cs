namespace Zios
{
#if UNITY_EDITOR

    public class InitializeOnLoadAttribute : UnityEditor.InitializeOnLoadAttribute { }

#else
	public class InitializeOnLoadAttribute : System.Attribute{}
#endif
}