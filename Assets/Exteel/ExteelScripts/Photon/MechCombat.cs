using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MechCombat : Combat {

	[SerializeField] Transform camTransform;
	[SerializeField] Animator animator;
	[SerializeField] public GameObject[] bullets = new GameObject[4];
	[SerializeField] MechController mechController;
	[SerializeField] CharacterController CharacterController;
	// Boost variables
	private float fuelDrain = 1.0f;
	private float fuelGain = 1.0f;
	private float minFuelRequired = 25f;
	private float currentFuel;
	private float jumpPower = 80.0f;
	private float moveSpeed = 40.0f;
	private float boostSpeed = 80f;
	private float verticalBoostSpeed = 1f;
	private float maxVerticalBoostSpeed;

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

	public bool CanSlash = true;
	// Left
	private bool fireL = false;
	private bool shootingL = false;
	public int isLSlashPlaying = 0;
	// Right
	private bool fireR = false;
	private bool shootingR = false;
	public int isRSlashPlaying = 0;

	// Transforms
	private Transform shoulderL;
	private Transform shoulderR;
	private Transform head;
	private Transform[] Hands;

	// GameObjects
	private GameObject[] weapons;
	private GameObject Target;

	// HUD
	private Slider healthBar;
	private Slider fuelBar;
	private HUD hud;
	private Camera cam; //*

	// Components
	private BuildMech bm;
	private Crosshair crosshair;
	private SlashDetector slashDetector;

	void Start() {
		findGameManager();
		initMechStats();
		initTransforms();
		initGameObjects();
		initComponents();
		initCombatVariables();
		initHUD();
		initCrosshair();
		initCam ();
		initSlashDetector();
	}

	void initMechStats() {
		currentHP = MAX_HP;
		currentFuel = MAX_FUEL;
		maxVerticalBoostSpeed = boostSpeed / 4;
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
	}

	void initComponents() {
		bm = GetComponent<BuildMech>();
		weapons = bm.weapons;
	}

	void initCombatVariables() {
		weaponScripts = bm.weaponScripts;
		timeOfLastShotL = Time.time;
		timeOfLastShotR = Time.time;
	}

	void initHUD() {
		initHealthAndFuelBars();
	}

	void initCam(){
		//cam = transform.Find("Camera").gameObject.GetComponent<Camera>();
	}

	void initCrosshair(){
		crosshair = cam.GetComponent<Crosshair> ();
	}
	float timeOfLastShot(int handPosition) {
		return handPosition == LEFT_HAND ? timeOfLastShotL : timeOfLastShotR;
	}

	void initHealthAndFuelBars() {
		Slider[] sliders = GameObject.Find("Canvas").GetComponentsInChildren<Slider>();
		if (sliders.Length > 0) {
			healthBar = sliders[0];
			healthBar.value = 1;

			if (sliders.Length > 1) {
				fuelBar = sliders[1];
				fuelBar.value = 1;
			}
		}
	}
	void initSlashDetector(){
		slashDetector = GetComponentInChildren<SlashDetector> ();
	}
	void FireRaycast(Vector3 start, Vector3 direction, int damage, float range , int handPosition) {
		if (crosshair == null) {
			Debug.Log ("Fatal error : crosshair is null");
		}

		//Target : GameObject
		Target = null;
		Transform target = (handPosition == 0)? crosshair.getCurrentTargetL() :crosshair.getCurrentTargetR()  ;
		if( target != null){
			Debug.Log("Hit tag: " + target.tag);
			Debug.Log("Hit name: " + target.name);
			// start : camera mid point
			print ("Hit.transform.gameObject.name : " + target.gameObject.name);

			photonView.RPC("RegisterBulletTrace", PhotonTargets.All, handPosition, direction , target.gameObject.name);

			if (target.tag == "Player" || target.tag == "Drone"){
				//* Apply damage when the bullet collides the target ( using calculated traveling time )

				target.GetComponent<PhotonView>().RPC("OnHit", PhotonTargets.All, damage, PhotonNetwork.playerName);
				Debug.Log("Damage: " + damage + ", Range: " + range);

				//* UI : calculate the traveling time s.t. it shows text when the bullet collides the target
				if (target.gameObject.GetComponent<Combat>().CurrentHP() <= 0) {
					hud.ShowText (cam, target.position, "Kill");
				} else {
					hud.ShowText (cam, target.position, "Hit");
				}
			} else if (target.tag == "Shield") {
				hud.ShowText(cam, target.position, "Defense");
			}
		}else{
			photonView.RPC("RegisterBulletTrace", PhotonTargets.All, handPosition, direction, string.Empty);
		}

	}

	void SlashDetect(int damage){
		Target = null;
		Transform target;

		if(slashDetector == null){
			Debug.Log ("Fatal error : slashDetector is null");
		}

		if ((target = slashDetector.getCurrentTarget ()) != null) {
			print ("Slash hit : " + target.gameObject.name);

			photonView.RPC ("SlashOnTarget", PhotonTargets.All, target.gameObject.name);

			if (target.tag == "Player" || target.tag == "Drone") {
				//* Apply damage when the bullet collides the target ( using calculated traveling time )

				target.GetComponent<PhotonView> ().RPC ("OnHit", PhotonTargets.All, damage, PhotonNetwork.playerName);

				if (target.gameObject.GetComponent<Combat> ().CurrentHP () <= 0) {
					hud.ShowText (cam, target.position, "Kill");
				} else {
					hud.ShowText (cam, target.position, "Hit");
				}
			} else if (target.tag == "Shield") {
				hud.ShowText (cam, target.position, "Defense");
			}
		} else{
			print ("no current target.");
			mechController.SetSlashMoving(cam.transform.forward,5f);

		}
			
	}

	[PunRPC]
	void SlashOnTarget(string name) {
		Target = GameObject.Find (name);
		//**
	}

	[PunRPC]
	void RegisterBulletTrace(int handPosition, Vector3 direction , string name) {
		Target = GameObject.Find (name);

		StartCoroutine (InstantiateBulletTrace (handPosition, direction, name));
	}
	IEnumerator InstantiateBulletTrace(int handPosition, Vector3 direction , string name){
		int i;
		if (usingRCLWeapon (handPosition)) {
			GameObject bullet;
			bullet = Instantiate (bullets[weaponOffset], Hands [handPosition].position, Quaternion.LookRotation (direction)) as GameObject;

			RCLBulletTrace RCLbullet = bullet.GetComponent<RCLBulletTrace> ();
			RCLbullet.hud = hud;
			RCLbullet.cam = cam;
			RCLbullet.Shooter = gameObject;

		} else {
			for (i = 0; i < 3; i++) {
				GameObject bullet;
				bullet = Instantiate ((handPosition==0)? bullets[weaponOffset] : bullets[weaponOffset+1], Hands [handPosition].position, Quaternion.LookRotation (direction)) as GameObject;


				if (string.IsNullOrEmpty (name) || Target == null) {
					Debug.Log ("target can not be found => move directly. ");
				} else {
					bullet.transform.SetParent (Target.transform);
					Debug.Log ("target is found.");
				}
				yield return new WaitForSeconds (0.3f);
			}
		}
	}

	/*
	[PunRPC]
	void BulletTrace(int handPosition, Vector3 direction) {
		GameObject bulletTraceClone = Instantiate(bulletTrace, Hands[handPosition].position, Quaternion.LookRotation(direction) ) as GameObject;
	}
	*/

	/*
	[PunRPC]
	void BulletImpact(Vector3 point, Vector3 rot) {
		GameObject bulletImpactClone = Instantiate(bulletImpact, point, new Quaternion(rot.x,rot.y,rot.z,1.0f)) as GameObject;
	}
	*/

	// Applies damage, and updates scoreboard + disables player on kill
	[PunRPC]
	public override void OnHit(int d, string shooter) {
		Debug.Log ("OnHit, isDead: " + isDead);
		// If already dead, do nothing
		if (isDead) {
			return;
		}
		// Apply damage
		print ("called onhit by :"+gameObject);
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

			DisablePlayer();

			// Update scoreboard
			gm.RegisterKill(shooter, GetComponent<PhotonView>().name);
		}
	}

	// Disable MechController, Crosshair, Renderers, and set layer to 0
	[PunRPC]
	void DisablePlayer() {
		gameObject.layer = 0;
		Crosshair ch = GetComponentInChildren<Crosshair>();
		ch.NoCrosshair();
		GetComponent<MechController>().enabled = false;
		if(ch!=null)
			ch.enabled = false;
		Renderer[] renderers = GetComponentsInChildren<Renderer> ();
		foreach (Renderer renderer in renderers) {
			renderer.enabled = false;
		}
		transform.Find("Camera/Canvas/CrosshairImage").gameObject.SetActive(false);
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
		transform.Find("Camera/Canvas/CrosshairImage").gameObject.SetActive(true);

	}

	// Update is called once per frame
	void Update () {
		if (!photonView.isMine || gm.GameOver()) return;

		// Respawn
		if (isDead && Input.GetKeyDown(KeyCode.R)) {
			isDead = false;
			photonView.RPC("EnablePlayer", PhotonTargets.All, null);
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
			photonView.RPC("SwitchWeapons", PhotonTargets.All, null);
		}

		if(CharacterController.isGrounded == true){
			CanSlash = true;
		}

		updateHUD();
	}

	// Set animations and tweaks
	void LateUpdate() {
		handleAnimation(LEFT_HAND);
		handleAnimation(RIGHT_HAND);
	}

	void handleCombat(int handPosition) {
		bool usingRanged = usingRangedWeapon(handPosition);
		bool usingMelee = usingMeleeWeapon(handPosition);
		bool usingShield = usingShieldWeapon(handPosition);
		bool usingRCL = usingRCLWeapon (handPosition);

		/*
		if (usingRanged || usingShield) {
			if (!Input.GetKey(handPosition == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1)) {
				setIsFiring(handPosition, false);
				return;
			}
		} else if (usingMelee) {
			if (!Input.GetKeyDown(handPosition == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1)) {
				setIsFiring(handPosition, false);
				return;
			}
		}*/
		if (!Input.GetKey(handPosition == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1)) {
			setIsFiring(handPosition, false);
			return;
		}


		if (usingRanged) {
			if (Time.time - ((handPosition == 1)? timeOfLastShotR :timeOfLastShotL) >= 1/bm.weaponScripts[weaponOffset + handPosition].Rate) {
				setIsFiring(handPosition, true);
				FireRaycast(cam.transform.TransformPoint(0, 0, Crosshair.CAM_DISTANCE_TO_MECH), cam.transform.forward, bm.weaponScripts[weaponOffset + handPosition].Damage, weaponScripts[weaponOffset + handPosition].Range , handPosition);
				if(handPosition == 1){
					timeOfLastShotR = Time.time;
				}else {
					timeOfLastShotL = Time.time;
				}
			}
		} else if (usingMelee) {
			if (Time.time -((handPosition == 1)? timeOfLastShotR :timeOfLastShotL) >= 1/bm.weaponScripts[weaponOffset + handPosition].Rate) {
				setIsFiring(handPosition, true);

				//SlashL2 & L3 is set to false by animation calling Combo.cs -> MechCombat.cs
				//* maybe put it in handleAnimation() ? 

				if(handPosition == 0){
					if (CharacterController.isGrounded == false)
						return;
					
					timeOfLastShotL = Time.time;
					if (usingMeleeWeapon (1))//both melee weapons should not be usable
						timeOfLastShotR = timeOfLastShotL;
					if (isLSlashPlaying == 1) {						
						if (animator.GetBool ("SlashL2") == false) {
							SlashDetect (bm.weaponScripts [weaponOffset + handPosition].Damage); // temporary put here
							animator.SetBool ("SlashL2", true);
						} else if(animator.GetBool("SlashL3") == false){
							SlashDetect (bm.weaponScripts [weaponOffset + handPosition].Damage);
							animator.SetBool ("SlashL3", true);
						}
					}
				}else if(handPosition == 1){
					if (CharacterController.isGrounded == false)
						return;
					
					timeOfLastShotR = Time.time;
					if (usingMeleeWeapon (0))
						timeOfLastShotL = timeOfLastShotR;
					if (isRSlashPlaying == 1) {
						if (animator.GetBool ("SlashR2") == false) {
							SlashDetect (bm.weaponScripts [weaponOffset + handPosition].Damage); // temp.
							animator.SetBool ("SlashR2", true);
						} else if(animator.GetBool("SlashR3") == false){
							SlashDetect (bm.weaponScripts [weaponOffset + handPosition].Damage);
							animator.SetBool ("SlashR3", true);
						}
					}
				}
			}
		} else if (usingShield) {
			setIsFiring(handPosition, true);
		}else if(usingRCL){
			if (Time.time - timeOfLastShotL >= 1/bm.weaponScripts[weaponOffset + handPosition].Rate) {
				setIsFiring(handPosition, true);

				//**Start Position
				FireRaycast(cam.transform.TransformPoint(0, 0, Crosshair.CAM_DISTANCE_TO_MECH), cam.transform.forward, bm.weaponScripts[weaponOffset + handPosition].Damage, weaponScripts[weaponOffset + handPosition].Range , handPosition);
				timeOfLastShotL = Time.time;
			}
		}
	}

	void handleAnimation(int handPosition) {
		// Name of animation, i.e. ShootR, SlashL, etc
		string animationStr = animationString(handPosition);

		if (getIsFiring(handPosition)) {
			// Rotate arm to point to where you are looking (left hand is opposite)
			float x = cam.transform.rotation.eulerAngles.x * (handPosition == LEFT_HAND ? -1 : 1);

			// Start animation

			// Tweaks
			if (usingRangedWeapon(handPosition)) { // Shooting
				animator.SetBool(animationStr, true);
				shoulderR.Rotate (0, x, 0);
			} else if (usingMeleeWeapon(handPosition)) { // Slashing
				if(animator.GetBool(animationStr) == false && ((handPosition == 1)? isRSlashPlaying : isLSlashPlaying) == 0){

					//if already melee attack in air => CanSlash is false
					if(CanSlash == true){
						CanSlash = false;  //This is in case not jumping but slash to the air
						//will set to true if on ground ( in update )
						animator.SetBool(animationStr, true);
						SlashDetect (bm.weaponScripts [weaponOffset + handPosition].Damage); // temporary put here
					}
				}
			}else if(usingShieldWeapon(handPosition)){
				animator.SetBool(animationStr, true);
				shoulderR.Rotate (0, x, 0);
			}else if(usingRCLWeapon(handPosition)){
				animator.SetBool(animationStr, true);
			}
		} else {
			if (!usingMeleeWeapon(handPosition) && !usingEmptyWeapon(handPosition)) {
				animator.SetBool(animationStr, false); // Stop animation
			}
		}
	}

	void updateHUD() {
		// Update Health bar gradually
		healthBar.value = calculateSliderPercent(healthBar.value, currentHP/(float)MAX_HP);

		// Update Fuel bar gradually
		fuelBar.value = calculateSliderPercent(fuelBar.value, currentFuel/(float)MAX_FUEL);
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
		// Stop current attacks
		setIsFiring(LEFT_HAND, false);
		setIsFiring(RIGHT_HAND, false);

		// Stop current animations
		animator.SetBool(animationString(LEFT_HAND), false);
		animator.SetBool(animationString(RIGHT_HAND), false);

		//Play switch weapon animation

		// Switch weapons by toggling each weapon's activeSelf
		for (int i = 0; i < weapons.Length; i++) {
			weapons[i].SetActive(!weapons[i].activeSelf);
		}

		// Change weaponOffset
		weaponOffset = (weaponOffset + 2) % 4;

		//Check crosshair
		crosshair.updateCrosshair (weaponOffset,weaponOffset+1);
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

	bool usingShieldWeapon(int handPosition) {
		return weaponScripts[weaponOffset+handPosition].Animation == "Block";
	}

	bool usingRCLWeapon(int handPosition) {
		return weaponScripts[weaponOffset+handPosition].Animation == "ShootRCL";
	}

	bool usingEmptyWeapon(int handPosition){
		return weaponScripts[weaponOffset+handPosition].Animation == "";
	}

	string animationString(int handPosition) {
		if(!usingRCLWeapon(handPosition))
			return weaponScripts[weaponOffset + handPosition].Animation + (handPosition == LEFT_HAND ? "L" : "R");
		else return "ShootRCL";
	}

	public void updataBullet(){
		
	}

	// Public functions
	public void IncrementFuel() {
		currentFuel += fuelGain;
		if (currentFuel > MAX_FUEL) currentFuel = MAX_FUEL;
	}

	public void DecrementFuel() {
		currentFuel -= fuelDrain;
		if (currentFuel < 0)
			currentFuel = 0;
	}

	public bool EnoughFuelToBoost() {
		return currentFuel >= minFuelRequired;
	}

	public bool FuelEmpty() {
		return currentFuel <= 0;
	}

	public float VerticalBoostSpeed() {
		return verticalBoostSpeed;
	}

	public float MoveSpeed() {
		return moveSpeed;
	}

	public float BoostSpeed() {
		return boostSpeed;
	}

	public float JumpPower() {
		return jumpPower;
	}

	public float MaxVerticalBoostSpeed() {
		return maxVerticalBoostSpeed;
	}

	public void SetIsLSlashPlaying(int isPlaying){
		isLSlashPlaying = isPlaying;
		print ("received isPlaying : " + isPlaying); //has received
	}
	public void SetSlashL2ToFalse(){
		animator.SetBool ("SlashL2", false);
	}
	public void SetSlashL3ToFalse(){
		animator.SetBool ("SlashL3", false);
	}
	public void SetIsRSlashPlaying(int isPlaying){
		isRSlashPlaying = isPlaying;
		print ("received isPlaying : " + isPlaying); //has received
	}
	public void SetSlashR2ToFalse(){
		animator.SetBool ("SlashR2", false);
	}
	public void SetSlashR3ToFalse(){
		animator.SetBool ("SlashR3", false);
	}
	public void SetSlashL1ToFalse(){
		animator.SetBool ("SlashL", false);
	}
	public void SetSlashR1ToFalse(){
		animator.SetBool ("SlashR", false);
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
