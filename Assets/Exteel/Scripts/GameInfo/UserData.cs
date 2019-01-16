using System.Collections.Generic;
using UnityEngine;

public class UserData : MonoBehaviour {
    public static Dictionary<int, Data> data;
    public static Data myData;
    public static int preferredFrameRate = 40;

    //Assigned when login
    public static string version = "0.0";
    public static CloudRegionCode region = CloudRegionCode.us;

    public static float cameraRotationSpeed = 5;//mouse sensitivity in game    
    public static float generalVolume = 1f;

    private void Awake() {
        PhotonNetwork.sendRate = 60;
        PhotonNetwork.sendRateOnSerialize = 30;

        if (FindObjectsOfType<UserData>().Length > 1)//already exist
            Destroy(transform.gameObject);

        SetVolume();
        InitMechs();
    }

    private void SetVolume() {
        AudioListener.volume = generalVolume;
    }

    private void InitMechs() {
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


[System.Serializable]
public struct User {
    public int Uid;
    public string Username;
    public string PilotName;
    public int Level;
    public string Rank;
    public int Credits;
}

[System.Serializable]
public struct Mech {
    public int Uid;
    public string Arms;
    public string Legs;
    public string Core;
    public string Head;
    public string Weapon1L;
    public string Weapon1R;
    public string Weapon2L;
    public string Weapon2R;
    public string Booster;
    public bool isPrimary;
    public int[] skillIDs;
    public string[] Parts;

    Mech(int u, string a, string l, string c, string h, string w1l, string w1r, string w2l, string w2r, string b, bool p, int[] skillIDs) {
        Uid = u;
        Arms = a;
        Legs = l;
        Core = c;
        Head = h;
        Weapon1L = w1l;
        Weapon1R = w1r;
        Weapon2L = w2l;
        Weapon2R = w2r;
        Booster = b;
        isPrimary = p;
        this.skillIDs = skillIDs;
        Parts = new string[] { a, l, c, h, w1l, w1r, w2l, w2r, b };
    }

    public void PopulateParts() {
        Parts = new string[] { Arms, Legs, Core, Head, Weapon1L, Weapon1R, Weapon2L, Weapon2R, Booster };
    }
}

[System.Serializable]
public struct Data {
    public Mech Mech0;
    public User User;
    public string[] Owns;

    public Mech[] Mech;
}