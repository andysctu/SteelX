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
	public InRoomChat InRoomChat;
	public Transform[] SpawnPoints;

	public int MaxTimeInSeconds = 300;
	public int MaxKills = 2;
	public int CurrentMaxKills = 0;

	private bool showboard = false;
	private HUD hud;
	private Camera cam;
	private bool gameEnding = false;

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
		GameInfo.MaxPlayers = int.Parse(PhotonNetwork.room.CustomProperties ["MaxPlayers"].ToString());
		GameInfo.MaxKills = int.Parse(PhotonNetwork.room.CustomProperties ["MaxKills"].ToString());
		GameInfo.MaxTime =  int.Parse(PhotonNetwork.room.CustomProperties ["MaxTime"].ToString());
		print ("GameInfo gamemode :" + GameInfo.GameMode + " MaxKills :" + GameInfo.MaxKills);

		if(GameInfo.GameMode.Contains("Team") || GameInfo.GameMode.Contains("Capture")){
			print ("Team mode is on.");
			isTeamMode = true;
		}else{
			print ("Team mode is off.");
			isTeamMode = false;
		}

		InRoomChat.enabled = true;
		MaxKills = GameInfo.MaxKills;
		MaxTimeInSeconds = GameInfo.MaxTime * 60;


		InstantiatePlayer (PlayerPrefab.name, SpawnPoints [0].position, SpawnPoints [0].rotation, 0);
		/*
		GameObject player = PhotonNetwork.Instantiate (PlayerPrefab.name, SpawnPoints[0].position, SpawnPoints[0].rotation, 0);
		//GameObject player = PlayerNetwork.instance.player;

		mechBuilder = player.GetComponent<BuildMech>();
		Mech m = UserData.myData.Mech;
		playerScorePanels = new Dictionary<string, GameObject>();
		mechBuilder.Build (m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R);


		cam = player.transform.Find("Camera").GetComponent<Camera>();
		hud = GameObject.Find("Canvas").GetComponent<HUD>();
		*/
		if (PhotonNetwork.isMasterClient) {
			SyncTime();
		}
	}
	public void InstantiatePlayer(string name, Vector3 StartPos, Quaternion StartRot, int group){
		GameObject player = PhotonNetwork.Instantiate (PlayerPrefab.name, SpawnPoints[0].position, SpawnPoints[0].rotation, 0);

		mechBuilder = player.GetComponent<BuildMech>();
		Mech m = UserData.myData.Mech;
		playerScorePanels = new Dictionary<string, GameObject>();
		mechBuilder.Build (m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R);

		cam = player.transform.Find("Camera").GetComponent<Camera>();
		hud = GameObject.Find("Canvas").GetComponent<HUD>();
	}
		
	public void RegisterPlayer(string name) {
		if (playerScores == null) {
			playerScores = new Dictionary<string, Score>();
		}
		playerScores.Add (name, new Score());

		GameObject ps = Instantiate (PlayerStat, new Vector3 (0, 0, 0), Quaternion.identity) as GameObject;
		ps.transform.Find("Pilot Name").GetComponent<Text>().text = name;
		ps.transform.Find("Kills").GetComponent<Text>().text = "0";
		ps.transform.Find("Deaths").GetComponent<Text>().text = "0";
		ps.transform.SetParent(Scoreboard.transform.Find("Team1").transform);
		ps.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
		playerScorePanels.Add(name, ps);
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
		if(!GameOver())
			Cursor.lockState = CursorLockMode.Locked;

		Scoreboard.SetActive(Input.GetKey(KeyCode.CapsLock));
		if (Input.GetKeyDown(KeyCode.Escape)) {
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
			PhotonNetwork.LeaveRoom();
			SceneManager.LoadScene("Lobby");
		}

		if (GameOver() && !gameEnding) {
			print ("called game over");
			gameEnding = true;
			Cursor.lockState = CursorLockMode.None;
			hud.ShowText(cam, cam.transform.position + new Vector3(0,0,0.5f), "GameOver");
			Scoreboard.SetActive(true);
			StartCoroutine(ExecuteAfterTime(3));
		}

		// Update time
		if (storedDuration != 0 && storedDuration != 0) {
			int timerDuration = (PhotonNetwork.ServerTimestamp - storedStartTime) / 1000;
			int currentTimer = storedDuration - timerDuration;

			int seconds = timerDuration % 60;
			int minutes = timerDuration / 60;
			Timer.text = minutes.ToString("D2") + ":" + seconds.ToString("D2");
		}
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
		return CurrentMaxKills >= MaxKills;
	}
	public void OnPhotonPlayerConnected(PhotonPlayer newPlayer){
		//if (!PhotonNetwork.isMasterClient)
		//	return;
		print ("player connected." + newPlayer);
	}

}
