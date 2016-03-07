using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MechCombat : NetworkBehaviour {

	public Transform camTransform;
	public float range = Mathf.Infinity;
	public int damage = 25;

	[SyncVar]
	public float MaxHP = 100.0f;
	[SyncVar]
	public float CurrentHP;

	[SyncVar]
	private bool isDead;

	[SyncVar]
	public Score score;


	private RaycastHit hit;
	private GameManager gm;


	void Start() {
		CurrentHP = MaxHP;
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
	}
	
	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer) return;
		if (Input.GetKeyDown(KeyCode.Mouse0)){
			CmdFireRaycast(camTransform.TransformPoint(0,0,0.5f), camTransform.forward);
		}

		if (isDead && Input.GetKeyDown(KeyCode.R)) {
			CmdEnablePlayer();
		}
	}

	[Server]
	void OnHit(uint shooterId, float d) {
		if (isDead) return;
		CurrentHP -= d;
		if (CurrentHP <= 0) {
			RpcDisablePlayer();
			isDead = true;
			gm.RegisterKill(shooterId, gameObject.GetComponent<NetworkIdentity>().netId.Value);
		}
	}

	[ClientRpc]
	void RpcDisablePlayer() {
		gameObject.layer = 0;
		GetComponent<NewMechController>().enabled = false;
		GetComponentInChildren<Crosshair>().enabled = false;
		Renderer[] renderers = GetComponentsInChildren<Renderer> ();
		foreach (Renderer renderer in renderers) {
			renderer.enabled = false;
		}
	}

	[ClientRpc]
	void RpcEnablePlayer() {
		gameObject.layer = 8;
		Renderer[] renderers = GetComponentsInChildren<Renderer> ();
		foreach (Renderer renderer in renderers) {
			renderer.enabled = true;
		}
		if (!isLocalPlayer) return;
		GetComponent<NewMechController>().enabled = true;
		GetComponentInChildren<Crosshair>().enabled = true;
	}

	[Command]
	void CmdEnablePlayer() {
		isDead = false;
		CurrentHP = MaxHP;
		RpcEnablePlayer();
	}

	[Command]
	void CmdFireRaycast(Vector3 start, Vector3 direction){
		Debug.Log("fired");
		if (Physics.Raycast (start, direction, out hit, range)){
			Debug.Log (hit.transform.tag);
			if (hit.transform.tag == "Player"){
				string uIdentity = hit.transform.name;
				Debug.Log (uIdentity + " was shot");
				hit.transform.gameObject.GetComponent<MechCombat>().OnHit(gameObject.GetComponent<NetworkIdentity>().netId.Value, damage);
			} else if (hit.transform.tag == "Drone"){
				string uIdentity = hit.transform.name;
				Debug.Log("Drone was shot");
			}
		}
	}
}
