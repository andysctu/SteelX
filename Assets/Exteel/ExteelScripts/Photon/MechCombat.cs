using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MechCombat : Combat {

	[SerializeField] Transform camTransform;
	[SerializeField] Animator animator;
	[SerializeField] MechController mechController;
	[SerializeField] CharacterController CharacterController;
	[SerializeField] Sounds Sounds;
	[SerializeField] HeatBar HeatBar;
	[SerializeField] InRoomChat InRoomChat;
	[SerializeField] ParticleSystem SwitchWeaponEffectL,SwitchWeaponEffectR;
	[SerializeField] DisplayPlayerInfo displayPlayerInfo;
	[SerializeField] Combo Combo;
	public TrailRenderer trailRendererL,trailRendererR;
	PlayerInZone Healthpool;
	HealthPoolBar HealthpoolBar;
	ParticleSystem MuzL,MuzR;
	// Boost variables
	private float fuelDrain = 5.0f;
	private float fuelGain = 5.0f;
	private float minFuelRequired = 450;
	private float currentFuel;
	public float jumpPower = 90.0f;
	public float moveSpeed = 35.0f;
	public float boostSpeed = 60f;
	private float verticalBoostSpeed = 1f;
	public float maxVerticalBoostSpeed = 30f;
	// Game variables
	public Score score;

	// Combat variables
	public bool isDead;
	public bool[] is_overheat = new bool[4]; // this is handled by HeatBar.cs , but other player also need to access it  
	private RaycastHit hit;
	private Weapon[] weaponScripts;
	private int weaponOffset = 0;
	private const int LEFT_HAND = 0;
	private const int RIGHT_HAND = 1;
	private float timeOfLastShotL;
	private float timeOfLastShotR;
	private int[] curWeaponNames = new int[2];
	enum WeaponTypes {RANGED, ENG, MELEE, SHIELD, RCL, BCN, EMPTY};

	public bool CanSlash = true;
	// Left
	private bool fireL = false;
	private bool shootingL = false;
	public int isLSlashPlaying = 0;
	// Right
	private bool fireR = false;
	private bool shootingR = false;
	public int isRSlashPlaying = 0;
	private bool isSwitchingWeapon = false;
	private bool receiveNextSlash = true;
	private bool isDeadFirstCall = true;
	private bool isOverHeat = false;//used to check if it is just overheat
	// Transforms
	private Transform shoulderL;
	private Transform shoulderR;
	private Transform head;
	private Transform[] Hands;

	// GameObjects
	private GameObject[] weapons;
	private GameObject[] bullets;
	private GameObject Target;
	private List<Transform> targets;

	// HUD
	private Slider healthBar;
	private Slider fuelBar;
	private Image fuelBar_fill;
	private bool isNotEnoughEffectPlaying = false;
	private bool isFuelAvailable = true;
	Text healthtext,fueltext;
	private HUD hud;
	private Camera cam;

	// Components
	private BuildMech bm;
	public Crosshair crosshair;
	private SlashDetector slashDetector;

	//Healthpool
	private float InPoolPreTime = 0f;
	private const float CallHealDeltaTime = 2f;

	private Coroutine bulletCoroutine;

	//for Debug
	public bool forceDead = false;

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
		UpdateCurWeaponType ();
		SyncWeaponOffset ();
		initHealthPool ();
		FindTrailRenderer ();
		UpdateMuz ();
	}

	void initMechStats() {
		currentHP = MAX_HP;
		currentFuel = MAX_FUEL;
		//maxVerticalBoostSpeed = boostSpeed / 2;
	}

	void initTransforms() {
		cam = transform.Find("Camera").gameObject.GetComponent<Camera>();
		head = transform.Find("CurrentMech/metarig/hips/spine/chest/fakeNeck/head");
		shoulderL = transform.Find("CurrentMech/metarig/hips/spine/chest/shoulder.L");
		shoulderR = transform.Find("CurrentMech/metarig/hips/spine/chest/shoulder.R");
		Hands = new Transform[2];
		Hands [0] = shoulderL.Find ("upper_arm.L/forearm.L/hand.L");
		Hands [1] = shoulderR.Find ("upper_arm.R/forearm.R/hand.R");
		SwitchWeaponEffectL.transform.SetParent (Hands [0]);
		SwitchWeaponEffectL.transform.localPosition = Vector3.zero;
		SwitchWeaponEffectL.Stop ();

		SwitchWeaponEffectR.transform.SetParent (Hands [1]);
		SwitchWeaponEffectR.transform.localPosition = Vector3.zero;
		SwitchWeaponEffectR.Stop ();
	}

	void initGameObjects() {
		InRoomChat = GameObject.Find ("InRoomChat") .GetComponent<InRoomChat>();
		hud = GameObject.FindObjectOfType<HUD> ();
		displayPlayerInfo.gameObject.SetActive (!photonView.isMine);
	}

	public void initComponents() {
		bm = GetComponent<BuildMech>();
		weapons = bm.weapons;
		bullets = bm.bulletPrefabs;
		weaponScripts = bm.weaponScripts;
		Sounds.ShotSounds = bm.ShotSounds;
	}

	public void initCombatVariables() {// this will be called also when respawn
		weaponOffset = 0;
		fireL = false;
		fireR = false;
		shootingL = false;
		shootingR = false;
		timeOfLastShotL = Time.time;
		timeOfLastShotR = Time.time;
		isOverHeat = false;
		isSwitchingWeapon = false;
		CanSlash = true;
		receiveNextSlash = true;
		isDeadFirstCall = true;
	}

	void initHUD() {
		if(photonView.isMine)
			initHealthAndFuelBars();
	}

	void initCam(){
		//cam = transform.Find("Camera").gameObject.GetComponent<Camera>();
	}

	void initCrosshair(){
		crosshair = cam.GetComponent<Crosshair> ();
	}

	void initHealthPool(){
		Healthpool = GameObject.Find ("HealthZone").GetComponent<PlayerInZone> ();
		HealthpoolBar = Healthpool.transform.root.GetComponent<HealthPoolBar> ();
		if(photonView.isMine){
			Healthpool.player_viewID = photonView.viewID;
		}

	}

	public void UpdateMuz (){
		MuzL = weapons [weaponOffset].GetComponentInChildren<ParticleSystem> ();
		MuzR = weapons [weaponOffset+1].GetComponentInChildren<ParticleSystem> ();
		if(MuzL!=null)
			MuzL.Stop ();
		if(MuzR!=null)
			MuzR.Stop ();
	}

	float timeOfLastShot(int handPosition) {
		return handPosition == LEFT_HAND ? timeOfLastShotL : timeOfLastShotR;
	}

	void initHealthAndFuelBars() {
		Slider[] sliders = GameObject.Find("PanelCanvas").GetComponentsInChildren<Slider>();
		if (sliders.Length > 0) {
			healthBar = sliders[0];
			healthBar.value = 1;
			healthtext = healthBar.GetComponentInChildren<Text> ();
			if (sliders.Length > 1) {
				fuelBar = sliders[1];
				fuelBar_fill = fuelBar.transform.Find ("Fill Area/Fill").GetComponent<Image> ();
				fuelBar.value = 1;
				fueltext = fuelBar.GetComponentInChildren<Text> ();
			}
		}
	}
	void initSlashDetector(){
		slashDetector = GetComponentInChildren<SlashDetector> ();
	}

	void SyncWeaponOffset (){
		//sync other player weapon offset
		if (!photonView.isMine) {
			if (photonView.owner.CustomProperties ["weaponOffset"] != null) {
				weaponOffset = int.Parse (photonView.owner.CustomProperties ["weaponOffset"].ToString ());
			}else//the player may just initialize
				weaponOffset = 0;

			weapons [(weaponOffset)].SetActive (true);
			weapons [(weaponOffset + 1)].SetActive (true);
			weapons [(weaponOffset + 2) % 4].SetActive (false);
			weapons [(weaponOffset + 3) % 4].SetActive (false);
		}
	}

	void FireRaycast(Vector3 start, Vector3 direction, int damage, float range , int handPosition) {
		Transform target = ((handPosition == 0)? crosshair.getCurrentTargetL() :crosshair.getCurrentTargetR());
		bool isSlowDown = weaponScripts [weaponOffset + handPosition].isSlowDown;
		if( target != null){
			//Debug.Log("Hit tag: " + target.tag);
			Debug.Log("Hit name: " + target.name);
			Debug.Log ("Hit layer: " + target.gameObject.layer);
			if (curWeaponNames [handPosition] != (int)WeaponTypes.ENG) {
				if (target.tag == "Player" || target.tag == "Drone") {
				
					photonView.RPC ("RegisterBulletTrace", PhotonTargets.All, handPosition, direction, target.transform.root.GetComponent<PhotonView> ().viewID, false);

					target.GetComponent<PhotonView> ().RPC ("OnHit", PhotonTargets.All, damage, photonView.viewID, (isSlowDown) ? 0.4f : 0);
					//Debug.Log ("Damage: " + damage + ", Range: " + range);

					if (target.gameObject.GetComponent<Combat> ().CurrentHP () <= 0) {
						hud.ShowText (cam, target.position, "Kill");
					} else {
						hud.ShowText (cam, target.position, "Hit");
					}
				} else if (target.tag == "Shield") {
					photonView.RPC ("RegisterBulletTrace", PhotonTargets.All, handPosition, direction, target.transform.root.GetComponent<PhotonView> ().viewID, true);

					//check what hand is it
					int hand = (target.transform.parent.parent.name [target.transform.parent.parent.name.Length - 1] == 'L') ? 0 : 1;

					MechCombat targetMcbt = target.transform.root.GetComponent<MechCombat> ();

					if(targetMcbt!=null){
						if(targetMcbt.is_overheat[targetMcbt.weaponOffset + hand]){
							Debug.Log ("onHitShield is overheat");
							targetMcbt.photonView.RPC ("ShieldOnHit", PhotonTargets.All, damage, photonView.viewID, hand);
						}else{
							Debug.Log ("onHitShield is not overheat");
							targetMcbt.photonView.RPC ("ShieldOnHit", PhotonTargets.All, damage/2, photonView.viewID, hand);
						}
					}

					hud.ShowText (cam, target.position, "Defense");
				}
			}else{//ENG
				photonView.RPC ("RegisterBulletTrace", PhotonTargets.All, handPosition, direction, target.transform.root.GetComponent<PhotonView> ().viewID, false);

				target.GetComponent<PhotonView> ().RPC ("OnHeal", PhotonTargets.All, photonView.viewID, damage);

				hud.ShowText (cam, target.position, "Hit");

			}
		}else{
			photonView.RPC("RegisterBulletTrace", PhotonTargets.All, handPosition, direction, -1, false);
		}
	}

	void SlashDetect(int damage){

		if(!mechController.grounded){
			mechController.Boost (true); // jump slash boost effect
			//mechController.SetSlashMoving(cam.transform.forward,8f);
		}
		if ((targets = slashDetector.getCurrentTargets ()).Count != 0) {

			Sounds.PlaySlashOnHit ();
			foreach(Transform target in targets){

				if(target == null){//it causes bug if target disconnect
					continue;
				}

				if (target.tag == "Player" || target.tag == "Drone") {
					
					target.GetComponent<PhotonView> ().RPC ("OnHit", PhotonTargets.All, damage, photonView.viewID, 0.4f);

					if (target.gameObject.GetComponent<Combat> ().CurrentHP () <= 0) {
						hud.ShowText (cam, target.position, "Kill");
					} else {
						hud.ShowText (cam, target.position, "Hit");
					}
				} else if (target.tag == "Shield") {
					hud.ShowText (cam, target.position, "Defense");
				}
			}

		} else{
			//the first one does not move
			//mechController.SetSlashMoving(cam.transform.forward,8f);

		}
			
	}

	[PunRPC]
	void RegisterBulletTrace(int handPosition, Vector3 direction , int playerPVid , bool isShield) {
		if (playerPVid != -1) {
			Target = PhotonView.Find (playerPVid).gameObject;
		}
		else
			Target = null;
		bulletCoroutine = StartCoroutine (InstantiateBulletTrace (handPosition, direction, isShield));
	}
	IEnumerator InstantiateBulletTrace(int handPosition, Vector3 direction, bool isShield){
		int i;
		if(handPosition==0){
			if (MuzL != null)
				MuzL.Play ();
		}else{
			if (MuzR != null)
				MuzR.Play ();
		}

		if(bullets[weaponOffset+handPosition]==null){//it happens when player die when shooting or switching weapons
			yield break;
		}

		if (usingRCLWeapon (handPosition)) { 
			GameObject bullet = Instantiate (bullets[weaponOffset], (Hands [handPosition].position + Hands[handPosition+1].position)/2 + transform.forward*3f + transform.up*3f, Quaternion.LookRotation (direction)) as GameObject;
			RCLBulletTrace RCLbullet = bullet.GetComponent<RCLBulletTrace> ();
			RCLbullet.hud = hud;
			RCLbullet.cam = cam;
			RCLbullet.Shooter = gameObject;
		} else if(usingENGWeapon(handPosition)){
			yield return new WaitForSeconds (0.1f);//in case the wrong start position makes glitch
			GameObject bullet = Instantiate (bullets[weaponOffset+handPosition], Hands [handPosition].position, Quaternion.LookRotation (direction)) as GameObject;
			bullet.transform.SetParent (Hands [handPosition]);
			bullet.GetComponent<ElectricBolt> ().dir = direction;
			bullet.GetComponent<ElectricBolt> ().cam = cam;
			if (Target != null) {
				bullet.GetComponent<ElectricBolt> ().Target = Target.transform;
			}
		}else {
			int bN = bm.weaponScripts[weaponOffset + handPosition].bulletNum;
			GameObject b = bullets [weaponOffset + handPosition];
			for (i = 0; i < bN; i++) {
				GameObject bullet = Instantiate (b , Hands [handPosition].position, Quaternion.LookRotation (direction)) as GameObject;
				BulletTrace bulletTrace = bullet.GetComponent<BulletTrace> ();
				bulletTrace.direction = cam.transform.forward;
				bulletTrace.HUD = hud;
				bulletTrace.cam = cam;
				bulletTrace.ShooterName = gameObject.name;
				bulletTrace.isTargetShield = isShield;
				if (bN > 1)
					bulletTrace.isLMG = true; //multiple messages
				
				if (Target != null){
					Vector3 scale = bullet.transform.localScale;
					bullet.transform.SetParent (Target.transform);
					bullet.transform.localScale = scale;
				}
				yield return new WaitForSeconds (1/bm.weaponScripts[weaponOffset + handPosition].Rate/bN);
			}
		}
	}

	// Applies damage, and updates scoreboard + disables player on kill
	[PunRPC]
	public override void OnHit(int d, int shooter_viewID, float slowdownDuration = 0) {
		//Debug.Log ("OnHit, isDead: " + isDead);
		// If already dead, do nothing
		if (isDead) {
			return;
		}
		if(slowdownDuration > 0){
			mechController.SlowDown (slowdownDuration);
		}
		// Apply damage
		currentHP -= d;
		//Debug.Log ("HP: " + currentHP);

		// If fatal hit,
		if (currentHP <= 0) {
			isDead = true;
			// UI for shooter

			DisablePlayer();

			// Update scoreboard
			gm.RegisterKill(shooter_viewID, photonView.viewID);

			string shooter = PhotonView.Find (shooter_viewID).owner.NickName;
			InRoomChat.AddLine(shooter + " killed " + photonView.name + ".");
		}
	}

	[PunRPC]
	void ShieldOnHit(int d, int shooter_viewID, int hand ){
		if (isDead) {
			return;
		}

		PhotonView pv = PhotonView.Find (shooter_viewID);
		currentHP -= d;
		Debug.Log ("HP: " + currentHP);
		if (currentHP <= 0) {
			isDead = true;
			// UI for shooter

			DisablePlayer();

			// Update scoreboard
			gm.RegisterKill(shooter_viewID, photonView.viewID);

			string shooter = pv.owner.NickName;
			InRoomChat.AddLine(shooter + " killed " + photonView.name + ".");
		}

		if(photonView.isMine && !is_overheat[weaponOffset+hand]){//heat
			if(hand==0){
				HeatBar.IncreaseHeatBarL (30);
			} else {
				HeatBar.IncreaseHeatBarR (30);
			}
		}
	}

	[PunRPC]
	void OnHeal(int viewID, int amount){
		if(isDead){
			return;
		}

		if(currentHP+amount >= MAX_HP){
			currentHP = MAX_HP;
		}else{
			currentHP += amount;
		}
	}

	public void SetCurrentHp(int amount){
		currentHP = amount;
	}

	/*public int GetCurrentHP(){
		return currentHP;
	}*/

	[PunRPC]
	void OnLocked(string name){
		if (PhotonNetwork.playerName != name)
			return;
		crosshair.ShowLocked ();
	}

	[PunRPC]
	public void ForceMove(Vector3 dir, float length){
		transform.position += dir * length;
	}

	// Disable MechController, Crosshair, Renderers, and set layer to 0
	[PunRPC]
	void DisablePlayer() {
		//check if he has the flag
		if(PhotonNetwork.isMasterClient){
			if (photonView.owner.NickName == ((gm.BlueFlagHolder == null)? "" : gm.BlueFlagHolder.NickName)) {
				print ("that dead man has the flag.");
				gm.GetComponent<PhotonView>().RPC ("DropFlag",  PhotonTargets.All, photonView.viewID, 0, transform.position);
			}else if(photonView.owner.NickName == ((gm.RedFlagHolder == null)? "" : gm.RedFlagHolder.NickName)){
				gm.GetComponent<PhotonView>().RPC ("DropFlag",  PhotonTargets.All, photonView.viewID, 1, transform.position);
			}
		}

		gameObject.transform.position = new Vector3 (0, 80, 0);

		if(bulletCoroutine != null)
			StopCoroutine (bulletCoroutine);

		setIsFiring (0, false);
		setIsFiring (1, false);

		displayPlayerInfo.gameObject.SetActive (false);

		gameObject.layer = 2;//2 : ignore ray cast
		Crosshair ch = GetComponentInChildren<Crosshair>();
		if(ch!=null){
			ch.NoCrosshair();
			ch.enabled = false;
		}
		GetComponent<MechController>().enabled = false;
		EnableAllRenderers (false);
		EnableAllColliders (false);

		gameObject.GetComponent<Collider> ().enabled = true;

		transform.Find("Camera/Canvas/CrosshairImage").gameObject.SetActive(false);
		transform.Find("Camera/Canvas/HeatBar").gameObject.SetActive(false);
	}

	// Enable MechController, Crosshair, Renderers, set layer to player layer, move player to spawn position
	[PunRPC]
	void EnablePlayer(int respawnPoint, int mech_num) {
		bm.SetMechNum (mech_num);
		if (photonView.isMine) { // build mech also init MechCombat
			Mech m = UserData.myData.Mech [mech_num];
			bm.Build (m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R);
		}
			
		initMechStats ();

		mechController.initControllerVar ();
		HeatBar.ResetHeatBar ();
		Sounds.UpdateSounds (weaponOffset);
		displayPlayerInfo.gameObject.SetActive (!photonView.isMine);

		transform.position = gm.SpawnPoints[respawnPoint].position;
		gameObject.layer = 8;
		isDead = false;
		if (!photonView.isMine) return;

		// If this is me, enable MechController and Crosshair
		GetComponent<MechController>().enabled = true;
		Crosshair ch = GetComponentInChildren<Crosshair>();
		ch.enabled = true;
		transform.Find("Camera/Canvas/CrosshairImage").gameObject.SetActive(true);
		transform.Find("Camera/Canvas/HeatBar").gameObject.SetActive(true);

	}

	// Update is called once per frame
	void Update () {
		if (!photonView.isMine || gm.GameOver()) return;

		// Drain HP bar gradually
		if (isDead) {
			if (healthBar.value > 0) healthBar.value = healthBar.value -0.01f;

			if(isDeadFirstCall){
				animator.SetBool ("BCNPose", false);
				animator.SetBool ("UsingBCN", false);
				animator.SetBool ("UsingRCL", false);
				isDeadFirstCall = false;
				gm.ShowRespawnPanel ();}
			return;
		}else{
			isDeadFirstCall = true;
		}

		//For debug
		if(forceDead){
			forceDead = false;
			photonView.RPC ("OnHit", PhotonTargets.All, 3000, photonView.viewID, 0f);
		}
			
		// Fix head to always look ahead
		head.LookAt(head.position + transform.forward * 10);

		if (!gm.GameIsBegin)
			return;
		// Animate left and right combat
		handleCombat(LEFT_HAND);
		handleCombat(RIGHT_HAND);

		// Switch weapons
		if (Input.GetKeyDown (KeyCode.R) && !isDead) {
			photonView.RPC("CallSwitchWeapons", PhotonTargets.All, null);
		}

		updateHUD();
	}

	// Set animations and tweaks
	void LateUpdate() {
		handleAnimation(LEFT_HAND);
		handleAnimation(RIGHT_HAND);
	}

	void FixedUpdate(){
		if(photonView.isMine && Healthpool != null){
			if(Healthpool.IsThePlayerInside() && HealthpoolBar.isAvailable){
				if(Time.time - InPoolPreTime >= CallHealDeltaTime){
					photonView.RPC ("OnHeal", PhotonTargets.All, photonView.viewID, Healthpool.healAmount);
					InPoolPreTime = Time.time;
				}
			}else{
				InPoolPreTime = Time.time;
			}
		}
	}

	void handleCombat(int handPosition) {
		switch(curWeaponNames[handPosition]){
		case (int)WeaponTypes.RANGED:
			if (bm.weaponScripts [weaponOffset + handPosition].bulletNum > 1) { //SMG : has a delay before putting down hands
				if (!Input.GetKey (handPosition == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1)) {
					if(handPosition == LEFT_HAND){
						if(Time.time - timeOfLastShotL >= 1 / bm.weaponScripts [weaponOffset + handPosition].Rate * 0.95f)
							setIsFiring (handPosition, false);
						return;
					}else{
						if(Time.time - timeOfLastShotR >= 1 / bm.weaponScripts [weaponOffset + handPosition].Rate * 0.95f)
							setIsFiring (handPosition, false);
						return;
					}
				}
			}else{
				if (!Input.GetKeyDown (handPosition == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1)) {
					if (Time.time - ((handPosition == 1) ? timeOfLastShotR : timeOfLastShotL) >= 0.1f)//0.1 < time of playing shoot animation once , to make sure other player catch this
						setIsFiring (handPosition, false);
					return;
				}
			}
		break;
		case (int)WeaponTypes.MELEE:
			if (!Input.GetKeyDown (handPosition == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1)) {
				setIsFiring (handPosition, false);
				return;
			}
		break;
		case (int)WeaponTypes.SHIELD:
			if (!Input.GetKey (handPosition == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1) || getIsFiring((handPosition+1)%2)) {
				setIsFiring (handPosition, false);
				return;
			}
		break;
		case (int)WeaponTypes.RCL:
			if (!Input.GetKeyDown (handPosition == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1)) {
				if (Time.time - timeOfLastShotL >= 0.4f)//0.4 < time of playing shoot animation once , to make sure other player catch this
					setIsFiring (handPosition, false);
				return;
			}
		break;
		case (int)WeaponTypes.BCN:
			if (Time.time - timeOfLastShotL >= 0.5f)
				setIsFiring (handPosition, false);
			if (Input.GetKeyDown (KeyCode.Mouse1)) {//right click cancel BCNPose
				animator.SetBool ("BCNPose", false);
				return;
			} else if (Input.GetKeyDown (KeyCode.Mouse0)) {
				if (!HeatBar.Is_HeatBarL_Overheat ()) {
					if (!animator.GetBool ("BCNPose")) {
						animator.SetBool ("BCNPose", true);
						timeOfLastShotL = Time.time - 1 / bm.weaponScripts [weaponOffset + handPosition].Rate / 2;
					}
				} else {
					animator.SetBool ("BCNPose", false);
				}
			}
		break;
		case (int)WeaponTypes.ENG:
			if (!Input.GetKey (handPosition == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1)) {
				if (Time.time - ((handPosition == 1) ? timeOfLastShotR : timeOfLastShotL) >= 1 / bm.weaponScripts [weaponOffset + handPosition].Rate )
					setIsFiring (handPosition, false);
				return;
			}
		break;
		
		default: //Empty weapon
			return;
		}
		if(handPosition==0){
			if (HeatBar.Is_HeatBarL_Overheat ()) {
				setIsFiring (handPosition, false);
				return;
			}
		}else{
			if (HeatBar.Is_HeatBarR_Overheat ()) {
				setIsFiring (handPosition, false);
				return;
			}
		}
		if(isSwitchingWeapon){
			return;
		}

		switch(curWeaponNames[handPosition]){

		case (int)WeaponTypes.RANGED:
			if (Time.time - ((handPosition == 1) ? timeOfLastShotR : timeOfLastShotL) >= 1 / bm.weaponScripts [weaponOffset + handPosition].Rate) {
				setIsFiring (handPosition, true);
				FireRaycast (cam.transform.TransformPoint (0, 0, Crosshair.CAM_DISTANCE_TO_MECH), cam.transform.forward, bm.weaponScripts [weaponOffset + handPosition].Damage, weaponScripts [weaponOffset + handPosition].Range, handPosition);
				if (handPosition == 1) {
					HeatBar.IncreaseHeatBarR (20); 
					timeOfLastShotR = Time.time;
				} else {
					HeatBar.IncreaseHeatBarL (20);
					timeOfLastShotL = Time.time;
				}
			}
		break;
		case (int)WeaponTypes.MELEE:
			if (Time.time - ((handPosition == 1) ? timeOfLastShotR : timeOfLastShotL) >= 1 / bm.weaponScripts [weaponOffset + handPosition].Rate) {
				if (!receiveNextSlash|| !CanSlash)
					return;

				if (curWeaponNames [(handPosition + 1) % 2] == (int)WeaponTypes.SHIELD && getIsFiring ((handPosition + 1) % 2)) 
					return;

				if (((handPosition == 1) ? isLSlashPlaying : isRSlashPlaying) != 0) { // only one can play at the same time
					return;
				}

				CanSlash = false;//this is set to true when grounded(update) , to avoid multi-hit in air
				receiveNextSlash = false;
				setIsFiring (handPosition, true);
				if (handPosition == 0) {
					HeatBar.IncreaseHeatBarL (5);
					timeOfLastShotL = Time.time;
					if (curWeaponNames[1]==(int)WeaponTypes.MELEE)
						timeOfLastShotR = timeOfLastShotL;

				} else if (handPosition == 1) {
					HeatBar.IncreaseHeatBarR (5);
					timeOfLastShotR = Time.time;
					if (curWeaponNames[0]==(int)WeaponTypes.MELEE)
						timeOfLastShotL = timeOfLastShotR;
				}
			}
		break;
		case (int)WeaponTypes.SHIELD:
			if (!getIsFiring ((handPosition + 1) % 2))
				setIsFiring (handPosition, true);
		break;
		case (int)WeaponTypes.RCL:
			if (Time.time - timeOfLastShotL >= 1/bm.weaponScripts[weaponOffset + handPosition].Rate) {
				setIsFiring(handPosition, true);
				HeatBar.IncreaseHeatBarL (25);

				FireRaycast(cam.transform.TransformPoint(0, 0, Crosshair.CAM_DISTANCE_TO_MECH), cam.transform.forward, bm.weaponScripts[weaponOffset + handPosition].Damage, weaponScripts[weaponOffset + handPosition].Range , handPosition);
				timeOfLastShotL = Time.time;
			}
		break;
		case (int)WeaponTypes.BCN:
			if (Time.time - timeOfLastShotL >= 1 / bm.weaponScripts [weaponOffset + handPosition].Rate) {
				if (!Input.GetKeyUp (KeyCode.Mouse0) || !animator.GetBool ("BCNPose") || !mechController.grounded)
					return;
				setIsFiring (handPosition, true);
				HeatBar.IncreaseHeatBarL (45); 
				//**Start Position
				FireRaycast (cam.transform.TransformPoint (0, 0, Crosshair.CAM_DISTANCE_TO_MECH), cam.transform.forward, bm.weaponScripts [weaponOffset + handPosition].Damage, weaponScripts [weaponOffset + handPosition].Range, handPosition);
				timeOfLastShotL = Time.time;
			}
		break;
		case (int)WeaponTypes.ENG:
			if (Time.time - ((handPosition == 1) ? timeOfLastShotR : timeOfLastShotL) >= 1 / bm.weaponScripts [weaponOffset + handPosition].Rate) {
				setIsFiring (handPosition, true);
				FireRaycast (cam.transform.TransformPoint (0, 0, Crosshair.CAM_DISTANCE_TO_MECH), cam.transform.forward, bm.weaponScripts [weaponOffset + handPosition].Damage, weaponScripts [weaponOffset + handPosition].Range, handPosition);
				if (handPosition == 1) {
					HeatBar.IncreaseHeatBarR (30); 
					timeOfLastShotR = Time.time;
				} else {
					HeatBar.IncreaseHeatBarL (30);
					timeOfLastShotL = Time.time;
				}
			}
		break;
		
		}
	}

	void handleAnimation(int handPosition) {
		// Name of animation, i.e. ShootR, SlashL, etc
		string animationStr = animationString(handPosition);

		if (getIsFiring(handPosition)) {
			// Rotate arm to point to where you are looking (left hand is opposite)
			float x = cam.transform.rotation.eulerAngles.x * (handPosition == LEFT_HAND ? -1 : 1);

			// Tweaks
			switch(curWeaponNames[handPosition]){
			case (int)WeaponTypes.RANGED:
				animator.SetBool(animationStr, true);
			break;
			case (int)WeaponTypes.MELEE:
				SlashDetect (bm.weaponScripts [weaponOffset + handPosition].Damage);
				Combo.Slash (handPosition);
			break;
			case (int)WeaponTypes.SHIELD:
				animator.SetBool(animationStr, true);
				shoulderR.Rotate (0, x, 0);
			break;
			case (int)WeaponTypes.RCL:
				animator.SetBool (animationStr, true);
			break;
			case (int)WeaponTypes.BCN:
				animator.SetBool (animationStr, true);
				//animator.SetBool ("BCNPose", false);
			break;
			case (int)WeaponTypes.ENG:
				if(handPosition==0)
					animator.SetBool ("ENGShootL", true);
				else
					animator.SetBool ("ENGShootR", true);
				break;
			}
		} else {// melee is set to false by animation
			if (curWeaponNames [handPosition] == (int)WeaponTypes.RANGED || curWeaponNames [handPosition] == (int)WeaponTypes.RCL || curWeaponNames [handPosition] == (int)WeaponTypes.SHIELD)
				animator.SetBool (animationStr, false);
			else if (curWeaponNames [handPosition] == (int)WeaponTypes.BCN)
				animator.SetBool ("ShootBCN", false);
			else if (curWeaponNames [handPosition] == (int)WeaponTypes.ENG) {
				if(handPosition==0)
					animator.SetBool ("ENGShootL", false);
				else{
					animator.SetBool ("ENGShootR", false);
				}
			}
		}
	}

	void updateHUD() {
		// Update Health bar gradually
		healthBar.value = calculateSliderPercent (healthBar.value, currentHP / (float)MAX_HP );
		healthtext.text = BarValueToString (currentHP, MAX_HP);
		// Update Fuel bar gradually
		fuelBar.value = calculateSliderPercent(fuelBar.value, currentFuel/(float)MAX_FUEL);
		fueltext.text = BarValueToString ((int)currentFuel, (int)MAX_FUEL);
	}

	// Returns currentPercent + 0.01 if currentPercent < targetPercent, else - 0.01
	float calculateSliderPercent(float currentPercent, float targetPercent) {
		float err = 0.005f;
		if (Mathf.Abs(currentPercent - targetPercent) > err) {
			currentPercent = currentPercent + (currentPercent > targetPercent ? -0.005f : 0.005f);
		}else{
			currentPercent = targetPercent;
		}
		return currentPercent;
	}

	// Switch weapons by increasing weaponOffset by 2
	// Each player holds 2 sets of weapons (4 total)
	// Switching weapons will switch from set 1 (weap 1 + 2) to set 2 (weap 3 + 4)
	[PunRPC]
	void CallSwitchWeapons() {
		//Play switch weapon animation
		SwitchWeaponEffectL.Play();
		SwitchWeaponEffectR.Play();


		//decrease energy


		Sounds.PlaySwitchWeapon ();
		isSwitchingWeapon = true;
		Invoke ("SwitchWeaponsBegin", 1f);
	}

	void SwitchWeaponsBegin(){
		if(isDead){
			return;
		}
		//update customproperty
		if (photonView.isMine) {
			ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
			h.Add ("weaponOffset", (weaponOffset + 2) % 4);
			photonView.owner.SetCustomProperties (h);
		}
		// Stop current attacks
		setIsFiring(LEFT_HAND, false);
		setIsFiring(RIGHT_HAND, false);

		// Stop current animations
		string strL = animationString(LEFT_HAND), strR = animationString(RIGHT_HAND);
		if(strL != ""){ // not empty weapon
			animator.SetBool(animationString(LEFT_HAND), false);
		}
		if(strR != ""){
			animator.SetBool(animationString(RIGHT_HAND), false);
		}


		if(bulletCoroutine != null)
			StopCoroutine (bulletCoroutine);

		// Switch weapons by toggling each weapon's activeSelf
		for (int i = 0; i < weapons.Length; i++) {
			if(weapons[i]==null){
				weapons [i] = bm.weapons [i];//bug : someimes weapons[i] doesn't update
			}
			weapons[i].SetActive(!weapons[i].activeSelf);
		}

		// Change weaponOffset
		weaponOffset = (weaponOffset + 2) % 4;
		Sounds.UpdateSounds (weaponOffset);
		HeatBar.UpdateHeatBar (weaponOffset);
		UpdateCurWeaponType ();
		UpdateMuz ();
		FindTrailRenderer ();

		//check if using RCL => RCLIdle
		animator.SetBool ("UsingRCL", curWeaponNames[0] == (int)WeaponTypes.RCL);
		animator.SetBool ("UsingBCN", curWeaponNames[0] == (int)WeaponTypes.BCN);

		animator.SetBool ("BCNPose", false);

		//Check crosshair
		crosshair.updateCrosshair (weaponOffset);

		//Stop switch weapon animation
		SwitchWeaponEffectL.Stop();
		SwitchWeaponEffectR.Stop();

		isSwitchingWeapon = false;


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

	public void UpdateCurWeaponType(){
		for(int i=0;i<2;i++){
			if (usingRangedWeapon (i))
				curWeaponNames [i] = (int)WeaponTypes.RANGED;
			else if (usingMeleeWeapon (i))
				curWeaponNames [i] = (int)WeaponTypes.MELEE;
			else if (usingShieldWeapon (i))
				curWeaponNames [i] = (int)WeaponTypes.SHIELD;
			else if (usingRCLWeapon (i))
				curWeaponNames [i] = (int)WeaponTypes.RCL;
			else if (usingBCNWeapon (i))
				curWeaponNames [i] = (int)WeaponTypes.BCN;
			else if (usingENGWeapon (i))
				curWeaponNames [i] = (int)WeaponTypes.ENG;
			else if (usingEmptyWeapon (i))
				curWeaponNames [i] = (int)WeaponTypes.EMPTY;
		}
	}

	[PunRPC]
	void SetOverHeat(bool b, int weaponOffset){
		is_overheat [weaponOffset] = b;
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

	bool usingBCNWeapon(int handPosition){
		return weaponScripts[weaponOffset+handPosition].Animation == "BCNPose";
	}

	bool usingEmptyWeapon(int handPosition){
		return weaponScripts[weaponOffset+handPosition].Animation == "";
	}

	bool usingENGWeapon(int handPosition){
		return weaponScripts[weaponOffset+handPosition].Animation == "ENGShoot";
	} 

	string animationString(int handPosition) {
		switch(curWeaponNames[handPosition]){
		case (int)WeaponTypes.RCL:
			return "ShootRCL";
		case (int)WeaponTypes.BCN:
			return "ShootBCN";
		case (int)WeaponTypes.EMPTY:
			return "";
		default:
			return weaponScripts[weaponOffset + handPosition].Animation + (handPosition == LEFT_HAND ? "L" : "R");
		}
	}

	public void EnableAllRenderers(bool b){
		Renderer[] renderers = GetComponentsInChildren<Renderer> ();
		foreach (Renderer renderer in renderers) {
			renderer.enabled = b;
		}
	}

	public void EnableAllColliders(bool b){
		Collider[] colliders = GetComponentsInChildren<Collider> ();
		foreach(Collider collider in colliders){
			//collider.enabled = b;
			if(!b){
				if(collider.gameObject.name != "Slash Detector")
					collider.gameObject.layer = 2;
			}else if (collider.gameObject.name != "Slash Detector")
				collider.gameObject.layer = 8;
			
		}
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
		if(currentFuel >= minFuelRequired){
			isFuelAvailable = true;
			return true;
		}else{//false -> play effect if not already playing
			if(!isNotEnoughEffectPlaying){
				StartCoroutine (FuelNotEnoughEffect());
			}
			if(!animator.GetBool("Boost"))//can set to false in transition to grounded state but not in transition from grounded state to boost state 
				isFuelAvailable = false;
			return false;
		}
	}

	IEnumerator FuelNotEnoughEffect(){
		isNotEnoughEffectPlaying = true;
		for (int i = 0; i < 4; i++) {
			fuelBar_fill.color = new Color32 (133, 133, 133, 255);
			yield return new WaitForSeconds (0.15f);
			fuelBar_fill.color = new Color32 (255, 255, 255, 255);
			yield return new WaitForSeconds (0.15f);
		}
		isNotEnoughEffectPlaying = false;
	}

	public bool IsFuelAvailable(){
		if(FuelEmpty()){
			isFuelAvailable = false;
		}
		return isFuelAvailable;
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

	public bool IsHpFull(){
		return (currentHP >= MAX_HP);
	}

	public void SetLSlashPlaying(int isPlaying){
		isLSlashPlaying = isPlaying;
	}

	public void SetRSlashPlaying(int isPlaying){// this is true when RSlash is playing ( slashR1 , ... )
		isRSlashPlaying = isPlaying;
	}

	public void SetReceiveNextSlash(int receive){ // this is called in the animation clip
		receiveNextSlash = (receive == 1) ? true : false;
	}

	public void FindTrailRenderer(){
		if(curWeaponNames[0] == (int)WeaponTypes.MELEE){
			trailRendererL = weapons [weaponOffset].GetComponentInChildren<TrailRenderer> ();
			if (trailRendererL != null) {
				trailRendererL.enabled = false;
			}
		}else{
			trailRendererL = null;
		}

		if(curWeaponNames[1] == (int)WeaponTypes.MELEE){
			trailRendererR = weapons [weaponOffset+1].GetComponentInChildren<TrailRenderer> ();
			if(trailRendererR!=null)
				trailRendererR.enabled = false;
		}else{
			trailRendererR = null;
		}
	}

	public void ShowTrailL(bool show){
		if (trailRendererL != null) {
			trailRendererL.enabled = show;
		}
	}
	public void ShowTrailR(bool show){
		if(trailRendererR!=null)
			trailRendererR.enabled = show;
	}

	private string BarValueToString(int curvalue, int maxvalue){
		string curvalueStr = curvalue.ToString ();
		string maxvalueStr = maxvalue.ToString ();

		string finalStr = string.Empty;
		for(int i=0;i<4-curvalueStr.Length;i++){
				finalStr += "0 ";
		}

		for(int i=0;i<curvalueStr.Length;i++){
			finalStr += (curvalueStr [i] + " ");
			
		}
		finalStr += "/ ";
		for(int i=0;i<3;i++){
			finalStr += (maxvalueStr [i] + " ");
		}
		finalStr += maxvalueStr [3];

		return finalStr;
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
