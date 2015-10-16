using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerDeath : NetworkBehaviour {

	private PlayerHealth healthScript;
	private Image crosshairImage;
	// Use this for initialization
	void Start () {
		crosshairImage = GetComponentInChildren<Image> (); //("Crosshair Image").GetComponent<Image> ();
		healthScript = GetComponent<PlayerHealth> ();
		healthScript.EventDie += DisablePlayer;

	}

	void OnDisable(){
		healthScript.EventDie -= DisablePlayer;
	}

	void DisablePlayer(){
		GetComponent<CharacterController> ().enabled = false;
		GetComponent<PlayerShoot> ().enabled = false;
		Renderer[] renderers = GetComponentsInChildren<Renderer> ();
		foreach (Renderer renderer in renderers) {
			renderer.enabled = false;
		}
		healthScript.isDead = true;

		if (isLocalPlayer) {
			crosshairImage.enabled = false;
			GetComponent<MechController> ().enabled = false;
			//Respawn button
		}
	}
}
