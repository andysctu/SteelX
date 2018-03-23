using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LobbyManager: MonoBehaviour {

	[SerializeField] Text RoomName;

	//just to see how many player in lobby
	[SerializeField] Text playercountText;
	private float checkPlayerTime = 0;
	private const float checkPlayerDeltaTime = 6f;

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
				PhotonNetwork.ConnectUsingSettings("1.0");
			}

			// generate a name for this player, if none is assigned yet
			if (string.IsNullOrEmpty(PhotonNetwork.playerName))
			{
				PhotonNetwork.playerName = "Guest" + Random.Range(1, 9999);
			}
		}
		PhotonNetwork.autoJoinLobby = true;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void FixedUpdate(){
		if(Time.time - checkPlayerTime >= checkPlayerDeltaTime){
			checkPlayerTime = Time.time;
			playercountText.text = PhotonNetwork.countOfPlayers.ToString();
		}
	}

	void OnJoinedLobby(){
		//PhotonNetwork.LoadLevel (1);
		print ("Joined Lobby");
	}

	public void CreateRoom() {
		Debug.Log ("Creating room: " + RoomName.text);

		//Default settings
		ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
		h.Add ("Map", "Simulation");
		h.Add ("GameMode", "DeathMatch");
		h.Add ("MaxKills", 1);
		h.Add ("MaxTime", 5); 
		//PhotonNetwork.CreateRoom(RoomName.text, new RoomOptions() {IsVisible = true, IsOpen = true, MaxPlayers = 10 },h, TypedLobby.Default);
		RoomOptions ro = new RoomOptions(){IsVisible = true, IsOpen = true, MaxPlayers = 4 };
		ro.CustomRoomProperties = h;
		string[] str = { "Map", "GameMode"};
		ro.CustomRoomPropertiesForLobby = str;
		PhotonNetwork.CreateRoom(RoomName.text,ro,TypedLobby.Default);
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
		Debug.Log("Room created successfully.");
		PhotonNetwork.LoadLevel("GameLobby");
	}

	public void OnDisconnectedFromPhoton()
	{
		Debug.Log("Disconnected from Photon.");
	}
}
