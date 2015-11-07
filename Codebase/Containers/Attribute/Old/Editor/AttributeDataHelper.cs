using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
namespace Zios{
	public static class AttributeDataHelper{
		[MenuItem ("Zios/Process/Attribute/Repair Data (Fix Serialized)")]
		public static void FixSerialized(){
			int count = 0;
			AssetDatabase.StartAssetEditing();
			var prefabs = FileManager.FindAll("*.prefab");
			var scenes = FileManager.FindAll("*.unity");
			var files = prefabs.Concat(scenes);
			var replacements = new Dictionary<string,string>();
			replacements["  data"] = "  oldData";
			replacements["location"] = "path";
			/*replacements["1855a962feb818e46abac4ccc75de4cb"] = "018096688de2d27449147b464ae44d1d"; //AttributeBool
			replacements["3d1c5738016cec84ab5d3eb4cd4f20e5"] = "5727155cfe6a2644aa866faeb7cc058e"; //AttributeInt
			replacements["ac4023662a7e58348afb7331af5f19cc"] = "b7c69f88de3bba44b80668fd23771e31"; //AttributeFloat
			replacements["fc75414e79ba22b4c9583700604e7bed"] = "f0cc9f772e182d143bad917aeb602fc3"; //AttributeString
			replacements["388517ed2859f1d45bd05e0cb90e1097"] = "1378ccc4573dce14f869ec472ba7df55"; //AttributeGameObject
			replacements["5bd276b9f61cc614aaa69e18d0ef39ee"] = "83639dc7708e9ad4584145044a83425d"; //AttributeVector3*/
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
			Debug.Log("[UpdateHelper] : Fixing old serialized names. " + count + " modified.");
		}
		[MenuItem ("Zios/Process/Attribute/Repair Data (Build New)")]
		public static void CopyData(){
			int count = 0;
			foreach(var attribute in Attribute.all){
				var info = attribute.info;
				info.data = new AttributeData[0];
				info.dataB = new AttributeData[0];
				info.dataC = new AttributeData[0];
				if(info.oldData.Length > 0 || info.oldDataB.Length > 0 ||info.oldDataC.Length > 0){
					foreach(var oldData in attribute.info.oldData){info.data = info.data.Add(AttributeDataHelper.CopyData(oldData));}
					foreach(var oldData in attribute.info.oldDataB){info.dataB = info.dataB.Add(AttributeDataHelper.CopyData(oldData));}
					foreach(var oldData in attribute.info.oldDataC){info.dataC = info.dataC.Add(AttributeDataHelper.CopyData(oldData));}
					Utility.SetDirty(attribute.info.parent);
				}
			}
			Events.Call("On Hierarchy Changed");
			Debug.Log("[UpdateHelper] : Copying OldAttributeData to AttributeData. " + count + " modified.");
		}
		public static AttributeData CopyData(OldAttributeData oldData){
			var newData = new AttributeData();
			if(oldData is OldAttributeBoolData){newData = oldData.As<OldAttributeBoolData>().Convert();}
			if(oldData is OldAttributeIntData){newData = oldData.As<OldAttributeIntData>().Convert();}
			if(oldData is OldAttributeFloatData){newData = oldData.As<OldAttributeFloatData>().Convert();}
			if(oldData is OldAttributeGameObjectData){newData = oldData.As<OldAttributeGameObjectData>().Convert();}
			if(oldData is OldAttributeVector3Data){newData = oldData.As<OldAttributeVector3Data>().Convert();}
			if(oldData is OldAttributeStringData){newData = oldData.As<OldAttributeStringData>().Convert();}
			newData.target = oldData.target;
			newData.usage = oldData.usage;
			newData.path = oldData.path;
			newData.referenceID = oldData.referenceID;
			newData.referencePath = oldData.referencePath;
			newData.operation = oldData.operation;
			newData.special = oldData.special;
			return newData;
		}
		[MenuItem ("Zios/Process/Attribute/Repair Data (Remove Old)")]
		public static void RemoveDeprecated(){
			int count = 0;
			foreach(var script in Locate.GetSceneComponents<OldAttributeData>()){
				count += 1;
				Utility.Destroy(script);
			}
			Debug.Log("[UpdateHelper] : Removing OldAttributeData components. " + count + " modified.");
		}
	}
}