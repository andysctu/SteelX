using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : Photon.MonoBehaviour {
//	[SyncVar] public float TimeLeft;

	[SerializeField] GameObject PlayerPrefab;

	public float MaxTime = 10f;
	public int MaxKills = 2;
	public int CurrentMaxKills = 0;

	private bool showboard = false;

	public Dictionary<string, Score> playerScores;

	void Start() {
		GameObject player = PhotonNetwork.Instantiate (PlayerPrefab.name, new Vector3 (0, 0, 0), Quaternion.identity, 0);
		BuildMech mechBuilder = player.GetComponent<BuildMech>();
		Mech m = UserData.myData.Mech;
		mechBuilder.Build(m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R);
	}
		
	public void RegisterPlayer(string name) {
		if (playerScores == null) {
			playerScores = new Dictionary<string, Score>();
		}
		playerScores.Add (name, new Score ());
	}

	void Update() {
		showboard = Input.GetKey(KeyCode.Tab);
	}

	void OnGUI() {
		if (showboard || GameOver()) {
			GUILayout.BeginArea(new Rect(Screen.width/4, Screen.height/4, Screen.width/2, Screen.height/2));
			foreach (KeyValuePair<string, Score> entry in playerScores)
			{
				GUILayout.Label(entry.Key + ": kills = " + entry.Value.Kills + ", deaths = " + entry.Value.Deaths);
			}
			GUILayout.EndArea();
		}
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

		Debug.Log (shooter + " has " + playerScores [shooter].Kills + " kills.");
		Debug.Log (victim + " has " + playerScores [victim].Deaths + " deaths.");
	}

	public bool GameOver(){
		return CurrentMaxKills >= MaxKills;
	}
}
