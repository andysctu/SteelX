using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneCombat : Combat {

	// Use this for initialization
	void Start () {
		currentHP = MAX_HP;
		findGameManager();
		gm.RegisterPlayer("Drone");
	}

	[PunRPC]
	public override void OnHit(int d, string shooter, float slowdownDuration = 0) {
		currentHP -= d;
		if (currentHP <= 0) {
//			if (shooter == PhotonNetwork.playerName) hud.ShowText(cam, transform.position, "Kill");
			DisableDrone ();
			//gm.RegisterKill(PhotonNetwork.playerName, "Drone");
			gm.RegisterKill(shooter, "Drone");
		}
	}

	[PunRPC]
	void ForceMove(Vector3 dir, float length){
		transform.position += dir * length;
	}

	void DisableDrone() {
		gameObject.layer = 2;
		Renderer[] renderers = GetComponentsInChildren<Renderer> ();
		foreach (Renderer renderer in renderers) {
			renderer.enabled = false;
		}
		GetComponent<CapsuleCollider>().enabled = false;
	}

	void EnableDrone() {
		gameObject.layer = 8;
		Renderer[] renderers = GetComponentsInChildren<Renderer> ();
		foreach (Renderer renderer in renderers) {
			renderer.enabled = true;
		}
		currentHP = MAX_HP;
		GetComponent<CapsuleCollider>().enabled = true;
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Z)) {
			EnableDrone();
		}
	}
}
