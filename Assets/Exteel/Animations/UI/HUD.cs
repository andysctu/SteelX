using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {

	//[SerializeField] GameObject Placeholder;
	//[SerializeField] Sprite Hit, Kill, Defense;
    [SerializeField] private HUDText Hit, Kill, Defense;

    //Only display one hit msg at the same time

    private void Start() {
        
    }

    public void DisplayHit(Camera cam) {//this is called by the player who needs to see
        Hit.Display(cam);
    }

    public void DisplayDefense(Camera cam) {
        Defense.Display(cam);
    }

    public void DisplayKill(Camera cam) {
        Kill.Display(cam);
    }

    /*
    public void ShowText(Camera cam, string Text) {
		GameObject i = Instantiate(Placeholder, cam.WorldToScreenPoint(transform.position), Quaternion.identity) as GameObject;
		if (Text != "GameOver") i.GetComponent<HUDText>().Set(cam, transform.position);
		i.transform.SetParent(gameObject.transform);
		Image im = i.GetComponent<Image>();
		switch (Text) {
		case "Hit": im.sprite = Hit; break;
		case "Kill": im.sprite = Kill; break;
		case "Defense": im.sprite = Defense; break;
		//case "GameOver": im.sprite = GameOver; i.GetComponent<HUDText>().enabled = false; break;
		}

		im.preserveAspect = true;
		im.SetNativeSize();
		i.transform.localScale = new Vector3(1,1,1);

		if (Text != "GameOver") Destroy(i, 0.5f);
	}*/
}
