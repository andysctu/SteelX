using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerDeath : NetworkBehaviour {

	private PlayerHealth healthScript;
	private Image crosshairImage;

	public override void PreStartClient ()
	{
		healthScript = GetComponent<PlayerHealth> ();
		healthScript.EventDie += DisablePlayer;
	}

	public override void OnStartLocalPlayer(){
		crosshairImage = GetComponentInChildren<Image> (); //("Crosshair Image").GetComponent<Image> ();
	}
	
	public override void OnNetworkDestroy(){
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
//			GameObject.Find ("GameManager").GetComponent<GameManagerReferences>().RespawnButton.SetActive (true);
		}
	}
}
