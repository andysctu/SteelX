using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : IScene {
    [SerializeField] private GameObject OperatorStatsUI, CreateRoomModel, RoomPanel;
    [SerializeField] private Text RoomName, playerCountText;
    [SerializeField] private Transform RoomsWrapper;
    [SerializeField] private AudioClip lobbyMusic;
    [SerializeField] private ChatNewGui ChatNewGui;
    private MusicManager MusicManager;
    private GameObject[] rooms;
    private string selectedRoom = "";
    private float lastCheckPlayerCountTime = 0;
    private const float checkPlayerCountDeltaTime = 6f;

    //Auto refresh
    private float lastRefreshTime = 0;
    private const float autoRefreshInterval = 5;

    public const string _sceneName = "Lobby";

    public override void StartScene() {
        base.StartScene();
        CheckIfPlayerConnected();
        PhotonNetwork.autoJoinLobby = true;
        OperatorStatsUI.gameObject.SetActive(true);
        ChatNewGui.Init();

        if (MusicManager == null)
            MusicManager = FindObjectOfType<MusicManager>();
        MusicManager.ManageMusic(lobbyMusic);
    }

    private void CheckIfPlayerConnected() {
        if (!PhotonNetwork.connected) {
            // the following line checks if this client was just created (and not yet online). if so, we connect
            if (PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated) {
                PhotonNetwork.ConnectToRegion(UserData.region, UserData.version);
            }

            if (string.IsNullOrEmpty(PhotonNetwork.playerName)) {
                PhotonNetwork.playerName = "Guest" + Random.Range(1, 9999);
            }
        }
    }

    private void FixedUpdate() {
        if (Time.time - lastCheckPlayerCountTime >= checkPlayerCountDeltaTime) {//update player count
            lastCheckPlayerCountTime = Time.time;
            playerCountText.text = PhotonNetwork.countOfPlayers.ToString();
        }

        if(Time.time - lastRefreshTime > autoRefreshInterval) {
            lastRefreshTime = Time.time;
            Refresh();
        }
    }

    public void CreateRoom() {
        Debug.Log("Creating room: " + RoomName.text);

        //Default settings
        ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
        h.Add("Map", "Simulation");
        h.Add("GameMode", "DeathMatch");
        h.Add("MaxKills", 1);//TODO : remove this
        h.Add("MaxTime", 5);
        h.Add("Status", (int)GameManager.Status.Waiting);
        h.Add("time", "05:00");

        RoomOptions ro = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 4 };
        ro.CustomRoomProperties = h;
        string[] str = { "Map", "GameMode", "Status", "time"};
        ro.CustomRoomPropertiesForLobby = str;
        PhotonNetwork.CreateRoom(RoomName.text, ro, TypedLobby.Default);
        HideCreateRoomModel();
    }

    public void Refresh() {
        lastRefreshTime = Time.time;

        if (rooms != null) for (int i = 0; i < rooms.Length; i++) Destroy(rooms[i]);

        RoomInfo[] roomsInfo = PhotonNetwork.GetRoomList();
        Debug.Log("roomsInfo.length :" + roomsInfo.Length);
        rooms = new GameObject[roomsInfo.Length];
        for (int i = 0; i < roomsInfo.Length; i++) {
            GameObject roomPanel = Instantiate(RoomPanel);
            Text[] info = roomPanel.GetComponentsInChildren<Text>();
            Debug.Log("Room : " + roomsInfo[i].Name);
            info[3].text = roomsInfo[i].PlayerCount + "/" + roomsInfo[i].MaxPlayers;

            if(roomsInfo[i].CustomProperties["Status"] != null) {
                int status = int.Parse(roomsInfo[i].CustomProperties["Status"].ToString());

                info[2].text = (status == (int)GameManager.Status.Waiting) ? "Waiting" : "In Battle";

                //Display time
                if(status == (int)GameManager.Status.InBattle && roomsInfo[i].CustomProperties["time"] != null) {                    
                    info[2].text += "(" + roomsInfo[i].CustomProperties["time"].ToString() + ")";
                }

            } else//Just in case different version
                info[2].text = "";

            info[1].text = roomsInfo[i].CustomProperties["GameMode"].ToString();
            info[0].text = roomsInfo[i].Name;

            roomPanel.transform.SetParent(RoomsWrapper);
            RectTransform rt = roomPanel.GetComponent<RectTransform>();
            rt.localPosition = new Vector3(0, 0, 0);
            rt.localScale = new Vector3(1, 1, 1);
            int index = i;
            roomPanel.GetComponent<Button>().onClick.AddListener(() => { selectedRoom = roomsInfo[index].Name; });
            rooms[i] = roomPanel;
        }
    }

    public void ShowCreateRoomModal() {
        //Reset input
        CreateRoomModel.transform.Find("InputField").GetComponent<InputField>().text = "";
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

    public void GoToHangar() {
        SceneStateController.LoadScene(HangarManager._sceneName);
    }

    public void GoToStore() {
        SceneStateController.LoadScene(StoreManager._sceneName);
    }

    public void ExitToLogin() {
        PhotonNetwork.Disconnect();
        SceneStateController.LoadScene(LoginManager._sceneName);
    }

    public override void EndScene() {
        base.EndScene();
        OperatorStatsUI.gameObject.SetActive(false);
        ChatNewGui.DisconnectClient();
    }

    public void OnJoinedRoom() {
        Debug.Log("OnJoinedRoom");
        SceneStateController.LoadScene(GameLobbyManager._sceneName);
    }

    public override string GetSceneName() {
        return _sceneName;
    }

    private void OnJoinedLobby() {
        print("Joined Lobby");
    }

    public void OnPhotonCreateRoomFailed() {
        //		ErrorDialog = "Error: Can't create room (room name maybe already used).";
        Debug.Log("OnPhotonCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name.");
    }

    public void OnPhotonJoinRoomFailed(object[] cause) {
        //		ErrorDialog = "Error: Can't join room (full or unknown room name). " + cause[1];
        Debug.Log("OnPhotonJoinRoomFailed got called. This can happen if the room is not existing or full or closed.");
    }

    public void OnPhotonRandomJoinFailed() {
        //		ErrorDialog = "Error: Can't join random room (none found).";
        Debug.Log("OnPhotonRandomJoinFailed got called. Happens if no room is available (or all full or invisible or closed). JoinrRandom filter-options can limit available rooms.");
    }

    public void OnCreatedRoom() {
        Debug.Log("Room created successfully.");
        //MySceneManager.GoToGameLobby();
    }

    public void OnDisconnectedFromPhoton() {
        Debug.Log("Disconnected from Photon.");
    }

    //public void OnReceivedRoomListUpdate() {
        //Debug.Log("Received: " + PhotonNetwork.GetRoomList().Length);
        //Refresh();
    //}
}