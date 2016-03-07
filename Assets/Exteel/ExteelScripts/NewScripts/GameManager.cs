using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour {
	[SyncVar] public float TimeLeft;

	public float MaxTime = 10f;
	private bool showboard = false;

	Dictionary<GameObject, Data> playerInfo;// = new Dictionary<GameObject, Data>();
	public Dictionary<uint, Score> playerScores;// = new Dictionary<uint, Score>();

	void Awake() {
		playerInfo = new Dictionary<GameObject, Data>();
		playerScores = new Dictionary<uint, Score>();
	}

	void Update() {
		showboard = Input.GetKey(KeyCode.Tab);
	}

	void OnGUI() {
		if (showboard) {
			GUILayout.BeginArea(new Rect(Screen.width/4, Screen.height/4, Screen.width/2, Screen.height/2), "scoreboard");
			foreach (KeyValuePair<uint, Score> entry in playerScores)
			{
				GUILayout.Label(entry.Key + ": kills = " + entry.Value.Kills + ", deaths = " + entry.Value.Deaths);
			}
			GUILayout.EndArea();
		}
	}

	[Server]
	public void RegisterPlayer(GameObject player, Data d){
		playerInfo.Add(player, d);
		playerScores.Add(player.GetComponent<NetworkIdentity>().netId.Value, new Score());
		int registeredPlayers = playerInfo.Count;
		int connectedPlayers = GameObject.Find("LobbyManager").GetComponent<NetworkLobbyManagerCustom>().numPlayers;
		Debug.Log("registered players so far: " + playerInfo.Count);
		Debug.Log("connected players: " + connectedPlayers);
		if (registeredPlayers == connectedPlayers){
			uint[] ids = new uint[playerInfo.Count];

			Debug.Log("All players registered, building mechs now");
			int i = 0;
			foreach (KeyValuePair<GameObject, Data> entry in playerInfo){
				ids[i] = entry.Key.GetComponent<NetworkIdentity>().netId.Value;
				Mech m = entry.Value.Mech;
				BuildMech mechBuilder = entry.Key.GetComponent<BuildMech>();
				Debug.Log("For player: " + entry.Key.name);
				Debug.Log("Has: " + m.Core + ", " + m.Arms + ", " + m.Legs + ", " + m.Head);
				mechBuilder.RpcBuildMech(m.Core, m.Arms, m.Legs, m.Head);
				mechBuilder.buildMech(m.Core, m.Arms, m.Legs, m.Head);
				mechBuilder.RpcInitScores(ids);
			}

			Debug.Log("Initializing scores");
			Debug.Log("Score count: " + playerScores.Count);
//			RpcInitScores(2);
		} 
	}

	[Server]
	public void RegisterKill(uint shooterId, uint victimId){
		int kills = playerScores[shooterId].IncrKill();
		int deaths = playerScores[victimId].IncrDeaths();
		RpcUpdateScore(shooterId, victimId, kills, deaths);
	}

	[ClientRpc]
	public void RpcInitScores(Dictionary<uint, Score> scores){
		Debug.Log("Client Score count: " + scores.Count);
		playerScores = scores;
	}

	[ClientRpc]
	void RpcUpdateScore(uint shooterId, uint victimId, int kills, int deaths){
		Score shooterScore = playerScores[shooterId];
		Score victimScore = playerScores[victimId];
		shooterScore.Kills = kills;
		victimScore.Deaths = deaths;
	}

}
