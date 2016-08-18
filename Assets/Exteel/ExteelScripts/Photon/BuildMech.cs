﻿using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class BuildMech : Photon.MonoBehaviour {

	private string[] defaultParts = {"CES301","AES104","LTN411","HDS003", "PBS000", "APS403", "APS403", "APS403", "SHL009"};
	private GameManager gm;
	public GameObject[] weapons;

	private Transform shoulderL;
	private Transform shoulderR;
	private Transform[] hands;

	public Weapon[] weaponScripts;
	private int weaponOffset = 0;

	// Use this for initialization
	void Start () {
		if (!photonView.isMine) return;

		photonView.RPC ("SetName", PhotonTargets.All, PhotonNetwork.playerName);

		Data data = UserData.myData;
		data.Mech.Core = data.Mech.Core == null ? defaultParts[0] : data.Mech.Core;
		data.Mech.Arms = data.Mech.Arms == null ? defaultParts[1] : data.Mech.Arms;
		data.Mech.Legs = data.Mech.Legs == null ? defaultParts[2] : data.Mech.Legs;
		data.Mech.Head = data.Mech.Head == null ? defaultParts[3] : data.Mech.Head;
		data.Mech.Booster = data.Mech.Booster == null ? defaultParts[4] : data.Mech.Booster;
		data.Mech.Weapon1L = data.Mech.Weapon1L == null ? defaultParts[5] : data.Mech.Weapon1L;
		data.Mech.Weapon1R = data.Mech.Weapon1R == null ? defaultParts[6] : data.Mech.Weapon1R;
		data.Mech.Weapon2L = data.Mech.Weapon2L == null ? defaultParts[7] : data.Mech.Weapon2L;
		data.Mech.Weapon2R = data.Mech.Weapon2R == null ? defaultParts[8] : data.Mech.Weapon2R;
		data.User.PilotName = data.User.PilotName == null ? "Default Pilot" : data.User.PilotName;
//		CmdRegister(GetComponent<NetworkIdentity>().netId.Value, data);
	}

	[PunRPC]
	void SetName(string name) {
		gameObject.name = name;
		findGameManager();
		gm.RegisterPlayer (name);
	}

//	[Command]
//	void CmdRegister(uint id, Data d) {
//		// Set player name based on network ID
//		gameObject.name = d.User.PilotName;
//
//		// Add player to GameManager
//		gm.playerInfo.Add(gameObject, d);
//		gm.playerScores.Add(GetComponent<NetworkIdentity>().netId.Value, new Score());
//
//		// Check if all players have registered themselves yet
//		int registeredPlayers = gm.playerInfo.Count;
//		int connectedPlayers = GameObject.Find("LobbyManager").GetComponent<NetworkLobbyManagerCustom>().numPlayers;
//
//		// After the last player is registered, build all players' mechs and initialize scores
//		if (registeredPlayers == connectedPlayers){
//			foreach (KeyValuePair<GameObject, Data> entry in gm.playerInfo){
//				Mech m = entry.Value.Mech;
//				BuildMech mechBuilder = entry.Key.GetComponent<BuildMech>();
//				mechBuilder.RpcBuildMech(m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R);
//				mechBuilder.buildMech(m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R);
//				RpcInitInfo(entry.Key, entry.Value);
//			}
//			uint[] ids = new uint[gm.playerInfo.Count];
//			gm.playerScores.Keys.CopyTo(ids,0);
//			RpcInitScores(ids);
//		} 
//	}
//
//	[ClientRpc]
//	public void RpcInitInfo(GameObject key, Data value) {
//		key.name = value.User.PilotName;
//		gm.playerInfo.Add(key, value);
//	}
//		
//	[ClientRpc]
//	public void RpcBuildMech(string c, string a, string l, string h, string b, string w1l, string w1r, string w2l, string w2r){
//		if (isServer) return;
//		buildMech(c,a,l,h,b,w1l,w1r,w2l,w2r);
//	}

	public void Build (string c, string a, string l, string h, string b, string w1l, string w1r, string w2l, string w2r) {
		photonView.RPC("buildMech", PhotonTargets.All, c, a, l, h, b, w1l, w1r, w2l, w2r);
	}

	private void init() {
		shoulderL = transform.FindChild("CurrentMech/metarig/hips/spine/chest/shoulder.L");
		shoulderR = transform.FindChild("CurrentMech/metarig/hips/spine/chest/shoulder.R");

		hands = new Transform[2];
		hands [0] = shoulderL.FindChild ("upper_arm.L/forearm.L/hand.L");
		hands [1] = shoulderR.FindChild ("upper_arm.R/forearm.R/hand.R");
	}

	[PunRPC]
	public void buildMech(string c, string a, string l, string h, string b, string w1l, string w1r, string w2l, string w2r) {
		init ();
		string[] parts = new string[9]{ c, a, l, h, b, w1l, w1r, w2l, w2r };
		for (int i = 0; i < parts.Length; i++) {
			parts [i] = parts [i] == null ? defaultParts [i] : parts [i];
		}

		GameObject[] partsGO = new GameObject[5];
		SkinnedMeshRenderer[] newSMR = new SkinnedMeshRenderer[5];
		SkinnedMeshRenderer[] curSMR = GetComponentsInChildren<SkinnedMeshRenderer> ();
		Material[] materials = new Material[5];
		for (int i = 0; i < 5; i++) {
			partsGO[i] = Resources.Load (parts [i], typeof(GameObject)) as GameObject;
			newSMR [i] = partsGO [i].GetComponentInChildren<SkinnedMeshRenderer> () as SkinnedMeshRenderer;
			materials [i] = Resources.Load (parts [i] + "mat", typeof(Material)) as Material;
		}

		for (int i = 0; i < curSMR.Length; i++){
			curSMR[i].sharedMesh = newSMR[i].sharedMesh;
			curSMR[i].material = materials[i];
			curSMR[i].enabled = true;
		}
		arm (new string[4]{parts[5],parts[6],parts[7],parts[8]});
	}

	private void arm (string[] weaponNames) {
		weapons = new GameObject[4];
		weaponScripts = new Weapon[4];
		for (int i = 0; i < weaponNames.Length; i++) {
			weapons [i] = Instantiate(Resources.Load(weaponNames [i]) as GameObject, hands [i % 2].position, Quaternion.identity) as GameObject;
			weapons [i].transform.SetParent (hands [i % 2]);

			switch (weaponNames[i]) {
			case "APS403": {
					weaponScripts[i] = new APS403();
					break;
				}
			case "SHL009": {
					weaponScripts[i] = new SHL009();
					break;
				}
			}
		}

		weaponOffset = 0;
		weapons [2].SetActive (false);
		weapons [3].SetActive (false);
	}

//	[ClientRpc]
//	void RpcInitScores(uint[] ids){
//		// Server already has scores initialized
//		if (isServer) {
//			return;
//		}
//		gm.playerScores = new Dictionary<uint, Score>();
//		foreach (uint id in ids) {
//			gm.playerScores.Add(id, new Score());
//		}
//	}
//
	private void findGameManager() {
		if (gm == null) {
			gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		}
	}
}