using UnityEngine;

public class EnableLocal : MonoBehaviour {
    [SerializeField] private Camera mainCamera, radar;

    private void Start() {
        EnableComponents(GetComponent<PhotonView>().isMine);
    }

    private void EnableComponents(bool b) {

        radar.enabled = b;
    }
}