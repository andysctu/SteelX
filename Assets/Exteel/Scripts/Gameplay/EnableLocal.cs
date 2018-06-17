using UnityEngine;

public class EnableLocal : MonoBehaviour {
    [SerializeField] private Camera mainCamera, radar;
    [SerializeField] private GameObject HeatBar, crosshairImage;

    private void Start() {
        EnableComponents(GetComponent<PhotonView>().isMine);
    }

    private void EnableComponents(bool b) {
        HeatBar.SetActive(b);
        crosshairImage.SetActive(b);
        GetComponent<MechController>().enabled = b;

        radar.enabled = b;
        mainCamera.enabled = b;
        mainCamera.GetComponent<MechCamera>().enabled = b;
        mainCamera.GetComponent<Crosshair>().enabled = b;
        GetComponentInChildren<AudioListener>().enabled = b;
    }
}