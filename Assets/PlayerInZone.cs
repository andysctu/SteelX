using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInZone : MonoBehaviour {

	public int player_viewID;
	public int healAmount = 200;
	private int playerCount = 0;
	private bool isThePlayerInside = false;
	List<Collider> players = new List<Collider>();
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void OnTriggerEnter (Collider collider) {
		if(collider.transform.root.GetComponent<PhotonView>().viewID == player_viewID){
			print("the player has entered the pool.");
			isThePlayerInside = true;
		}
		players.Add (collider);
		playerCount++;
	}

	void OnTriggerExit(Collider collider){
		if(collider.transform.root.GetComponent<PhotonView>().viewID == player_viewID){
			print("the player has left the pool.");
			isThePlayerInside = false;
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
	//public int getBlueTeamPlayerCount(){
		
	//}

//	public int getRedTeamPlayerCount(){

//	}

	public bool IsThePlayerInside(){
		return isThePlayerInside;
	}

}
