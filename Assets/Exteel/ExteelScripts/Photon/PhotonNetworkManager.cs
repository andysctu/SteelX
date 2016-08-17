using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PhotonNetworkManager : MonoBehaviour {

	[SerializeField] Text RoomName;

	// Use this for initialization
	void Start () {
		// For debugging, so we don't have to login each time
		if (!PhotonNetwork.connected) {
			// this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
			PhotonNetwork.automaticallySyncScene = true;

			// the following line checks if this client was just created (and not yet online). if so, we connect
			if (PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated)
			{
				// Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
				PhotonNetwork.ConnectUsingSettings("0.9");
			}

			// generate a name for this player, if none is assigned yet
			if (string.IsNullOrEmpty(PhotonNetwork.playerName))
			{
				PhotonNetwork.playerName = "Guest" + Random.Range(1, 9999);
			}
				
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void CreateRoom() {
		Debug.Log ("Creating room: " + RoomName.text);
		PhotonNetwork.CreateRoom(RoomName.text, new RoomOptions() { MaxPlayers = 10 }, null);
	}

	public void OnJoinedRoom()
	{
		Debug.Log("OnJoinedRoom");
	}

	public void OnPhotonCreateRoomFailed()
	{
//		ErrorDialog = "Error: Can't create room (room name maybe already used).";
		Debug.Log("OnPhotonCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name.");
	}

	public void OnPhotonJoinRoomFailed(object[] cause)
	{
//		ErrorDialog = "Error: Can't join room (full or unknown room name). " + cause[1];
		Debug.Log("OnPhotonJoinRoomFailed got called. This can happen if the room is not existing or full or closed.");
	}

	public void OnPhotonRandomJoinFailed()
	{
//		ErrorDialog = "Error: Can't join random room (none found).";
		Debug.Log("OnPhotonRandomJoinFailed got called. Happens if no room is available (or all full or invisible or closed). JoinrRandom filter-options can limit available rooms.");
	}

	public void OnCreatedRoom()
	{
		Debug.Log("OnCreatedRoom");
		PhotonNetwork.LoadLevel("GameLobby");
	}

	public void OnDisconnectedFromPhoton()
	{
		Debug.Log("Disconnected from Photon.");
	}
}
