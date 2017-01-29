using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : Photon.MonoBehaviour {

	public int CurrentHP;
	public int MaxHP = 100;

	// Use this for initialization
	void Start () {
		CurrentHP = MaxHP;
	}

	[PunRPC]
	void OnHit(int d, string shooter) {
		Debug.Log ("Drone got shot");
		CurrentHP -= d;
		Debug.Log ("HP: " + CurrentHP);
		if (CurrentHP <= 0) {
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
