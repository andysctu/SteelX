using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class BuildMech : NetworkBehaviour {

	public string core;
	public string arms;
	public string legs;
	public string head;

	// Use this for initialization
	void Start () {
		if (!isLocalPlayer) return;
//		uint connId = GetComponent<NetworkIdentity>().netId.Value;
//		Debug.Log(connId + ": " + UserData.data[(int)connId].User.PilotName);
		Data data = UserData.myData;
		core = data.Mech.Core;
		arms = data.Mech.Arms;
		legs = data.Mech.Legs;
		head = data.Mech.Head;
		CmdBuildMech(core, arms, legs, head);
	}

	[Command]
	void CmdBuildMech(string c, string a, string l, string h){
		List<string> parts = new List<string>();
		parts.Add(a);
		parts.Add(l);
		parts.Add(h);

		Debug.Log("cmd: " + c + a + l + h);
		MechCreator mc = new MechCreator(c, parts);
		GameObject model = mc.CreateLobbyMech();
		model.transform.SetParent(transform);
		RpcBuildMech(c,a,l,h);
	}

	[ClientRpc]
	void RpcBuildMech(string c, string a, string l, string h){
		if (isServer) return;
		List<string> parts = new List<string>();
		parts.Add(a);
		parts.Add(l);
		parts.Add(h);

		Debug.Log("rpc: " + c + a + l + h);
		MechCreator mc = new MechCreator(c, parts);
		GameObject model = mc.CreateLobbyMech();
		model.transform.SetParent(transform);
	}
}
