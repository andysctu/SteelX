using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerID : NetworkBehaviour {

	[SyncVar] public string uniquePlayerIdentity;
	private NetworkInstanceId playerNetID;
	private Transform myTransform;

	public override void OnStartLocalPlayer ()
	{
		GetNetIdentity ();
		SetIdentity ();
	}

	void Awake(){
		myTransform = transform;
	}
	// Update is called once per frame
	void Update () {
		if (myTransform.name == "MainMech(Clone)" || myTransform.name == "") {
			SetIdentity ();
		}
	}

	[Client]
	void GetNetIdentity(){
		playerNetID = GetComponent<NetworkIdentity> ().netId;
		CmdTellServerMyIdentity (MakeUniqueIdentity());
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
}
