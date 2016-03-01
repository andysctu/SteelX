using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class BuildMech : NetworkBehaviour {

	public string core;
	public string arms;
	public string legs;
	public string head;

	private string[] defaultParts = {"CES301","AES104","LTN411","HDS003"};
	// Use this for initialization
	void Start () {
		if (!isLocalPlayer) return;
		Data data = UserData.myData;
		core = data.Mech.Core == null ? defaultParts[0] : data.Mech.Core;
		arms = data.Mech.Arms == null ? defaultParts[1] : data.Mech.Arms;
		legs = data.Mech.Legs == null ? defaultParts[2] : data.Mech.Legs;
		head = data.Mech.Head == null ? defaultParts[3] : data.Mech.Head;
		CmdBuildMech(core, arms, legs, head);
	}

	[Command]
	void CmdBuildMech(string c, string a, string l, string h){
		GameObject coreGO = Resources.Load(c, typeof(GameObject)) as GameObject;
		GameObject armsGO = Resources.Load(a, typeof(GameObject)) as GameObject;
		GameObject legsGO = Resources.Load(l, typeof(GameObject)) as GameObject;
		GameObject headGO = Resources.Load(h, typeof(GameObject)) as GameObject;

		SkinnedMeshRenderer[] newSMR = new SkinnedMeshRenderer[4];
		newSMR[0] = coreGO.GetComponentInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer;
		newSMR[1] = armsGO.GetComponentInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer;
		newSMR[2] = legsGO.GetComponentInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer;
		newSMR[3] = headGO.GetComponentInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer;

		SkinnedMeshRenderer[] curSMR = GetComponentsInChildren<SkinnedMeshRenderer>();

		Material[] materials = new Material[4];
		materials[0] = Resources.Load(c+"mat", typeof(Material)) as Material;
		materials[1] = Resources.Load(a+"mat", typeof(Material)) as Material;
		materials[2] = Resources.Load(l+"mat", typeof(Material)) as Material;
		materials[3] = Resources.Load(h+"mat", typeof(Material)) as Material;

		for (int i = 0; i < curSMR.Length; i++){
			curSMR[i].sharedMesh = newSMR[i].sharedMesh;
			curSMR[i].material = materials[i];
			curSMR[i].enabled = true;
		}
		RpcBuildMech(c,a,l,h);
	}

	[ClientRpc]
	void RpcBuildMech(string c, string a, string l, string h){
		if (isServer) return;
		GameObject coreGO = Resources.Load(c, typeof(GameObject)) as GameObject;
		GameObject armsGO = Resources.Load(a, typeof(GameObject)) as GameObject;
		GameObject legsGO = Resources.Load(l, typeof(GameObject)) as GameObject;
		GameObject headGO = Resources.Load(h, typeof(GameObject)) as GameObject;

		SkinnedMeshRenderer[] newSMR = new SkinnedMeshRenderer[4];
		newSMR[0] = coreGO.GetComponentInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer;
		newSMR[1] = armsGO.GetComponentInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer;
		newSMR[2] = legsGO.GetComponentInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer;
		newSMR[3] = headGO.GetComponentInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer;

		SkinnedMeshRenderer[] curSMR = GetComponentsInChildren<SkinnedMeshRenderer>();

		Material[] materials = new Material[4];
		materials[0] = Resources.Load(c+"mat", typeof(Material)) as Material;
		materials[1] = Resources.Load(a+"mat", typeof(Material)) as Material;
		materials[2] = Resources.Load(l+"mat", typeof(Material)) as Material;
		materials[3] = Resources.Load(h+"mat", typeof(Material)) as Material;

		for (int i = 0; i < curSMR.Length; i++){
			curSMR[i].sharedMesh = newSMR[i].sharedMesh;
			curSMR[i].material = materials[i];
			curSMR[i].enabled = true;
		}
	}
}
