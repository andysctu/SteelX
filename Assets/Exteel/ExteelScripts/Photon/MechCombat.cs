using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class MechCombat : Photon.MonoBehaviour {

	[SerializeField] Transform camTransform;
	public int MaxHP = 100;
	public int CurrentHP;

	private bool isDead;

	public Score score;

	private RaycastHit hit;
	private bool fireL = false;
	private bool shootingL = false;

	private bool fireR = false;
	private bool shootingR = false;

	private Transform shoulderL;
	private Transform shoulderR;

	private GameManager gm;
	private GameObject[] weapons;
	private Transform[] Hands;
	private Weapon[] weaponScripts;

	private int weaponOffset = 0;

	private BuildMech bm;

	void Start() {
		CurrentHP = MaxHP;
		findGameManager();
		shoulderL = transform.FindChild("CurrentMech/metarig/hips/spine/chest/shoulder.L");
		shoulderR = transform.FindChild("CurrentMech/metarig/hips/spine/chest/shoulder.R");

		Hands = new Transform[2];
		Hands [0] = shoulderL.FindChild ("upper_arm.L/forearm.L/hand.L");
		Hands [1] = shoulderR.FindChild ("upper_arm.R/forearm.R/hand.R");

		bm = GetComponent<BuildMech> ();
		weapons = bm.weapons;
		weaponScripts = bm.weaponScripts;
	}
		
	void FireRaycast(Vector3 start, Vector3 direction, int damage, float range) {
		if (Physics.Raycast (start, direction, out hit, range, 1 << 8)){
			Debug.Log ("Hit tag: " + hit.transform.tag);
			Debug.Log("Hit name: " + hit.transform.name);
			if (hit.transform.tag == "Player"){
				Debug.Log("Damage: " + damage + ", Range: " + range);
				hit.transform.GetComponent<PhotonView>().RPC("OnHit", PhotonTargets.All, damage, PhotonNetwork.playerName);
			} else if (hit.transform.tag == "Drone"){

			}
		}
	}

	[PunRPC]
	void OnHit(int d, string shooter) {
		Debug.Log ("Got shot");
		Debug.Log ("isDead: " + isDead);
		if (isDead) return;
		CurrentHP -= d;
		Debug.Log ("HP: " + CurrentHP);
		if (CurrentHP <= 0) {
			isDead = true;
			Debug.Log ("Dead");
			gm.RegisterKill(shooter, GetComponent<PhotonView>().name);
			DisablePlayer ();
		}
	}

	[PunRPC]
	void DisablePlayer() {
		Debug.Log ("DisablePlayer()");
		gameObject.layer = 0;
		GetComponent<MechController>().enabled = false;
		GetComponentInChildren<Crosshair>().enabled = false;
		Renderer[] renderers = GetComponentsInChildren<Renderer> ();
		foreach (Renderer renderer in renderers) {
			renderer.enabled = false;
		}
	}

	[PunRPC]
	void EnablePlayer() {
		gameObject.layer = 8;
		Renderer[] renderers = GetComponentsInChildren<Renderer> ();
		foreach (Renderer renderer in renderers) {
			renderer.enabled = true;
		}
		CurrentHP = MaxHP;
		isDead = false;
		if (!photonView.isMine) return;
		GetComponent<MechController>().enabled = true;
		GetComponentInChildren<Crosshair>().enabled = true;
	}

	// Update is called once per frame
	void Update () {
//		if (!isLocalPlayer || gm.GameOver()) return;
		if (!photonView.isMine) return;
		if (isDead && Input.GetKeyDown(KeyCode.R)) {
			isDead = false;
			photonView.RPC ("EnablePlayer", PhotonTargets.All, null);
		}

		if (isDead) return;

		if (Input.GetKey(KeyCode.Mouse0)){
			Debug.Log ("Fire");
			FireRaycast(camTransform.TransformPoint(0,0,0.5f), camTransform.forward, bm.weaponScripts[weaponOffset].Damage, weaponScripts[weaponOffset].Range);
			fireL = true;
		} else {
			fireL = false;
		}

		if (Input.GetKey(KeyCode.Mouse1)){
			FireRaycast(camTransform.TransformPoint(0,0,0.5f), camTransform.forward, bm.weaponScripts[weaponOffset+1].Damage, weaponScripts[weaponOffset+1].Range);
			fireR = true;
		} else {
			fireR = false;
		}

		if (!isDead && Input.GetKeyDown (KeyCode.R)) {
			photonView.RPC ("SwitchWeapons", PhotonTargets.All, null);
		}

//		if (CurrentHP <= 0 && !isDead) {
//			isDead = true;
//			photonView.RPC ("DisablePlayer", PhotonTargets.All, null);
//		}
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
		shoulderL.Rotate(0,90,0);
	}

	void stopShootAnimationL() {
		shoulderL.Rotate(0,-90,0);
	}

	void playShootAnimationR() {
		shoulderR.Rotate(0,-90,0);
	}

	void stopShootAnimationR() {
		shoulderR.Rotate(0,90,0);
	}
		
	[PunRPC]
	void SwitchWeapons() {
		for (int i = 0; i < weapons.Length; i++) {
			weapons[i].SetActive(!weapons[i].activeSelf);
		}
	}

//
//	// Update is called once per frame
//	void Update () {
//		if (!isLocalPlayer || gm.GameOver()) return;
//		if (Input.GetKey(KeyCode.Mouse0)){
//			CmdFireRaycast(camTransform.TransformPoint(0,0,0.5f), camTransform.forward, weaponScripts[weaponOffset].Damage, weaponScripts[weaponOffset].Range);
//			fireL = true;
//		} else {
//			fireL = false;
//		}
//
//		if (Input.GetKey(KeyCode.Mouse1)){
//			CmdFireRaycast(camTransform.TransformPoint(0,0,0.5f), camTransform.forward, weaponScripts[weaponOffset+1].Damage, weaponScripts[weaponOffset+1].Range);
//			fireR = true;
//		} else {
//			fireR = false;
//		}
//
//		if (Input.GetKeyDown (KeyCode.R)) {
//			switchWeapons ();
//		}
//
//		if (isDead && Input.GetKeyDown(KeyCode.R)) {
//			CmdEnablePlayer();
//		}
//	}
//
//	[Server]
//	public void RegisterKill(uint shooterId, uint victimId){
//		int kills = gm.playerScores[shooterId].IncrKill();
//		int deaths = gm.playerScores[victimId].IncrDeaths();
//		Debug.Log(string.Format("Server: Registering a kill {0}, {1}, {2}, {3} ", shooterId, victimId, kills, deaths));
//		if (isServer) {
//			RpcUpdateScore(shooterId, victimId, kills, deaths);
//		}
//	}
//
//	[Server]
//	public void EndGame() {
//		Debug.Log("Ending game");
//		RpcDisablePlayer();
//		foreach (KeyValuePair<GameObject, Data> entry in gm.playerInfo) {
//			entry.Key.GetComponent<MechCombat>().RpcDisablePlayer();
//		}
//	}
//		
//	[ClientRpc]
//	void RpcUpdateScore(uint shooterId, uint victimId, int kills, int deaths){
//		Debug.Log( string.Format("Client: Updating score {0}, {1}, {2}, {3} ", shooterId, victimId, kills, deaths)); 
//		Score shooterScore = gm.playerScores[shooterId];
//		Score victimScore = gm.playerScores[victimId];
//		Score newShooterScore = new Score();
//		Score newVictimScore = new Score();
//		newShooterScore.Kills = kills;
//		newShooterScore.Deaths = shooterScore.Deaths;
//		newVictimScore.Deaths = deaths;
//		newVictimScore.Kills = victimScore.Kills;
//
//		if (kills > gm.CurrentMaxKills) gm.CurrentMaxKills = kills;
//		gm.playerScores[shooterId] = newShooterScore;
//		gm.playerScores[victimId] = newVictimScore;
//	}
//
	private void findGameManager() {
		if (gm == null) {
			gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		}
	}
}
