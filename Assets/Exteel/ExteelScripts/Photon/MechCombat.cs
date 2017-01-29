using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MechCombat : Combat {

	[SerializeField] Transform camTransform;
	[SerializeField] Animator animator;
	[SerializeField] GameObject bulletTrace;
	[SerializeField] GameObject bulletImpact;

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

	private Slider healthBar;

	// Control rate of fire
	float timeOfLastShotL;
	float timeOfLastShotR;

	private HUD hud;
	private Camera cam;

	void Start() {
		MaxHP = 100;
		CurrentHP = MaxHP;
		findGameManager();

		hud = GameObject.Find("Canvas").GetComponent<HUD>();
		cam = transform.FindChild("Camera").gameObject.GetComponent<Camera>();

		shoulderL = transform.FindChild("CurrentMech/metarig/hips/spine/chest/shoulder.L");
		shoulderR = transform.FindChild("CurrentMech/metarig/hips/spine/chest/shoulder.R");

		Slider[] sliders = GameObject.Find("Canvas").GetComponentsInChildren<Slider>();
		if (sliders.Length > 0) {
			healthBar = sliders[0];
			healthBar.value = 1;
		} else {
			Debug.Log("Health bar null");
		}

		Hands = new Transform[2];
		Hands [0] = shoulderL.FindChild ("upper_arm.L/forearm.L/hand.L");
		Hands [1] = shoulderR.FindChild ("upper_arm.R/forearm.R/hand.R");

		bm = GetComponent<BuildMech> ();
		weapons = bm.weapons;
		weaponScripts = bm.weaponScripts;

		timeOfLastShotL = Time.time;
		timeOfLastShotR = Time.time;
	}
		
	void FireRaycast(Vector3 start, Vector3 direction, int damage, float range) {
//		photonView.RPC("BulletTrace", PhotonTargets.All, start, direction);
		if (Physics.Raycast (start, direction, out hit, range, 1 << 8)){
			Debug.Log ("Hit tag: " + hit.transform.tag);
			Debug.Log("Hit name: " + hit.transform.name);
			hit.transform.GetComponent<PhotonView>().RPC("OnHit", PhotonTargets.All, damage, PhotonNetwork.playerName);
			if (hit.transform.tag == "Player" || hit.transform.tag == "Drone"){
				Debug.Log("Damage: " + damage + ", Range: " + range);
				photonView.RPC("BulletImpact", PhotonTargets.All, hit.point, hit.normal);
				if (hit.transform.gameObject.GetComponent<Combat>().CurrentHP <= 0) hud.ShowText(cam, hit.point, "Kill");
				else hud.ShowText(cam, hit.point, "Hit");
			}
		}
	}
		
//	public void BulletTraceEvent() {
//		photonView.RPC("BulletTraceRPC", PhotonTargets.All);
//	}
//
//	[PunRPC]
//	void BulletTraceRPC() {
//		Camera cam = transform.FindChild("Camera").gameObject.GetComponent<Camera>();
//		Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
//		Vector3 diff = worldPoint - Hands[0].position;
//		GameObject bulletTraceClone = Instantiate(bulletTrace, Hands[0].position, new Quaternion(diff.x, diff.y, diff.z, 1.0f)) as GameObject;
//	}


	[PunRPC]
	void BulletTrace(Vector3 start, Vector3 direction) {
		GameObject bulletTraceClone = Instantiate(bulletTrace, Hands[0].position, new Quaternion(direction.x, direction.y, direction.z, 1.0f)) as GameObject;
	}

	[PunRPC]
	void BulletImpact(Vector3 point, Vector3 rot) {
		GameObject bulletImpactClone = Instantiate(bulletImpact, point, new Quaternion(rot.x,rot.y,rot.z,1.0f)) as GameObject;
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
			if (shooter == PhotonNetwork.playerName) hud.ShowText(cam, transform.position, "Kill");
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
		transform.position = gm.SpawnPoints [0].position;
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
		if (!photonView.isMine || gm.GameOver()) return;
		if (isDead && Input.GetKeyDown(KeyCode.R)) {
			isDead = false;
			photonView.RPC ("EnablePlayer", PhotonTargets.All, null);
		}

		if (isDead) {
			if (healthBar.value > 0) healthBar.value = healthBar.value -0.01f;
			return;
		}
			
		if (Input.GetKey(KeyCode.Mouse0)){
			fireL = true;
		
			if (Time.time - timeOfLastShotL >= 1/bm.weaponScripts[weaponOffset].Rate) {
				FireRaycast(camTransform.TransformPoint(0,0,0.5f), camTransform.forward, bm.weaponScripts[weaponOffset].Damage, weaponScripts[weaponOffset].Range);
				timeOfLastShotL = Time.time;
			}

		} else {
			fireL = false;
		}

		if (Input.GetKey(KeyCode.Mouse1)){
			fireR = true;

			if (Time.time - timeOfLastShotR >= 1/bm.weaponScripts[weaponOffset + 1].Rate) {
				FireRaycast(camTransform.TransformPoint(0,0,0.5f), camTransform.forward, bm.weaponScripts[weaponOffset + 1].Damage, weaponScripts[weaponOffset + 1].Range);
				timeOfLastShotR = Time.time;
			}

		} else {
			fireR = false;
		}

		if (!isDead && Input.GetKeyDown (KeyCode.R)) {
			photonView.RPC ("SwitchWeapons", PhotonTargets.All, null);
		}

		// Update Health bar
		if (healthBar == null) {
			Slider[] sliders = GameObject.Find("Canvas/HUDPanel/HUD").GetComponentsInChildren<Slider>();
			if (sliders.Length > 0) {
				healthBar = sliders[0];
			}
		} else {
			float currentPercent = healthBar.value;
			float targetPercent = CurrentHP/(float)MaxHP;
			float err = 0.01f;
			if (Mathf.Abs(currentPercent - targetPercent) > err) {
				currentPercent = currentPercent + (currentPercent > targetPercent ? -0.01f : 0.01f);
			}

			healthBar.value = currentPercent;
		}
	}

	void LateUpdate() {
		float x = camTransform.rotation.eulerAngles.x;
		if (fireL) {
			animator.SetBool(weaponScripts[0].Animation + "L", true);
			if (weaponScripts[0].Animation == "Shoot") shoulderL.Rotate(0, -x, 0);
		} else {
			animator.SetBool(weaponScripts[0].Animation + "L", false);
		}

		if (fireR) {
			animator.SetBool(weaponScripts[1].Animation + "R", true);
			if (weaponScripts[1].Animation == "Shoot") shoulderR.Rotate(0, x, 0);
		} else {
			animator.SetBool(weaponScripts[1].Animation + "R", false);
		}
	}
		
	[PunRPC]
	void SwitchWeapons() {
		for (int i = 0; i < weapons.Length; i++) {
			weapons[i].SetActive(!weapons[i].activeSelf);
			weaponOffset = (weaponOffset + 2) % 2;
		}
	}
		
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
