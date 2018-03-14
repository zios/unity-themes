using UnityEngine;
namespace Zios.Unity.Extensions.Convert{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Unity.SystemAttributes;
	[InitializeOnLoad]
	public static class ConvertGUIContent{
		static ConvertGUIContent(){Setup();}
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void Setup(){
			ConvertObject.serializeMethods.Add((current,separator,changesOnly)=>{
				return current is GUIContent ? current.As<GUIContent>().Serialize() : null;
			});
			ConvertString.deserializeMethods.Add((type,current,separator)=>{
				return type == typeof(GUIContent) ? new GUIContent().Deserialize(current).Box() : null;
			});
		}
		//============================
		// From
		//============================
		public static string Serialize(this GUIContent current,bool ignoreDefault=false,string defaultValue=""){
			if(ignoreDefault && current.text == defaultValue){return "";}
			var data = current.image.IsNull() ? "" : current.image.As<Texture2D>().Serialize();
			return current.text+"||"+current.tooltip+"||"+data;
		}
		public static string ToString(this GUIContent current){return current.text;}
		//============================
		// To
		//============================
		public static GUIContent Deserialize(this GUIContent current,string value){
			var data = value.Split("||");
			current.text = data[0];
			current.tooltip = data[1];
			current.image = data[2].IsEmpty() ? null : new Texture2D(1,1).Deserialize(data[2]);
			return current;
		}
		public static GUIContent ToContent(this string current){return new GUIContent(current);}
	}
}