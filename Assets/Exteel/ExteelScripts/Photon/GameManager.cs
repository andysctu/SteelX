using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : Photon.MonoBehaviour {
	public static bool isTeamMode;
	public float TimeLeft;

	[SerializeField] GameObject PlayerPrefab;
	[SerializeField] GameObject Scoreboard;
	[SerializeField] GameObject PlayerStat;
	[SerializeField] Text Timer;
	[SerializeField] bool Offline;
	[SerializeField] GameObject Panel_RedTeam, Panel_BlueTeam;
	[SerializeField] GameObject RedScore, BlueScore;
	[SerializeField] Text RedScoreText, BlueScoreText;

	public InRoomChat InRoomChat;
	public Transform[] SpawnPoints;

	public int MaxTimeInSeconds = 300;
	public int MaxKills = 2;
	public int CurrentMaxKills = 0;
	private int bluescore = 0, redscore = 0;
	private int timerDuration;
	private int currentTimer = 999;

	private bool showboard = false;
	private HUD hud;
	private Camera cam;
	private bool gameEnding = false;
	private bool OnSyncTimeRequest = false;
	private bool IsMasterInitGame = false; 
	private bool OnCheckInitGame = false;
	private int sendTimes = 0;

	private Dictionary<string, GameObject> playerScorePanels;
	public Dictionary<string, Score> playerScores;

	int storedStartTime;
	int storedDuration;

	private BuildMech mechBuilder;
	float curtime;

	void Start() {
		if (Offline) {
			PhotonNetwork.offlineMode = true;
			PhotonNetwork.CreateRoom("offline");
			GameInfo.MaxKills = 1;
			GameInfo.MaxTime = 1;
		}

		//Load game info
		GameInfo.Map = PhotonNetwork.room.CustomProperties ["Map"].ToString();
		GameInfo.GameMode = PhotonNetwork.room.CustomProperties ["GameMode"].ToString();
		GameInfo.MaxKills = int.Parse(PhotonNetwork.room.CustomProperties ["MaxKills"].ToString());
		GameInfo.MaxTime =  int.Parse(PhotonNetwork.room.CustomProperties ["MaxTime"].ToString());
		GameInfo.MaxPlayers = PhotonNetwork.room.MaxPlayers;
		MaxKills = GameInfo.MaxKills;
		MaxTimeInSeconds = GameInfo.MaxTime * 60;
		InRoomChat.enabled = true;
		playerScorePanels = new Dictionary<string, GameObject>();

		Debug.Log ("GameInfo gamemode :" + GameInfo.GameMode + " MaxKills :" + GameInfo.MaxKills);

		//If is master client , initialize room's properties ( team score, ... )
		if (PhotonNetwork.isMasterClient) {
			MasterInitGame();
		}

		//Check team mode
		if(GameInfo.GameMode.Contains("Team") || GameInfo.GameMode.Contains("Capture")){
			Debug.Log ("Team mode is on.");
			isTeamMode = true;
		}else{
			Debug.Log ("Team mode is off.");
			//close Panel_RedTeam on Scorepanel
			Panel_RedTeam.SetActive (false);
			//close teamscores
			RedScore.SetActive (false);
			BlueScore.SetActive (false);
			isTeamMode = false;
		}
			
		StartCoroutine(LateStart());

		// client ini himself
		ExitGames.Client.Photon.Hashtable h2 = new ExitGames.Client.Photon.Hashtable ();
		h2.Add ("Kills", 0);
		h2.Add ("Deaths", 0);
		PhotonNetwork.player.SetCustomProperties (h2);

		InstantiatePlayer (PlayerPrefab.name, SpawnPoints [0].position, SpawnPoints [0].rotation, 0);
	}
		
	IEnumerator LateStart(){
		if(!IsMasterInitGame && sendTimes <10){
				sendTimes++;
				InRoomChat.AddLine ("Sending sync game request..." + sendTimes);
				yield return new WaitForSeconds (0.4f);
				SendSyncInitGameRequest ();
				yield return StartCoroutine (LateStart ());
		}else{
			if(sendTimes >= 10){
				InRoomChat.AddLine ("Failed to sync game properties. Is master disconnected ? ");
				Debug.Log ("master not connected");
			}else{
				InRoomChat.AddLine ("Game is sync.");
			}

			BlueScoreText.text = (PhotonNetwork.room.CustomProperties ["BlueScore"]==null)? "0" : PhotonNetwork.room.CustomProperties ["BlueScore"].ToString();
			RedScoreText.text = (PhotonNetwork.room.CustomProperties ["RedScore"]==null)? "0" : PhotonNetwork.room.CustomProperties ["RedScore"].ToString ();
			bluescore = int.Parse (BlueScoreText.text);
			redscore = int.Parse (RedScoreText.text);
		}
	}

	void SendSyncInitGameRequest(){
		if(bool.Parse(PhotonNetwork.room.CustomProperties["GameInit"].ToString()) ==true){
			IsMasterInitGame = true;
		}
	}

	public void InstantiatePlayer(string name, Vector3 StartPos, Quaternion StartRot, int group){
		GameObject player = PhotonNetwork.Instantiate (PlayerPrefab.name, SpawnPoints[0].position, SpawnPoints[0].rotation, 0);

		mechBuilder = player.GetComponent<BuildMech>();
		Mech m = UserData.myData.Mech;
		mechBuilder.Build (m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R);

		cam = player.transform.Find("Camera").GetComponent<Camera>();
		hud = GameObject.Find("Canvas").GetComponent<HUD>();
	}
		
	public void RegisterPlayer(int viewID, int teamID) {
		PhotonView pv = PhotonView.Find (viewID);
		string name;
		if(viewID == 2){//Drone
			name = "Drone";
		}else{
			name = pv.owner.NickName;
		}

		//bug : client is not ini. K/D even if ini. in start
		if(pv.isMine){
			ExitGames.Client.Photon.Hashtable h2 = new ExitGames.Client.Photon.Hashtable ();
			h2.Add ("Kills", 0);
			h2.Add ("Deaths", 0);
			PhotonNetwork.player.SetCustomProperties (h2);
		}

		if (playerScores == null) {
			playerScores = new Dictionary<string, Score>();
		}

		GameObject ps = Instantiate (PlayerStat, new Vector3 (0, 0, 0), Quaternion.identity) as GameObject;
		ps.transform.Find("Pilot Name").GetComponent<Text>().text = name;
		ps.transform.Find("Kills").GetComponent<Text>().text = "0";
		ps.transform.Find("Deaths").GetComponent<Text>().text = "0";

		Score score = new Score ();
		if (viewID != 2) {
			string kills, deaths;
			kills = pv.owner.CustomProperties ["Kills"].ToString ();
			deaths = pv.owner.CustomProperties ["Deaths"].ToString ();
			ps.transform.Find ("Kills").GetComponent<Text> ().text = kills;
			ps.transform.Find ("Deaths").GetComponent<Text> ().text = deaths;

			score.Kills = int.Parse(kills);
			score.Deaths = int.Parse(deaths);
		}
		playerScores.Add (name, score);

		if(isTeamMode){
			if(teamID == 0){
				ps.transform.SetParent(Panel_BlueTeam.transform);
			}else{
				ps.transform.SetParent(Panel_RedTeam.transform);
			}
		}else
			ps.transform.SetParent(Panel_BlueTeam.transform);

		ps.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
		playerScorePanels.Add(name, ps);
	}

	void MasterInitGame(){
		ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
		h.Add ("BlueScore", 0);
		h.Add ("RedScore", 0);
		PhotonNetwork.room.SetCustomProperties (h);

		h = new ExitGames.Client.Photon.Hashtable ();
		h.Add ("GameInit", true);//this is set to false when master pressing "start"
		PhotonNetwork.room.SetCustomProperties (h);

		SyncTime();
		IsMasterInitGame = true;
	}
		
	void SyncTime() {
		int startTime = PhotonNetwork.ServerTimestamp;
		ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable() { { "startTime", startTime }, { "duration", MaxTimeInSeconds } };
		Debug.Log("Setting " + startTime + ", " + MaxTimeInSeconds);
		PhotonNetwork.room.SetCustomProperties(ht);
	}

	public void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged) {
		if (propertiesThatChanged.ContainsKey("startTime") && propertiesThatChanged.ContainsKey("duration")) {
			storedStartTime = (int)propertiesThatChanged["startTime"];
			storedDuration = (int)propertiesThatChanged["duration"];
		}
	}

	void Update() {
		if (!GameOver ()) {
			Cursor.lockState = CursorLockMode.Locked;
			Scoreboard.SetActive(Input.GetKey(KeyCode.CapsLock));
		}
			
		if (Input.GetKeyDown(KeyCode.Escape)) {
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
			PhotonNetwork.LeaveRoom();
			SceneManager.LoadScene("Lobby");
		}
		// Update time
		if (storedStartTime != 0 && storedDuration != 0) {
			timerDuration = (PhotonNetwork.ServerTimestamp - storedStartTime) / 1000;
			currentTimer = storedDuration - timerDuration;

			int seconds = currentTimer % 60;
			int minutes = currentTimer / 60;
			Timer.text = minutes.ToString("D2") + ":" + seconds.ToString("D2");
		}else{
			if (!OnSyncTimeRequest)
				StartCoroutine (SyncTimeRequest(1f));
		}

		if (GameOver() && !gameEnding) {
			print ("called game over");
			gameEnding = true;
			Cursor.lockState = CursorLockMode.None;
			hud.ShowText(cam, cam.transform.position + new Vector3(0,0,0.5f), "GameOver");
			Scoreboard.SetActive(true);
			StartCoroutine(ExecuteAfterTime(3));
		}
	}

	IEnumerator SyncTimeRequest(float time){
		OnSyncTimeRequest = true;
		storedStartTime = int.Parse(PhotonNetwork.room.CustomProperties ["startTime"].ToString());
		storedDuration = int.Parse(PhotonNetwork.room.CustomProperties["duration"].ToString());
		yield return new WaitForSeconds (time);
		OnSyncTimeRequest = false;
	}

	IEnumerator ExecuteAfterTime(float time)
	{
		yield return new WaitForSeconds(time);

		//Final stage

		// Code to execute after the delay
		Cursor.visible = true;
		PhotonNetwork.LoadLevel("GameLobby");
	}

	public void RegisterKill(int shooter_viewID, int victim_viewID) {

		if(victim_viewID == 2){//drone
			return;
		}
		PhotonPlayer shooter_player = null,victime_player = null;
		shooter_player = PhotonView.Find (shooter_viewID).owner;
		victime_player = PhotonView.Find (victim_viewID).owner;

		string shooter = shooter_player.NickName, victim = victime_player.NickName;

		//only master update the room properties
		if(PhotonNetwork.isMasterClient){
			ExitGames.Client.Photon.Hashtable h2 = new ExitGames.Client.Photon.Hashtable ();
			h2.Add ("Kills", playerScores[shooter].Kills + 1);

			ExitGames.Client.Photon.Hashtable h3 = new ExitGames.Client.Photon.Hashtable ();
			h3.Add ("Deaths", playerScores[victim].Deaths + 1 );
			shooter_player.SetCustomProperties (h2);
			victime_player.SetCustomProperties (h3);

			if (GameInfo.GameMode == "Team Deathmatch") {
				if (shooter_player.GetTeam () == PunTeams.Team.blue || shooter_player.GetTeam () == PunTeams.Team.none) {
					bluescore++;
					BlueScoreText.text = bluescore.ToString();
					ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
					h.Add ("BlueScore", bluescore);
					PhotonNetwork.room.SetCustomProperties (h);
				}else{
					redscore++;
					RedScoreText.text = redscore.ToString();
					ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
					h.Add ("RedScore", redscore);
					PhotonNetwork.room.SetCustomProperties (h);
				}
			}
		}else{
			if(GameInfo.GameMode == "Team Deathmatch"){
				if (shooter_player.GetTeam () == PunTeams.Team.blue || shooter_player.GetTeam () == PunTeams.Team.none) {
					bluescore++;
					BlueScoreText.text = bluescore.ToString();
				} else {
					redscore++;
					RedScoreText.text = redscore.ToString();
				}
			}
		}
		Score newShooterScore = new Score ();
		newShooterScore.Kills = playerScores[shooter].Kills + 1;
		newShooterScore.Deaths = playerScores[shooter].Deaths;
		playerScores [shooter] = newShooterScore;

		Score newVictimScore = new Score ();
		newVictimScore.Kills = playerScores [victim].Kills;
		newVictimScore.Deaths = playerScores [victim].Deaths + 1;
		playerScores [victim] = newVictimScore;

		playerScorePanels [shooter].transform.Find("Kills").GetComponent<Text>().text = playerScores[shooter].Kills.ToString();
		playerScorePanels [victim].transform.Find("Deaths").GetComponent<Text>().text = playerScores[victim].Deaths.ToString();

		if (newShooterScore.Kills > CurrentMaxKills) CurrentMaxKills = newShooterScore.Kills;
	}

	public bool GameOver() {
		if (storedStartTime != 0 && storedDuration != 0) {
			if (currentTimer <= 0) {
				return true;
			} else {
				return CurrentMaxKills >= MaxKills;
			}
		} else
			return false;
	}
	void OnPhotonPlayerConnected(PhotonPlayer newPlayer){
		//if (!PhotonNetwork.isMasterClient)
		//	return;
		InRoomChat.AddLine (newPlayer + " is connected.");
	}

	void OnPhotonPlayerDisconnected(PhotonPlayer player){
		InRoomChat.AddLine (player + " is disconnected.");
		playerScorePanels.Remove (player.NickName);
		playerScores.Remove (player.NickName);

		//Remove datas from scoreboard
		Text[] Ts = Scoreboard.GetComponentsInChildren<Text> ();
		foreach(Text text in Ts){
			if(text.text == player.NickName){
				Destroy (text.transform.parent.gameObject);
				break;
			}
		}
	}

}
