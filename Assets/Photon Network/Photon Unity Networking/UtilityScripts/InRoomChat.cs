using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PhotonView))]
public class InRoomChat : Photon.MonoBehaviour {
    [SerializeField] private Text CurrentChannelText;
    [SerializeField] private InputField InputFieldChat;
    public int maxLength = 150;

    public void OnEnable() {
        CurrentChannelText.text = "";
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            InputFieldChat.Select();
        }
    }

    public void OnEnterSend() {
        if (!PhotonNetwork.inRoom) return;

        if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter)) {
            if(InputFieldChat.text == "")
                return;

            string subText = (InputFieldChat.text.Length > maxLength) ? InputFieldChat.text.Substring(0, maxLength) : InputFieldChat.text;

            photonView.RPC("Chat", PhotonTargets.All, subText);
            this.InputFieldChat.text = "";
        }
    }

    [PunRPC]
    public void Chat(string newLine, PhotonMessageInfo mi) {
        string senderName = "anonymous";

        if (mi.sender != null) {
            if (!string.IsNullOrEmpty(mi.sender.NickName)) {
                senderName = mi.sender.NickName;
            } else {
                senderName = "player " + mi.sender.ID;
            }
        }

        CurrentChannelText.text += senderName + ": " + newLine + '\n';
    }

    public void AddLine(string newLine) {
        CurrentChannelText.text += newLine + '\n';
    }

    public void Clear() {
        CurrentChannelText.text = "";
    }
}