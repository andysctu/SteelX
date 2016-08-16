using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class MechCombat : NetworkBehaviour {

	public Transform camTransform;
//	public float range = Mathf.Infinity;
//	public int damage = 25;
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

	public GameObject boostFlame;

	void Start() {
		CurrentHP = MaxHP;
		findGameManager();
		shoulderL = transform.FindChild("CurrentMech/metarig/hips/spine/chest/shoulder.L");
		shoulderR = transform.FindChild("CurrentMech/metarig/hips/spine/chest/shoulder.R");

		Hands = new Transform[2];
		Hands [0] = shoulderL.FindChild ("upper_arm.L/forearm.L/hand.L");
		Hands [1] = shoulderR.FindChild ("upper_arm.R/forearm.R/hand.R");
	}
		
	public void Arm (string[] weaponNames) {
		weapons = new GameObject[4];
		weaponScripts = new Weapon[4];
		for (int i = 0; i < weaponNames.Length; i++) {
			weapons [i] = Instantiate (Resources.Load (weaponNames [i], typeof(GameObject)) as GameObject, Hands [i%2].position, Quaternion.identity) as GameObject;
			weapons [i].transform.parent = Hands [i % 2];

			switch (weaponNames[i]) {
				case "APS403": {
					weaponScripts[i] = new APS403();
					Debug.Log("Added APS403");
					break;
				}
				case "SHL009": {
						weaponScripts[i] = new SHL009();
						Debug.Log("Added SHL009");
						break;
				}
			}
		}

		weaponOffset = 0;
		weapons [2].SetActive (false);
		weapons [3].SetActive (false);
//		gameObject.AddComponent<APS403>().SetCam(camTransform);
	}

	private void switchWeapons() {
		if (isServer) {
			RpcSwitchWeapons();
		} else {
			CmdSwitchWeapons();
		}
	}

	[Command]
	private void CmdSwitchWeapons() {
//		Debug.Log("Cmd: isServer: " + isServer + ", isClient: " + isClient);
//		for (int i = 0; i < weapons.Length; i++) {
//			weapons[i].SetActive(!weapons[i].activeSelf);
//		}
		RpcSwitchWeapons ();
	}

	[ClientRpc]
	private void RpcSwitchWeapons() {
//		Debug.Log("Rpc: isServer: " + isServer + ", isClient: " + isClient);
		for (int i = 0; i < weapons.Length; i++) {
			weapons[i].SetActive(!weapons[i].activeSelf);
		}
		weaponOffset = (weaponOffset + 2) % 4;
	}

	public void SetBoost(bool boost) {
		if (isServer) {
//			boostFlame.SetActive(boost);
			RpcSetBoost(boost);
		} else {
			CmdSetBoost(boost);
		}
	}

	[Command]
	public void CmdSetBoost(bool boost) {
		RpcSetBoost(boost);
	}

	[ClientRpc]
	private void RpcSetBoost(bool boost){
		boostFlame.SetActive(boost);
	}

	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer || gm.GameOver()) return;
		if (Input.GetKey(KeyCode.Mouse0)){
			CmdFireRaycast(camTransform.TransformPoint(0,0,0.5f), camTransform.forward, weaponScripts[weaponOffset].Damage, weaponScripts[weaponOffset].Range);
			fireL = true;
		} else {
			fireL = false;
		}

		if (Input.GetKey(KeyCode.Mouse1)){
			CmdFireRaycast(camTransform.TransformPoint(0,0,0.5f), camTransform.forward, weaponScripts[weaponOffset+1].Damage, weaponScripts[weaponOffset+1].Range);
			fireR = true;
		} else {
			fireR = false;
		}

		if (Input.GetKeyDown (KeyCode.R)) {
			switchWeapons ();
		}

		if (isDead && Input.GetKeyDown(KeyCode.R)) {
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
		GetComponent<MechController>().enabled = false;
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
		GetComponent<MechController>().enabled = true;
		GetComponentInChildren<Crosshair>().enabled = true;
	}

	[Command]
	void CmdEnablePlayer() {
		isDead = false;
		CurrentHP = MaxHP;
		RpcEnablePlayer();
	}

	[Command]
	public void CmdFireRaycast(Vector3 start, Vector3 direction, int damage, float range){
		if (Physics.Raycast (start, direction, out hit, range, 1 << 8)){
			Debug.Log ("Hit tag: " + hit.transform.tag);
			Debug.Log("Hit name: " + hit.transform.name);
//			Debug.Log("Parent name: " + hit.transform.parent.name);
//			Debug.Log("Parent parent name: " + hit.transform.parent.parent.name);
			Debug.Log("Damage: " + damage + ", Range: " + range);
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
