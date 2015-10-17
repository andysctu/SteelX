using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerShoot : NetworkBehaviour {

	private int damage = 25;
	private float range = Mathf.Infinity;
	[SerializeField] Transform camTransform;
	private RaycastHit hit;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		CheckIfShooting ();
	}

	void CheckIfShooting(){
		if (!isLocalPlayer)
			return;
		if (Input.GetKeyDown (KeyCode.Mouse0)) {
			Shoot();
		}
	}

	void Shoot(){
		if (Physics.Raycast (camTransform.TransformPoint(0,0,0.5f), camTransform.forward, out hit, range)){
			//Debug.Log (hit.transform.tag);

			if (hit.transform.tag == "Player"){
				string uIdentity = hit.transform.name;
				Debug.Log (uIdentity);
				CmdTellServerWhoWasShot(uIdentity, damage);
			} else if (hit.transform.tag == "Drone"){
				string uIdentity = hit.transform.name;
				Debug.Log (uIdentity);
				CmdTellServerWhichDroneWasShot(uIdentity, damage);
			}
		}
	}

	[Command]
	void CmdTellServerWhoWasShot(string uniqueID, int damage){
		GameObject go = GameObject.Find (uniqueID);
		go.GetComponent<PlayerHealth> ().OnHit(damage);
		//Apply damage
	}

	[Command]
	void CmdTellServerWhichDroneWasShot(string uniqueID, int damage){
		GameObject go = GameObject.Find (uniqueID);
		go.GetComponent<DroneHealth> ().OnHit(damage);
		//Apply damage
	}
}
