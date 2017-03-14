using UnityEngine;
namespace Zios{
	[InitializeOnLoad]
	public static class AssetSettings{
		static AssetSettings(){
			#if !UNITY_THEMES
			if(!FileManager.Exists("Assets/Settings")){
				Debug.Log("[AssetSettings] : Rebuilding missing Settings folder assets.");
				foreach(var item in Utility.GetTypes<Singleton>()){
					item.CallMethod("Get");
				}
			}
			foreach(var item in FileManager.FindAll("Settings/*.asset",false)){
				item.GetAsset<Singleton>();
			}
			#endif
		}
	}
}