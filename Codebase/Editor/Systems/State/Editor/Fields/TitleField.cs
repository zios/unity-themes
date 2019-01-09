using System;
using UnityEditor;
using UnityEngine;
using MenuFunction = UnityEditor.GenericMenu.MenuFunction;
namespace Zios.Unity.Editor.State{
	using Zios.Extensions.Convert;
	using Zios.State;
	using Zios.Unity.Editor.Drawers.Table;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.EditorUI;
	using Zios.Unity.Extensions;
	using Zios.Unity.Editor.Extensions;
	using Zios.Unity.Style;
	public class TitleField : TableField{
		public TitleField(object target=null,TableRow row=null) : base(target,row){}
		public override void Draw(){
			var window = StateWindow.Get();
			var title = new GUIContent((string)this.target.As<StateTable>().gameObject.name);
			var style = Style.Get("title");
			if(window.target.advanced){
				title.text += window.tableIndex == 1 ? " <b>End</b>" : " <b>Start</b>";
			}
			style.fixedWidth = Screen.width-24;
			Rect next = GUILayoutUtility.GetRect(title,style);
			title.ToLabel().DrawLabel(next.AddXY(window.scroll),style);
			this.CheckClicked();
		}
		private void CheckClicked(){
			throw new NotImplementedException();
		}
		public override void Clicked(int button){
			var window = StateWindow.Get();
			if(button == 0){
				if(window.target.advanced){
					window.tableIndex = window.tableIndex == 0 ? 1 : 0;
					window.BuildTable();
				}
			}
			if(button == 1){
				var menu = new GenericMenu();
				MenuFunction markDirty = ()=>ProxyEditor.SetDirty(window.target);
				MenuFunction toggleAdvanced = ()=>{
					ProxyEditor.RecordObject(window.target,"State Window - Advanced Toggle");
					window.target.advanced = !window.target.advanced;
					window.tableIndex = 0;
					window.BuildTable();
				};
				MenuFunction toggleManual = ()=>{
					ProxyEditor.RecordObject(window.target,"State Window - Manual Toggle");
					window.target.manual = !window.target.manual;
					window.BuildTable();
				};
				menu.AddItem("Advanced",window.target.advanced,toggleAdvanced+markDirty);
				if(window.target.controller != null){
					menu.AddItem("Manual",window.target.manual,toggleManual+markDirty);
				}
				menu.AddItem("Rebuild",false,window.BuildTable);
				menu.ShowAsContext();
			}
		}
	}
}