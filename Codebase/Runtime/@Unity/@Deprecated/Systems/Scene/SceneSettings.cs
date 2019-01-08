using UnityEngine;
namespace Zios.Unity.Deprecated{
	using Zios.Unity.Log;
	using Zios.Unity.Proxy;
	[AddComponentMenu("Zios/Deprecated/Scene Settings")][ExecuteInEditMode]
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
			string mapName = SceneSettings.currentMap;
			if(values.Length > 1){
				try{
					Proxy.LoadScene(values[1]);
					mapName = values[1];
				}
				catch{
					Log.Show("^1Map not found : " + values[1]);
					return;
				}
			}
			SceneSettings.currentMap = mapName;
			Log.Show("^10Current Map is :^3 " + SceneSettings.currentMap);
		}
	}
}