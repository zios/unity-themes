using UnityEngine;
public static class Button{
	public static bool CheckEventKeyDown(KeyCode code){
		return Event.current.type == EventType.KeyDown && Event.current.keyCode == code;
	}
	public static bool CheckEventKeyUp(KeyCode code){
		return Event.current.type == EventType.KeyUp && Event.current.keyCode == code;
	}
}