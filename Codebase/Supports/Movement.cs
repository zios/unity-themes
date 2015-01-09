using UnityEngine;
//====================================
// 8-Directional Movement
//====================================
public static class Movement{
	public static Vector3 ToAngle(this Vector3 vector){
		bool up = vector.z > 0;
		bool down = vector.z < 0;
		bool left = vector.x > 0;
		bool right = vector.x < 0;
		return Movement.GetDirection(up,down,left,right);
	}
	public static bool[] GetDirectionButtons(this Vector3 direction){
		bool up = Mathf.Abs(direction.y) == 45 || direction.y == 0;
		bool down = Mathf.Abs(direction.y) == 135 || direction.y == 180;
		bool left = direction.y == -135 || direction.y == -90 || direction.y == -45;
		bool right = direction.y == 135 || direction.y == 90 || direction.y == 45;
		return new bool[4]{up,down,left,right};
	}
	public static Vector3 GetDirection(bool up,bool down,bool left,bool right){
		Vector3 value = Vector3.zero;
		if(up && right){
			value = new Vector3(0,45,0);
		}
		else if(up && left){
			value = new Vector3(0,-45,0);
		}
		else if(down && right){
			value = new Vector3(0,135,0);
		}
		else if(down && left){
			value = new Vector3(0,-135,0);
		}
		else if(up){
			value = new Vector3(0,0,0);
		}
		else if(down){
			value = new Vector3(0,180,0);
		}
		else if(left){
			value = new Vector3(0,-90,0);
		}
		else if(right){
			value = new Vector3(0,90,0);
		}
		return value;
	}
	public static Vector3 GetMove(Vector3 start,Vector3 direction,float speed,float[] scale){
		bool[] buttons = Movement.GetDirectionButtons(direction);
		return GetMove(start,buttons[0],buttons[1],buttons[2],buttons[3],speed,scale);
	}
	public static Vector3 GetMove(Vector3 move,bool up,bool down,bool left,bool right,float speed,float[] scale){
		if(up){move.z = speed * scale[0];}
		if(down){move.z = -speed * scale[1];}
		if(left){move.x = -speed * scale[2];}
		if(right){move.x = speed * scale[3];}
		return move;
	}
}