using System.Collections.Generic;
using UnityEngine;

public class UserData : MonoBehaviour {
    public static Dictionary<int, Data> data;
    public static Data myData;
    public static int preferredFrameRate = 40;

    //Assigned when login
    public static string version = "0.0";
    public static CloudRegionCode region = CloudRegionCode.jp;

    private void Awake() {
        PhotonNetwork.sendRate = 60;
        PhotonNetwork.sendRateOnSerialize = 30;

        if (FindObjectsOfType<UserData>().Length > 1)//already exist
            Destroy(transform.parent.gameObject);
    }

    private void Start() {
        myData.Mech0 = new Mech();
        myData.Mech = new Mech[4];
        data = new Dictionary<int, Data>();
    }
}

//	public void UpdatePlayerDict(int connId, Data d){
//		CmdUpdatePlayerDict(connId, d);
//	}
//
//	[Command]
//	void CmdUpdatePlayerDict(int connId, Data d){
//		data[connId] = d;
//	}