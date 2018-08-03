using UnityEngine;
using UnityEngine.UI;

public class DMMsgDisplayer : MonoBehaviour {
    [SerializeField] Image WaitOtherPlayer;
    [SerializeField] Text Ping;

    private int ping;

    private void FixedUpdate() {
        ping = PhotonNetwork.GetPing();
        Ping.text = "Ping : " + ping;

        if (ping < 100) {
            Ping.color = Color.green;
        } else if (ping < 200) {
            Ping.color = Color.yellow;
        } else {
            Ping.color = Color.red;
        }
    }

    public void ShowWaitOtherPlayer(bool b) {
        WaitOtherPlayer.enabled = b;
    }

    public void ShowGameOver() {

    }
}