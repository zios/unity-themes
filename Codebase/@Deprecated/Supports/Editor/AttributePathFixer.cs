using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
namespace Zios.Attributes{
	public static class AttributePathFixer{
		[MenuItem ("Zios/Attribute/Fix RawType Serializations")]
		public static void FixSerialized(){
			int count = 0;
			AssetDatabase.StartAssetEditing();
			var prefabs = FileManager.FindAll("*.prefab");
			var scenes = FileManager.FindAll("*.unity");
			var files = prefabs.Concat(scenes);
			var replacements = new Dictionary<string,string>();
			replacements["Zios.AttributeInt"] = "Zios.Attributes.AttributeInt";
			replacements["Zios.AttributeBool"] = "Zios.Attributes.AttributeBool";
			replacements["Zios.AttributeFloat"] = "Zios.Attributes.AttributeFloat";
			replacements["Zios.AttributeString"] = "Zios.Attributes.AttributeString";
			replacements["Zios.AttributeVector3"] = "Zios.Attributes.AttributeVector3";
			replacements["Zios.AttributeGameObject"] = "Zios.Attributes.AttributeGameObject";
			foreach(var file in files){
				var text = file.GetText();
				var original = string.Copy(text);
				foreach(var item in replacements){text = text.Replace(item.Key,item.Value);}
				if(!text.Equals(original)){
					count += 1;
					file.WriteText(text);
				}
			}
			AssetDatabase.StopAssetEditing();
			AssetDatabase.Refresh();
			Debug.Log("[UpdateHelper] : Fixing serialized attribute type paths. " + count + " modified.");
		}
	}
}