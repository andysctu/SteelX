using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayPlayerHP : MonoBehaviour {
	Camera cam;
	GameObject player;
	MechCombat mcbt;

	[SerializeField]
	Image bar;
	private float LastInitRequestTime = 0;

	void Start () {
		mcbt = transform.root.GetComponent<MechCombat> ();

		if(GameManager.isTeamMode){
			if(PhotonNetwork.player.GetTeam() != transform.root.GetComponent<PhotonView> ().owner.GetTeam())
				bar.color = new Color32 (255, 0, 0, 255); //enemy
			else 
				bar.color = new Color32 (223, 234, 11, 255);
		}else{
			bar.color = new Color32 (255, 0, 0, 255); //enemy
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(cam!=null)
			transform.LookAt (cam.transform);
		else{
			if (Time.time - LastInitRequestTime >0.5f) {
				player = GameObject.Find (PhotonNetwork.playerName);
				if (player != null) {
					cam = player.GetComponentInChildren<Camera> ();
				}
				LastInitRequestTime = Time.time;
			}
		}

		//update bar value
		bar.fillAmount = (float)mcbt.CurrentHP () / mcbt.GetMaxHp ();

	}
}
