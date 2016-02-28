using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class UserData : NetworkBehaviour {
	public static Dictionary<int, Data> data;

	public static Data myData;

//	public void UpdatePlayerDict(int connId, Data d){
//		CmdUpdatePlayerDict(connId, d);
//	}
//
//	[Command]
//	void CmdUpdatePlayerDict(int connId, Data d){
//		data[connId] = d;
//	}

	// Use this for initialization
	void Start () {
		data = new Dictionary<int,Data>();
		DontDestroyOnLoad(this);
	}
}
