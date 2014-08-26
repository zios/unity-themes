using UnityEngine;
using System.Collections;
using Zios;
public class MoveTest : MonoBehaviour{
	public KeyCode keyForward = KeyCode.W;
	public KeyCode keyBack = KeyCode.S;
	public ColliderController cMove;
	private Vector3 moveVector;
	void Start(){
		cMove=gameObject.GetComponent<ColliderController>();}
	void Update(){
		if (Input.GetKeyDown(keyForward)){moveVector = new Vector3(0,0,30);
		}if (Input.GetKeyUp(keyForward)){moveVector = new Vector3(0,0,0);}
		if (Input.GetKeyDown(keyBack)){moveVector = new Vector3(0,0,-30);
		}if (Input.GetKeyUp(keyBack)){moveVector = new Vector3(0,0,0);}
		cMove.move.Add(moveVector);}}