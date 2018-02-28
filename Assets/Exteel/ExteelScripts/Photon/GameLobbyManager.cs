using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameLobbyManager : Photon.MonoBehaviour {

	[SerializeField] GameObject LobbyPlayer;
	[SerializeField] GameObject Team1, Team2, MenuBar, MapInfo;
	[SerializeField] Dropdown Map, GameMode, MaxKills, MaxPlayers, MaxTime;
	[SerializeField] private InRoomChat InRoomChat;

	private bool callStartgame = false;
	private string[] Maps = new string[3]{"Simulation", "V-Hill", "City"};
	List<GameObject> players ;
	// Use this for initialization
	void Start () {
		// For debugging, so we don't have to login each time
		//if (!PhotonNetwork.connected) {
		//	Debug.Log ("Not connected");
			// this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
			//PhotonNetwork.automaticallySyncScene = true;

			// the following line checks if this client was just created (and not yet online). if so, we connect
//			if (PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated) {
			// Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
//			PhotonNetwork.ConnectUsingSettings ("0.9");
//			}

			// generate a name for this player, if none is assigned yet
			/*if (string.IsNullOrEmpty (PhotonNetwork.playerName)) {
				PhotonNetwork.playerName = "Guest" + Random.Range (1, 9999);
			}*/
			//yield return new WaitUntil (PhotonNetwork.connected);
			//PhotonNetwork.CreateRoom("Test Room", new RoomOptions() { MaxPlayers = 10 }, null);
	//	}

		if (!PhotonNetwork.connected) {
			PhotonNetwork.LoadLevel("Lobby");
			return;
		}

//		Team1.GetComponent<RectTransform> ().sizeDelta = new Vector2 (Screen.width * 0.6f, Screen.height * 0.4f);
//		Team2.GetComponent<RectTransform> ().sizeDelta = new Vector2 (Screen.width * 0.6f, Screen.height * 0.4f);
//		MenuBar.GetComponent<RectTransform> ().sizeDelta = new Vector2 (Screen.width * 0.6f, Screen.height * 0.2f);
//		MapInfo.GetComponent<RectTransform> ().sizeDelta = new Vector2 (Screen.width * 0.4f, Screen.height);

		players = new List<GameObject>();
		for (int i = 0; i < PhotonNetwork.playerList.Length; i++) {
			PhotonPlayer player = PhotonNetwork.playerList[i];

			if(player.GetTeam() == null){
				player.SetTeam (PunTeams.Team.blue);
			}

			addPlayer(player.name, player.GetTeam());
		}

		PhotonNetwork.automaticallySyncScene = true;

		if (PhotonNetwork.isMasterClient) {
			GameObject startButton = GameObject.Find("Canvas/MenuBar/Start");
			startButton.GetComponent<Button>().interactable = true;
			Map.interactable = true;
			GameMode.interactable = true;
			MaxKills.interactable = true;
			MaxPlayers.interactable = true;
			MaxTime.interactable = true;
		}

		//set default team
		if(PhotonNetwork.player.GetTeam()==PunTeams.Team.none){
			PhotonNetwork.player.SetTeam (PunTeams.Team.blue);
		}

		LoadRoomInfo ();//Update the caption text
	}

	private void addPlayer(string name, PunTeams.Team team) {//addPlayer also setTeam
		GameObject lobbyPlayer = PhotonNetwork.Instantiate (LobbyPlayer.name, transform.position, Quaternion.identity, 0);

		if (team == PunTeams.Team.blue || team == PunTeams.Team.none) {
			lobbyPlayer.transform.SetParent (Team1.transform);
		} else {
			lobbyPlayer.transform.SetParent (Team2.transform);
		}

		lobbyPlayer.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
		lobbyPlayer.name = name;
		lobbyPlayer.GetComponentInChildren<Text> ().text = name;

		players.Add(lobbyPlayer);
	}

	public void SwitchTeamBlue(){
		if(callStartgame){
			return;
		}
		if(PunTeams.Team.blue == PhotonNetwork.player.GetTeam()){
			return;
		}else{
			PhotonNetwork.player.SetTeam (PunTeams.Team.blue);
			photonView.RPC ("SwitchTeam",PhotonTargets.All, PhotonNetwork.player.name, 0);
		}
	}
	public void SwitchTeamRed(){
		if(callStartgame){
			return;
		}
		if(PunTeams.Team.red == PhotonNetwork.player.GetTeam()){
			return;
		}else{
			PhotonNetwork.player.SetTeam (PunTeams.Team.red);
			photonView.RPC ("SwitchTeam",PhotonTargets.All,  PhotonNetwork.player.name, 1);
		}
	}

	// Update is called once per frame
	void Update () {
	
	}

	public void StartGame() {
		if(callStartgame){
			return;
		}
		Debug.Log ("Starting game");
		PhotonNetwork.room.open = false;//join mid game

		ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
		h.Add ("GameInit", false);
		PhotonNetwork.room.SetCustomProperties (h);

		photonView.RPC ("CallStartGame", PhotonTargets.AllBuffered);

		Invoke ("MasterLoadLevel", 3f);
		//PhotonNetwork.LoadLevel(PhotonNetwork.room.CustomProperties["Map"].ToString());
	}

	void MasterLoadLevel(){
		PhotonNetwork.LoadLevel(PhotonNetwork.room.CustomProperties["Map"].ToString());
	}

	public void LeaveGame() {
		Debug.Log("Leaving game");
		PhotonNetwork.LeaveRoom();
		PhotonNetwork.LoadLevel("Lobby");
	}

	public void OnPhotonPlayerConnected(PhotonPlayer newPlayer) {
		Debug.Log("Player connected: " + newPlayer.name);
		addPlayer(newPlayer.name, newPlayer.GetTeam());
	}

	public void OnPhotonPlayerDisconnected(PhotonPlayer disconnectedPlayer) {
		Debug.Log ("Player disconnected: " + disconnectedPlayer.name);
		GameObject player = null;
		foreach (GameObject lobbyPlayer in players) {
			if (lobbyPlayer.name == disconnectedPlayer.name) {
				player = lobbyPlayer;
			}
		}

		if(player!=null){
			PhotonNetwork.Destroy(player);
			players.Remove(player);
		}
	}

	public void OnMasterClientSwitched(PhotonPlayer newMaster){
		Debug.Log ("Master switched.");
		InRoomChat.AddLine ("The Master is switched to " + newMaster.NickName);
		if (PhotonNetwork.isMasterClient) {
			GameObject startButton = GameObject.Find("Canvas/MenuBar/Start");
			startButton.GetComponent<Button>().interactable = true;
			Map.interactable = true;
			GameMode.interactable = true;
			MaxKills.interactable = true;
			MaxPlayers.interactable = true;
			MaxTime.interactable = true;
		}
	}
	public void LoadRoomInfo(){
		Map.captionText.text = PhotonNetwork.room.CustomProperties["Map"].ToString();
		MaxTime.captionText.text = PhotonNetwork.room.CustomProperties["MaxTime"].ToString();
		MaxPlayers.captionText.text = PhotonNetwork.room.MaxPlayers.ToString() ;
		MaxKills.captionText.text = PhotonNetwork.room.CustomProperties["MaxKills"].ToString();
		GameMode.captionText.text = PhotonNetwork.room.CustomProperties["GameMode"].ToString();
	}
		
	public void ChangeMap() {
		photonView.RPC("ChangeMap", PhotonTargets.All, Map.captionText.text);

		ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
		h.Add ("Map", Map.captionText.text);
		PhotonNetwork.room.SetCustomProperties (h);
	}

	public void ChangeGameMode() {
		photonView.RPC("ChangeGameMode", PhotonTargets.All, GameMode.captionText.text);

		ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
		h.Add ("GameMode", GameMode.captionText.text);
		PhotonNetwork.room.SetCustomProperties (h);
	}

	public void ChangeMaxKills() {
		photonView.RPC("ChangeMaxKills", PhotonTargets.All, MaxKills.captionText.text);

		ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
		h.Add ("MaxKills", int.Parse(MaxKills.captionText.text));
		PhotonNetwork.room.SetCustomProperties (h);
	}

	public void ChangeMaxPlayers() {
		photonView.RPC("ChangeMaxPlayers", PhotonTargets.All, MaxPlayers.captionText.text);
		PhotonNetwork.room.MaxPlayers = int.Parse (MaxPlayers.captionText.text);
	}

	public void ChangeMaxTime() {
		photonView.RPC("ChangeMaxTime", PhotonTargets.All, MaxTime.captionText.text);

		ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
		h.Add ("MaxTime", int.Parse(MaxTime.captionText.text));
		PhotonNetwork.room.SetCustomProperties (h);
	}

	// RPCs
	[PunRPC]
	public void ChangeMap(string map) {
		int i = Array.IndexOf(Maps, map);
		Map.captionText.text = map;
	}

	[PunRPC]
	public void ChangeMaxTime(string time) {
		MaxTime.captionText.text = time;
	}

	// Not implemented yet
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
	public void SwitchTeam(string name, int teamID){
		GameObject playerToDestroy = null;
		foreach (GameObject lobbyPlayer in players) {
			if (lobbyPlayer.name == name) {
				playerToDestroy = lobbyPlayer;
				break;
			}
		}
		if (playerToDestroy != null) {
			PhotonNetwork.Destroy (playerToDestroy);
			players.Remove (playerToDestroy);
		}

		addPlayer (name, (teamID == 0) ? PunTeams.Team.blue : PunTeams.Team.red);
	}

	[PunRPC]
	void CallStartGame(){
		callStartgame = true;
		InRoomChat.AddLine ("Game will start in 3 sec.");
	}
}
