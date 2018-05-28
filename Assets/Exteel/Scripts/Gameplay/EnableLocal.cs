using System;
using UnityEngine;

public class EnableLocal : MonoBehaviour {

	[SerializeField]private Camera mainCamera, radar;
    [SerializeField]private GameObject HeatBar;
    private Canvas canvas;//setting the screen space

    void Start () {
        EnableComponents(GetComponent<PhotonView>().isMine);
	}

    private void EnableComponents(bool b) {
        HeatBar.SetActive(b);
        GetComponent<MechController>().enabled = b;

        mainCamera.enabled = b;
        mainCamera.GetComponent<MechCamera>().enabled = b;
        mainCamera.GetComponent<Crosshair>().enabled = b;
        GetComponentInChildren<AudioListener>().enabled = b;
        radar.enabled = b;

        Transform crossHairImage = mainCamera.transform.Find("Canvas/CrosshairImage");
        if(crossHairImage == null) {
            Debug.LogError("crosshairImage is null.");
        } else {
            crossHairImage.gameObject.SetActive(b);
        }
        
        if (GetComponent<PhotonView>().isMine) {
            GameObject PanelCanvas = GameObject.Find("PanelCanvas");
            if (PanelCanvas != null) {
                canvas = PanelCanvas.GetComponent<Canvas>();
                canvas.worldCamera = mainCamera;
                canvas.planeDistance = 1;
            } else {
                Debug.LogError("Can't find PanelCanvas");
            }
        }
    }
}
