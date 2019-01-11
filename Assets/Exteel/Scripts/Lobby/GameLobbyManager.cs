using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameLobbyManager : IScene {
    [SerializeField] private GameObject LobbyPlayer;
    [SerializeField] private GameObject Team1, Team2, MenuBar, MapInfo;
    [SerializeField] private Dropdown Map, GameMode, MaxKills, MaxPlayers, MaxTime;
    [SerializeField] private Toggle JoinMidGameToggle;
    [SerializeField] private InRoomChat InRoomChat;
    [SerializeField] private Button startButton;
    [SerializeField] private PhotonView photonView;
    [SerializeField] private AudioClip gameLobbyMusic;
    private MusicManager MusicManager;
    private bool callStartgame = false;
    private string[] Maps = new string[1] { "Simulation" };
    private List<GameObject> players;

    public const string _sceneName = "GameLobby";

    public override void StartScene() {
        base.StartScene();

        if (!PhotonNetwork.connected) {
            SceneStateController.LoadScene(LobbyManager._sceneName);
            return;
        }

        if (PhotonNetwork.isMasterClient) {//The room is open after game
            PhotonNetwork.room.IsOpen = true;
        }

        //check if previous players are not destroyed
        if (players != null && players.Count > 0) {
            foreach (GameObject player in players) {
                if (player != null) Destroy(player);
            }
        }

        players = new List<GameObject>();
        for (int i = 0; i < PhotonNetwork.playerList.Length; i++) {
            PhotonPlayer player = PhotonNetwork.playerList[i];

            if (player.GetTeam() == PunTeams.Team.none) {
                player.SetTeam(PunTeams.Team.blue);
            }
            AddPlayer(player.NickName, player.GetTeam());
        }

        PhotonNetwork.automaticallySyncScene = true;

        bool isMasterClient = PhotonNetwork.isMasterClient;
        startButton.interactable = Map.interactable = GameMode.interactable = MaxKills.interactable = MaxPlayers.interactable = MaxTime.interactable = JoinMidGameToggle.interactable = isMasterClient;
        //Reset options
        MaxTime.value = MaxPlayers.value = MaxKills.value = GameMode.value = Map.value = 0;

        //set default team
        if (PhotonNetwork.player.GetTeam() == PunTeams.Team.none) {
            PhotonNetwork.player.SetTeam(PunTeams.Team.blue);
        }

        LoadRoomInfo();//Update the caption text
        InRoomChat.Clear();
        if (MusicManager == null)
            MusicManager = FindObjectOfType<MusicManager>();
        MusicManager.ManageMusic(gameLobbyMusic);
    }

    private void AddPlayer(string name, PunTeams.Team team) {//addPlayer also setTeam
        GameObject lobbyPlayer = Instantiate(LobbyPlayer, transform.position, Quaternion.identity, null);

        lobbyPlayer.transform.SetParent((team == PunTeams.Team.blue || team == PunTeams.Team.none) ? Team1.transform : Team2.transform);
        lobbyPlayer.transform.localPosition = Vector3.zero;

        lobbyPlayer.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        lobbyPlayer.name = name;
        lobbyPlayer.GetComponentInChildren<Text>().text = name;

        players.Add(lobbyPlayer);
    }

    public void SwitchTeamBlue() {
        if (callStartgame) return;

        if (PunTeams.Team.blue == PhotonNetwork.player.GetTeam()) {
            return;
        } else {
            PhotonNetwork.player.SetTeam(PunTeams.Team.blue);
            photonView.RPC("SwitchTeam", PhotonTargets.All, PhotonNetwork.player.NickName, 0);
        }
    }

    public void SwitchTeamRed() {
        if (callStartgame) return;

        if (PunTeams.Team.red == PhotonNetwork.player.GetTeam()) {
            return;
        } else {
            PhotonNetwork.player.SetTeam(PunTeams.Team.red);
            photonView.RPC("SwitchTeam", PhotonTargets.All, PhotonNetwork.player.NickName, 1);
        }
    }

    public void StartGame() {//called by master
        if (callStartgame) return;

        PhotonNetwork.room.IsOpen = JoinMidGameToggle.isOn;

        ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
        h.Add("GameInit", false);
        h.Add("Status", (int)GameManager.Status.InBattle);
        PhotonNetwork.room.SetCustomProperties(h);

        photonView.RPC("CallStartGame", PhotonTargets.All);

        Invoke("MasterLoadLevel", 0f);
    }

    private void MasterLoadLevel() {
        PhotonNetwork.LoadLevel("Game");
    }

    public void LeaveGame() {
        Debug.Log("Leaving game");
        PhotonNetwork.LeaveRoom();
        SceneStateController.LoadScene(LobbyManager._sceneName);
    }

    public void OnPhotonPlayerConnected(PhotonPlayer newPlayer) {
        Debug.Log("GameLobby : "+ newPlayer.NickName + "is connected.");
        AddPlayer(newPlayer.NickName, newPlayer.GetTeam());
    }

    public void OnPhotonPlayerDisconnected(PhotonPlayer disconnectedPlayer) {
        Debug.Log("GameLobby : " + disconnectedPlayer.NickName + "is disconnected.");
        GameObject player = null;
        foreach (GameObject lobbyPlayer in players) {
            if (lobbyPlayer.name == disconnectedPlayer.NickName) {
                player = lobbyPlayer;
            }
        }

        if (player != null) {
            Destroy(player);
            players.Remove(player);
        }
    }

    public void OnMasterClientSwitched(PhotonPlayer newMaster) {
        Debug.Log("Master switched.");
        InRoomChat.AddLine("The Master is switched to " + newMaster.NickName);
        if (PhotonNetwork.isMasterClient) {
            startButton.interactable = true;
            Map.interactable = true;
            GameMode.interactable = true;
            MaxKills.interactable = true;
            MaxPlayers.interactable = true;
            MaxTime.interactable = true;
            JoinMidGameToggle.interactable = true;
        }
    }

    public void LoadRoomInfo() {
        Map.captionText.text = PhotonNetwork.room.CustomProperties["Map"].ToString();
        MaxTime.captionText.text = PhotonNetwork.room.CustomProperties["MaxTime"].ToString();
        MaxPlayers.captionText.text = PhotonNetwork.room.MaxPlayers.ToString();
        MaxKills.captionText.text = PhotonNetwork.room.CustomProperties["MaxKills"].ToString();
        GameMode.captionText.text = PhotonNetwork.room.CustomProperties["GameMode"].ToString();
    }

    public void ChangeMap() {
        photonView.RPC("ChangeMap", PhotonTargets.All, Map.captionText.text);

        ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
        h.Add("Map", Map.captionText.text);
        PhotonNetwork.room.SetCustomProperties(h);
    }

    public void ChangeGameMode() {
        photonView.RPC("ChangeGameMode", PhotonTargets.All, GameMode.captionText.text);

        ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
        h.Add("GameMode", GameMode.captionText.text);
        PhotonNetwork.room.SetCustomProperties(h);
    }

    public void ChangeMaxKills() {
        photonView.RPC("ChangeMaxKills", PhotonTargets.All, MaxKills.captionText.text);

        ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
        h.Add("MaxKills", int.Parse(MaxKills.captionText.text));
        PhotonNetwork.room.SetCustomProperties(h);
    }

    public void ChangeMaxPlayers() {
        photonView.RPC("ChangeMaxPlayers", PhotonTargets.All, MaxPlayers.captionText.text);
        PhotonNetwork.room.MaxPlayers = int.Parse(MaxPlayers.captionText.text);
    }

    public void ChangeMaxTime() {
        photonView.RPC("ChangeMaxTime", PhotonTargets.All, MaxTime.captionText.text);

        ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
        h.Add("MaxTime", int.Parse(MaxTime.captionText.text));
        h.Add("time", MaxTime.captionText.text + ":00");
        PhotonNetwork.room.SetCustomProperties(h);
    }

    public void JoinMidGame() {
        bool b = JoinMidGameToggle.isOn;
        photonView.RPC("ChangeJoinMidGame", PhotonTargets.All, b);
    }

    // RPCs
    [PunRPC]
    public void ChangeMap(string map) {
        Map.captionText.text = map;
    }

    [PunRPC]
    public void ChangeMaxTime(string time) {
        MaxTime.captionText.text = time;
    }

    [PunRPC]
    public void ChangeGameMode(string gameMode) {
        GameMode.captionText.text = gameMode;
    }

    [PunRPC]
    public void ChangeMaxKills(string maxKills) {
        MaxKills.captionText.text = maxKills;
    }

    [PunRPC]
    public void ChangeMaxPlayers(string maxPlayers) {
        MaxPlayers.captionText.text = maxPlayers;
    }

    [PunRPC]
    public void ChangeJoinMidGame(bool b) {
        JoinMidGameToggle.isOn = b;
    }

    [PunRPC]
    public void SwitchTeam(string name, int teamID) {
        GameObject playerToDestroy = null;
        foreach (GameObject lobbyPlayer in players) {
            if (lobbyPlayer.name == name) {
                playerToDestroy = lobbyPlayer;
                break;
            }
        }
        if (playerToDestroy != null) {
            Destroy(playerToDestroy);
            players.Remove(playerToDestroy);
        }

        AddPlayer(name, (teamID == 0) ? PunTeams.Team.blue : PunTeams.Team.red);
    }

    [PunRPC]
    private void CallStartGame() {
        callStartgame = true;
        SceneStateController.SetSceneToLoadOnLoaded(GameSceneManager._sceneName);
        InRoomChat.AddLine("Game is starting.");
    }

    public override void EndScene() {
        base.EndScene();
        MusicManager.ManageMusic(null);
    }

    public override string GetSceneName() {
        return _sceneName;
    }
}