using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerNetworkSetup : NetworkBehaviour {
	
	// Use this for initialization
	public override void OnStartLocalPlayer () {
//		GameObject.Find ("Scene").SetActive(false);
		//GetComponent<Animator>().enabled = true;
		GetComponent<MechController>().enabled = true;

		foreach (Camera c in GetComponentsInChildren<Camera>()){
			c.enabled = true;
		}
//		GetComponentInChildren<Camera>().enabled = true;
		GetComponentInChildren<AudioListener>().enabled = true;
		GetComponentInChildren<Crosshair>().enabled = true;
		GetComponentInChildren<MechCamera>().enabled = true;

	}

}
