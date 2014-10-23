using Zios;
using System;
using UnityEngine;
public enum BlockType{Blocked,Unblocked}
public enum Direction{Up,Down,Left,Right,Forward,Back}
[AddComponentMenu("Zios/Component/Action/Part/Blocked State")]
public class BlockedState : ActionPart{
	public BlockType type;
	public Direction direction;
	public float duration;
	public Target target = new Target();
	public override void OnValidate(){
		this.DefaultPriority(5);
		base.OnValidate();
		this.target.Update(this);
	}
	public void Start(){
		this.target.Setup(this);
	}
	public override void Use(){
		string direction = this.direction.ToString().ToLower();
		float duration = (float)this.target.Query("GetUnblocked",direction);
		bool state = this.type == BlockType.Blocked ? duration < this.duration : duration > this.duration;
		this.Toggle(state);
	}
}
