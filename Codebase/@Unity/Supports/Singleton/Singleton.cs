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
		public static string storagePath = "/Settings/";
		public Action SetupHooks = ()=>{};
		#if !ZIOS_MINIMAL
		[MenuItem("Zios/Singleton/Create")]
		public static ScriptableObject Create(){
			var path = EditorUtility.SaveFilePanelInProject("Create Singleton","","","");
			return Singleton.Create<ScriptableObject>(path);
		}
		[MenuItem("Zios/Singleton/Create All")]
		public static void CreateAll(){
			foreach(var item in Reflection.GetSubTypes<Singleton>()){
				item.Call("Get");
			}
		}
		#endif
		public static Type Create<Type>(string path,bool createPath=true) where Type : ScriptableObject{
			var name = path.GetPathTerm();
			var useName = typeof(Type).Name == "ScriptableObject";
			var folder = Application.dataPath + Singleton.storagePath;
			path = folder.GetAssetPath() + path;
			if(createPath){File.Create(folder);}
			ProxyEditor.ImportAsset(folder.GetAssetPath());
			try{
				ScriptableObject instance = useName ? ScriptableObject.CreateInstance(name) : ScriptableObject.CreateInstance<Type>();
				ProxyEditor.CreateAsset(instance,path);
				ProxyEditor.RefreshAssets();
				return instance.As<Type>();
			}
			catch{Log.Warning("[Utility] No scriptableObject exists named -- " + name + ".asset");}
			return null;
		}
		public static Type Get<Type>(bool create=true) where Type : ScriptableObject{
			if(ProxyEditor.IsSwitching()){return null;}
			var name = typeof(Type).Name;
			return Locate.GetAsset<Type>(name+".asset") ?? ScriptableObject.FindObjectOfType<Type>() ?? create ? Singleton.Create<Type>(name+".asset") : null;
		}
	}
}
#else
using System;
using UnityEngine;
namespace Zios.Unity.Supports.Singleton{
	public class Singleton : ScriptableObject{
		public static string storagePath = "";
		public Action SetupHooks = ()=>{};
		public static ScriptableObject Create(){return null;}
		public static ScriptableObject Create(string path,bool createPath=true){return null;}
		public static Type Get<Type>(bool create=true) where Type : ScriptableObject{
			return ScriptableObject.FindObjectOfType<Type>();
		}
	}
}
#endif