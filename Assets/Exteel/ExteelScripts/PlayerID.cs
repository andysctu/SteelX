using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class PlayerID : NetworkBehaviour {

	[SyncVar] public string uniquePlayerIdentity;
//	[SyncVar] public List<string> players;
	private NetworkInstanceId playerNetID;
	private Transform myTransform;

//	private bool nameSet = false;

//	public override void OnStartLocalPlayer ()
//	{
//		GetNetIdentity ();
//		if (myTransform.tag == "Player") {
//			
//			SetIdentity ();
//		}
//	}

	void Start(){
		myTransform = transform;
		GetNetIdentity ();
		if (myTransform.tag == "Player") {

			SetIdentity ();
		}
	}
//	// Update is called once per frame
//	void Update () {
//		if (nameSet) return;
//		if (myTransform.name == "RushnikOnlyMesh(Clone)" || myTransform.name == "") {
//			SetIdentity ();
//			nameSet = true;
//		}
//	}

	[Client]
	void GetNetIdentity(){
		playerNetID = GetComponent<NetworkIdentity> ().netId;
		Debug.Log("Player Net ID is: " + playerNetID);
		CmdTellServerMyIdentity (MakeUniqueIdentity());
//		CmdAddNewPlayer(uniquePlayerIdentity);
	}

	void SetIdentity(){
		if (!isLocalPlayer) {
			myTransform.name = uniquePlayerIdentity;
		} else {
			myTransform.name = MakeUniqueIdentity();
		}
	}

	string MakeUniqueIdentity(){
		string uniqueID = "Player " + playerNetID.ToString ();
		return uniqueID;
	}

	[Command]
	void CmdTellServerMyIdentity(string id){
		uniquePlayerIdentity = id;
	}

//	[Command]
//	void CmdAddNewPlayer(string playerName){
//		players.Add(playerName);
//	}
}
