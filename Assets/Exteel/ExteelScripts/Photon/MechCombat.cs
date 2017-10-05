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

	// Boost variables
	private float fuelDrain = 1.0f;
	private float fuelGain = 1.0f;
	private float minFuelRequired = 75f;
	private float currentFuel;

	// Game variables
	public Score score;

	// Combat variables
	private bool isDead;
	private RaycastHit hit;
	private Weapon[] weaponScripts;
	private int weaponOffset = 0;
	private const int LEFT_HAND = 0;
	private const int RIGHT_HAND = 1;
	private float timeOfLastShotL;
	private float timeOfLastShotR;

	// Left
	private bool fireL = false;
	private bool shootingL = false;

	// Right
	private bool fireR = false;
	private bool shootingR = false;

	// Transforms
	private Transform shoulderL;
	private Transform shoulderR;
	private Transform head;
	private Transform[] Hands;

	// GameObjects
	private GameObject[] weapons;

	// HUD
	private Slider healthBar;
	private Slider fuelBar;
	private HUD hud;
	private Camera cam;

	// Components
	private BuildMech bm;

	void Start() {
		findGameManager();
		initMechStats();
		initTransforms();
		initGameObjects();
		initComponents();
		initCombatVariables();
		initHUD();
	}

	void initMechStats() {
		currentHP = MAX_HP;
		currentFuel = MAX_FUEL;
	}

	void initTransforms() {
		cam = transform.Find("Camera").gameObject.GetComponent<Camera>();
		head = transform.Find("CurrentMech/metarig/hips/spine/chest/fakeNeck/head");
		shoulderL = transform.Find("CurrentMech/metarig/hips/spine/chest/shoulder.L");
		shoulderR = transform.Find("CurrentMech/metarig/hips/spine/chest/shoulder.R");
		Hands = new Transform[2];
		Hands [0] = shoulderL.Find ("upper_arm.L/forearm.L/hand.L");
		Hands [1] = shoulderR.Find ("upper_arm.R/forearm.R/hand.R");
	}

	void initGameObjects() {
		hud = GameObject.Find("Canvas").GetComponent<HUD>();
		weapons = bm.weapons;
	}

	void initComponents() {
		bm = GetComponent<BuildMech> ();
	}

	void initCombatVariables() {
		weaponScripts = bm.weaponScripts;
		timeOfLastShotL = Time.time;
		timeOfLastShotR = Time.time;
	}

	void initHUD() {
		initHealthAndFuelBars();
	}

	void timeOfLastShot(int handPosition) {
		return handPosition == LEFT_HAND ? timeOfLastShotL : timeOfLastShotR;
	}

	Slider initHealthAndFuelBars() {
		Slider[] sliders = GameObject.Find("Canvas").GetComponentsInChildren<Slider>();
		if (sliders.Length > 0) {
			healthBar = sliders[0];
			healthBar.value = 1;

			if (sliders.Length > 1) {
				fuelBar = sliders [1];
				fuelBar.value = 1;
			}
		}
	}

	void FireRaycast(Vector3 start, Vector3 direction, int damage, float range) {
//		photonView.RPC("BulletTrace", PhotonTargets.All, start, direction);
		if (Physics.Raycast (start, direction, out hit, range, 1 << 8)){
			Debug.Log("Hit tag: " + hit.transform.tag);
			Debug.Log("Hit name: " + hit.transform.name);
			if (hit.transform.tag == "Player" || hit.transform.tag == "Drone"){
				// Apply damage
				hit.transform.GetComponent<PhotonView>().RPC("OnHit", PhotonTargets.All, damage, PhotonNetwork.playerName);
				Debug.Log("Damage: " + damage + ", Range: " + range);

				// Effects
				photonView.RPC("BulletImpact", PhotonTargets.All, hit.point, hit.normal);

				// UI
				if (hit.transform.gameObject.GetComponent<Combat> ().CurrentHP <= 0) {
					hud.ShowText (cam, hit.point, "Kill");
				} else {
					hud.ShowText (cam, hit.point, "Hit");
				}
			} else if (hit.transform.tag == "Shield") {
				hud.ShowText(cam, hit.point, "Defense");
			}
		}
	}

	[PunRPC]
	void BulletTrace(Vector3 start, Vector3 direction) {
		GameObject bulletTraceClone = Instantiate(bulletTrace, Hands[0].position, new Quaternion(direction.x, direction.y, direction.z, 1.0f)) as GameObject;
	}

	[PunRPC]
	void BulletImpact(Vector3 point, Vector3 rot) {
		GameObject bulletImpactClone = Instantiate(bulletImpact, point, new Quaternion(rot.x,rot.y,rot.z,1.0f)) as GameObject;
	}

	// Applies damage, and updates scoreboard + disables player on kill
	[PunRPC]
	public override void OnHit(int d, string shooter) {
		Debug.Log ("OnHit, isDead: " + isDead);
		// If already dead, do nothing
		if (isDead) {
			return;
		}
		// Apply damage
		currentHP -= d;
		Debug.Log ("HP: " + currentHP);

		// If fatal hit,
		if (currentHP <= 0) {
			isDead = true;
			// UI for shooter
			if (shooter == PhotonNetwork.playerName) {
				// Do we need this? Already being displayed in OnHit
				hud.ShowText(cam, transform.position, "Kill");
			}
			// Update scoreboard
			gm.RegisterKill(shooter, GetComponent<PhotonView>().name);

			DisablePlayer();
		}
	}

	// Disable MechController, Crosshair, Renderers, and set layer to 0
	[PunRPC]
	void DisablePlayer() {
		gameObject.layer = 0;
		GetComponent<MechController>().enabled = false;
		Crosshair ch = GetComponentInChildren<Crosshair>();
		ch.NoCrosshair();
		ch.enabled = false;
		Renderer[] renderers = GetComponentsInChildren<Renderer> ();
		foreach (Renderer renderer in renderers) {
			renderer.enabled = false;
		}
	}

	// Enable MechController, Crosshair, Renderers, set layer to player layer, move player to spawn position
	[PunRPC]
	void EnablePlayer() {
		transform.position = gm.SpawnPoints[0].position;
		gameObject.layer = 8;
		Renderer[] renderers = GetComponentsInChildren<Renderer> ();
		foreach (Renderer renderer in renderers) {
			renderer.enabled = true;
		}
		currentHP = MAX_HP;
		isDead = false;
		if (!photonView.isMine) return;

		// If this is me, enable MechController and Crosshair
		GetComponent<MechController>().enabled = true;
		Crosshair ch = GetComponentInChildren<Crosshair>();
		ch.enabled = true;

	}

	// Update is called once per frame
	void Update () {
		if (!photonView.isMine || gm.GameOver()) return;

		// Respawn
		if (isDead && Input.GetKeyDown(KeyCode.R)) {
			isDead = false;
			photonView.RPC ("EnablePlayer", PhotonTargets.All, null);
		}

		// Drain HP bar gradually
		if (isDead) {
			if (healthBar.value > 0) healthBar.value = healthBar.value -0.01f;
			return;
		}

		// Fix head to always look ahead
		head.LookAt(head.position + transform.forward * 10);

		// Animate left and right combat
		handleCombat(LEFT_HAND);
		handleCombat(RIGHT_HAND);

		// Switch weapons
		if (!isDead && Input.GetKeyDown (KeyCode.R)) {
			photonView.RPC ("SwitchWeapons", PhotonTargets.All, null);
		}

		updateHUD();
	}

	// Set animations and tweaks
	void LateUpdate() {
		handleAnimation(LEFT_HAND);
		handleAnimation(RIGHT_HAND);
	}

	void handleCombat(int handPosition) {
		if (!Input.GetKey (handPosition == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1)) {
			setIsFiring(handPosition, false);
			return;
		}

		if (usingRangedWeapon(handPosition)) {
			setIsFiring(handPosition, true);
			if (Time.time - timeOfLastShotR >= 1/bm.weaponScripts[weaponOffset + handPosition].Rate) {
				FireRaycast(camTransform.TransformPoint(0,0,0.5f), camTransform.forward, bm.weaponScripts[weaponOffset + handPosition].Damage, weaponScripts[weaponOffset + handPosition].Range);
				timeOfLastShotR = Time.time;
			}
		} else if (usingMeleeWeapon(handPosition)) {
			if (Time.time - timeOfLastShotR >= 1/bm.weaponScripts[weaponOffset + handPosition].Rate) {
				FireRaycast(camTransform.TransformPoint(0,0,0.5f), camTransform.forward, bm.weaponScripts[weaponOffset + handPosition].Damage, weaponScripts[weaponOffset + handPosition].Range);
				timeOfLastShotR = Time.time;
				setIsFiring(handPosition, true);
			} else {
				setIsFiring(handPosition, false);
			}
		}
	}

	void handleAnimation(int handPosition) {
		if (getIsFiring(handPosition)) {
			// Rotate arm to point to where you are looking (left hand is opposite)
			float x = camTransform.rotation.eulerAngles.x * handPosition == LEFT_HAND ? -1 : 1;

			// Name of animation, i.e. ShootR, SlashL, etc
			string animationString = animationString(handPosition);

			// Start animation
			animator.SetBool(animationString(handPosition), true);

			// Tweaks
			if (usingRangedWeapon(handPosition)) { // Shooting
				shoulderR.Rotate (0, x, 0);
			} else if (usingMeleeWeapon(handPosition)) { // Slashing
				
			}
		} else {
			animator.SetBool(animationString, false); // Stop animation
		}
	}

	void updateHUD() {
		// Update Health bar gradually
		healthBar.value = calculateSliderPercent(healthBar.value, currentHP/(float)MAX_HP);

		// Update Fuel bar gradually
		fuelBar.value = calculateSliderPercent(healthBar.value, currentFuel/(float)MAX_FUEL);
	}

	// Returns currentPercent + 0.01 if currentPercent < targetPercent, else - 0.01
	float calculateSliderPercent(float currentPercent, float targetPercent) {
		float err = 0.01f;
		if (Mathf.Abs(currentPercent - targetPercent) > err) {
			currentPercent = currentPercent + (currentPercent > targetPercent ? -0.01f : 0.01f);
		}
		return currentPercent;
	}

	// Switch weapons by increasing weaponOffset by 2
	// Each player holds 2 sets of weapons (4 total)
	// Switching weapons will switch from set 1 (weap 1 + 2) to set 2 (weap 3 + 4)
	[PunRPC]
	void SwitchWeapons() {
		for (int i = 0; i < weapons.Length; i++) {
			weapons[i].SetActive(!weapons[i].activeSelf);
		}
		weaponOffset = (weaponOffset + 2) % 4;
		Debug.Log("weapon offset is now: " + weaponOffset);
	}

	bool getIsFiring(int handPosition) {
		return handPosition == LEFT_HAND ? fireL : fireR;
	}

	void setIsFiring(int handPosition, bool isFiring) {
		if (handPosition == LEFT_HAND) {
			fireL = isFiring;
		} else {
			fireR = isFiring;
		}
	}

	bool usingRangedWeapon(int handPosition) {
		return weaponScripts[weaponOffset+handPosition].Animation == "Shoot";
	}

	bool usingMeleeWeapon(int handPosition) {
		return weaponScripts[weaponOffset+handPosition].Animation == "Slash";
	}

	string animationString(int handPosition) {
		return weaponScripts[weaponOffset + handPosition].Animation + (handPosition == LEFT_HAND ? "L" : "R");
	}

	// Public functions
	public void IncrementFuel() {
		currentFuel += fuelGain;
		if (currentFuel > MAX_FUEL) currentFuel = MAX_FUEL;
	}

	public bool DecrementFuel() {
		currentFuel -= fuelDrain;
	}

	public bool CanBoost() {
		return currentFuel >= minFuelRequired;
	}

	public bool FuelEmpty() {
		return currentFuel > 0;
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
}
