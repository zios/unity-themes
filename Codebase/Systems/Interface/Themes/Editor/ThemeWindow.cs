using UnityEditor;
namespace Zios.Interface{
	public class ThemeWindow : EditorWindow{
		public void OnGUI(){
			EditorApplication.update -= Theme.ShowWindow;
			Theme.Step();
			Theme.Update();
			this.Repaint();
		}
	}
}