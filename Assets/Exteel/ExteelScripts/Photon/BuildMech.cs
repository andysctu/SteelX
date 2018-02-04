using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class BuildMech : Photon.MonoBehaviour {

	private string[] defaultParts = {"CES301","AES104","LTN411","HDS003", "PBS000", "SHL009", "APS403", "SHS309","RCL034", "BCN029","BRF025","SGN150","LMG012" };
	private GameManager gm;
	public GameObject[] weapons;
	public GameObject[] bulletPrefabs;
	public AudioClip[] ShotSounds;

	private Transform shoulderL;
	private Transform shoulderR;
	private Transform[] hands;
	private Animator animator;

	public Weapon[] weaponScripts;
	private String[] curWeapons = new String[4];
	private int weaponOffset = 0;

	private bool inHangar = false;

	void Start () {
		if (SceneManagerHelper.ActiveSceneName == "Hangar" || SceneManagerHelper.ActiveSceneName == "Lobby") inHangar = true;
		// If this is not me, don't build this mech. Someone else will RPC build it
		if (!photonView.isMine && !inHangar) return;

		if(string.IsNullOrEmpty(UserData.myData.Mech.Core)){
			UserData.myData.Mech.Core = defaultParts [0];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech.Arms)){
			UserData.myData.Mech.Arms = defaultParts [1];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech.Legs)){
			UserData.myData.Mech.Legs = defaultParts [2];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech.Head)){
			UserData.myData.Mech.Head = defaultParts [3];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech.Booster)){
			UserData.myData.Mech.Booster = defaultParts [4];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech.Weapon1L)){
			UserData.myData.Mech.Weapon1L = defaultParts [12];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech.Weapon1R)){
			UserData.myData.Mech.Weapon1R = defaultParts [12];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech.Weapon2L)){
			UserData.myData.Mech.Weapon2L = defaultParts [6];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech.Weapon2R)){
			UserData.myData.Mech.Weapon2R = defaultParts [6];
		}
		if(string.IsNullOrEmpty(UserData.myData.User.PilotName)){
			UserData.myData.User.PilotName = "Default Pilot";
		}
		// Get parts info
		Data data = UserData.myData;
		animator = GetComponentInChildren<Animator> ();
		weaponOffset = 0;
		if (inHangar) {
			buildMech(data.Mech);
		} else { // Register my name on all clients
			photonView.RPC("SetName", PhotonTargets.AllBuffered, PhotonNetwork.playerName);
		}
	}
		
	[PunRPC]
	void SetName(string name) {
		gameObject.name = name;
		findGameManager();
		gm.RegisterPlayer(name);
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
		for (int i = 0; i < curSMR.Length; i++){
			if (newSMR[i] == null) Debug.Log(i + " is null");
			curSMR[i].sharedMesh = newSMR[i].sharedMesh;
			curSMR[i].material = materials[i];
			curSMR[i].enabled = true;
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

		MechCombat mechCombat = GetComponent<MechCombat>();

		for (int i = 0; i < weaponNames.Length; i++) {
			Vector3 p = new Vector3(hands[i%2].position.x, hands[i%2].position.y - 0.4f, hands[i%2].position.z);
			weapons [i] = Instantiate(Resources.Load(weaponNames [i]) as GameObject, p, transform.rotation) as GameObject;
			switch (weaponNames[i]) {
			case "APS403": {
					weaponScripts[i] = new APS403();
					weapons[i].transform.Rotate(0f, 0f, 8f * ((i % 2) == 0 ? -1 : 1));
					weapons [i].transform.SetParent (hands [i % 2]);
					bulletPrefabs [i] = Resources.Load ("APS403B") as GameObject;
					ShotSounds [i] = Resources.Load ("Sounds/Planet_Fire") as AudioClip;
					break;
				}
			case "SHL009": {
					weaponScripts[i] = new SHL009();
					float rot = -135;
					weapons[i].transform.rotation = Quaternion.Euler(new Vector3(90,((i%2)==0?rot - 60:rot),0));
					weapons[i].transform.position.Set(p.x, p.y, p.z);
					weapons [i].transform.SetParent (hands [i % 2]);
					bulletPrefabs [i] = null;

					//Load sound 

					break;
				}
			case "SHS309": {
					weaponScripts[i] = new SHS309();
					weapons[i].transform.Rotate(0, 0, (i % 2 == 0 ? -1 : 0) * 180);
					weapons[i].transform.position = new Vector3(p.x + ((i % 2) == 0 ? 0 : 1) * 0.25f, p.y + 0.8f, p.z + 0.5f);
					weapons [i].transform.SetParent (hands [i % 2]);
					bulletPrefabs [i] = null;
					break;
				}
			case "LMG012": {
					weaponScripts[i] = new LMG012();
					weapons[i].transform.Rotate(0f, 0f, 8f * ((i % 2) == 0 ? -1 : 1));
					weapons[i].transform.rotation = Quaternion.Euler(new Vector3(90,180,0));
					weapons [i].transform.SetParent (hands [i % 2]);
					bulletPrefabs [i] = Resources.Load ("LMG012B") as GameObject;
					ShotSounds [i] = Resources.Load ("Sounds/Planet_Fire") as AudioClip;
					break;
				}
			case "BRF025": {
					weaponScripts[i] = new BRF025();
					weapons[i].transform.Rotate(0f, 0f, 8f * ((i % 2) == 0 ? -1 : 1));
					weapons[i].transform.rotation = Quaternion.Euler(new Vector3(0,180,0));
					weapons [i].transform.SetParent (hands [i % 2]);
					bulletPrefabs [i] = Resources.Load ("BRF025B") as GameObject;
					ShotSounds [i] = Resources.Load ("Sounds/Planet_Fire") as AudioClip;
					break;
				}
		/*	case "BCN029": {
					weaponScripts[i] = new BRF025();
					weapons[i].transform.Rotate(0f, 0f, 8f * ((i % 2) == 0 ? -1 : 1));
					weapons[i].transform.rotation = Quaternion.Euler(new Vector3(0,180,0));
					weapons [i].transform.SetParent (hands [i % 2]);
					bulletPrefabs [i] = Resources.Load ("BRF025B") as GameObject;
					ShotSounds [i] = Resources.Load ("Sounds/Planet_Fire") as AudioClip;
					break;
				}*/
			case "SGN150": {
					weaponScripts[i] = new SGN150();
					weapons[i].transform.Rotate(0f, 0f, 8f * ((i % 2) == 0 ? -1 : 1));
					weapons[i].transform.rotation = Quaternion.Euler(new Vector3(90,180,0));
					weapons [i].transform.SetParent (hands [i % 2]);
					bulletPrefabs [i] = Resources.Load ("SGN150") as GameObject;
					ShotSounds [i] = Resources.Load ("Sounds/Planet_Fire") as AudioClip;
					break;
				}
			case "RCL034":{
					//Since the launch button is on right hand
					p = new Vector3 (hands [1].position.x, hands [1].position.y - 0.4f, hands [1].position.z);
					weapons [i].transform.SetParent (hands [1]);
					weaponScripts[i] = new RCL034();

					weapons[i].transform.rotation = Quaternion.Euler(new Vector3(-90,180,0));
					weapons[i].transform.position = new Vector3(p.x , p.y , p.z);
					bulletPrefabs [i] = Resources.Load ("RCL034B")  as GameObject;
					ShotSounds [i] = Resources.Load ("Sounds/Hell_Fire") as AudioClip;

					if(i==weaponOffset){
						if(animator!=null){
							//In Hangar or Lobby
							animator.SetBool ("UsingRCL",true);
						}else {
							//In game
							animator = GetComponentInChildren<MeleeCombat> ().GetComponent<Animator> ();
							animator.SetBool ("UsingRCL",true);
						}
					}
					weaponScripts [i + 1] = new EmptyWeapon ();
					weapons [i + 1] = Instantiate (Resources.Load ("EmptyWeapon") as GameObject, p, transform.rotation) as GameObject;
					weapons [i+1].transform.SetParent (hands [i % 2]);
					weapons [i + 1].SetActive (false);
					bulletPrefabs [i+1] = null;

					i++;//skip the right hand
					break;
				}
			}
		}
		UpdateCurWeapons ();
		weapons [(weaponOffset+2)%4].SetActive (false);
		weapons [(weaponOffset+3)%4].SetActive (false);
	}


	public void EquipWeapon(string weapon, int weapPos) {
		//if previous is two-handed => also destroy left hand 
		if(weapPos>=2){
			if (curWeapons[2].Contains("RCL") || curWeapons[2].Contains ("MSR") || curWeapons[2].Contains ("LCN")) {
				Destroy (weapons [2]);
			}
		}else{
			if (curWeapons[0].Contains("RCL") || curWeapons[0].Contains ("MSR") || curWeapons[0].Contains ("LCN")) {
				Destroy (weapons [0]);
			}
		}
		//if the new one is two-handed => also destroy right hand
		if(weapon.Contains("RCL") || weapon.Contains ("MSR") || weapon.Contains ("LCN")){
			Destroy (weapons[weapPos + 1]);
		}
		Destroy(weapons[weapPos]);
		Vector3 p = new Vector3(hands[weapPos%2].position.x, hands[weapPos%2].position.y - 0.4f, hands[weapPos%2].position.z);
		weapons [weapPos] = Instantiate(Resources.Load(weapon) as GameObject, p, transform.rotation) as GameObject;
		print ("load weapon :" + weapon);
		switch (weapon) {
		case "APS403":
			{
				weaponScripts [weapPos] = new APS403 ();
				weapons [weapPos].transform.Rotate (0f, 0f, 8f * ((weapPos % 2) == 0 ? -1 : 1));
				weapons [weapPos].transform.SetParent (hands [weapPos % 2]);
				break;
			}
		case "SHL009":
			{
				weaponScripts [weapPos] = new SHL009 ();
				float rot = -165;
				weapons [weapPos].transform.rotation = Quaternion.Euler (new Vector3 (90, rot, 0));
				weapons [weapPos].transform.SetParent (hands [weapPos % 2]);
				break;
			}
		case "SHS309":
			{
				weaponScripts [weapPos] = new SHS309 ();
				weapons [weapPos].transform.Rotate (0, 0, (weapPos % 2 == 0 ? -1 : 0) * 180);
				weapons [weapPos].transform.position = new Vector3 (p.x, p.y + 0.8f, p.z + 0.5f);
				weapons [weapPos].transform.SetParent (hands [weapPos % 2]);
				break;
			}
		case "LMG012": {
				weaponScripts[weapPos] = new LMG012();
				weapons[weapPos].transform.Rotate(0f, 0f, 8f * ((weapPos % 2) == 0 ? -1 : 1));
				weapons[weapPos].transform.rotation = Quaternion.Euler(new Vector3(90,180,0));
				weapons [weapPos].transform.SetParent (hands [weapPos % 2]);
				break;
			}
		case "SGN150": {
				weaponScripts[weapPos] = new SGN150();
				weapons[weapPos].transform.Rotate(0f, 0f, 8f * ((weapPos % 2) == 0 ? -1 : 1));
				weapons[weapPos].transform.rotation = Quaternion.Euler(new Vector3(90,180,0));
				weapons [weapPos].transform.SetParent (hands [weapPos % 2]);
				bulletPrefabs [weapPos] = Resources.Load ("SGN150") as GameObject;
				ShotSounds [weapPos] = Resources.Load ("Sounds/Planet_Fire") as AudioClip;
				break;
			}
		case "BRF025": {
				weaponScripts[weapPos] = new BRF025();
				weapons[weapPos].transform.Rotate(0f, 0f, 8f * ((weapPos% 2) == 0 ? -1 : 1));
				weapons[weapPos].transform.rotation = Quaternion.Euler(new Vector3(0,180,0));
				weapons [weapPos].transform.SetParent (hands [weapPos % 2]);
				break;
			}
		case "RCL034":
			{
				weapPos = (weapPos >= 2) ? 2 : 0; //script is on left hand
				p = new Vector3 (hands [1].position.x, hands [1].position.y - 0.4f, hands [1].position.z);
				weapons [weapPos].transform.SetParent (hands [1]); //but the parent is always set to right hand ( for nice look)
				weaponScripts [weapPos] = new RCL034 ();
				weapons [weapPos].transform.rotation = Quaternion.Euler (new Vector3 (-90, 180, 0));
				weapons [weapPos].transform.position = new Vector3 (p.x, p.y , p.z);

				weapons [weapPos + 1] =  Instantiate(Resources.Load("EmptyWeapon") as GameObject, p, transform.rotation) as GameObject;
				weaponScripts [weapPos + 1] = new EmptyWeapon ();
				weapons [weapPos + 1].SetActive (false);

				if(weapPos>=2)UserData.myData.Mech.Weapon2R = "EmptyWeapon";
				else UserData.myData.Mech.Weapon1R = "EmptyWeapon";

				break;
			}
		}
		if (weapPos != weaponOffset && weapPos != weaponOffset + 1)
			weapons [weapPos].SetActive (false);
		else {
			weapons [weapPos].SetActive (true);
		}

		UpdateCurWeapons ();
		CheckAnimatorState ();
	}
		
	private void findGameManager() {
		if (gm == null) {
			gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		}
	}

	public void DisplayFirstWeapons(){
		weaponOffset = 0;

		for(int i=0;i<4;i++)if(curWeapons[i]!="EmptyWeapon")EquipWeapon (curWeapons [i],i);
	}

	public void DisplaySecondWeapons(){
		weaponOffset = 2;

		for(int i=0;i<4;i++)if(curWeapons[i]!="EmptyWeapon")EquipWeapon (curWeapons [i],i);
	}

	private void UpdateCurWeapons(){
		curWeapons [0] = UserData.myData.Mech.Weapon1L;
		curWeapons [1] = UserData.myData.Mech.Weapon1R;
		curWeapons [2] = UserData.myData.Mech.Weapon2L;
		curWeapons [3] = UserData.myData.Mech.Weapon2R;
	}
	private void CheckAnimatorState(){
		if(curWeapons[weaponOffset] == "RCL034"){
			animator.SetBool ("UsingRCL", true);
		}else{
			animator.SetBool ("UsingRCL", false);
		}
	}
}