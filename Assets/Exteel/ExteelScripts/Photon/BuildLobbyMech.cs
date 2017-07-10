using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class BuildLobbyMech : MonoBehaviour {
	private string[] defaultParts = {"CES301","AES104","LTN411","HDS003", "PBS000", "APS403", "SHL009", "SHL009", "SHL009"};

	private GameObject[] weapons;

	void Start() {
		// Get parts info
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

		// Replace with correct parts
		buildMech(data.Mech);
	}

	void buildMech(Mech m) {
		string[] parts = new string[9]{ m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R };
	
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
			curSMR[i].sharedMesh = newSMR[i].sharedMesh;
			curSMR[i].material = materials[i];
			curSMR[i].enabled = true;
		}

		// Make weapons
		buildWeapons (new string[4]{parts[5],parts[6],parts[7],parts[8]});
	}

	void buildWeapons(string[] weaponNames) {
		Transform shoulderL = transform.Find("CurrentMech/metarig/hips/spine/chest/shoulder.L");
		Transform shoulderR = transform.Find("CurrentMech/metarig/hips/spine/chest/shoulder.R");

		Transform[] hands = new Transform[2];
		hands [0] = shoulderL.Find ("upper_arm.L/forearm.L/hand.L");
		hands [1] = shoulderR.Find ("upper_arm.R/forearm.R/hand.R");

		if (weapons != null) for (int i = 0; i < 4; i++) if (weapons[i] != null) Destroy(weapons[i]);
		weapons = new GameObject[4];
		Weapon[] weaponScripts = new Weapon[4];
		for (int i = 0; i < weaponNames.Length; i++) {
			Vector3 p = new Vector3(hands[i%2].position.x, hands[i%2].position.y-0.4f, hands[i%2].position.z);
			weapons [i] = Instantiate(Resources.Load(weaponNames [i]) as GameObject, p, transform.rotation) as GameObject;

			switch (weaponNames[i]) {
			case "APS403": {
					weaponScripts[i] = new APS403();
					break;
				}
			case "SHL009": {
					weaponScripts[i] = new SHL009();
					float rot = -135;
					weapons[i].transform.rotation = Quaternion.Euler(new Vector3(90,rot,0));
					weapons[i].transform.position.Set(p.x, p.y, p.z);
					break;
				}
			}
			weapons [i].transform.SetParent (hands [i % 2]);
		}
		weapons [2].SetActive (false);
		weapons [3].SetActive (false);
	}
}
