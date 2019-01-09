using System.Linq;
using UnityEngine;
namespace Zios.Unity.Editor.State{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.State;
	using Zios.Unity.Editor.Drawers.Table;
	using Zios.Unity.Editor.Pref;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.EditorUI;
	using Zios.Unity.Extensions;
	using Zios.Unity.Proxy;
	using Zios.Unity.Style;
	public class StateField : TableField{
		public StateField(object target=null,TableRow row=null) : base(target,row){}
		public override void Draw(){
			int state = 0;
			var window = StateWindow.Get();
			var requirement = this.target.As<StateRequirement>();
			if(window.target.manual || requirement.name != "@External"){
				if(requirement.requireOn){state = 1;}
				if(requirement.requireOff){state = 2;}
				if(requirement.requireUsed){state = 3;}
				this.DrawStyle(state);
				this.CheckClicked();
			}
		}
		public virtual void DrawStyle(int state=0){
			var window = StateWindow.Get();
			string value = "";
			var stateRow = (StateRow)this.row.target;
			var row = this.row.target.As<StateRow>();
			int rowIndex = window.rowIndex[row];
			var mode = (HeaderMode)EditorPref.Get<int>("StateWindow-Mode",3);
			GUIStyle style = new GUIStyle(GUI.skin.button);
			value = stateRow.requirements.Length > 1 ? (rowIndex+1).ToString() : "";
			if(this.row.selected){style = Style.Get("buttonSelected",true);}
			else if(state == -1){style = Style.Get("buttonDisabled",true);}
			else if(state == 1){style = Style.Get("buttonOn",true);}
			else if(state == 2){style = Style.Get("buttonOff",true);}
			else if(state == 3){style = Style.Get("buttonUsed",true);}
			style.fixedWidth = window.cellSize;
			if(mode == HeaderMode.HorizontalFit){
				bool lastEnabled = this.row.fields.Last(x=>!x.disabled) == this;
				style.margin.right = lastEnabled ? 18 : 0;
				style.fixedWidth = lastEnabled ? 0 : style.fixedWidth;
			}
			float headerSize = window.headerSize;
			StateWindow.Clip(value.ToLabel(),style,GUI.skin.label.fixedWidth+7,headerSize);
			if(!Proxy.IsPlaying() && GUILayoutUtility.GetLastRect().Hovered()){
				window.row = this.row.order;
				window.column = this.order;
				window.hovered = true;
			}
		}
		public override void Clicked(int button){
			var window = StateWindow.Get();
			ProxyEditor.RecordObject(window.target,"State Window - Field Toggle");
			this.row.selected = false;
			int state = 0;
			var requirement = (StateRequirement)this.target;
			if(requirement.requireOn){state = 1;}
			if(requirement.requireOff){state = 2;}
			if(requirement.requireUsed){state = 3;}
			int amount = button == 0 ? 1 : -1;
			state += amount;
			state = state.Modulus(4);
			requirement.requireOn = false;
			requirement.requireOff = false;
			requirement.requireUsed = false;
			if(state == 1){requirement.requireOn = true;}
			if(state == 2){requirement.requireOff = true;}
			if(state == 3){requirement.requireUsed = true;}
			ProxyEditor.SetDirty(window.target);
			window.target.UpdateStates();
			window.Repaint();
		}
	}
}