using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class BuildMech : NetworkBehaviour {

	private string[] defaultParts = {"CES301","AES104","LTN411","HDS003"};
	private GameManager gm;
	// Use this for initialization
	void Start () {
		if (!isLocalPlayer) return;
		Data data = UserData.myData;
		data.Mech.Core = data.Mech.Core == null ? defaultParts[0] : data.Mech.Core;
		data.Mech.Arms = data.Mech.Arms == null ? defaultParts[1] : data.Mech.Arms;
		data.Mech.Legs = data.Mech.Legs == null ? defaultParts[2] : data.Mech.Legs;
		data.Mech.Head = data.Mech.Head == null ? defaultParts[3] : data.Mech.Head;
		CmdRegister(GetComponent<NetworkIdentity>().netId.Value, data);
		if (isServer) {
			Debug.Log("Is server");
			gm = GameObject.Find("GameManager").GetComponent<GameManager>();
			if (gm == null) {
				Debug.Log("gm is null");
			}
		}
	}

	[Command]
	void CmdRegister(uint id, Data d) {
		gameObject.name = "Player" + id;
		if (gm != null) {
			gm.RegisterPlayer(gameObject, d);
		} else {
			gm = GameObject.Find("GameManager").GetComponent<GameManager>();
			Debug.Log("gm is null, trying to find it...");
			if (gm == null) {
				Debug.Log("gm is still null");
			} else {
				gm.RegisterPlayer(gameObject, d);
			}
		}
	}

//	[Command]
//	void CmdBuildMech(string c, string a, string l, string h){
//		buildMech(c,a,l,h);
//		Debug.Log("CmdBuildMech");
//		RpcBuildMech(c,a,l,h);
//	}

	[ClientRpc]
	public void RpcBuildMech(string c, string a, string l, string h){
//		if (isServer) {
//			Debug.Log("RpcBuildMech not running on server");
//			return;
//		}
		Debug.Log("RpcBuildMech");
		buildMech(c,a,l,h);
	}

	public void buildMech(string c, string a, string l, string h) {
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

	[ClientRpc]
	public void RpcInitScores(uint[] ids){
		if (isServer) {
			Debug.Log("Is server2");
			return;
		}
		Debug.Log("Client Score count: " + ids.Length);
		for (int i = 0; i < ids.Length; i++){
			if (gm == null) {
				Debug.Log("gm is null in rpc");
				gm = GameObject.Find("GameManager").GetComponent<GameManager>();
				if (gm == null) {
					Debug.Log("gm still null!");
				}
			}
			if (gm.playerScores == null) {
				gm.playerScores = new Dictionary<uint, Score>();
			}

			if (gm.playerScores.ContainsKey(ids[i])) {
				gm.playerScores[ids[i]] = new Score();
			} else {
				gm.playerScores.Add(ids[i], new Score());
			}
		}
	}
}
