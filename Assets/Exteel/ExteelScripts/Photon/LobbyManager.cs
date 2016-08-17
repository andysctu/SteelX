using UnityEngine;
using System.Collections;

public class LobbyManager : MonoBehaviour {

	[SerializeField] GameObject LobbyPlayer;
	[SerializeField] GameObject Team1, Team2;

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
		}

		GameObject lobbyPlayer = PhotonNetwork.Instantiate (LobbyPlayer.name, transform.position, Quaternion.identity, 0);
		lobbyPlayer.transform.SetParent (Team1.transform);
		lobbyPlayer.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
//		PhotonNetwork.LoadLevel ("Game");
	}

	// Update is called once per frame
	void Update () {
	
	}

	public void StartGame() {
		Debug.Log ("Starting game");
		PhotonNetwork.LoadLevel ("Game");
	}

	public void LeaveGame() {
		Debug.Log("Leaving game");
		PhotonNetwork.LoadLevel ("Lobby");
	}
}
