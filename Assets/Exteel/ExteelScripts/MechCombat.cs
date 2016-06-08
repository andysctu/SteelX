using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class MechCombat : NetworkBehaviour {

	public Transform camTransform;
	public float range = Mathf.Infinity;
	public int damage = 25;
//	public Animator animator;

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

	private bool fireL = false;
	private bool shootingL = false;

	private bool fireR = false;
	private bool shootingR = false;

	private Transform shoulderL;
	private Transform shoulderR;

	void Start() {
		CurrentHP = MaxHP;
		findGameManager();
		shoulderL = transform.FindChild("CurrentMech/metarig/hips/spine/chest/shoulder.L");
		shoulderR = transform.FindChild("CurrentMech/metarig/hips/spine/chest/shoulder.R");
	}
	
	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer) return;
		if (Input.GetKey(KeyCode.Mouse0) && !gm.GameOver()){
			CmdFireRaycast(camTransform.TransformPoint(0,0,0.5f), camTransform.forward);
			fireL = true;
		} else {
			fireL = false;
		}

		if (Input.GetKey(KeyCode.Mouse1) && !gm.GameOver()){
			CmdFireRaycast(camTransform.TransformPoint(0,0,0.5f), camTransform.forward);
			fireR = true;
		} else {
			fireR = false;
		}

		if (isDead && Input.GetKeyDown(KeyCode.R) && !gm.GameOver()) {
			CmdEnablePlayer();
		}
	}

	void LateUpdate() {
		if (fireL) {
			playShootAnimationL();
			shootingL = true;
		} else if (shootingL) {
			stopShootAnimationL();
			shootingL = false;
		}

		if (fireR) {
			playShootAnimationR();
			shootingR = true;
		} else if (shootingR) {
			stopShootAnimationR();
			shootingR = false;
		}
	}

	void playShootAnimationL() {
		if (shoulderL == null) {
			Debug.Log("Could not find shoulderL");
		}
		Debug.Log("Rotating");
		shoulderL.Rotate(0,90,0);
	}

	void stopShootAnimationL() {
		Debug.Log("Rotating back");
		shoulderL.Rotate(0,-90,0);
	}

	void playShootAnimationR() {
		if (shoulderR == null) {
			Debug.Log("Could not find shoulderL");
		}
		Debug.Log("Rotating");
		shoulderR.Rotate(0,-90,0);
	}

	void stopShootAnimationR() {
		Debug.Log("Rotating back");
		shoulderR.Rotate(0,90,0);
	}

	[Server]
	void OnHit(uint shooterId, float d) {
		if (isDead) return;
		CurrentHP -= d;
		if (CurrentHP <= 0) {
			RpcDisablePlayer();
			isDead = true;
			RegisterKill(shooterId, gameObject.GetComponent<NetworkIdentity>().netId.Value);
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
		Debug.Log("Shot fired");
		if (Physics.Raycast (start, direction, out hit, range, 1 << 8)){
			Debug.Log ("Hit tag: " + hit.transform.tag);
			Debug.Log("Hit name: " + hit.transform.name);
//			Debug.Log("Parent name: " + hit.transform.parent.name);
//			Debug.Log("Parent parent name: " + hit.transform.parent.parent.name);

			if (hit.transform.tag == "Player"){
				hit.transform.GetComponent<MechCombat>().OnHit(gameObject.GetComponent<NetworkIdentity>().netId.Value, damage);
			} else if (hit.transform.tag == "Drone"){

			}
		}
	}



	[Server]
	public void RegisterKill(uint shooterId, uint victimId){
		int kills = gm.playerScores[shooterId].IncrKill();
		int deaths = gm.playerScores[victimId].IncrDeaths();
		Debug.Log(string.Format("Server: Registering a kill {0}, {1}, {2}, {3} ", shooterId, victimId, kills, deaths));
		if (isServer) {
			RpcUpdateScore(shooterId, victimId, kills, deaths);
		}
	}

	[Server]
	public void EndGame() {
		Debug.Log("Ending game");
		RpcDisablePlayer();
		foreach (KeyValuePair<GameObject, Data> entry in gm.playerInfo) {
			entry.Key.GetComponent<MechCombat>().RpcDisablePlayer();
		}
	}
		
	[ClientRpc]
	void RpcUpdateScore(uint shooterId, uint victimId, int kills, int deaths){
		Debug.Log( string.Format("Client: Updating score {0}, {1}, {2}, {3} ", shooterId, victimId, kills, deaths)); 
		Score shooterScore = gm.playerScores[shooterId];
		Score victimScore = gm.playerScores[victimId];
		Score newShooterScore = new Score();
		Score newVictimScore = new Score();
		newShooterScore.Kills = kills;
		newShooterScore.Deaths = shooterScore.Deaths;
		newVictimScore.Deaths = deaths;
		newVictimScore.Kills = victimScore.Kills;

		if (kills > gm.CurrentMaxKills) gm.CurrentMaxKills = kills;
		gm.playerScores[shooterId] = newShooterScore;
		gm.playerScores[victimId] = newVictimScore;
	}

	private void findGameManager() {
		if (gm == null) {
			gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		}
	}
}
