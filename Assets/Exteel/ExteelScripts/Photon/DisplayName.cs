using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayName : MonoBehaviour {
	private string text ; //name
	[SerializeField]
	private TextMesh textMesh;
	Camera cam;
	GameObject player;
	private float LastInitRequestTime;
	// Use this for initialization
	void Start () {
		if (transform.root.GetComponent<PhotonView> ().isMine)
			gameObject.SetActive (false);
		
		LastInitRequestTime = Time.time;
		text = transform.root.GetComponent<PhotonView> ().owner.NickName;
		textMesh.text = text;
		textMesh.color = new Color32 (255, 0, 0, 255);
		if(GameManager.isTeamMode){
			print ("playerName color is on");
			if(PhotonNetwork.player.GetTeam() != transform.root.GetComponent<PhotonView> ().owner.GetTeam())
				textMesh.color = new Color32 (255, 0, 0, 255); //enemy
			else 
				textMesh.color = new Color32 (255, 255, 255, 255);
		}

	}
	
	// Update is called once per frame
	void Update () {
		if(cam!=null)
			transform.LookAt (cam.transform);
		else{
			if (Time.time - LastInitRequestTime >0.5f) {
				player = GameObject.Find (PhotonNetwork.playerName);
				if (player != null)
					cam = player.GetComponentInChildren<Camera> ();
				LastInitRequestTime = Time.time;
			}
			
		}
	}

}
