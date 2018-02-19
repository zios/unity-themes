#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Supports.Singleton{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.File;
	using Zios.Reflection;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.Locate;
	using Zios.Unity.Log;
	public class Singleton : ScriptableObject{
		public Action SetupHooks = ()=>{};
		[MenuItem("Zios/Create Singleton")]
		public static ScriptableObject Create(){
			var path = EditorUtility.SaveFilePanelInProject("Create Singleton","","","");
			return Singleton.Create<ScriptableObject>(path);
		}
		public static Type Create<Type>(string path,bool createPath=true) where Type : ScriptableObject{
			var name = path.GetPathTerm();
			var folder = Application.dataPath + "/" + path.TrimLeft("Assets/").GetDirectory();
			var useName = typeof(Type).Name == "ScriptableObject";
			if(createPath){File.Create(folder);}
			ProxyEditor.ImportAsset(folder.GetAssetPath());
			try{
				ScriptableObject instance = useName ? ScriptableObject.CreateInstance(name) : ScriptableObject.CreateInstance<Type>();
				ProxyEditor.CreateAsset(instance,path+".asset");
				ProxyEditor.RefreshAssets();
				return instance.As<Type>();
			}
			catch{Log.Warning("[Utility] No scriptableObject exists named -- " + name + ".asset");}
			return null;
		}
		public static Type Get<Type>(bool create=true) where Type : ScriptableObject{
			if(ProxyEditor.IsSwitching()){return null;}
			var name = typeof(Type).FullName;
			return Locate.GetAsset<Type>(name+".asset") ?? ScriptableObject.FindObjectOfType<Type>() ?? create ? Singleton.Create<Type>("Assets/Settings/"+name) : null;
		}
	}
	[InitializeOnLoad]
	public static class SingletonManager{
		static SingletonManager(){
			if(!File.Exists("Assets/Settings")){
				Log.Show("[AssetSettings] : Rebuilding missing Settings folder assets.");
				foreach(var item in Reflection.GetSubTypes<Singleton>()){
					item.Call("Get");
				}
			}
			foreach(var item in File.FindAll("Settings/*.asset",false)){
				item.GetAsset<Singleton>();
			}
		}
	}
}
#else
using System;
using UnityEngine;
namespace Zios.Unity.Supports.Singleton{
	public class Singleton : ScriptableObject{
		public Action SetupHooks = ()=>{};
		public static ScriptableObject Create(){return null;}
		public static ScriptableObject Create(string path,bool createPath=true){return null;}
		public static Type Get<Type>(bool create=true) where Type : ScriptableObject{
			return ScriptableObject.FindObjectOfType<Type>();
		}
	}
}
#endif