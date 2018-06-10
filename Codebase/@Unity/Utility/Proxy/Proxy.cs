using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
namespace Zios.Unity.Proxy{
	using Source = UnityEngine.Application;
	public static class Application{
		public static bool isLoadingLevel{get{return Source.isLoadingLevel;}}
		public static bool isPlaying{get{return Source.isPlaying;}}
		public static bool isEditor{get{return Source.isEditor;}}
		public static bool isFocused{get{return Source.isFocused;}}
		public static int levelCount{
			get{
				return SceneManager.sceneCountInBuildSettings;
				#if UNITY_5_3_OR_NEWER
				#else
				return Source.levelCount;
				#endif
			}
		}
		public static int loadedLevel{
			get{
				#if UNITY_5_3_OR_NEWER
				return SceneManager.GetSceneByBuildIndex(loadedLevel).buildIndex;
				#else
				return Source.loadedLevel;
				#endif
			}
		}
		public static int targetFrameRate{get{return Source.targetFrameRate;}set{Source.targetFrameRate=value;}}
		public static string loadedLevelName{
			get{
				#if UNITY_5_3_OR_NEWER
				return SceneManager.GetActiveScene().name;
				#else
				return Source.loadedLevelName;
				#endif
			}
		}
		public static string dataPath{get{return Source.dataPath;}}
		public static string persistentDataPath{get{return Source.persistentDataPath;}}
		public static string streamingAssetsPath{get{return Source.streamingAssetsPath;}}
		public static string temporaryCachePath{get{return Source.temporaryCachePath;}}
		public static bool runInBackground{get{return Source.runInBackground;}}
		public static RuntimePlatform platform{get{return Source.platform;}}
		public static string companyName{get{return Source.companyName;}}
		public static string productName{get{return Source.productName;}}
		public static string absoluteURL{get{return Source.absoluteURL;}}
		public static Type LogCallback{get{return typeof(Source.LogCallback);}}
		public static void Quit(){Source.Quit();}
		public static void LoadLevel(int id){
			#if UNITY_5_3_OR_NEWER
			SceneManager.LoadScene(id);
			#else
			Source.LoadLevel(id);
			#endif

		}
		public static void LoadLevelAdditive(int id){
			#if UNITY_5_3_OR_NEWER
			SceneManager.LoadScene(id,LoadSceneMode.Additive);
			#else
			Source.LoadLevelAdditive(id);
			#endif
		}
		public static void LoadLevelAdditiveAsync(int id){
			#if UNITY_5_3_OR_NEWER
			SceneManager.LoadSceneAsync(id,LoadSceneMode.Additive);
			#else
			Source.LoadLevelAdditiveAsync(id);
			#endif
		}
		public static void LoadLevelAsync(int id){
			#if UNITY_5_3_OR_NEWER
			SceneManager.LoadSceneAsync(id);
			#else
			Source.LoadLevelAsync(id);
			#endif
		}
	}
	public static partial class Proxy{
		public static List<Func<bool>> busyMethods = new List<Func<bool>>(){()=>Proxy.IsLoading()};
		public static void AddLogCallback(Source.LogCallback method){Source.logMessageReceived += method;}
		public static void RemoveLogCallback(Source.LogCallback method){Source.logMessageReceived -= method;}
		public static bool IsRepainting(){return Event.current.type == EventType.Repaint;}
		public static bool IsLoading(){return Source.isLoadingLevel;}
		public static bool IsPlaying(){return Source.isPlaying;}
		public static bool IsEditor(){return Source.isEditor;}
		public static bool IsBusy(){
			foreach(var method in Proxy.busyMethods){
				if(method()){return true;}
			}
			return false;
		}
		public static void LoadScene(string name){
			#if UNITY_5_3_OR_NEWER
			SceneManager.LoadScene(name);
			#else
			Application.LoadLevel(name);
			#endif
		}
	}
}
namespace Zios.Unity.Proxy{
	using Zios.Extensions;
	public static class MonoBehaviourExtensions{
		public static bool CanValidate(this MonoBehaviour current){
			bool enabled = !current.IsNull() && current.gameObject.activeInHierarchy && current.enabled;
			return !Proxy.IsPlaying() && !Proxy.IsBusy() && enabled;
		}
	}
}