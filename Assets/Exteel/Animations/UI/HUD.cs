using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {

	[SerializeField] GameObject Placeholder;
	[SerializeField] Sprite Hit, Kill, Defense, GameOver;
	[SerializeField] GameObject WaitOtherPlayer;
	[SerializeField] GameObject GameStart;
    [SerializeField] Text Ping;

	public void ShowText(Camera cam, Vector3 p, string Text) {
		GameObject i = Instantiate(Placeholder, cam.WorldToScreenPoint(p), Quaternion.identity) as GameObject;
		if (Text != "GameOver") i.GetComponent<HUDText>().Set(cam, p);
		i.transform.SetParent(gameObject.transform);
		Image im = i.GetComponent<Image>();
		switch (Text) {
		case "Hit": im.sprite = Hit; break;
		case "Kill": im.sprite = Kill; break;
		case "Defense": im.sprite = Defense; break;
		case "GameOver": im.sprite = GameOver; i.GetComponent<HUDText>().enabled = false; break;
		}

		im.preserveAspect = true;
		im.SetNativeSize();
		i.transform.localScale = new Vector3(1,1,1);

		if (Text != "GameOver") Destroy(i, 0.5f);
	}

    public void ShowMultipleHitMsg(Camera cam, Transform target, Vector3 offset, string msg, int times, float interval){
        StartCoroutine(MultipleHitMsg(cam, target, offset, msg, times, interval));
    }

    IEnumerator MultipleHitMsg(Camera cam, Transform target, Vector3 offset, string msg, int times, float interval){
        for(int i = 0; i < times; i++){
            ShowText(cam, target.position + offset, msg);
            yield return new WaitForSeconds(interval);
        }
    }

	public void ShowWaitOtherPlayer(bool b){
		if(b){
			WaitOtherPlayer.SetActive (true);
		}else{
			WaitOtherPlayer.SetActive (false);
		}
	}

	public void ShowGameStart(){
		GameStart.SetActive(true);
	}

    private void FixedUpdate()
    {
        Ping.text = "Ping : " + PhotonNetwork.GetPing();
    }
}
