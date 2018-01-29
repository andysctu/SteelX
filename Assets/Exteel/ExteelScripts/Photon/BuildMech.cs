﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class BuildMech : Photon.MonoBehaviour {

	private string[] defaultParts = {"CES301","AES104","LTN411","HDS003", "PBS000", "SHS309", "APS403", "SHL009", "SHL009" , "RCL034"};
	private GameManager gm;
	public GameObject[] weapons;

	private Transform shoulderL;
	private Transform shoulderR;
	private Transform[] hands;
	private Animator animator;

	public Weapon[] weaponScripts;
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
			UserData.myData.Mech.Weapon1L = defaultParts [9];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech.Weapon1R)){
			UserData.myData.Mech.Weapon1R = defaultParts [9];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech.Weapon2L)){
			UserData.myData.Mech.Weapon2L = defaultParts [7];
		}
		if(string.IsNullOrEmpty(UserData.myData.Mech.Weapon2R)){
			UserData.myData.Mech.Weapon2R = defaultParts [8];
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
		for (int i = 0; i < parts.Length; i++) {
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
		for (int i = 0; i < weaponNames.Length; i++) {
			Vector3 p = new Vector3(hands[i%2].position.x, hands[i%2].position.y - 0.4f, hands[i%2].position.z);
			weapons [i] = Instantiate(Resources.Load(weaponNames [i]) as GameObject, p, transform.rotation) as GameObject;
			switch (weaponNames[i]) {
			case "APS403": {
					weaponScripts[i] = new APS403();
					weapons[i].transform.Rotate(0f, 0f, 8f * ((i % 2) == 0 ? -1 : 1));
					weapons [i].transform.SetParent (hands [i % 2]);
					break;
				}
			case "SHL009": {
					weaponScripts[i] = new SHL009();
					float rot = -135;
					weapons[i].transform.rotation = Quaternion.Euler(new Vector3(90,((i%2)==0?rot - 60:rot),0));
					weapons[i].transform.position.Set(p.x, p.y, p.z);
					weapons [i].transform.SetParent (hands [i % 2]);
					break;
				}
			case "SHS309": {
					weaponScripts[i] = new SHS309();
					weapons[i].transform.Rotate(0, 0, (i % 2 == 0 ? -1 : 0) * 180);
					weapons[i].transform.position = new Vector3(p.x + ((i % 2) == 0 ? 0 : 1) * 0.25f, p.y + 0.8f, p.z + 0.5f);
					weapons [i].transform.SetParent (hands [i % 2]);
					break;
				}
			case "RCL034":{

					//Since the launch button is on right hand
					p = new Vector3 (hands [(i + 1) % 2].position.x, hands [(i + 1) % 2].position.y - 0.4f, hands [(i+1)% 2].position.z);
					weapons [i].transform.SetParent (hands [(i+1) % 2]);
					weaponScripts[i] = new RCL034();

					//by trial and error :(
					weapons[i].transform.rotation = Quaternion.Euler(new Vector3(-90,180,0));
					weapons[i].transform.position = new Vector3(p.x , p.y - 1f, p.z);

					if(i==weaponOffset){
						if(animator!=null){
							//In Hangar or Lobby
							animator.SetBool ("UsingRCL",true);
						}else {
							//In game
							animator = GetComponentInChildren<MeleeCombat> ().GetComponent<Animator> ();
							if(animator!=null){
								animator.SetBool ("UsingRCL",true);
							}else{
								Debug.LogError ("Can't find animator.");
							}
						}
					}
					break;
				}
			}
		}
		weapons [1].SetActive (false);//temp 

		weapons [(weaponOffset+2)%4].SetActive (false);
		weapons [(weaponOffset+3)%4].SetActive (false);
	}
	void Update(){
		print ("P : "+weapons [0].transform.position);
	}
	public void EquipWeapon(string weapon, int weapPos) {
		Destroy(weapons[weapPos]);
		Vector3 p = new Vector3(hands[weapPos%2].position.x, hands[weapPos%2].position.y - 0.4f, hands[weapPos%2].position.z);
		weapons [weapPos] = Instantiate(Resources.Load(weapon) as GameObject, p, transform.rotation) as GameObject;
		switch (weapon) {
		case "APS403": {
				weaponScripts[weapPos] = new APS403();
				weapons[weapPos].transform.Rotate(0f, 0f, 8f * ((weapPos % 2) == 0 ? -1 : 1));
				break;
			}
		case "SHL009": {
				weaponScripts[weapPos] = new SHL009();
				float rot = -165;
				weapons[weapPos].transform.rotation = Quaternion.Euler(new Vector3(90,rot,0));
				break;
			}
		case "SHS309": {
				weaponScripts[weapPos] = new SHS309();
				weapons[weapPos].transform.Rotate(0, 0, (weapPos % 2 == 0 ? -1 : 0) * 180);
				weapons[weapPos].transform.position = new Vector3(p.x, p.y + 0.8f, p.z + 0.5f);
				break;
			}
		}
		weapons [weapPos].transform.SetParent (hands [weapPos % 2]);
		if (weapPos >= 2) weapons[weapPos].SetActive(false);
	}
		
	private void findGameManager() {
		if (gm == null) {
			gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		}
	}
}