using UnityEngine;

public class EnableLocal : MonoBehaviour {

	[SerializeField] private Camera cam, radar;
    private Canvas canvas;//setting the screen space

    void Start () {//TODO : tidy up 

		if (!GetComponent<PhotonView>().isMine) {
            cam.transform.Find("Canvas/HeatBar").gameObject.SetActive(false);
            return;
        }

		// Enable mech controller
		GetComponent<MechController>().enabled = true;

        // Enable camera/radar
        cam.enabled = true;
        radar.enabled = true;

        GameObject PanelCanvas = GameObject.Find("PanelCanvas");
        if (PanelCanvas != null) {
            canvas = PanelCanvas.GetComponent<Canvas>();
            canvas.worldCamera = cam;
            canvas.planeDistance = 1;
        } else {
            Debug.LogError("Can't find PanelCanvas");
        }

        cam.GetComponent<MechCamera>().enabled = true;
		GetComponentInChildren<AudioListener> ().enabled = true;

        cam.GetComponent<Crosshair>().enabled = true;

        cam.transform.Find("Canvas/HeatBar").gameObject.SetActive(true);

        GameObject crossHairImage = cam.transform.Find("Canvas/CrosshairImage").gameObject;
		crossHairImage.SetActive(true);
	}
}
