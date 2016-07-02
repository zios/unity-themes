using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Zios.Editors.StateEditors{
	using Interface;
	using Actions;
	public class HeaderField : TableField{
		public HeaderField(object target=null,TableRow row=null) : base(target,row){}
		public override void Draw(){
			var window = StateWindow.Get();
			var hidden = !this.target.Equals("") && !window.target.manual && this.target.As<StateRequirement>().name == "@External";
			if(hidden){return;}
			var target = this.target is StateRequirement ? this.target.As<StateRequirement>() : null;
			var script = !target.IsNull() ? target.target.As<StateMonoBehaviour>() : null;
			var scroll = window.scroll;
			var label = this.target is string ? new GUIContent("") : new GUIContent(this.target.GetVariable<string>("name"));
			GUIStyle style = new GUIStyle(GUI.skin.label);
			var mode = (HeaderMode)EditorPrefs.GetInt("StateWindow-Mode",2);
			bool darkSkin = EditorGUIUtility.isProSkin || EditorPrefs.GetBool("EditorTheme-Dark",false);
			Color textColor = Colors.Get("Gray");
			string background = darkSkin ? "BoxBlackAWarm30" : "BoxWhiteBWarm50";
			if(window.target.external){
				textColor = darkSkin ? Colors.Get("Silver") : Colors.Get("Black");
				background = darkSkin ? "BoxBlackA30" : "BoxWhiteBWarm";
			}
			if(label.text == ""){
				this.disabled = this.row.fields.Skip(1).Count(x=>!x.disabled) < 1;
				window.headerSize = 64;
				style.margin.left = 5;
				style.hover = style.normal;
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
				background = darkSkin ? "BoxBlackHighlightBlueAWarm" : "BoxBlackHighlightBlueDWarm";
				textColor = darkSkin ? Colors.Get("ZestyBlue") : Colors.Get("White");
			}
			if(Application.isPlaying && !script.IsNull()){
				textColor = Colors.Get("Gray");
				background = darkSkin ? "BoxBlackAWarm30" : "BoxWhiteBWarm50";
				bool usable = target.name == "@External" ? window.target.external : script.usable;
				bool active = target.name == "@External" ? window.target.external : script.active;
				if(usable){
					textColor = darkSkin ? Colors.Get("Silver") : Colors.Get("Black");
					background = darkSkin ? "BoxBlackA30" : "BoxWhiteBWarm";
				}
				if(script.used){
					textColor = darkSkin ? Colors.Get("White") : Colors.Get("White");
					background = darkSkin ? "BoxBlackHighlightYellowAWarm" : "BoxBlackHighlightYellowDWarm";
				}
				if(active){
					textColor = darkSkin ? Colors.Get("White") : Colors.Get("White");
					background = darkSkin ? "BoxBlackHighlightPurpleAWarm" : "BoxBlackHighlightPurpleDWarm";
				}
			}
			style.normal.textColor = textColor;
			style.normal.background = FileManager.GetAsset<Texture2D>(background);
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
				label.ToLabel().DrawLabel(position,style);
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
		}
	}
}