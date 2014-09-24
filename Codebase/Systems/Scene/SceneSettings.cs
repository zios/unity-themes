using UnityEngine;
using System.Collections.Generic;
namespace Zios{
	[AddComponentMenu("Zios/Singleton/Scene")]
	public class SceneSettings : MonoBehaviour{
		public List<string> scenes;
		public void Awake(){
			Scene.settings = this;
		}
	}
	public static class Scene{
		public static SceneSettings settings;
		public static string currentMap = "";
		public static int GetMapID(string name){
			for(int index=0;index<Scene.settings.scenes.Count;++index){
				if(Scene.settings.scenes[index] == name){
					return index;
				}
			}
			return -1;
		}
		public static void LoadMap(string[] values){
			string mapName = Application.loadedLevelName;
			if(values.Length > 1){
				try{
					Application.LoadLevel(values[1]);
					mapName = values[1];
				}
				catch{
					Debug.Log("^1Map not found : " + values[1]);
					return;
				}
			}
			Scene.currentMap = mapName;
			Debug.Log("^10Current Map is :^3 " + Scene.currentMap);
		}
	}
}