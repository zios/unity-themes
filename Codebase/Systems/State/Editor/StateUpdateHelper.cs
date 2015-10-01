using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using CallbackFunction = UnityEditor.EditorApplication.CallbackFunction;
namespace Zios{
    public static class StateUpdateHelper{
        [MenuItem ("Zios/Process/States/Repair (All)")]
	    public static void FixBroken(){
			StateUpdateHelper.RepairGUIDs();
			CallbackFunction delayed = ()=>{
				StateUpdateHelper.CopyData();
				StateUpdateHelper.RemoveDeprecated();
			};
			Utility.EditorDelayCall(delayed,1);
		}
		[MenuItem ("Zios/Process/States/Repair (GUID)")]
		public static void RepairGUIDs(){
			int count = 0;
			AssetDatabase.StartAssetEditing();
			var prefabs = FileManager.FindAll("*.prefab");
			var scenes = FileManager.FindAll("*.unity");
			var files = prefabs.Concat(scenes);
			string actionReady = "2d2c3095ed3786d4cb99a269e5e55882";
			string actionTable = "5701b12795b13f745a1a7a75b69238fd";
			string stateLink = "b6a5365a236805842985225b566116a5";
			string stateMonoBehaviour = "907269ee4d9eab540b6dae698a0c006b";
			string stateTable = "4833115f047751b48be018c26164a652";
			foreach(var file in files){
				var text = file.GetText();
				if(text.ContainsAny(actionReady,actionTable,stateLink)){
					count += 1;
					text = text.Replace(actionReady,stateMonoBehaviour);
					text = text.Replace(actionTable,stateTable);
					text = text.Replace(stateLink,stateMonoBehaviour);
					file.WriteText(text);
				}
			}
			AssetDatabase.StopAssetEditing();
			AssetDatabase.Refresh();
			Debug.Log("[UpdateHelper] : Fixing old prefab/scene GUIDs. " + count + " modified.");
		}
		[MenuItem ("Zios/Process/States/Repair (Copy Data)")]
		public static void CopyData(){
			int count = 0;
			foreach(var stateTable in Locate.GetAssets<StateTable>()){
				int readyIndex = stateTable.table.ToList().FindIndex(x=>x.name=="@Ready");
				int activeIndex = stateTable.table.ToList().FindIndex(x=>x.name=="@Active");
				if(readyIndex != -1 && activeIndex != -1){
					count += 1;
					stateTable.table[activeIndex].requirements = stateTable.table[readyIndex].requirements;
					stateTable.tableOff[activeIndex].requirements = stateTable.tableOff[readyIndex].requirements;
					StateUpdateHelper.CopyRequirements(stateTable.table);
					StateUpdateHelper.CopyRequirements(stateTable.tableOff);
					Utility.SetDirty(stateTable);
				}
			}
			Events.Call("On Hierarchy Changed");
			Debug.Log("[UpdateHelper] : Copying Ready/StateLink data. " + count + " modified.");
		}
		public static void CopyRequirements(StateRow[] rows){
			foreach(var requirementRow in rows.Select(x=>x.requirements)){
				foreach(var requirement in requirementRow){
					int readyIndex = requirement.data.ToList().FindIndex(x=>x.name=="@Ready");
					int activeIndex = requirement.data.ToList().FindIndex(x=>x.name=="@Active");
					requirement.data[activeIndex].requireOn = requirement.data[readyIndex].requireOn;
					requirement.data[activeIndex].requireOff = requirement.data[readyIndex].requireOff;
				}
			}
		}
		[MenuItem ("Zios/Process/States/Repair (Remove Deprecated)")]
		public static void RemoveDeprecated(){
			int count = 0;
			foreach(var script in Locate.GetAssets<StateMonoBehaviour>()){
				if(script.GetType() == typeof(StateMonoBehaviour)){
					count += 1;
					Utility.Destroy(script);
				}
			}
			Debug.Log("[UpdateHelper] : Removing StateLink & StateReady components. " + count + " modified.");
		}
	}
}