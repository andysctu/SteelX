using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerNetworkSetup : NetworkBehaviour {

	[SerializeField] Camera mechCamera;
	[SerializeField] AudioListener audioListener;
	[SerializeField] GameObject cam;
	// Use this for initialization
	void Start () {
		if (isLocalPlayer) {
			GameObject.Find ("Scene").SetActive(false);
			GetComponent<CharacterController>().enabled = true;
			GetComponent<MechController>().enabled = true;
			GetComponent<AudioSource>().enabled = true;
			mechCamera.enabled = true;
			cam.GetComponent<MechCamera>().enabled = true;
			audioListener.enabled = true;   
		}
	}

}
