using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerNetworkSetup : NetworkBehaviour {
	
	// Use this for initialization
	public override void OnStartLocalPlayer () {
		GetComponent<MechController>().enabled = true;

		foreach (Camera c in GetComponentsInChildren<Camera>()){
			c.enabled = true;
		}
		GetComponentInChildren<MechCamera>().enabled = true;
		GetComponentInChildren<AudioListener>().enabled = true;
		GetComponentInChildren<Crosshair>().enabled = true;
//		Animator[] animators = GetComponentsInChildren<Animator>();
//		if (animators.Length > 1) {
//			GetComponent<NetworkAnimator>().animator = animators[0];
//		}
		transform.Find("Cube").gameObject.SetActive(false);
	}

}
