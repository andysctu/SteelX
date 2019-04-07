using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//[RequireComponent(typeof (ChatNewGui))]
public class LobbyChat : MonoBehaviour {
	private const string UserNamePlayerPref = "NamePickUserName";

	//public ChatNewGui chatNewComponent;

	// Use this for initialization
	void Start () {
		//chatNewComponent = FindObjectOfType<ChatNewGui>();
		//chatNewComponent.UserName = PhotonNetwork.playerName;
		//chatNewComponent.Connect();
		enabled = false;

		//PlayerPrefs.SetString(UserNamePlayerPref, chatNewComponent.UserName);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
