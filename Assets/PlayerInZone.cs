using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInZone : MonoBehaviour {
	public bool isSeperateTeam = false;
	private int playerCount = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void OnTriggerEnter () {
		playerCount++;
	}

	void OnTriggerExit(){
		playerCount--;
	}
	public int getPlayerCount(){
		return playerCount;
	}
}
