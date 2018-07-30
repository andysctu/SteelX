using UnityEngine;

public class EnableLocal : MonoBehaviour {
    [SerializeField] private GameObject mainCamera, radar;

    private void Awake() {
        EnableComponents(GetComponent<PhotonView>().isMine);
        Destroy(this);
    }

    private void EnableComponents(bool b) {
        mainCamera.SetActive(b);
        radar.SetActive(b);
    }
}