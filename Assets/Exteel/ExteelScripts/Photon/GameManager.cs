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
	[SerializeField] GameObject RedTeam,BlueTeam;
	public InRoomChat InRoomChat;
	public Transform[] SpawnPoints;

	public int MaxTimeInSeconds = 300;
	public int MaxKills = 2;
	public int CurrentMaxKills = 0;

	private int timerDuration;
	private int currentTimer = 999;

	private bool showboard = false;
	private HUD hud;
	private Camera cam;
	private bool gameEnding = false;
	private bool OnSyncTimeRequest = false;
	private bool IsMasterInitGame = false; 
	private bool OnCheckInitGame = false;

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

		//Check team mode
		if(GameInfo.GameMode.Contains("Team") || GameInfo.GameMode.Contains("Capture")){
			Debug.Log ("Team mode is on.");
			isTeamMode = true;
		}else{
			Debug.Log ("Team mode is off.");
			RedTeam.SetActive (false);
			isTeamMode = false;
		}

		//If is master client , initialize room's properties ( team score, ... )
		if (PhotonNetwork.isMasterClient) {
			MasterInitGame();
			SyncTime();
		}

		// client ini himself
		ExitGames.Client.Photon.Hashtable h2 = new ExitGames.Client.Photon.Hashtable ();
		h2.Add ("Kills", 0);
		h2.Add ("Deaths", 0);
		PhotonNetwork.player.SetCustomProperties (h2);
		InstantiatePlayer (PlayerPrefab.name, SpawnPoints [0].position, SpawnPoints [0].rotation, 0);
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
		/*
		ps.transform.Find("Kills").GetComponent<Text>().text = "0";
		ps.transform.Find("Deaths").GetComponent<Text>().text = "0";
		*/
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
				ps.transform.SetParent(BlueTeam.transform);
			}else{
				ps.transform.SetParent(RedTeam.transform);
			}
		}else
			ps.transform.SetParent(Scoreboard.transform.Find("Blue").transform);

		ps.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
		playerScorePanels.Add(name, ps);
	}

	void MasterInitGame(){

		ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
		h.Add ("BlueKills", 0);
		h.Add ("RedKills", 0);
		h.Add ("BlueScore", 0);
		h.Add ("RedScore", 0);
		PhotonNetwork.room.SetCustomProperties (h);

		ExitGames.Client.Photon.Hashtable h2 = new ExitGames.Client.Photon.Hashtable ();
		h2.Add ("Kills", 0);
		h2.Add ("Deaths", 0);

		ExitGames.Client.Photon.Hashtable h3 = new ExitGames.Client.Photon.Hashtable ();
		h3.Add ("GameInit", true);
		PhotonNetwork.room.SetCustomProperties (h3);

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

	public void RegisterKill(string shooter, string victim) {
		//Display Log on UI
		int shooterID,victimID;
		if(PhotonNetwork.isMasterClient){
			ExitGames.Client.Photon.Hashtable h2 = new ExitGames.Client.Photon.Hashtable ();
			h2.Add ("Kills", playerScores[shooter].Kills + 1);

			ExitGames.Client.Photon.Hashtable h3 = new ExitGames.Client.Photon.Hashtable ();
			h3.Add ("Deaths", playerScores[victim].Deaths + 1 );

			foreach(PhotonPlayer player in PhotonNetwork.playerList){
				if(player.NickName == shooter){
					player.SetCustomProperties (h2);
				}else if(player.NickName == victim){
					player.SetCustomProperties (h3);
				}
			}
			/*
			PhotonView pv = PhotonView.Find (viewID);
			ExitGames.Client.Photon.Hashtable h2 = new ExitGames.Client.Photon.Hashtable ();
			h2.Add ("Kills", playerScores[shooter].Kills + 1);
			h2.Add ("Deaths", playerScores[shooter].Deaths);

			ExitGames.Client.Photon.Hashtable h2 = new ExitGames.Client.Photon.Hashtable ();
			h2.Add ("Kills", playerScores[shooter].Kills + 1);
			h2.Add ("Deaths", playerScores[shooter].Deaths);*/


		}
		Debug.Log(shooter + " killed " + victim);
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
		Debug.Log (shooter + " has " + playerScores [shooter].Kills + " kills.");
		Debug.Log (victim + " has " + playerScores [victim].Deaths + " deaths.");

		if (newShooterScore.Kills > CurrentMaxKills) CurrentMaxKills = newShooterScore.Kills;
	}

	public bool GameOver() {
		if (storedStartTime != 0 && storedDuration != 0) {
			if (currentTimer <= 0) {
				print ("end game becuase of currentTimer");
				return false; // temp.
				//return true;
			} else {
				return CurrentMaxKills >= MaxKills;
			}
		} else
			return false;
	}
	void OnPhotonPlayerConnected(PhotonPlayer newPlayer){
		//if (!PhotonNetwork.isMasterClient)
		//	return;
		InRoomChat.AddLine ("player connected : " + newPlayer);
	}

	void OnPhotonPlayerDisconnected(PhotonPlayer player){
		InRoomChat.AddLine ("player disconnected : " + player);
		playerScorePanels.Remove (player.NickName);
		playerScores.Remove (player.NickName);
		Text[] Ts = Scoreboard.GetComponentsInChildren<Text> ();

		foreach(Text text in Ts){
			if(text.text == player.NickName){
				Destroy (text.transform.parent.gameObject);
				break;
			}
		}
	}

}
