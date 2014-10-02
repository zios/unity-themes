using Zios;
using System;
using UnityEngine;
public enum BlockType{Blocked,Unblocked}
public enum Direction{Up,Down,Left,Right,Forward,Back}
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Blocked State")]
public class BlockedState : ActionPart{
	[NonSerialized] bool lastState;
	public ColliderController controller;
	public BlockType type;
	public Direction direction;
	public float duration;
	public void Start(){
		if(this.controller == null){
			Debug.LogWarning("BlockedState (" + this.alias + ") : No ColliderController found.");
		}
	}
	public override void OnValidate(){
		this.DefaultPriority(5);
		if(this.controller == null){
			this.controller = this.gameObject.GetComponentInParents<ColliderController>();
		}
		base.OnValidate();
	}
	public override void Use(){
		string direction = this.direction.ToString().ToLower();
		float duration = this.controller.GetUnblockedDuration(direction);
		bool state = this.type == BlockType.Blocked ? duration < this.duration : duration > this.duration;
		this.Toggle(state);
	}
}