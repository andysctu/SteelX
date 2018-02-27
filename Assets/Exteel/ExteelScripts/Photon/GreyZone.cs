using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GreyZone : MonoBehaviour {

	int curZoneState = -1; //0:blue team ; 1:red team ; -1 : none
	int curBarState = -1; // same
	bool switchBarColor = true; // true : need to change color due to barstate change
	float coeff = 0.005f;
	private float trueAmount = 0;

	[SerializeField]Image bar, mark;
	[SerializeField]Sprite bar_blue, bar_blue1, bar_red, bar_red1; //bar_blue1 is the light color one
	[SerializeField]Sprite mark_blue, mark_red;
	[SerializeField]Sprite Panel_bluemark, Panel_redmark, Panel_greymark;
	[SerializeField]Material base_none, base_blue, base_red;
	[SerializeField]PlayerInZone PlayerInZone;
	[SerializeField]GameObject barCanvas;
	[SerializeField]Image Zone_mark_onPanel;
	Camera cam;
	GameObject player;
	PhotonView pv;

	private float LastInitRequestTime = 0;

	void Start () {
		pv = GetComponent<PhotonView> ();
		mark.enabled = false;
	}

	void Update () {
		if (cam != null) {
			barCanvas.transform.LookAt (new Vector3 (cam.transform.position.x, barCanvas.transform.position.y, cam.transform.position.z));

		}else{
			if (Time.time - LastInitRequestTime >0.5f) {
				player = GameObject.Find (PhotonNetwork.playerName);
				if (player != null) {
					cam = player.GetComponentInChildren<Camera> ();
				}
				LastInitRequestTime = Time.time;
			}
		}

		bar.fillAmount = Mathf.Lerp (bar.fillAmount, trueAmount, Time.deltaTime*10f);

		if(curZoneState==-1 && switchBarColor){
			if(curBarState == 0){
				bar.sprite = bar_blue1;
				bar.color = new Color32 (255, 255, 255, 255);
				switchBarColor = false;
			}else{
				bar.sprite = bar_red1;
				bar.color = new Color32 (255, 255, 255, 255);
				switchBarColor = false;
			}
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
		if (stream.isWriting) {
			if(PhotonNetwork.isMasterClient){
				if (PlayerInZone.whichTeamDominate () != -1) {
					
					if (curBarState == PlayerInZone.whichTeamDominate ()) {
						if (bar.fillAmount + PlayerInZone.PlayerCountDiff() * coeff >= 1) {
							bar.fillAmount = 1;
							if(curBarState != curZoneState){
								Debug.Log ("change to zone : " + curBarState);
								pv.RPC ("ChangeZone", PhotonTargets.All, curBarState);
							}
						} else {
							bar.fillAmount += PlayerInZone.PlayerCountDiff ()* coeff;
						}
					} else {
						if(bar.fillAmount - PlayerInZone.PlayerCountDiff() * coeff <=0){
							bar.fillAmount = 0;
							curBarState = PlayerInZone.whichTeamDominate ();

							Debug.Log ("change to zone : " + curBarState);
							pv.RPC ("ChangeZone", PhotonTargets.All, -1);//change to grey zone
						} else {
							bar.fillAmount -= PlayerInZone.PlayerCountDiff() * coeff;
						}
					}

				}
				trueAmount = bar.fillAmount;
				stream.SendNext (bar.fillAmount);
				stream.SendNext (curBarState);
			}

		}else{
			trueAmount = (float)stream.ReceiveNext ();
			curBarState = (int)stream.ReceiveNext ();


		}
	}

	[PunRPC]
	public void ChangeZone(int num){
		curZoneState = num;
		if(num==0){
			GetComponent<MeshRenderer> ().material = base_blue;
			bar.sprite = bar_blue;
			mark.enabled = true;
			mark.sprite = mark_blue;
			bar.color = new Color32 (20, 41, 182, 255);
		}else if(num==1){
			GetComponent<MeshRenderer> ().material = base_red;
			bar.sprite = bar_red;
			mark.enabled = true;
			mark.sprite = mark_red;
			bar.color = new Color32 (232, 34, 0, 255);
		}else{
			GetComponent<MeshRenderer> ().material = base_none;
			mark.sprite = null;
			mark.enabled = false;
			switchBarColor = true;
		}

		//change mark
		if(num==0){
			Zone_mark_onPanel.sprite = Panel_bluemark;
		}else if(num==1){
			Zone_mark_onPanel.sprite = Panel_redmark;
		}else{
			Zone_mark_onPanel.sprite = Panel_greymark;
		}


		//for new player to load
		if(PhotonNetwork.isMasterClient){
			ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable ();
			h.Add ("Zone", num);
			PhotonNetwork.room.SetCustomProperties (h);
		}
	}
}
