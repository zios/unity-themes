using UnityEngine;
using UnityEditor;
using System.Linq;
namespace Zios.UI{
	public class GroupField : StateField{
		public StateField[] columnFields = new StateField[0];
		public GroupField(object target=null,TableRow row=null) : base(target,row){}
		public int GetState(StateRequirement requirement){
			if(requirement.requireOn){return 1;}
			if(requirement.requireOff){return 2;}
			return 0;
		}
		public override void Draw(){
			if(columnFields.Length < 1){return;}
			int baseState = this.GetState(this.columnFields[0].target.As<StateRequirement>());
			bool mismatched = this.columnFields.Count(x=>this.GetState(x.target.As<StateRequirement>())!=baseState) > 0;
			this.DrawStyle(mismatched ? -1 : baseState);
			this.CheckClicked();
		}
		public override void Clicked(int button){
			int baseState = this.GetState(this.columnFields[0].target.As<StateRequirement>());
			int mismatched = this.columnFields.Count(x=>this.GetState(x.target.As<StateRequirement>())!=baseState);
			if(mismatched == 0){
				foreach(var field in this.columnFields){
					field.Clicked(button);
				}
			}
		}
	}
}