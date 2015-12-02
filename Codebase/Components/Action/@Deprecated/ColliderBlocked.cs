using UnityEngine;
using Zios;
public enum BlockType{Blocked,Unblocked}
public enum Direction{Up,Down,Left,Right,Forward,Back}
[AddComponentMenu("Zios/Component/Action/Blocked State")]
public class BlockedState : StateMonoBehaviour{
	//public AttributeEnum type = new AttributeEnum(BlockType);
	public BlockType type;
	public Direction direction;
	public AttributeFloat duration = 0;
	public AttributeGameObject target = new AttributeGameObject();
	public override void Awake(){
		base.Awake();
		string warning = "This component has been deprecated and likely should not be used.";
		if(!this.dependents.Exists(x=>x.message==warning)){
			this.dependents.AddNew().message = warning;
		}
		this.target.Setup("Target",this);
		this.duration.Setup("Duration",this);
	}
	public override void Use(){
		float duration = 0;
		//string direction = this.direction.ToString().ToLower();
		//float duration = this.target.GetDictionary<string,float>("GetUnblocked")[direction];
		bool state = this.type == BlockType.Blocked ? duration < this.duration : duration > this.duration;
		this.Toggle(state);
	}
}