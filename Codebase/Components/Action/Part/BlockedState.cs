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
	public void OnValidate(){
		this.DefaultPriority(5);
		if(this.controller == null){
			this.controller = this.gameObject.GetComponentInParents<ColliderController>();
		}
	}
	public void Awake(){
		if(this.controller == null){
			Debug.LogWarning("No ColliderController found for use with 'Grounded' Action.");
		}
	}
	public override void Use(){
		string direction = this.direction.ToString().ToLower();
		float duration = this.controller.GetUnblockedDuration(direction);
		this.inUse = this.type == BlockType.Blocked ? duration < this.duration : duration > this.duration;
		if(this.lastState != this.inUse){
			this.lastState = this.inUse;
			this.Toggle(this.inUse);
			this.SetDirty(true);
		}
	}
}