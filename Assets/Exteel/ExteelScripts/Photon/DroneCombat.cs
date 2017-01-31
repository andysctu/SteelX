using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneCombat : Combat {

	private HUD hud;
	private Camera cam;

	// Use this for initialization
	void Start () {
		MaxHP = 100;
		CurrentHP = MaxHP;

//		hud = GameObject.Find("Canvas").GetComponent<HUD>();
//		cam = transform.FindChild("Camera").gameObject.GetComponent<Camera>();
	}

	[PunRPC]
	public override void OnHit(int d, string shooter) {
		CurrentHP -= d;
		if (CurrentHP <= 0) {
//			if (shooter == PhotonNetwork.playerName) hud.ShowText(cam, transform.position, "Kill");
			DisableDrone ();
		}
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
		CurrentHP = MaxHP;
		GetComponent<CapsuleCollider>().enabled = true;
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Z)) {
			EnableDrone();
		}
	}
}
