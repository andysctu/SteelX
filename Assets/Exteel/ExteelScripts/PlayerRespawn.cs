using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerRespawn : NetworkBehaviour {

	private PlayerHealth healthScript;
	private Image crosshairImage;
	private GameObject respawnButton;

	public override void PreStartClient ()
	{
		healthScript = GetComponent<PlayerHealth> ();
		healthScript.EventRespawn += EnablePlayer;
	}
	// Use this for initialization
	public override void OnStartLocalPlayer () {
		crosshairImage = GetComponentInChildren<Image> ();
		SetRespawnButton ();
	}

	public override void OnNetworkDestroy(){
		healthScript.EventRespawn -= EnablePlayer;
	}

	void EnablePlayer(){
		GetComponent<CharacterController> ().enabled = true;
		GetComponent<PlayerShoot> ().enabled = true;
		Renderer[] renderers = GetComponentsInChildren<Renderer> ();
		foreach (Renderer renderer in renderers) {
			renderer.enabled = true;
		}
		
		if (isLocalPlayer) {
			crosshairImage.enabled = true;
			GetComponent<MechController> ().enabled = true;
			respawnButton.SetActive(false);
		}
	}

	void SetRespawnButton(){
		if (isLocalPlayer) {
			respawnButton = GameObject.Find ("GameManager").GetComponent<GameManagerReferences>().RespawnButton;
			respawnButton.GetComponent<Button>().onClick.AddListener(CommenceRespawn);
			respawnButton.SetActive (false);
		}
	}

	void CommenceRespawn(){
		CmdRespawnOnServer ();
	}

	[Command]
	void CmdRespawnOnServer(){
		healthScript.ResetHealth ();
	}
}
