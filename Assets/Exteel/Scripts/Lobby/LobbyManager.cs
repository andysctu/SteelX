using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LobbyManager: MonoBehaviour {
    [SerializeField] GameObject CreateRoomModel;
    [SerializeField] Text RoomName;
    [SerializeField] GameObject RoomPanel;
    [SerializeField] Transform RoomsWrapper; 
    //just to see how many player in lobby
    [SerializeField] Text playercountText;
    private MySceneManager MySceneManager;
    private GameObject[] rooms;
    private float roomHeight = 50;
    private float checkPlayerTime = 0;
    private string selectedRoom = "";
    private const float checkPlayerDeltaTime = 6f;

    private void Awake() {
        MySceneManager = FindObjectOfType<MySceneManager>();
    }
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
                PhotonNetwork.ConnectUsingSettings("1.4");//TODO : connect to region set in login
            }

			// generate a name for this player, if none is assigned yet
			if (string.IsNullOrEmpty(PhotonNetwork.playerName))
			{
				PhotonNetwork.playerName = "Guest" + Random.Range(1, 9999);
			}
		}
		PhotonNetwork.autoJoinLobby = true;
	}
	
	void FixedUpdate(){
		if(Time.time - checkPlayerTime >= checkPlayerDeltaTime){//update player count
			checkPlayerTime = Time.time;
			playercountText.text = PhotonNetwork.countOfPlayers.ToString();
		}
	}

	void OnJoinedLobby(){
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
        HideCreateRoomModel();

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
		//PhotonNetwork.LoadLevel("GameLobby");
        MySceneManager.GoToGameLobby();
	}

	public void OnDisconnectedFromPhoton()
	{
		Debug.Log("Disconnected from Photon.");
	}

    public void OnReceivedRoomListUpdate() {
        Debug.Log("Received: " + PhotonNetwork.GetRoomList().Length);
        Refresh();
    }

    public void Refresh() {
        if (rooms != null) {
            for (int i = 0; i < rooms.Length; i++) {
                Destroy(rooms[i]);
            }
        }

        RoomInfo[] roomsInfo = PhotonNetwork.GetRoomList();
        Debug.Log("roomsInfo.length :" + roomsInfo.Length);
        rooms = new GameObject[roomsInfo.Length];
        for (int i = 0; i < roomsInfo.Length; i++) {
            GameObject roomPanel = Instantiate(RoomPanel);
            Text[] info = roomPanel.GetComponentsInChildren<Text>();
            Debug.Log(roomsInfo[i].name);
            info[3].text = "Players: " + roomsInfo[i].playerCount + "/" + roomsInfo[i].MaxPlayers;
            info[2].text = "GameMode: " + roomsInfo[i].CustomProperties["GameMode"];
            info[1].text = "Map: " + roomsInfo[i].CustomProperties["Map"];
            info[0].text = "Room Name: " + roomsInfo[i].name;

            roomPanel.transform.SetParent(RoomsWrapper);
            RectTransform rt = roomPanel.GetComponent<RectTransform>();
            rt.localPosition = new Vector3(0, 0, 0);
            rt.localScale = new Vector3(1, 1, 1);
            int index = i;
            roomPanel.GetComponent<Button>().onClick.AddListener(() => {
                selectedRoom = roomsInfo[index].name;
            });
            rooms[i] = roomPanel;
        }
        //RoomsWrapper.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 50 * roomsInfo.Length);
    }

    public void ShowCreateRoomModal() {
        CreateRoomModel.SetActive(true);
    }

    public void HideCreateRoomModel() {
        CreateRoomModel.SetActive(false);
    }

    public void JoinRoom() {
        if (!string.IsNullOrEmpty(selectedRoom)) {
            Debug.Log("Joining Room " + selectedRoom);
            PhotonNetwork.JoinRoom(selectedRoom);
        }
    }

    public void OnJoinedRoom() {
        Debug.Log("OnJoinedRoom");
        //PhotonNetwork.LoadLevel("GameLobby");
        MySceneManager.GoToGameLobby();
    }
}
