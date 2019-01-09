using System.Collections.Generic;
using UnityEditor;
namespace Zios.Unity.Editor.Locate{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Reflection;
	using Zios.Unity.Locate;
	using Zios.Unity.Proxy;
	public static class LocateEditor{
		public static Dictionary<string,AssetImporter> importers = new Dictionary<string,AssetImporter>();
		public static Type GetImporter<Type>(string path) where Type : AssetImporter{
			if(Proxy.IsLoading()){return default(Type);}
			if(!LocateEditor.importers.ContainsKey(path)){LocateEditor.importers[path] = AssetImporter.GetAtPath(path);}
			return LocateEditor.importers[path].As<Type>();
		}
		public static void RebuildAll(){
			var windows = Locate.GetAssets<EditorWindow>();
			foreach(var window in windows){
				if(windows.IsNull()){continue;}
				var tracker = window.GetVariable<ActiveEditorTracker>("m_Tracker");
				if(tracker == null || System.Object.Equals(tracker,null)){continue;}
				tracker.ForceRebuild();
			}
		}
	}
}