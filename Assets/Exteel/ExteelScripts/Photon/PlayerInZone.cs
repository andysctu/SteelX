using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInZone : MonoBehaviour {

	public int player_viewID;
	public int healAmount = 200;
	private int playerCount = 0;
	private bool isThePlayerInside = false;
	private int blueTeamPlayerCount = 0, redTeamPlayerCount = 0;
	List<Collider> players = new List<Collider>();
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void OnTriggerEnter (Collider collider) {
		PhotonView pv = collider.transform.root.GetComponent<PhotonView> ();
		if(pv.viewID == player_viewID){
			isThePlayerInside = true;
		}
		if(pv.owner.GetTeam()==PunTeams.Team.red){
			redTeamPlayerCount++;
		}else{
			blueTeamPlayerCount++;
		}
		players.Add (collider);
		playerCount++;
	}

	void OnTriggerExit(Collider collider){
		PhotonView pv = collider.transform.root.GetComponent<PhotonView> ();
		if(pv.viewID == player_viewID){
			isThePlayerInside = false;
		}
		if(pv.owner.GetTeam()==PunTeams.Team.red){
			redTeamPlayerCount--;
		}else{
			blueTeamPlayerCount--;
		}
		players.Remove (collider);
		playerCount--;
	}
	public int getPlayerCount(){
		return playerCount;
	}

	public int getNotFullHPPlayerCount(){
		int tempCount = 0;
		foreach(Collider player in players){
			if(player.transform.root.GetComponent<MechCombat>().IsHpFull()){
				tempCount++;
			}
		}
		//Debug.Log ("Not full hp player count : " + (playerCount - tempCount));
		return playerCount - tempCount;
	}
	public int getBlueTeamPlayerCount(){
		return blueTeamPlayerCount;
	}

	public int getRedTeamPlayerCount(){
		return redTeamPlayerCount;
	}

	public int PlayerCountDiff(){
		if(whichTeamDominate()==0){
			return blueTeamPlayerCount - redTeamPlayerCount;
		}else{
			return redTeamPlayerCount - blueTeamPlayerCount;
		}
	}

	public int whichTeamDominate(){
		if (redTeamPlayerCount > blueTeamPlayerCount) {
			return 1;
		} else if (blueTeamPlayerCount > redTeamPlayerCount) {
			return 0;
		} else
			return -1;
	}

	public bool IsThePlayerInside(){
		return isThePlayerInside;
	}

}
