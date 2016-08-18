using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {
//	[SyncVar] public float TimeLeft;

	[SerializeField] GameObject PlayerPrefab;

	public float MaxTime = 10f;
	public int MaxKills = 2;
	public int CurrentMaxKills = 0;

	private bool showboard = false;


	public Dictionary<GameObject, Data> playerInfo;// = new Dictionary<GameObject, Data>();
	public Dictionary<uint, Score> playerScores;// = new Dictionary<uint, Score>();

	void Awake() {
		playerInfo = new Dictionary<GameObject, Data>();
		playerScores = new Dictionary<uint, Score>();
	}

	void Start() {
		GameObject player = PhotonNetwork.Instantiate (PlayerPrefab.name, new Vector3 (0, 0, 0), Quaternion.identity, 0);
		BuildMech mechBuilder = player.GetComponent<BuildMech>();
		Mech m = UserData.myData.Mech;
		mechBuilder.Build(m.Core, m.Arms, m.Legs, m.Head, m.Booster, m.Weapon1L, m.Weapon1R, m.Weapon2L, m.Weapon2R);
	}

	void Update() {
		showboard = Input.GetKey(KeyCode.Tab);
	}

	void OnGUI() {
		if (showboard || GameOver()) {
			GUILayout.BeginArea(new Rect(Screen.width/4, Screen.height/4, Screen.width/2, Screen.height/2));
			foreach (KeyValuePair<uint, Score> entry in playerScores)
			{
				GUILayout.Label("Player " + entry.Key + ": kills = " + entry.Value.Kills + ", deaths = " + entry.Value.Deaths);
			}
			GUILayout.EndArea();
		}
	}

	public bool GameOver(){
		return CurrentMaxKills >= MaxKills;
	}
}
