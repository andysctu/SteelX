using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : Photon.MonoBehaviour {
	public float TimeLeft;

	[SerializeField] GameObject PlayerPrefab;
	[SerializeField] GameObject Scoreboard;
	[SerializeField] GameObject PlayerStat;
	[SerializeField] bool Offline;

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

	void Start() {
		if (Offline) {
			PhotonNetwork.offlineMode = true;
			PhotonNetwork.CreateRoom("offline");
			GameInfo.MaxKills = 1;
			GameInfo.MaxTime = 1;
		}

		MaxKills = GameInfo.MaxKills;
		MaxTimeInSeconds = GameInfo.MaxTime * 60;

		GameObject player = PhotonNetwork.Instantiate (PlayerPrefab.name, SpawnPoints[0].position, SpawnPoints[0].rotation, 0);
		BuildMech mechBuilder = player.GetComponent<BuildMech>();
		Mech m = UserData.myData.Mech;
		mechBuilder.Build(m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R);
		playerScorePanels = new Dictionary<string, GameObject>();

		cam = player.transform.Find("Camera").GetComponent<Camera>();
		hud = GameObject.Find("Canvas").GetComponent<HUD>();

//		if (PhotonNetwork.isMasterClient) {
		SyncTime();
//		}
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
		if (!photonView.isMine)
			return;
		
		Scoreboard.SetActive(Input.GetKey(KeyCode.Tab));
		if (Input.GetKeyDown(KeyCode.Escape)) {
			Cursor.visible = true;
			PhotonNetwork.LeaveRoom();
			SceneManager.LoadScene("Lobby");
		}

		if (GameOver() && !gameEnding) {
			gameEnding = true;
			hud.ShowText(cam, cam.transform.position + new Vector3(0,0,0.5f), "GameOver");
			Scoreboard.SetActive(true);
			StartCoroutine(ExecuteAfterTime(3));
		}

		// Update time
		if (storedDuration != 0 && storedDuration != 0) {
			int timerDuration = (PhotonNetwork.ServerTimestamp - storedStartTime) / 1000;
			int currentTimer = storedDuration - timerDuration;

			Debug.Log(timerDuration + "   " + currentTimer);
		}
	}

	IEnumerator ExecuteAfterTime(float time)
	{
		yield return new WaitForSeconds(time);

		// Code to execute after the delay
		Cursor.visible = true;
		PhotonNetwork.LoadLevel("GameLobby");
	}

	public void RegisterKill(string shooter, string victim) {
		Debug.Log(shooter + " killed " + victim);
		Score newShooterScore = new Score ();
		newShooterScore.Kills = playerScores [shooter].Kills + 1;
		newShooterScore.Deaths = playerScores [shooter].Deaths;
		playerScores [shooter] = newShooterScore;

		Score newVictimScore = new Score ();
		newVictimScore.Kills = playerScores [victim].Kills;
		newVictimScore.Deaths = playerScores [victim].Deaths + 1;
		playerScores [victim] = newVictimScore;

		playerScorePanels [shooter].transform.Find("Kills").GetComponent<Text> ().text = playerScores [shooter].Kills.ToString();
		playerScorePanels [victim].transform.Find("Deaths").GetComponent<Text> ().text = playerScores [victim].Deaths.ToString();
		Debug.Log (shooter + " has " + playerScores [shooter].Kills + " kills.");
		Debug.Log (victim + " has " + playerScores [victim].Deaths + " deaths.");

		if (newShooterScore.Kills > CurrentMaxKills) CurrentMaxKills = newShooterScore.Kills;
	}

	public bool GameOver() {
		return CurrentMaxKills >= MaxKills;
	}
}
