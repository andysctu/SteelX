using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class BuildMech : Photon.MonoBehaviour {

	private string[] defaultParts = {"CES301","AES104","LTN411","HDS003", "PBS000", "SHL009", "APS403", "SHS309","RCL034", "BCN029","BRF025","SGN150","LMG012","ENG041", "ADR000" };
																																								//eng : 13
	[SerializeField]private GameObject RespawnPanel;
	[SerializeField]private MechCombat mcbt = null;

	private GameManager gm;
	public AudioClip[] ShotSounds;
	private Animator animator;

	private Transform shoulderL;
	private Transform shoulderR;
	private Transform[] hands;


	public Weapon[] weaponScripts;
	public GameObject[] weapons;
	public GameObject[] bulletPrefabs;
	private String[] curWeaponNames = new String[4]; //this is the weapon name array

	private ParticleSystem Muz;
	private int weaponOffset = 0;

	private bool inHangar = false;
	private bool inStore = false;
	public bool onPanel = false;
	public int Total_Mech = 4;
	public int Mech_Num = 0;
	private const int BLUE = 0, RED = 1;

	void Start () {
		if (SceneManagerHelper.ActiveSceneName == "Hangar" || SceneManagerHelper.ActiveSceneName == "Lobby" || onPanel) inHangar = true;

		if (SceneManagerHelper.ActiveSceneName == "Store")inStore = true;

		// If this is not me, don't build this mech. Someone else will RPC build it
		if (!photonView.isMine && !inHangar && !inStore) return;

		for(int i=0;i<Total_Mech;i++){//init all datas
			SetMechDefaultIfEmpty (i);
		}

		// Get parts info
		Data data = UserData.myData;
		animator = transform.Find("CurrentMech").GetComponent<Animator> ();
		weaponOffset = 0;

		if (inHangar || inStore) {
			buildMech(data.Mech[Mech_Num]);
		} else { // Register my name on all clients
			photonView.RPC("SetName", PhotonTargets.AllBuffered, PhotonNetwork.playerName);
		}

		if(onPanel){
			gameObject.transform.SetParent (RespawnPanel.transform);
		}
	}
		
	[PunRPC]
	void SetName(string name) {
		gameObject.name = name;
		findGameManager();
		gm.RegisterPlayer(photonView.viewID, (photonView.owner.GetTeam()==PunTeams.Team.red)? RED : BLUE);// blue & none team => set to blue
	}
		
	public void Build(string c, string a, string l, string h, string b, string w1l, string w1r, string w2l, string w2r) {
		photonView.RPC("buildMech", PhotonTargets.AllBuffered, c, a, l, h, b, w1l, w1r, w2l, w2r);
	}
				
	private void findHands() {
		shoulderL = transform.Find("CurrentMech/metarig/hips/spine/chest/shoulder.L");
		shoulderR = transform.Find("CurrentMech/metarig/hips/spine/chest/shoulder.R");

		hands = new Transform[2];
		hands [0] = shoulderL.Find("upper_arm.L/forearm.L/hand.L");
		hands [1] = shoulderR.Find("upper_arm.R/forearm.R/hand.R");
	}

	private void buildMech(Mech m) {
		buildMech(m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R);
	}

	[PunRPC]
	public void buildMech(string c, string a, string l, string h, string b, string w1l, string w1r, string w2l, string w2r) {
		findHands ();
		string[] parts = new string[9]{ c, a, l, h, b, w1l, w1r, w2l, w2r };

		//intentionally not checking if weapon is null 
		for (int i = 0; i < parts.Length-4; i++) {
			parts [i] = string.IsNullOrEmpty(parts[i])? defaultParts [i] : parts [i];
		}
			
		// Create new array to store skinned mesh renderers 
		SkinnedMeshRenderer[] newSMR = new SkinnedMeshRenderer[5];

		Material[] materials = new Material[5];
		for (int i = 0; i < 5; i++) {
			// Load mech part
			GameObject part = Resources.Load (parts [i], typeof(GameObject)) as GameObject;

			// Extract Skinned Mesh
			newSMR [i] = part.GetComponentInChildren<SkinnedMeshRenderer> () as SkinnedMeshRenderer;

			// Load texture
			materials [i] = Resources.Load (parts [i] + "mat", typeof(Material)) as Material;
		}

		// Replace all
		SkinnedMeshRenderer[] curSMR = GetComponentsInChildren<SkinnedMeshRenderer> ();

		int j = 0;//if call build when it's already build , we need to make sure not selecting weapons
		for (int i = 0; i < curSMR.Length; i++) {
			if(CheckIsWeapon(curSMR[i].ToString())){
				continue;
			}

			if (newSMR[j] == null) Debug.LogError(i + " is null.");
			curSMR[i].sharedMesh = newSMR[j].sharedMesh;
			curSMR[i].material = materials[j];
			curSMR[i].enabled = true;
			j++;
		}
			
		// Replace weapons
		buildWeapons(new string[4]{parts[5],parts[6],parts[7],parts[8]});
	}

	private void buildWeapons (string[] weaponNames) {
		if (weapons != null) for (int i = 0; i < weapons.Length; i++) if (weapons[i] != null) Destroy(weapons[i]);
		weapons = new GameObject[4];
		weaponScripts = new Weapon[4];
		bulletPrefabs = new GameObject[4];
		ShotSounds = new AudioClip[4];

		for (int i = 0; i < weaponNames.Length; i++) {
			weapons [i] = Instantiate(Resources.Load(weaponNames [i]) as GameObject, Vector3.zero, transform.rotation) as GameObject;

			if(onPanel){//resize
				weapons [i].transform.localScale *=22f;
			}else if(SceneManagerHelper.ActiveSceneName == "Lobby"){
				weapons [i].transform.localScale *= 0.7f;
			}else if(SceneManagerHelper.ActiveSceneName == "Hangar"){
				
			}

			//turn off muz
			Muz = weapons [i].GetComponentInChildren<ParticleSystem> ();
			if (Muz != null) {
				Muz.Stop ();
				if (inHangar||inStore) {
					Muz.gameObject.SetActive (false);
				}
			}
				
			switch (weaponNames[i]) {
			case "APS403": {
					weaponScripts [i] = new APS403 ();
					weapons [i].transform.rotation = hands [i % 2].rotation;
					weapons [i].transform.SetParent (hands [i % 2]);
					if(i % 2 == 0){
						weapons [i].transform.localRotation = Quaternion.Euler (new Vector3 (170, -90, 0));
						weapons [i].transform.position = hands [i % 2].position - weapons [i].transform.up*0.5f - weapons [i].transform.forward*0.1f + weapons [i].transform.right*0.2f;
					}else{
						weapons [i].transform.localRotation = Quaternion.Euler (new Vector3 (170, 70, 0));
						weapons [i].transform.position = hands [i % 2].position - weapons [i].transform.up*0.5f - weapons [i].transform.forward*0.1f - weapons [i].transform.right*0.2f;
					}
					bulletPrefabs [i] = Resources.Load ("APS403B") as GameObject;
					ShotSounds [i] = Resources.Load ("Sounds/Planet_Fire 04") as AudioClip;
					break;
				}
			case "SHL009": {
					weaponScripts [i] = new SHL009 ();
					weapons [i].transform.rotation = hands [i % 2].rotation;
					weapons [i].transform.SetParent (hands [i % 2]);
					if(i % 2 == 0){
						weapons [i].transform.localRotation = Quaternion.Euler (new Vector3 (-90, -80, 0));
						weapons [i].transform.position = hands [i % 2].position - weapons [i].transform.up*0f+ weapons [i].transform.forward * 0.5f  + weapons [i].transform.right*0.2f;
					}else{
						weapons [i].transform.localRotation = Quaternion.Euler (new Vector3 (-90, 70, 0));
						weapons [i].transform.position = hands [i % 2].position - weapons [i].transform.up*0f+ weapons [i].transform.forward * 0.5f - weapons [i].transform.right*0.2f;
					}
					bulletPrefabs [i] = null;

					//Load sound 
					break;
				}
			case "ADR000": {
					weaponScripts [i] = new ADR000 ();
					weapons [i].transform.rotation = hands [i % 2].rotation;
					weapons [i].transform.SetParent (hands [i % 2]);
					if(i % 2 == 0){
						weapons [i].transform.localRotation = Quaternion.Euler (new Vector3 (-90, -80, 0));
						weapons [i].transform.position = hands [i % 2].position - weapons [i].transform.up*0f+ weapons [i].transform.forward * 0.35f  + weapons [i].transform.right*0.2f;
					}else{
						weapons [i].transform.localRotation = Quaternion.Euler (new Vector3 (-90, 70, 0));
						weapons [i].transform.position = hands [i % 2].position - weapons [i].transform.up*0f+ weapons [i].transform.forward * 0.35f - weapons [i].transform.right*0.2f;
					}
					bulletPrefabs [i] = null;

					//Load sound 
					break;
				}
			case "SHS309": {
					weaponScripts [i] = new SHS309 ();
					weapons [i].transform.rotation = hands [i % 2].rotation;
					weapons [i].transform.SetParent (hands [i % 2]);
					if(i % 2 == 0){
						weapons [i].transform.localRotation = Quaternion.Euler (new Vector3 (180, -80, 180));
						weapons [i].transform.position = hands [i % 2].position - weapons [i].transform.up*0f - weapons [i].transform.forward * 0.9f  - weapons [i].transform.right*0.2f;
					}else{
						weapons [i].transform.localRotation = Quaternion.Euler (new Vector3 (180, 80, 0));
						weapons [i].transform.position = hands [i % 2].position - weapons [i].transform.up*0f - weapons [i].transform.forward * 0.9f - weapons [i].transform.right*0.2f;
					}
					bulletPrefabs [i] = null;

					//also set the child collider
					GameObject collider = weapons [i].GetComponentInChildren<Collider> ().gameObject;
					if(i % 2 == 0){
						collider.transform.localRotation = Quaternion.Euler(0,5,10);
						collider.transform.localPosition = new Vector3(0.10f,0,0);
					}else{
						collider.transform.localRotation = Quaternion.Euler(0,5,-10);
						collider.transform.localPosition =  new Vector3(0.10f,0,0);
					}


					break;
				}
			case "LMG012": {
					weaponScripts [i] = new LMG012 ();
					weapons [i].transform.rotation = hands [i % 2].rotation;
					weapons [i].transform.SetParent (hands [i % 2]);
					if(i % 2 == 0){
						weapons [i].transform.localRotation = Quaternion.Euler (new Vector3 (-90, -90, 0));
						weapons [i].transform.position = hands [i % 2].position - weapons [i].transform.up*0f + weapons [i].transform.forward*0.6f + weapons [i].transform.right*0.2f;
					}else{
						weapons [i].transform.localRotation = Quaternion.Euler (new Vector3 (-90, 70, 0));
						weapons [i].transform.position = hands [i % 2].position - weapons [i].transform.up*0f + weapons [i].transform.forward*0.6f - weapons [i].transform.right*0.2f;
					}
					bulletPrefabs [i] = Resources.Load ("LMG012B") as GameObject;
					ShotSounds [i] = Resources.Load ("Sounds/beam_fire 2") as AudioClip;
					break;
				}
			case "BRF025": {
					weaponScripts [i] = new BRF025 ();
					weapons [i].transform.rotation = hands [i % 2].rotation;
					weapons [i].transform.SetParent (hands [i % 2]);
					if(i % 2 == 0){
						weapons [i].transform.localRotation = Quaternion.Euler (new Vector3 (180, -90, 0));
						weapons [i].transform.position = hands[i % 2].position - weapons [i].transform.up*0.5f + weapons [i].transform.forward*0.1f + weapons [i].transform.right*0.2f;
					}else{
						weapons [i].transform.localRotation = Quaternion.Euler (new Vector3 (180, 70, 0));
						weapons [i].transform.position = hands[i % 2].position - weapons [i].transform.up*0.5f + weapons [i].transform.forward*0.1f - weapons [i].transform.right*0.2f;
					}

					bulletPrefabs [i] = Resources.Load ("BRF025B") as GameObject;
					ShotSounds [i] = Resources.Load ("Sounds/Zeus_Fire") as AudioClip;
					break;
				}
			case "BCN029": {
					weaponScripts[i] = new BCN029();
					weapons [i].transform.rotation = hands [1].rotation;
					weapons [i].transform.SetParent (hands [1]);
					weapons [i].transform.localRotation = Quaternion.Euler(new Vector3(170,90,-25));
					weapons [i].transform.position = hands[1].position - weapons [i].transform.up*0.5f - weapons [i].transform.forward*0.1f;
					bulletPrefabs [i] = Resources.Load ("BCN029B") as GameObject;
					ShotSounds [i] = Resources.Load ("Sounds/POSE_Fire") as AudioClip;

					weaponScripts [i + 1] = new EmptyWeapon ();
					weapons [i + 1] = Instantiate (Resources.Load ("EmptyWeapon") as GameObject, Vector3.zero, transform.rotation) as GameObject;
					weapons [i+1].transform.SetParent (hands [i % 2]);
					weapons [i + 1].SetActive (false);
					bulletPrefabs [i+1] = null;

					i++;//skip the right hand
					break;
				}
			case "SGN150": {
					weaponScripts [i] = new SGN150 ();
					weapons [i].transform.rotation = hands [i % 2].rotation;
					weapons [i].transform.SetParent (hands [i % 2]);
					if(i % 2 == 0){
						weapons [i].transform.localRotation = Quaternion.Euler (new Vector3 (-90, -90, 0));
						weapons [i].transform.position = hands[i % 2].position + weapons [i].transform.forward*0.5f + weapons [i].transform.right*0.2f;
					}else{
						weapons [i].transform.localRotation = Quaternion.Euler (new Vector3 (-90, 70, 0));
						weapons [i].transform.position = hands[i % 2].position + weapons [i].transform.forward*0.5f - weapons [i].transform.right*0.2f;
					}

					bulletPrefabs [i] = Resources.Load ("SGN150B") as GameObject;
					ShotSounds [i] = Resources.Load ("Sounds/Spatter_Fire") as AudioClip;
					break;
				}
			case "RCL034":{
					weaponScripts [i] = new RCL034 ();
					weapons [i].transform.rotation = hands [1].rotation;
					weapons [i].transform.SetParent (hands [1]); //the parent is always set to right hand ( for nice look)
					weapons [i].transform.localRotation = Quaternion.Euler(new Vector3(95,90,-10));
					weapons [i].transform.position = hands[1].position - weapons [i].transform.up*0f - weapons [i].transform.forward*0.1f;

					bulletPrefabs [i] = Resources.Load ("RCL034B")  as GameObject;
					ShotSounds [i] = Resources.Load ("Sounds/Hell_Fire") as AudioClip;

					weaponScripts [i + 1] = new EmptyWeapon ();
					weapons [i + 1] = Instantiate (Resources.Load ("EmptyWeapon") as GameObject, Vector3.zero, transform.rotation) as GameObject;
					weapons [i+1].transform.SetParent (hands [i % 2]);
					weapons [i + 1].SetActive (false);
					bulletPrefabs [i+1] = null;

					i++;//skip the right hand
					break;
				}
			case "ENG041":{
					weaponScripts [i] = new ENG041 ();
					weapons [i].transform.rotation = hands [i % 2].rotation;
					weapons [i].transform.SetParent (hands [i % 2]);
					if(i % 2 == 0){
						weapons [i].transform.localRotation = Quaternion.Euler (new Vector3 (-90, -90, 0));
						weapons [i].transform.position = hands[i % 2].position + weapons [i].transform.forward*0.5f + weapons [i].transform.right*0.2f;
					}else{
						weapons [i].transform.localRotation = Quaternion.Euler (new Vector3 (-90, 70, 0));
						weapons [i].transform.position = hands[i % 2].position + weapons [i].transform.forward*0.5f - weapons [i].transform.right*0.2f;
					}

					bulletPrefabs [i] = Resources.Load ("ENG041B") as GameObject;
					ShotSounds [i] = Resources.Load ("Sounds/Heal_loop") as AudioClip;

					break;
				}
			default:{
					weaponScripts [i] = new EmptyWeapon ();
					weapons [i].transform.SetParent (hands [i % 2]);
			break;
			}

			}
		}

		UpdateCurWeaponNames (weaponNames);

		animator = transform.Find("CurrentMech").GetComponent<Animator> ();//if in game , then animator is not ini. in start
		if (animator != null)CheckAnimatorState ();
		weapons [(weaponOffset+2)%4].SetActive (false);
		weapons [(weaponOffset+3)%4].SetActive (false);

		if(mcbt!=null)UpdateMechCombatVars ();//this will turn trail on ( enable all renderer)

		for (int i = 0; i < 4; i++)
			ShutDownTrail (weapons [i]);
	}


	public void EquipWeapon(string weapon, int weapPos) {
		//if previous is two-handed => also destroy left hand 
		if(weapPos==3){
			if (weapons [2] != null) {
				if (CheckIsTwoHanded(curWeaponNames [2])) {
					if (!inStore)UserData.myData.Mech [Mech_Num].Weapon2L = "EmptyWeapon";

					Destroy (weapons [2]);
					curWeaponNames [2] = "EmptyWeapon";
				}
			}
		}else if(weapPos==1){
			if (weapons [0] != null) {
				if (CheckIsTwoHanded(curWeaponNames [0])) {
					if (!inStore)UserData.myData.Mech [Mech_Num].Weapon1L = "EmptyWeapon";

					Destroy (weapons [0]);
					curWeaponNames [0] = "EmptyWeapon";
				}
			}
		}
		//if the new one is two-handed => also destroy right hand
		if(CheckIsTwoHanded(weapon)){
			if(weapons[weapPos+1]!=null)
				Destroy (weapons[weapPos + 1]);
			if(weapPos==0){
				if(!inStore)
					UserData.myData.Mech [Mech_Num].Weapon1R = "EmptyWeapon";
				curWeaponNames [1] = "EmptyWeapon";
			}else if(weapPos==2){
				if(!inStore)
					UserData.myData.Mech [Mech_Num].Weapon2R = "EmptyWeapon";
				curWeaponNames [3] = "EmptyWeapon";
			}
		}

		//destroy the current weapon on the hand position
		if (weapons [weapPos] != null) 
			Destroy (weapons [weapPos]);

		weapons [weapPos] = Instantiate(Resources.Load(weapon) as GameObject, Vector3.zero, transform.rotation) as GameObject;

		switch (weapon) {
		case "APS403":
			{
				weaponScripts [weapPos] = new APS403 ();
				weapons [weapPos].transform.rotation = hands [weapPos % 2].rotation;
				weapons [weapPos].transform.SetParent (hands [weapPos % 2]);
				if(weapPos % 2 == 0){
					weapons [weapPos].transform.localRotation = Quaternion.Euler (new Vector3 (170, -90, 0));
					weapons [weapPos].transform.position = hands[weapPos%2].position - weapons [weapPos].transform.up*0.5f - weapons [weapPos].transform.forward*0.1f + weapons [weapPos].transform.right*0.2f;
				}else{
					weapons [weapPos].transform.localRotation = Quaternion.Euler (new Vector3 (170, 70, 0));
					weapons [weapPos].transform.position = hands[weapPos%2].position - weapons [weapPos].transform.up*0.5f - weapons [weapPos].transform.forward*0.1f - weapons [weapPos].transform.right*0.2f;
				}
				break;
			}
		case "SHL009":
			{
				weaponScripts [weapPos] = new SHL009 ();
				weapons [weapPos].transform.rotation = hands [weapPos % 2].rotation;
				weapons [weapPos].transform.SetParent (hands [weapPos % 2]);
				if(weapPos % 2 == 0){
					weapons [weapPos].transform.localRotation = Quaternion.Euler (new Vector3 (-90, -80, 0));
					weapons [weapPos].transform.position = hands[weapPos % 2].position - weapons [weapPos].transform.up*0f+ weapons [weapPos].transform.forward * 0.5f  + weapons [weapPos].transform.right*0.2f;
				}else{
					weapons [weapPos].transform.localRotation = Quaternion.Euler (new Vector3 (-90, 70, 0));
					weapons [weapPos].transform.position = hands[weapPos % 2].position - weapons [weapPos].transform.up*0f+ weapons [weapPos].transform.forward * 0.5f - weapons [weapPos].transform.right*0.2f;
				}
				break;
			}
		case "ADR000":
			{
				weaponScripts [weapPos] = new ADR000 ();
				weapons [weapPos].transform.rotation = hands [weapPos % 2].rotation;
				weapons [weapPos].transform.SetParent (hands [weapPos % 2]);
				if(weapPos % 2 == 0){
					weapons [weapPos].transform.localRotation = Quaternion.Euler (new Vector3 (-90, -80, 0));
					weapons [weapPos].transform.position = hands[weapPos % 2].position - weapons [weapPos].transform.up*0f+ weapons [weapPos].transform.forward * 0.35f  + weapons [weapPos].transform.right*0.2f;
				}else{
					weapons [weapPos].transform.localRotation = Quaternion.Euler (new Vector3 (-90, 70, 0));
					weapons [weapPos].transform.position = hands[weapPos % 2].position - weapons [weapPos].transform.up*0f+ weapons [weapPos].transform.forward * 0.35f - weapons [weapPos].transform.right*0.2f;
				}
				break;
			}
		case "SHS309":
			{
				weaponScripts [weapPos] = new SHS309 ();
				weapons [weapPos].transform.rotation = hands [weapPos % 2].rotation;
				weapons [weapPos].transform.SetParent (hands [weapPos % 2]);
				if(weapPos % 2 == 0){
					weapons [weapPos].transform.localRotation = Quaternion.Euler (new Vector3 (180, -80, 180));
					weapons [weapPos].transform.position = hands[weapPos % 2].position - weapons [weapPos].transform.up*0f - weapons [weapPos].transform.forward * 0.9f  - weapons [weapPos].transform.right*0.2f;
				}else{
					weapons [weapPos].transform.localRotation = Quaternion.Euler (new Vector3 (180, 70, 0));
					weapons [weapPos].transform.position = hands[weapPos % 2].position - weapons [weapPos].transform.up*0f - weapons [weapPos].transform.forward * 0.9f - weapons [weapPos].transform.right*0.2f;
				}
				break;
			}
		case "LMG012": {
				weaponScripts [weapPos] = new LMG012 ();
				weapons [weapPos].transform.rotation = hands [weapPos % 2].rotation;
				weapons [weapPos].transform.SetParent (hands [weapPos % 2]);
				if(weapPos % 2 == 0){
					weapons [weapPos].transform.localRotation = Quaternion.Euler (new Vector3 (-90, -90, 0));
					weapons [weapPos].transform.position = hands[weapPos % 2].position - weapons [weapPos].transform.up*0f + weapons [weapPos].transform.forward*0.6f + weapons [weapPos].transform.right*0.2f;
				}else{
					weapons [weapPos].transform.localRotation = Quaternion.Euler (new Vector3 (-90, 70, 0));
					weapons [weapPos].transform.position = hands[weapPos % 2].position - weapons [weapPos].transform.up*0f + weapons [weapPos].transform.forward*0.6f - weapons [weapPos].transform.right*0.2f;
				}
				break;
			}
		case "SGN150": {
				weaponScripts [weapPos] = new SGN150 ();
				weapons [weapPos].transform.rotation = hands [weapPos % 2].rotation;
				weapons [weapPos].transform.SetParent (hands [weapPos % 2]);
				if(weapPos % 2 == 0){
					weapons [weapPos].transform.localRotation = Quaternion.Euler (new Vector3 (-90, -90, 0));
					weapons [weapPos].transform.position = hands[weapPos % 2].position + weapons [weapPos].transform.forward*0.5f + weapons [weapPos].transform.right*0.2f;
				}else{
					weapons [weapPos].transform.localRotation = Quaternion.Euler (new Vector3 (-90, 70, 0));
					weapons [weapPos].transform.position = hands[weapPos % 2].position + weapons [weapPos].transform.forward*0.5f - weapons [weapPos].transform.right*0.2f;
				}
				break;
			}
		case "BRF025": {
				weaponScripts [weapPos] = new BRF025 ();
				weapons [weapPos].transform.rotation = hands [weapPos % 2].rotation;
				weapons [weapPos].transform.SetParent (hands [weapPos % 2]);
				if(weapPos % 2 == 0){
					weapons [weapPos].transform.localRotation = Quaternion.Euler (new Vector3 (180, -90, 0));
					weapons [weapPos].transform.position = hands[weapPos % 2].position - weapons [weapPos].transform.up*0.5f + weapons [weapPos].transform.forward*0.1f + weapons [weapPos].transform.right*0.2f;
				}else{
					weapons [weapPos].transform.localRotation = Quaternion.Euler (new Vector3 (180, 70, 0));
					weapons [weapPos].transform.position = hands[weapPos % 2].position - weapons [weapPos].transform.up*0.5f + weapons [weapPos].transform.forward*0.1f - weapons [weapPos].transform.right*0.2f;
				}
				break;
			}
		case "BCN029":
			{
				weapPos = (weapPos >= 2) ? 2 : 0; //script is on left hand

				weaponScripts [weapPos] = new BCN029 ();
				weapons [weapPos].transform.rotation = hands [1].rotation;
				weapons [weapPos].transform.SetParent (hands [1]); //the parent is always set to right hand ( for nice look)
				weapons [weapPos].transform.localRotation = Quaternion.Euler(new Vector3(170,90,-25));
				weapons [weapPos].transform.position = hands[1].position - weapons [weapPos].transform.up*0.5f - weapons [weapPos].transform.forward*0.1f;

				weapons [weapPos + 1] =  Instantiate(Resources.Load("EmptyWeapon") as GameObject, hands[0].position, transform.rotation) as GameObject;
				weaponScripts [weapPos + 1] = new EmptyWeapon ();
				weapons [weapPos + 1].SetActive (false);

				if (weapPos >= 2) {
					if(!inStore)
						UserData.myData.Mech [Mech_Num].Weapon2R = "EmptyWeapon";
					curWeaponNames[3] = "EmptyWeapon";
				} else {
					if(!inStore)
						UserData.myData.Mech [Mech_Num].Weapon1R = "EmptyWeapon";
					curWeaponNames[1] = "EmptyWeapon";
				}

				break;
			}
		case "RCL034":
			{
				weapPos = (weapPos >= 2) ? 2 : 0; //script is on left hand

				weaponScripts [weapPos] = new RCL034 ();
				weapons [weapPos].transform.rotation = hands [1].rotation;
				weapons [weapPos].transform.SetParent (hands [1]); //the parent is always set to right hand ( for nice look)
				weapons [weapPos].transform.localRotation = Quaternion.Euler(new Vector3(95,90,-10));
				weapons [weapPos].transform.position = hands[1].position - weapons [weapPos].transform.up*0f - weapons [weapPos].transform.forward*0.1f;

				weapons [weapPos + 1] =  Instantiate(Resources.Load("EmptyWeapon") as GameObject, hands[0].position, transform.rotation) as GameObject;
				weaponScripts [weapPos + 1] = new EmptyWeapon ();
				weapons [weapPos + 1].SetActive (false);

				if (weapPos >= 2) {
					if(!inStore)
						UserData.myData.Mech [Mech_Num].Weapon2R = "EmptyWeapon";
					curWeaponNames[3] = "EmptyWeapon";
				} else {
					if(!inStore)
						UserData.myData.Mech [Mech_Num].Weapon1R = "EmptyWeapon";
					curWeaponNames[1] = "EmptyWeapon";
				}

				break;
			}

		case "ENG041":{
				weaponScripts [weapPos] = new LMG012 ();
				weapons [weapPos].transform.rotation = hands [weapPos % 2].rotation;
				weapons [weapPos].transform.SetParent (hands [weapPos % 2]);
				if(weapPos % 2 == 0){
					weapons [weapPos].transform.localRotation = Quaternion.Euler (new Vector3 (-90, -90, 0));
					weapons [weapPos].transform.position = hands[weapPos % 2].position + weapons [weapPos].transform.forward*0.5f + weapons [weapPos].transform.right*0.2f;
				}else{
					weapons [weapPos].transform.localRotation = Quaternion.Euler (new Vector3 (-90, 70, 0));
					weapons [weapPos].transform.position = hands[weapPos % 2].position + weapons [weapPos].transform.forward*0.5f - weapons [weapPos].transform.right*0.2f;
				}
				break;
			}
		}
			
		weapons [weapPos].SetActive (weapPos == weaponOffset || weapPos == weaponOffset+1);

		Muz = weapons [weapPos].GetComponentInChildren<ParticleSystem> ();
		if (Muz != null) {
			Muz.Stop ();
			if (inHangar||inStore) {
				Muz.gameObject.SetActive (false);
			}
		}
			
		curWeaponNames [weapPos] = weapon;
		ShutDownTrail (weapons[weapPos]);
		if(animator!=null)CheckAnimatorState ();
	}
		
	private void findGameManager() {
		if (gm == null) {
			gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		}
	}

	public void DisplayFirstWeapons(){
		weaponOffset = 0;

		for(int i=0;i<4;i++)if(curWeaponNames[i]!="EmptyWeapon")EquipWeapon (curWeaponNames [i],i);
	}

	public void DisplaySecondWeapons(){
		weaponOffset = 2;

		for(int i=0;i<4;i++)if(curWeaponNames[i]!="EmptyWeapon")EquipWeapon (curWeaponNames [i],i);
	}

	public void UpdateCurWeaponNames(string[] weaponNames){
		curWeaponNames [0] = weaponNames[0];
		curWeaponNames [1] = weaponNames[1];
		curWeaponNames [2] = weaponNames[2];
		curWeaponNames [3] = weaponNames[3];
	}
	public void CheckAnimatorState(){
		if (animator == null)
			return;
		
		if(curWeaponNames[weaponOffset] == "RCL034"){
			animator.SetBool ("UsingRCL", true);
		}else{
			animator.SetBool ("UsingRCL", false);
		}

		if(curWeaponNames[weaponOffset] == "BCN029"){
			animator.SetBool ("UsingBCN", true);
		}else{
			animator.SetBool ("UsingBCN", false);
		}
	}

	void SetMechDefaultIfEmpty(int mehc_num){
		if(string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Core)){
			UserData.myData.Mech[mehc_num].Core = defaultParts [0];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Arms)){
			UserData.myData.Mech[mehc_num].Arms = defaultParts [1];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Legs)){
			UserData.myData.Mech[mehc_num].Legs = defaultParts [2];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Head)){
			UserData.myData.Mech[mehc_num].Head = defaultParts [3];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Booster)){
			UserData.myData.Mech[mehc_num].Booster = defaultParts [4];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Weapon1L)){
			UserData.myData.Mech[mehc_num].Weapon1L = defaultParts [7];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Weapon1R)){
			UserData.myData.Mech[mehc_num].Weapon1R = defaultParts [12];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Weapon2L)){
			UserData.myData.Mech[mehc_num].Weapon2L = defaultParts [5];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech[mehc_num].Weapon2R)){
			UserData.myData.Mech[mehc_num].Weapon2R = defaultParts [11];
		}
	}

	bool CheckIsWeapon(string name){
		return (name.Contains ("BCN") || name.Contains ("RCL") || name.Contains ("ENG") || name.Contains ("BRF") || name.Contains ("SHL") || name.Contains ("LMG") || name.Contains ("APS") || name.Contains ("SHS") || name.Contains ("SGN"));
	}
	bool CheckIsTwoHanded(string name){
		return (name.Contains ("RCL") || name.Contains ("MSR") || name.Contains ("LCN") || name.Contains ("BCN"));
	}

	public void SetMechNum(int num){
		Mech_Num = num;
	}

	void ShutDownTrail(GameObject weapon){
		TrailRenderer trailRenderer = weapon.GetComponentInChildren<TrailRenderer> ();

		if (trailRenderer == null)
			return;

		if(inHangar || inStore){//set active to false
			trailRenderer.gameObject.SetActive (false);
		}else{//in game -> disable
			trailRenderer.enabled = false;
		}
	}

	void UpdateMechCombatVars(){
		if (mcbt == null)
			return;
		mcbt.initComponents ();
		mcbt.initCombatVariables ();
		mcbt.UpdateCurWeaponType ();
		if(mcbt.crosshair!=null)
			mcbt.crosshair.updateCrosshair (0);
		mcbt.FindTrailRenderer ();
		mcbt.EnableAllRenderers (true);
		mcbt.EnableAllColliders (true);
		mcbt.UpdateMuz ();
	}
}