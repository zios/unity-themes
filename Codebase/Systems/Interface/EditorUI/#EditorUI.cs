#if !UNITY_EDITOR
namespace Zios.Interface{
	public static partial class EditorUI{
		public static bool DrawDialog(this string title,string prompt,string confirm,string cancel){return false;}
		public static bool DrawProgressBar(this string title,string message,float percent){return false;}
		public static void ClearProgressBar(){}
	}
}
#endif