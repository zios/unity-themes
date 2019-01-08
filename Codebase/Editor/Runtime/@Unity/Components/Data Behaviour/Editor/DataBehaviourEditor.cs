using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.Inspectors.DataBehaviour{
	using Zios.Events;
	using Zios.Extensions;
	using Zios.Reflection;
	using Zios.Unity.Call;
	using Zios.Unity.Components.DataBehaviour;
	using Zios.Unity.Editor.MonoBehaviourEditor;
	using Zios.Unity.EditorUI;
	using Zios.Unity.Extensions;
	//asm Zios.Shortcuts;
	//asm Zios.Unity.Editor.Inspectors;
	//asm Zios.Unity.Shortcuts;
	[CustomEditor(typeof(DataBehaviour),true)][CanEditMultipleObjects]
	public class DataBehaviourEditor : MonoBehaviourEditor{
		public static DataBehaviour current;
		public static void CheckDependents(){
			if(DataBehaviourEditor.current.IsNull()){return;}
			DataBehaviourEditor.current.CheckDependents();
		}
		public override void OnInspectorGUI(){
			if(!Event.current.IsUseful()){return;}
			EditorUI.Reset();
			Events.Add("On Components Changed",DataBehaviourEditor.CheckDependents);
			Events.Add("On Attributes Ready",DataBehaviourEditor.CheckDependents);
			var target = DataBehaviourEditor.current = (DataBehaviour)this.target;
			bool targetsMissing = false;
			string message = "";
			foreach(var warning in target.warnings){
				warning.DrawHelp("Warning");
			}
			foreach(var dependent in target.dependents){
				if(dependent.exists){continue;}
				if(!target.IsEnabled()){break;}
				message = dependent.message;
				if(dependent.target.IsNull() && (dependent.dynamicTarget.IsNull() || dependent.dynamicTarget.Call<GameObject>("Get").IsNull())){
					targetsMissing = true;
					continue;
				}
				if(!dependent.target.IsNull() || (!dependent.dynamicTarget.IsNull() && !dependent.dynamicTarget.Call<GameObject>("Get").IsNull())){
					string targetName = dependent.dynamicTarget.IsNull() ? dependent.target.name : dependent.dynamicTarget.Call<GameObject>("Get").name;
					if(!dependent.scriptName.IsEmpty()){targetName = dependent.scriptName;}
					message = message.Replace("[target]",targetName);
				}
				if(!dependent.types.IsNull()){
					string names = "";
					foreach(var type in dependent.types){names += type.Name + " or ";}
					message = message.Replace("[type]",names.Trim(" or "));
				}
				if(!Application.isPlaying){
					GUI.enabled = !dependent.processing;
					message.DrawHelp("Warning");
					GUI.enabled = true;
					if(!dependent.processing){
						Rect area = GUILayoutUtility.GetLastRect();
						EditorGUIUtility.AddCursorRect(area,MouseCursor.Link);
						if(area.Clicked(0) && dependent.method != null){
							dependent.method();
						}
					}
					Call.Delay(this.Repaint,0.25f);
				}
			}
			if(!Application.isPlaying && targetsMissing){
				message = "One or more target fields are missing.";
				message.DrawHelp("Warning");
			}
			base.OnInspectorGUI();
		}
	}
}