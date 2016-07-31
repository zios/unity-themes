using UnityEngine;
using UnityEngine.SceneManagement;
namespace Zios{
	[AddComponentMenu("Zios/Singleton/Scene Settings")][ExecuteInEditMode]
	public class SceneSettings : MonoBehaviour{
		public static SceneSettings instance;
		public static string currentMap = "";
		public string[] scenes;
		public static SceneSettings Get(){return SceneSettings.instance;}
		public void OnEnable(){this.Setup();}
		public void Awake(){this.Setup();}
		public void Setup(){SceneSettings.instance = this;}
		public static int GetMapID(string name){
			for(int index=0;index<SceneSettings.Get().scenes.Length;++index){
				if(SceneSettings.Get().scenes[index] == name){
					return index;
				}
			}
			return -1;
		}
		public static void LoadMap(string[] values){
			string mapName = SceneManager.GetActiveScene().name;
			if(values.Length > 1){
				try{
					SceneManager.LoadScene(values[1]);
					mapName = values[1];
				}
				catch{
					Debug.Log("^1Map not found : " + values[1]);
					return;
				}
			}
			SceneSettings.currentMap = mapName;
			Debug.Log("^10Current Map is :^3 " + SceneSettings.currentMap);
		}
	}
}