using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneCombat : Combat {

	private int default_layer = 0, player_layer = 8;
	// Use this for initialization
	void Start () {
		currentHP = MAX_HP;
		findGameManager();
		gm.RegisterPlayer(photonView.viewID, 0);
	}

	[PunRPC]
	public override void OnHit(int d, int shooter_viewID, string weapon, bool isSlowDown = false) {
		currentHP -= d;
		if (currentHP <= 0) {
//			if (shooter == PhotonNetwork.playerName) hud.ShowText(cam, transform.position, "Kill");
			DisableDrone ();
			gm.RegisterKill(shooter_viewID, photonView.viewID);
		}
	}

	[PunRPC]
	public void ShieldOnHit(int d, int shooter_viewID, int hand, string weapon) {
		currentHP -= d;
		if (currentHP <= 0) {
			//			if (shooter == PhotonNetwork.playerName) hud.ShowText(cam, transform.position, "Kill");
			DisableDrone ();
			gm.RegisterKill(shooter_viewID, photonView.viewID);
		}
	}

	[PunRPC]
	void ForceMove(Vector3 dir, float length){
		transform.position += dir * length;
	}

	void DisableDrone() {
		gameObject.layer = default_layer;
		Renderer[] renderers = GetComponentsInChildren<Renderer> ();
		foreach (Renderer renderer in renderers) {
			renderer.enabled = false;
		}
		GetComponent<CapsuleCollider>().enabled = false;
	}

	void EnableDrone() {
		gameObject.layer = player_layer;
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
