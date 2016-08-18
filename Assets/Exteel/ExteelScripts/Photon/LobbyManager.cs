﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour {

	[SerializeField] GameObject LobbyPlayer;
	[SerializeField] GameObject Team1, Team2, MenuBar, MapInfo;

	private List<GameObject> players;

	// Use this for initialization
	void Start () {

//		// For debugging, so we don't have to login each time
//		if (!PhotonNetwork.connected) {
//			Debug.Log ("Not connected");
//			// this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
//			PhotonNetwork.automaticallySyncScene = true;
//
//			// the following line checks if this client was just created (and not yet online). if so, we connect
//			if (PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated) {
//				// Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
//				PhotonNetwork.ConnectUsingSettings ("0.9");
//			}
//
//			// generate a name for this player, if none is assigned yet
//			if (string.IsNullOrEmpty (PhotonNetwork.playerName)) {
//				PhotonNetwork.playerName = "Guest" + Random.Range (1, 9999);
//			}
////			yield return new WaitUntil (PhotonNetwork.connected);
//			PhotonNetwork.CreateRoom("Test Room", new RoomOptions() { MaxPlayers = 10 }, null);
//		}

		if (!PhotonNetwork.connected) {
			PhotonNetwork.LoadLevel ("Lobby");
			return;
		}

//		Team1.GetComponent<RectTransform> ().sizeDelta = new Vector2 (Screen.width * 0.6f, Screen.height * 0.4f);
//		Team2.GetComponent<RectTransform> ().sizeDelta = new Vector2 (Screen.width * 0.6f, Screen.height * 0.4f);
//		MenuBar.GetComponent<RectTransform> ().sizeDelta = new Vector2 (Screen.width * 0.6f, Screen.height * 0.2f);
//		MapInfo.GetComponent<RectTransform> ().sizeDelta = new Vector2 (Screen.width * 0.4f, Screen.height);

		players = new List<GameObject> ();
		for (int i = 0; i < PhotonNetwork.playerList.Length; i++) {
			PhotonPlayer player = PhotonNetwork.playerList[i];
			addPlayer (player.name);
		}

		PhotonNetwork.automaticallySyncScene = true;

		if (PhotonNetwork.isMasterClient) {
			GameObject startButton = GameObject.Find ("Canvas/MenuBar/Start");
			startButton.GetComponent<Button> ().interactable = true;
		}
	}

	private void addPlayer(string name) {
		GameObject lobbyPlayer = PhotonNetwork.Instantiate (LobbyPlayer.name, transform.position, Quaternion.identity, 0);
		lobbyPlayer.transform.SetParent (Team1.transform);
		lobbyPlayer.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
		lobbyPlayer.name = name;
		lobbyPlayer.GetComponentInChildren<Text> ().text = name;
		players.Add(lobbyPlayer);
	}

	// Update is called once per frame
	void Update () {
	
	}

	public void StartGame() {
		Debug.Log ("Starting game");
		PhotonNetwork.room.open = false;
		PhotonNetwork.LoadLevel ("Game");
	}

	public void LeaveGame() {
		Debug.Log("Leaving game");
		PhotonNetwork.LeaveRoom ();
		PhotonNetwork.LoadLevel ("Lobby");
	}

	public void OnPhotonPlayerConnected(PhotonPlayer newPlayer) {
		Debug.Log ("Player connected: " + newPlayer.name);
		addPlayer (newPlayer.name);
	}

	public void OnPhotonPlayerDisconnected(PhotonPlayer disconnectedPlayer) {
		Debug.Log ("Player disconnected: " + disconnectedPlayer.name);
		foreach (GameObject lobbyPlayer in players) {
			if (lobbyPlayer.name == disconnectedPlayer.name) {
				PhotonNetwork.Destroy (lobbyPlayer);
				players.Remove (lobbyPlayer);
			}
		}
	}
}