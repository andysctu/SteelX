using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class BuildMech : NetworkBehaviour {

	private string[] defaultParts = {"CES301","AES104","LTN411","HDS003", "PBS000", "APS403", "APS403", "APS403", "SHL009"};
	private GameManager gm;

//	public Transform[] Hands =
	// Use this for initialization
	void Start () {
		findGameManager();
		if (!isLocalPlayer) return;
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
		CmdRegister(GetComponent<NetworkIdentity>().netId.Value, data);
	}

	[Command]
	void CmdRegister(uint id, Data d) {
		// Set player name based on network ID
		gameObject.name = "Player" + id;

		// Add player to GameManager
		gm.playerInfo.Add(gameObject, d);
		gm.playerScores.Add(GetComponent<NetworkIdentity>().netId.Value, new Score());

		// Check if all players have registered themselves yet
		int registeredPlayers = gm.playerInfo.Count;
		int connectedPlayers = GameObject.Find("LobbyManager").GetComponent<NetworkLobbyManagerCustom>().numPlayers;

		// Once all players are registered, build all players' mechs and initialize scores
		if (registeredPlayers == connectedPlayers){
			foreach (KeyValuePair<GameObject, Data> entry in gm.playerInfo){
				Mech m = entry.Value.Mech;
				BuildMech mechBuilder = entry.Key.GetComponent<BuildMech>();
				mechBuilder.RpcBuildMech(m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R);
				mechBuilder.buildMech(m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R);
			}
			uint[] ids = new uint[gm.playerInfo.Count];
			gm.playerScores.Keys.CopyTo(ids,0);
			RpcInitScores(ids);
		} 
	}
		
	[ClientRpc]
	public void RpcBuildMech(string c, string a, string l, string h, string b, string w1l, string w1r, string w2l, string w2r){
		buildMech(c,a,l,h,b,w1l,w1r,w2l,w2r);
	}

	public void buildMech(string c, string a, string l, string h, string b, string w1l, string w1r, string w2l, string w2r) {
		GameObject coreGO = Resources.Load(c, typeof(GameObject)) as GameObject;
		GameObject armsGO = Resources.Load(a, typeof(GameObject)) as GameObject;
		GameObject legsGO = Resources.Load(l, typeof(GameObject)) as GameObject;
		GameObject headGO = Resources.Load(h, typeof(GameObject)) as GameObject;
		GameObject bstrGO = Resources.Load(b, typeof(GameObject)) as GameObject;
		GameObject w1lGO = Resources.Load(w1l, typeof(GameObject)) as GameObject;
		GameObject w1rGO = Resources.Load(w1r, typeof(GameObject)) as GameObject;
		GameObject w2lGO = Resources.Load(w2l, typeof(GameObject)) as GameObject;
		GameObject w2rGO = Resources.Load(w2r, typeof(GameObject)) as GameObject;
		SkinnedMeshRenderer[] newSMR = new SkinnedMeshRenderer[9];
		newSMR[0] = coreGO.GetComponentInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer;
		newSMR[1] = armsGO.GetComponentInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer;
		newSMR[2] = legsGO.GetComponentInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer;
		newSMR[3] = headGO.GetComponentInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer;
		newSMR[4] = bstrGO.GetComponentInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer;
		newSMR[5] = w1lGO.GetComponentInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer;
		newSMR[6] = w1rGO.GetComponentInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer;
		newSMR[7] = w2lGO.GetComponentInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer;
		newSMR[8] = w2rGO.GetComponentInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer;

		SkinnedMeshRenderer[] curSMR = GetComponentsInChildren<SkinnedMeshRenderer>();

		Material[] materials = new Material[9];
		materials[0] = Resources.Load(c+"mat", typeof(Material)) as Material;
		materials[1] = Resources.Load(a+"mat", typeof(Material)) as Material;
		materials[2] = Resources.Load(l+"mat", typeof(Material)) as Material;
		materials[3] = Resources.Load(h+"mat", typeof(Material)) as Material;
		materials[4] = Resources.Load(b+"mat", typeof(Material)) as Material;
		materials[5] = Resources.Load(w1l+"mat", typeof(Material)) as Material;
		materials[6] = Resources.Load(w1r+"mat", typeof(Material)) as Material;
		materials[7] = Resources.Load(w2l+"mat", typeof(Material)) as Material;
		materials[8] = Resources.Load(w2r+"mat", typeof(Material)) as Material;

//		MeshCollider[] curMC = GetComponentsInChildren<MeshCollider>();

		for (int i = 0; i < curSMR.Length; i++){
			curSMR[i].sharedMesh = newSMR[i].sharedMesh;
//			curMC[i].sharedMesh = newSMR[i].sharedMesh;
			curSMR[i].material = materials[i];
			curSMR[i].enabled = true;
		}
	}

	[ClientRpc]
	void RpcInitScores(uint[] ids){
		// Server already has scores initialized
		if (isServer) {
			return;
		}
		gm.playerScores = new Dictionary<uint, Score>();
		foreach (uint id in ids) {
			gm.playerScores.Add(id, new Score());
		}
	}

	private void findGameManager() {
		if (gm == null) {
			gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		}
	}
}
