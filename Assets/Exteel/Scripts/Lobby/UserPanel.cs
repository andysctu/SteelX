using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UserPanel : MonoBehaviour {
	[SerializeField]
	private Text name;
	// Use this for initialization
	void Start () {
		name.text = PhotonNetwork.player.NickName;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
