using UnityEngine;
using UnityEditor;
using System.Linq;
namespace Zios.UI{
	public class HeaderField : TableField{
		public HeaderField(object target=null,TableRow row=null) : base(target,row){}
		public override void Draw(){
			var window = StateWindow.Get();
			var scroll = window.scroll;
			var label = this.target is string ? new GUIContent("") : new GUIContent(this.target.GetVariable<string>("name"));
			GUIStyle style = new GUIStyle(GUI.skin.label);
			var mode = (HeaderMode)EditorPrefs.GetInt("StateWindow-Mode",2);
			bool darkSkin = EditorGUIUtility.isProSkin;
			if(label.text == ""){
				this.disabled = this.row.fields.Skip(1).Count(x=>!x.disabled) < 1;
				window.headerSize = 64;
				style.margin.left = 5;
				string background = darkSkin ? "BoxBlackA30" : "BoxWhiteBWarm";
				style.normal.background = FileManager.GetAsset<Texture2D>(background);
				if(mode == HeaderMode.Vertical){
					window.headerSize = 35;
					style.fixedHeight = style.fixedWidth;
					StateWindow.Clip(label,style,0,window.headerSize);
				}
				if(mode != HeaderMode.Vertical){StateWindow.Clip(label,style,-1,-1);}
				return;
			}
			bool fieldHovered = window.column == this.order;
			if(fieldHovered){
				string background = darkSkin ? "BoxBlackHighlightBlueAWarm" : "BoxBlackHighlightBlueDWarm";
				style.normal.textColor = darkSkin ? Colors.Get("ZestyBlue") : Colors.Get("White");
				style.normal.background = FileManager.GetAsset<Texture2D>(background);
			}
			if(mode == HeaderMode.Vertical){
				window.cellSize = style.fixedHeight;
				float halfWidth = style.fixedWidth / 2;
				float halfHeight = style.fixedHeight / 2;
				GUIStyle rotated = new GUIStyle(style).Rotate90();
				Rect last = GUILayoutUtility.GetRect(new GUIContent(""),rotated);
				GUIUtility.RotateAroundPivot(90,last.center);
				Rect position = new Rect(last.x,last.y,0,0);
				position.x +=  halfHeight-halfWidth;
				position.y += -halfHeight+halfWidth;
				style.overflow.left = (int)-scroll.y;
				label.text = style.overflow.left >= -(position.width/4)-9 ? label.text : "";
				label.DrawLabel(position,style);
				GUI.matrix = Matrix4x4.identity;
			}
			else{
				style.fixedWidth -= 36;
				window.cellSize = style.fixedWidth;
				if(mode == HeaderMode.HorizontalFit){
					var visible = this.row.fields.Skip(1).Where(x=>!x.disabled).ToList();
					float area = window.cellSize = (Screen.width-style.fixedWidth-56)/visible.Count;
					area = window.cellSize = Mathf.Floor(area-2);
					bool lastEnabled = visible.Last() == this;
					style.margin.right = lastEnabled ? 18 : 0;
					style.fixedWidth = lastEnabled ? 0 : area;
				}
				style.alignment = TextAnchor.MiddleCenter;
				StateWindow.Clip(label,style,GUI.skin.label.fixedWidth+7,-1);
			}
			this.CheckClicked(0,scroll.y);
		}
		public override void Clicked(int button){
			if(button == 0){
				int mode = (EditorPrefs.GetInt("StateWindow-Mode",2)+1)%3;
				EditorPrefs.SetInt("StateWindow-Mode",mode);
				this.row.table.ShowAll();
				StateWindow.Get().Repaint();
				return;
			}
			/*var menu = new GenericMenu();
			GUIContent toggleUpdateText = new GUIContent(" Always Update");
			MenuFunction toggleUpdate = ()=>Utility.ToggleEditorPref("StateWindow-AlwaysUpdate");
			menu.AddItem(toggleUpdateText,false,toggleUpdate);
			menu.ShowAsContext();*/
		}
	}
}