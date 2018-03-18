using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayPlayerInfo : MonoBehaviour {
	[SerializeField]private TextMesh textMesh;
	[SerializeField]Canvas barcanvas;
	[SerializeField]Image bar;

	MechCombat mcbt;
	Camera cam;
	GameObject player;
	private string text ; //name
	private float LastInitRequestTime;

	void Start () {
		mcbt = transform.root.GetComponent<MechCombat> ();
		text = transform.root.GetComponent<PhotonView> ().owner.NickName;
		textMesh.text = text;
		textMesh.color = new Color32 (255, 0, 0, 255);

		if(GameManager.isTeamMode){
			if(PhotonNetwork.player.GetTeam() != transform.root.GetComponent<PhotonView> ().owner.GetTeam()){
				bar.color = new Color32 (255, 0, 0, 255); //enemy
				textMesh.color = new Color32 (255, 0, 0, 255); 
			} else {
				bar.color = new Color32 (223, 234, 11, 255);
				textMesh.color = new Color32 (255, 255, 255, 255);
			}
		}else{
			bar.color = new Color32 (255, 0, 0, 255); //enemy
			textMesh.color = new Color32 (255, 0, 0, 255); 

		}
	}
	

	void Update () {
		if (cam != null) {
			transform.LookAt (cam.transform);

			//update scale
			float distance = Vector3.Distance (transform.position, cam.transform.position);
			distance = Mathf.Clamp (distance, 0, 200f);
			transform.localScale = new Vector3 (1 + distance / 100 * 1.5f, 1 + distance / 100 * 1.5f, 1);
		}else{
			if (Time.time - LastInitRequestTime >0.5f) {
				player = GameObject.Find (PhotonNetwork.playerName);
				if (player != null)
					cam = player.GetComponentInChildren<Camera> ();
				LastInitRequestTime = Time.time;
			}
		}

		//update bar value
		bar.fillAmount = (float)mcbt.CurrentHP () / mcbt.GetMaxHp ();
	}
}
