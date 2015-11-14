using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
namespace Zios{
	public static class AttributeDataHelper{
		[MenuItem ("Zios/Process/Attribute/Repair Data [Prefabs & Scene]")]
		public static void Repair(){
			AttributeDataHelper.InstancePrefabs();
			AttributeDataHelper.FixSerialized();
			AttributeDataHelper.CopyData();
			AttributeDataHelper.RemoveDeprecated();
		}
		[MenuItem ("Zios/Process/Attribute/Repair Data [Scene]")]
		public static void RepairScene(){
			AttributeDataHelper.FixSerialized();
			AttributeDataHelper.CopyData();
			AttributeDataHelper.RemoveDeprecated();
		}
		[MenuItem ("Zios/Process/Attribute/Repair Data [Fix Serialized]")]
		public static void FixSerialized(){
			int count = 0;
			AssetDatabase.StartAssetEditing();
			var prefabs = FileManager.FindAll("*.prefab");
			var scenes = FileManager.FindAll("*.unity");
			var files = prefabs.Concat(scenes);
			var replacements = new Dictionary<string,string>();
			replacements["  data"] = "  oldData";
			replacements["location"] = "path";
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
		[MenuItem ("Zios/Process/Attribute/Repair Data [Copy Data]")]
		public static void CopyData(){
			int count = 0;
			foreach(var attribute in Attribute.all){
				var info = attribute.info;
				if(info.oldData.Length > 0 || info.oldDataB.Length > 0 ||info.oldDataC.Length > 0){
					info.data = new AttributeData[0];
					info.dataB = new AttributeData[0];
					info.dataC = new AttributeData[0];
					foreach(var oldData in attribute.info.oldData.Where(x=>!x.IsNull())){info.data = info.data.Add(AttributeDataHelper.CopyData(oldData));}
					foreach(var oldData in attribute.info.oldDataB.Where(x=>!x.IsNull())){info.dataB = info.dataB.Add(AttributeDataHelper.CopyData(oldData));}
					foreach(var oldData in attribute.info.oldDataC.Where(x=>!x.IsNull())){info.dataC = info.dataC.Add(AttributeDataHelper.CopyData(oldData));}
					info.oldData = new OldAttributeData[0];
					info.oldDataB = new OldAttributeData[0];
					info.oldDataC = new OldAttributeData[0];
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
		[MenuItem ("Zios/Process/Attribute/Repair Data [Instance Prefabs]")]
		public static void InstancePrefabs(){
			var prefabs = FileManager.FindAll("*.prefab").Select(x=>x.GetAsset<GameObject>()).ToArray();
			Events.AddStepper("On Editor Update",AttributeDataHelper.InstancePrefabsStep,prefabs,1);
			//Events.disabled = EventDisabled.Add;
			//Events.Pause("On Hierarchy Changed");
		}
		public static void InstancePrefabsStep(object collection,int index){
			var prefabs = (GameObject[])collection;
			var prefab = prefabs[index];
			if(prefab.IsNull()){return;}
			Events.stepperTitle = "Instancing " + index + " ∕ " + prefabs.Length + " Prefabs.";
			Events.stepperMessage = "Instancing prefab : " + prefab.name;
			if(prefab.name == "Main" || prefab.name == "@Main"){return;}
			var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
			if(!instance.HasComponent<OldAttributeData>()){
				Utility.Destroy(instance);
			}
			/*bool changes = false;
			bool last = index == prefabs.Length-1;
			if(instance.HasComponent<OldAttributeData>()){
				Attribute.all.Clear();
				AttributeDataHelper.CopyData();
				AttributeDataHelper.RemoveDeprecated();
				changes = true;
			}
			if(instance.HasComponent<InputHeld>()){
				Locate.SetDirty();
				AttributeDataHelper.FixManualIntensity();
				changes = true;
			}
			if(changes){
				Utility.SetDirty(instance);
				PrefabUtility.ReplacePrefab(instance,prefab);
			}
			Utility.Destroy(instance);
			if(last){
				Debug.Log("[Attribute Data Helper] : Repaired all prefabs.");
				Events.disabled = 0;
				Events.Resume("On Hierarchy Changed");
				Utility.EditorDelayCall(()=>AssetDatabase.Refresh(),1);
			}*/
		}
		[MenuItem ("Zios/Process/Attribute/Repair Data [Remove Old]")]
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