using System;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Zios{
	public static class RectExtension{
		/*public static bool ContainsPoint(this Rect area,Vector3 position){
			return (position.x > area.xMin) && (position.x < area.xMax) && (position.y > area.yMin) && (position.y < area.yMax);
		}*/
		public static Rect OverrideX(this Rect current,float value){
			current.x = value;
			return current;
		}
		public static Rect OverrideY(this Rect current,float value){
			current.y = value;
			return current;
		}
		public static Rect OverrideWidth(this Rect current,float value){
			current.width = value;
			return current;
		}
		public static Rect OverrideHeight(this Rect current,float value){
			current.height = value;
			return current;
		}
		public static Rect Scale(this Rect current,float x,float y){
			return new Rect(current).OverrideWidth(current.width*x).OverrideHeight(current.height*y);
		}
		public static Rect SetXY(this Rect current,float x,float y){
			return new Rect(current).OverrideX(x).OverrideY(y);
		}
		public static Rect SetSize(this Rect current,float width,float height){
			return new Rect(current).OverrideWidth(width).OverrideHeight(height);
		}
		public static Rect SetX(this Rect current,float value){
			return new Rect(current).OverrideX(value);
		}
		public static Rect SetY(this Rect current,float value){
			return new Rect(current).OverrideY(value);
		}
		public static Rect SetWidth(this Rect current,float value){
			return new Rect(current).OverrideWidth(value);
		}
		public static Rect SetHeight(this Rect current,float value){
			return new Rect(current).OverrideHeight(value);
		}
		public static Rect AddXY(this Rect current,Vector2 value){return current.AddX(value.x).AddY(value.y);}
		public static Rect AddXY(this Rect current,float x,float y){return current.AddX(x).AddY(y);}
		public static Rect AddX(this Rect current,float value){return current.Add(value);}
		public static Rect AddY(this Rect current,float value){return current.Add(0,value);}
		public static Rect AddSize(this Rect current,float width,float height){return current.AddWidth(width).AddHeight(height);}
		public static Rect AddWidth(this Rect current,float value){return current.Add(0,0,value);}
		public static Rect AddHeight(this Rect current,float value){return current.Add(0,0,0,value);}
		public static Rect Add(this Rect current,params float[] other){
			return current.Add(other.ToRect());
		}
		public static Rect Add(this Rect current,Rect other){
			Rect result = new Rect(current);
			result.x += other.x;
			result.y += other.y;
			result.width += other.width;
			result.height += other.height;
			return result;
		}
		public static bool Hovered(this Rect current,string cursor="Link"){
			Vector2 mouse = Event.current.mousePosition;
			bool state = current.Contains(mouse);
			if(state && !cursor.IsEmpty()){
				#if UNITY_EDITOR
				var pointer = (MouseCursor)Enum.Parse(typeof(MouseCursor),cursor);
				EditorGUIUtility.AddCursorRect(current,pointer);
				#endif
			}
			return state;
		}
		public static bool Clicked(this Rect current,int button=-1){
			bool eventMatch = Event.current.type == EventType.MouseDown;
			bool buttonMatch = button == -1 ? true : Event.current.button == button;
			return current.Hovered("") && eventMatch && buttonMatch;
		}
		public static bool InFocusedWindow(this Rect current){
			Rect windowRect = new Rect(0,0,Screen.width,Screen.height);
			#if UNITY_EDITOR
			if(EditorWindow.focusedWindow){
				//EditorWindow.focusedWindow.maxSize
				Vector2 scroll = EditorWindow.focusedWindow.GetVariable<Vector2>("m_ScrollPosition");
				windowRect.y = scroll.y;
			}
			#endif
			return current.Overlaps(windowRect);
		}
		public static bool IsEmpty(this Rect current){
			bool oneSize = current.x == 0 && current.y == 0 && current.width == 1 && current.height == 1;
			bool noSize = current.x == 0 && current.y == 0 && current.width == 0 && current.height == 0;
			return oneSize || noSize;
		}
		#if UNITY_EDITOR
		public static EditorWindow GetInspectorWindow(this Rect current){
			Type inspectorWindow = Utility.GetInternalType("InspectorWindow");
			return EditorWindow.GetWindowWithRect(inspectorWindow,current);
		}
		public static Rect GetInspectorArea(this Rect current,EditorWindow window=null){
			Rect windowRect = new Rect(0,0,Screen.width,Screen.height);
			//var window = current.GetInspectorWindow();
			//Debug.Log(window.GetVariable<Rect>("position"));
			if(window == null){window = Utility.GetInspectors().First();}
			Vector2 scroll = window.GetVariable<Vector2>("m_ScrollPosition");
			windowRect.x = scroll.x;
			windowRect.y = scroll.y;
			return windowRect;
		}
		public static bool InInspectorWindow(this Rect current,EditorWindow window=null){
			if(current.IsEmpty()){return false;}
			Rect windowRect = current.GetInspectorArea(window);
			return current.Overlaps(windowRect);
		}
		#endif
	}
}