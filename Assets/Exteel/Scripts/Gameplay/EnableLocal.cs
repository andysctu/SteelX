using UnityEngine;

public class EnableLocal : MonoBehaviour {
    [SerializeField] private GameObject mainCamera, crosshairImage,radar;

    private void Awake() {
        EnableComponents(GetComponent<PhotonView>().isMine);
        Destroy(this);
    }

    private void EnableComponents(bool b) {
        crosshairImage.SetActive(b);
        mainCamera.GetComponent<Camera>().enabled = b;
        mainCamera.GetComponent<MechCamera>().enabled = b;
        mainCamera.GetComponentInChildren<Canvas>().enabled = b;
        mainCamera.GetComponent<Crosshair>().enabled = b;

        radar.SetActive(b);
    }
}