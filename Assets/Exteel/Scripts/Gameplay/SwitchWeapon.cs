using UnityEngine;
using UnityEngine.Networking;

public class SwitchWeapon : NetworkBehaviour {
    private Transform[] hands;
    private Transform[] weapons;
    private void Start() {
        hands = new Transform[2];
        hands[0] = transform.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand");
        hands[1] = transform.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand");

        int r = hands[0].childCount;
        int l = hands[1].childCount;
        int w = r + l;

        weapons = new Transform[w];
        for (int i = 0; i < w; i++) {
            weapons[i] = hands[i % 2].GetChild(i / 2) as Transform;
        }
    }

    private void Update() {
        if (!isLocalPlayer) return;
        if (Input.GetKeyDown(KeyCode.R)) {
            if (!isServer) { // Client
                CmdToggleWeapon();
            } else {
                RpcToggleWeapon();
            }
        }
    }

    [Command]
    private void CmdToggleWeapon() {
        RpcToggleWeapon();
    }

    [ClientRpc]
    private void RpcToggleWeapon() {
        for (int i = 0; i < weapons.Length; i++) {
            weapons[i].gameObject.SetActive(!weapons[i].gameObject.activeSelf);
        }
    }
}

//public class SwitchWeapon : NetworkBehaviour {
//
//	public GameObject[] Weapons;
//	private Transform[] hands;
//	private GameObject[] weapons;
//
//	private Vector3 translateOffsetR = new Vector3(0.77f,-0.21f,0.25f);
//	private Vector3 translateOffsetL = new Vector3(-0.5f, -0.20f, -0.22f);
//	private Quaternion rotateOffsetR = Quaternion.Euler(0,-90,180);
//	private Quaternion rotateOffsetL = Quaternion.Euler(0,90,180);
//	private Vector3 swordTR = new Vector3(0.68f,-0.3f,0f);
//	private Vector3 swordTL = new Vector3(-0.68f,-0.3f,0f);
//	private Quaternion swordR = Quaternion.Euler(-90,90,0);
//
//	// Use this for initialization
//	void Start () {
//		Debug.Log("Start");
//		hands = new Transform[2];
//		hands[0] = transform.FindChild("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand");
//		hands[1] = transform.FindChild("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand");
//
//		weapons = new GameObject[Weapons.Length];
//		for (int i = 0; i < weapons.Length; i++) {
//			Transform h = hands[i%2];
//			Vector3 pos = h.position+(i%2==0? translateOffsetR : translateOffsetL);
//			Quaternion rot = h.rotation*(i%2==0? rotateOffsetR : rotateOffsetL);
//			if (Weapons[i].name.ToLower().Contains("sword")) {
//				pos = h.position + (i%2==0?swordTR:swordTL);
//				rot = h.rotation*swordR;
//			}
//			weapons[i] = Instantiate(Weapons[i], pos, rot) as GameObject;
////			ClientScene.RegisterPrefab(Weapons[i]);
//			NetworkServer.Spawn(weapons[i]);
//			weapons[i].transform.parent = h;
//			if (i >= 2) weapons[i].gameObject.SetActive(false);
//
//		}
//
//	}
//
//	// Update is called once per frame
//	void Update () {
//		if (!isLocalPlayer) return;
//		if (Input.GetKeyDown(KeyCode.R)){
//
//			for (int i = 0; i < weapons.Length; i++) {
//				weapons[i].gameObject.SetActive(!weapons[i].gameObject.activeSelf);
//			}
//
//		}
//	}
//}