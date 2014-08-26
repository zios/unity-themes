using UnityEngine;
using System.Collections;
using Zios;
[RequireComponent(typeof(ColliderController))]
public class MoveTest : MonoBehaviour{
	public KeyCode keyForward = KeyCode.W;
	public KeyCode keyBack = KeyCode.S;
	public KeyCode keyLeft = KeyCode.A;
	public KeyCode keyRight = KeyCode.D;
	public ColliderController characterMove;
	private Vector3 moveVector;
	void Start(){
		characterMove=gameObject.GetComponent<ColliderController>();
	}
	void Update(){
		if(Input.GetKey(keyForward)){moveVector = new Vector3(0,0,7);}
		else if(Input.GetKey(keyBack)){moveVector = new Vector3(0,0,-7);}
		else if(Input.GetKey(keyLeft)){moveVector = new Vector3(-7,0,0);}
		else if(Input.GetKey(keyRight)){moveVector = new Vector3(7,0,0);}
		else{moveVector = new Vector3(0,0,0);}
		characterMove.move.Add(moveVector);
	}
}