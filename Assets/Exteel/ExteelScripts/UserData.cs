using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.SceneManagement;

public class UserData : NetworkBehaviour {
	public static Dictionary<int, Data> data;

	private PhotonView PhotonView;
	private int PlayersInGame = 0;

	public static Data myData;

//	public void UpdatePlayerDict(int connId, Data d){
//		CmdUpdatePlayerDict(connId, d);
//	}
//
//	[Command]
//	void CmdUpdatePlayerDict(int connId, Data d){
//		data[connId] = d;
//	}

	void Awake(){
		PhotonNetwork.sendRate = 60;
		PhotonNetwork.sendRateOnSerialize = 30;
	}
	// Use this for initialization
	void Start () {
		myData.Mech = new Mech[4];
		data = new Dictionary<int,Data>(); 
	}
}
