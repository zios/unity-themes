using UnityEngine;
using System.Collections;
public class FollowController : MonoBehaviour{
	public GameObject player;
	public KeyCode playerTurnKey = KeyCode.W;
	public KeyCode playerTurnKeyOptional = KeyCode.S;
	public float x,y,z,xSpeed,ySpeed,xMax,xMin,yMax,yMin;
	public Follow follow;
	void Start(){
		follow=gameObject.GetComponent<Follow>();}
	void Update(){
		x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
		y += Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
		z += Input.GetAxis("Mouse ScrollWheel");
		y = Mathf.Clamp (y,yMin,yMax);
		follow.orbitAngles.x = x;
		follow.orbitAngles.y = y;
		follow.targetOffset.z = z;
		if(Input.GetKey(playerTurnKey) || Input.GetKey(playerTurnKeyOptional)){player.transform.rotation = gameObject.transform.rotation;}}}