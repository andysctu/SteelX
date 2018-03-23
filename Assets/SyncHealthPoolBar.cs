using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SyncHealthPoolBar : Photon.MonoBehaviour {

	[SerializeField] Sprite bar_green,bar_grey;
	[SerializeField] Image bar;
	[SerializeField] PlayerInZone PlayerInZone;
	[SerializeField] float increaseAmount = 0.001f;
	public bool isAvailable = true;
	private float trueAmount = 1;
	private const int GREEN = 0, GREY = 1;

	void Start () {
		if(isAvailable){//check state
			SetColor (GREEN);
		}else{
			SetColor (GREY);
		}
	}

	[PunRPC]
	void SetColor(int color){
		if(color == GREEN){
			isAvailable = true;
			bar.sprite = bar_green;
		}else{
			isAvailable = false;
			bar.sprite = bar_grey;
		}
	}

	void Update () {
		bar.fillAmount = Mathf.Lerp (bar.fillAmount, trueAmount, Time.deltaTime*10f);
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
		if (stream.isWriting) {
			if(PhotonNetwork.isMasterClient){
				if(!isAvailable){
					bar.fillAmount += increaseAmount;
					if(bar.fillAmount >= 1){
						isAvailable = true;
						photonView.RPC ("SetColor", PhotonTargets.All, GREEN);
					}
				}

				if (bar.fillAmount > 0 && isAvailable) {
					bar.fillAmount -= PlayerInZone.getNotFullHPPlayerCount () * increaseAmount;
				}

				if(bar.fillAmount<=0 && isAvailable){
					isAvailable = false;
					photonView.RPC ("SetColor", PhotonTargets.All, GREY);
				}
				trueAmount = bar.fillAmount;
				stream.SendNext (bar.fillAmount);
			}

		}else{
			trueAmount = (float)stream.ReceiveNext ();
		}
	}
}
