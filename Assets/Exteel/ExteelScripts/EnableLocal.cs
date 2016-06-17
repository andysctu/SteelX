﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class EnableLocal : NetworkBehaviour {

	// Use this for initialization
	public override void OnStartLocalPlayer () {

		// Enable mech controller
		GetComponent<NewMechController>().enabled = true;

		// Enable camera/radar
		foreach (Camera c in GetComponentsInChildren<Camera>()){
			c.enabled = true;
		}
		GetComponentInChildren<MechCamera>().enabled = true;
		GetComponent<NameTags>().enabled = true;
//		GetComponentInChildren<AudioListener>().enabled = true;

		// Enable crosshair
		GetComponentInChildren<Crosshair>().enabled = true;

		// Disable your own cube
//		transform.Find("Cube").gameObject.SetActive(false);
	}
}
