using UnityEditor;
namespace Zios.Interface{
	public class ThemeWindow : EditorWindow{
		public void OnGUI(){
			Theme.Setup();
			this.Repaint();
		}
	}
}