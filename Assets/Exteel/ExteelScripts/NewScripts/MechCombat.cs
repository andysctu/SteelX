using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MechCombat : NetworkBehaviour {

	public Transform camTransform;
	public float range = Mathf.Infinity;
	public int damage = 25;

	[SyncVar]
	public float MaxHP = 100.0f;
	[SyncVar]
	public float CurrentHP;

	[SyncVar]
	private bool isDead;

	[SyncVar]
	public Score score;


	private RaycastHit hit;
	private GameManager gm;


	void Start() {
		CurrentHP = MaxHP;
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
	}
	
	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer) return;
		if (Input.GetKeyDown(KeyCode.Mouse0)){
			CmdFireRaycast(camTransform.TransformPoint(0,0,0.5f), camTransform.forward);
		}

		if (isDead && Input.GetKeyDown(KeyCode.R)) {
			CmdEnablePlayer();
		}
	}

	[Server]
	void OnHit(uint shooterId, float d) {
		if (isDead) return;
		CurrentHP -= d;
		if (CurrentHP <= 0) {
			RpcDisablePlayer();
			isDead = true;
			if (gm == null) {
				Debug.Log("gm is null 27");
				gm = GameObject.Find("GameManager").GetComponent<GameManager>();
				if (gm == null) {
					Debug.Log("gm still null 27");
				} else {
					Debug.Log("gm not null 27 2nd time");
					RegisterKill(shooterId, gameObject.GetComponent<NetworkIdentity>().netId.Value);
				}
			} else {
				Debug.Log("gm not null 27 first time");
				RegisterKill(shooterId, gameObject.GetComponent<NetworkIdentity>().netId.Value);
			}
		}
	}

	[ClientRpc]
	void RpcDisablePlayer() {
		gameObject.layer = 0;
		GetComponent<NewMechController>().enabled = false;
		GetComponentInChildren<Crosshair>().enabled = false;
		Renderer[] renderers = GetComponentsInChildren<Renderer> ();
		foreach (Renderer renderer in renderers) {
			renderer.enabled = false;
		}
	}

	[ClientRpc]
	void RpcEnablePlayer() {
		gameObject.layer = 8;
		Renderer[] renderers = GetComponentsInChildren<Renderer> ();
		foreach (Renderer renderer in renderers) {
			renderer.enabled = true;
		}
		if (!isLocalPlayer) return;
		GetComponent<NewMechController>().enabled = true;
		GetComponentInChildren<Crosshair>().enabled = true;
	}

	[Command]
	void CmdEnablePlayer() {
		isDead = false;
		CurrentHP = MaxHP;
		RpcEnablePlayer();
	}

	[Command]
	void CmdFireRaycast(Vector3 start, Vector3 direction){
		Debug.Log("fired");
		if (Physics.Raycast (start, direction, out hit, range)){
			Debug.Log (hit.transform.tag);
			if (hit.transform.tag == "Player"){
				string uIdentity = hit.transform.name;
				Debug.Log (uIdentity + " was shot");
				hit.transform.gameObject.GetComponent<MechCombat>().OnHit(gameObject.GetComponent<NetworkIdentity>().netId.Value, damage);
			} else if (hit.transform.tag == "Drone"){
				string uIdentity = hit.transform.name;
				Debug.Log("Drone was shot");
			}
		}
	}



	[Server]
	public void RegisterKill(uint shooterId, uint victimId){
		int kills = gm.playerScores[shooterId].IncrKill();
		int deaths = gm.playerScores[victimId].IncrDeaths();
		Debug.Log(string.Format("Server: Registering a kill {0}, {1}, {2}, {3} ", shooterId, victimId, kills, deaths));
		if (isServer) {
			RpcUpdateScore(shooterId, victimId, kills, deaths);
		} else {
			Debug.Log("not serverr");
		}
	}
		
	[ClientRpc]
	void RpcUpdateScore(uint shooterId, uint victimId, int kills, int deaths){
		Debug.Log( string.Format("Client: Updating score {0}, {1}, {2}, {3} ", shooterId, victimId, kills, deaths)); 
		Score shooterScore = gm.playerScores[shooterId];
		Score victimScore = gm.playerScores[victimId];
		Score newShooterScore = new Score();
		Score newVictimScore = new Score();
		newShooterScore.Kills = kills;
		newShooterScore.Deaths = shooterScore.Deaths;
		newVictimScore.Deaths = deaths;
		newVictimScore.Kills = victimScore.Kills;
		gm.playerScores[shooterId] = newShooterScore;
		gm.playerScores[victimId] = newVictimScore;
	}
}
