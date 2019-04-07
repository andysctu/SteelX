using UnityEngine;
using UnityEngine.UI;

public class GameMsgDisplayer : MonoBehaviour {
    [SerializeField] private GameObject WaitOtherPlayer;
    [SerializeField] private GameObject GameStart;
    [SerializeField] private Text Ping;

    private int ping;

    private void FixedUpdate() {
        //ping = PhotonNetwork.GetPing();
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
        WaitOtherPlayer.SetActive(b);
    }

    public void ShowGameStart() {
        GameStart.SetActive(true);
    }

    public void ShowGameOver() {
    }
}