using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : Photon.MonoBehaviour {
//	[SyncVar] public float TimeLeft;

	[SerializeField] GameObject PlayerPrefab;
	[SerializeField] GameObject Scoreboard;
	[SerializeField] GameObject PlayerStat;
	[SerializeField] bool Offline;

	public Transform[] SpawnPoints;

	public int MaxTime = 10;
	public int MaxKills = 2;
	public int CurrentMaxKills = 0;

	private bool showboard = false;
	private HUD hud;
	private Camera cam;
	private bool gameEnding = false;

	private Dictionary<string, GameObject> playerScorePanels;
	public Dictionary<string, Score> playerScores;

	void Start() {
		if (Offline) {
			PhotonNetwork.offlineMode = true;
			PhotonNetwork.CreateRoom("offline");
			GameInfo.MaxKills = 1;
		}

		MaxKills = GameInfo.MaxKills;

		GameObject player = PhotonNetwork.Instantiate (PlayerPrefab.name, SpawnPoints[0].position, SpawnPoints[0].rotation, 0);
		BuildMech mechBuilder = player.GetComponent<BuildMech>();
		Mech m = UserData.myData.Mech;
		mechBuilder.Build(m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R);
		playerScorePanels = new Dictionary<string, GameObject> ();

		cam = player.transform.Find("Camera").GetComponent<Camera>();
		hud = GameObject.Find("Canvas").GetComponent<HUD>();
	}
		
	public void RegisterPlayer(string name) {
		if (playerScores == null) {
			playerScores = new Dictionary<string, Score>();
		}
		playerScores.Add (name, new Score ());

		GameObject ps = Instantiate (PlayerStat, new Vector3 (0, 0, 0), Quaternion.identity) as GameObject;
		ps.transform.Find ("Pilot Name").GetComponent<Text> ().text = name;
		ps.transform.Find ("Kills").GetComponent<Text> ().text = "0";
		ps.transform.Find ("Deaths").GetComponent<Text> ().text = "0";
		ps.transform.SetParent(Scoreboard.transform.Find ("Team1").transform);
		ps.GetComponent<RectTransform> ().localScale = new Vector3 (1, 1, 1);
		playerScorePanels.Add (name, ps);
	}

	void Update() {
		Scoreboard.SetActive(Input.GetKey(KeyCode.Tab));
		if (Input.GetKeyDown(KeyCode.Escape)) {
			PhotonNetwork.LeaveRoom();
			SceneManager.LoadScene("Lobby");
			Cursor.visible = true;
		}

		if (GameOver() && !gameEnding) {
			gameEnding = true;
			hud.ShowText(cam, cam.transform.position + new Vector3(0,0,0.5f), "GameOver");
			StartCoroutine(ExecuteAfterTime(3));
			Cursor.visible = true;
		}
	}

	IEnumerator ExecuteAfterTime(float time)
	{
		yield return new WaitForSeconds(time);

		// Code to execute after the delay
		PhotonNetwork.LoadLevel("GameLobby");
		Cursor.visible = true;
	}

	public void RegisterKill (string shooter, string victim) {
		Debug.Log(shooter + " killed " + victim);
		Score newShooterScore = new Score ();
		newShooterScore.Kills = playerScores [shooter].Kills + 1;
		newShooterScore.Deaths = playerScores [shooter].Deaths;
		playerScores [shooter] = newShooterScore;

		Score newVictimScore = new Score ();
		newVictimScore.Kills = playerScores [victim].Kills;
		newVictimScore.Deaths = playerScores [victim].Deaths + 1;
		playerScores [victim] = newVictimScore;

		playerScorePanels [shooter].transform.Find ("Kills").GetComponent<Text> ().text = playerScores [shooter].Kills.ToString();
		playerScorePanels [victim].transform.Find ("Deaths").GetComponent<Text> ().text = playerScores [victim].Deaths.ToString();
		Debug.Log (shooter + " has " + playerScores [shooter].Kills + " kills.");
		Debug.Log (victim + " has " + playerScores [victim].Deaths + " deaths.");

		if (newShooterScore.Kills > CurrentMaxKills) CurrentMaxKills = newShooterScore.Kills;
	}

	public bool GameOver() {
		return CurrentMaxKills >= MaxKills;
	}
}
