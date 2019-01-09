using UnityEngine;
namespace Zios.Unity.Extensions.Convert{
	using Zios.Extensions.Convert;
	using Zios.Unity.SystemAttributes;
	[InitializeOnLoad]
	public static class ConvertTexture2D{
		static ConvertTexture2D(){Setup();}
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void Setup(){
			ConvertObject.serializeMethods.Add((current,separator,changesOnly)=>{
				return current is Texture2D ? current.As<Texture2D>().Serialize() : null;
			});
			ConvertString.deserializeMethods.Add((type,current,separator)=>{
				return type == typeof(Texture2D) ? new Texture2D(1,1).Deserialize(current).Box() : null;
			});
		}
		//============================
		// From
		//============================
		public static string Serialize(this Texture2D current){
			return current.GetPixels32().Serialize();
		}
		//============================
		// To
		//============================
		public static Texture2D Deserialize(this Texture2D current,string data){
			current.LoadImage(new byte[0].Deserialize(data));
			return current;
		}
	}
}