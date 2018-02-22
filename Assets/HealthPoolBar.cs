using System.Collections;
using System.Collections.Generic;
using Photon;
using UnityEngine;
using UnityEngine.UI;
public class HealthPoolBar : Photon.MonoBehaviour {
	
	[SerializeField] Image bar;
	[SerializeField] PlayerInZone PlayerInZone;
	Camera cam;
	GameObject player;
	MechCombat mechCombat;
	PhotonView player_pv;
	public bool isAvailable = true;
	private float LastInitRequestTime = 0;
	private float trueAmount = 1;
	// Use this for initialization
	void Start () {
		LastInitRequestTime = Time.time;
	}

	// Update is called once per frame
	void Update () {
		if (cam != null) {
			transform.LookAt (new Vector3 (cam.transform.position.x, transform.position.y, cam.transform.position.z));
			//transform.LookAt (cam.transform);

			bar.fillAmount = Mathf.Lerp (bar.fillAmount, trueAmount, Time.deltaTime*10f);
		}else{
			if (Time.time - LastInitRequestTime >0.5f) {
				player = GameObject.Find (PhotonNetwork.playerName);
				if (player != null) {
					cam = player.GetComponentInChildren<Camera> ();
					mechCombat = player.GetComponent<MechCombat> ();
				}
				LastInitRequestTime = Time.time;
			}
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
		if (stream.isWriting) {
			if(PhotonNetwork.isMasterClient){
				if(!isAvailable){
					bar.fillAmount += 0.005f;
					if(bar.fillAmount >= 1){
						isAvailable = true;
					}
				}

				if (bar.fillAmount > 0 && isAvailable) {
					bar.fillAmount -= PlayerInZone.getNotFullHPPlayerCount () * 0.005f;
				}

				if(bar.fillAmount<=0){
					isAvailable = false;
				}
				trueAmount = bar.fillAmount;
				stream.SendNext (bar.fillAmount);
				stream.SendNext (isAvailable);
			}
				
		}else{
			bar.fillAmount = (float)stream.ReceiveNext ();
			isAvailable = (bool)stream.ReceiveNext ();
			trueAmount = bar.fillAmount;
		}
	}


}
