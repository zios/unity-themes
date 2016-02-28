using UnityEditor;
using UnityEngine;
using UnityEvent = UnityEngine.Event;
namespace Zios.Editors{
	using Interface;
	using Events;
	[CustomEditor(typeof(DataMonoBehaviour),true)][CanEditMultipleObjects]
	public class DataMonoBehaviourEditor : MonoBehaviourEditor{
		public static DataMonoBehaviour current;
		public static void CheckDependents(){
			if(DataMonoBehaviourEditor.current.IsNull()){return;}
			DataMonoBehaviourEditor.current.CheckDependents();
		}
		public override void OnInspectorGUI(){
			if(!UnityEvent.current.IsUseful()){return;}
			Event.Add("On Components Changed",DataMonoBehaviourEditor.CheckDependents);
			Event.Add("On Attributes Ready",DataMonoBehaviourEditor.CheckDependents);
			var target = DataMonoBehaviourEditor.current = (DataMonoBehaviour)this.target;
			bool targetsMissing = false;
			string message = "";
			foreach(var warning in target.warnings){
				warning.DrawHelp("Warning");
			}
			foreach(var dependent in target.dependents){
				if(dependent.exists){continue;}
				if(!target.IsEnabled()){break;}
				message = dependent.message;
				if(dependent.target.IsNull() && (dependent.dynamicTarget.IsNull() || dependent.dynamicTarget.Get().IsNull())){
					targetsMissing = true;
					continue;
				}
				if(!dependent.target.IsNull() || (!dependent.dynamicTarget.IsNull() && !dependent.dynamicTarget.Get().IsNull())){
					string targetName = dependent.dynamicTarget.IsNull() ? dependent.target.name : dependent.dynamicTarget.Get().name;
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
					Utility.DelayCall(this.Repaint,0.25f);
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