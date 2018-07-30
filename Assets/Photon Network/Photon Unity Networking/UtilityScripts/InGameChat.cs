using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PhotonView))]
public class InGameChat : Photon.MonoBehaviour {
    [SerializeField] private GameObject Content;
    [SerializeField] private InputField InputFieldChat;
    [SerializeField] private GameObject LinePrefab;
    private Image chat_image;
    private List<Text> lines = new List<Text>();
    private List<float> lineStartTimes = new List<float>();
    private List<int> removeIndexs = new List<int>();

    public float Duration = 7, decreaseAmount = 0.03f;
    public int maxLine = 3, maxLengthPerLine = 45;

    private void Awake() {
        chat_image = InputFieldChat.GetComponent<Image>();
    }

    public void OnEnterSend() {
        if (!PhotonNetwork.inRoom) return;

        //TODO : improve user input (double enter with null string)
        if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter)) {            
            if(string.IsNullOrEmpty(InputFieldChat.text)) {
                return;
            }
            string subText = (InputFieldChat.text.Length > maxLine * maxLengthPerLine) ? InputFieldChat.text.Substring(0, maxLine*maxLengthPerLine) : InputFieldChat.text;
            photonView.RPC("Chat", PhotonTargets.All, subText);
            this.InputFieldChat.text = "";
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            chat_image.enabled = !chat_image.enabled;
            if(chat_image.enabled)
                InputFieldChat.Select();
        }
    }

    private void FixedUpdate() {
        foreach (float f in lineStartTimes) {
            if (Time.time - f > Duration) {
                int index = lineStartTimes.IndexOf(f);
                Text text = lines[index];
                text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - decreaseAmount);
                
                if(text.color.a <= decreaseAmount) {
                    Destroy(text.gameObject);
                    removeIndexs.Add(index);
                }
            }
        }

        if (removeIndexs.Count != 0) {
            foreach (int i in removeIndexs) {
                lines.RemoveAt(i);
                lineStartTimes.RemoveAt(i);
            }
            removeIndexs.Clear();
        }
    }

    [PunRPC]
    public void Chat(string str, PhotonMessageInfo mi) {
        string senderName = "anonymous";

        if (mi.sender != null) {
            if (!string.IsNullOrEmpty(mi.sender.NickName)) {
                senderName = mi.sender.NickName;
            } else {
                senderName = "player " + mi.sender.ID;
            }
        }
        AddLine(senderName + " : " + str);
    }

    public void AddLine(string str, Color color = default(Color)) {
        GameObject g = Instantiate(LinePrefab, Content.transform);

        Text newline = g.GetComponent<Text>();
        int i;
        for(i = 0; i < maxLine && str.Length > maxLengthPerLine * (i + 1) ; i++);

        newline.rectTransform.sizeDelta = new Vector3(newline.rectTransform.sizeDelta.x, newline.rectTransform.sizeDelta.y * (i+1));
        newline.color = (color == default(Color))? Color.white : color;
        newline.text = str;

        lines.Add(newline);
        lineStartTimes.Add(Time.time);

        //TODO : remove this
        if(str.Contains("endGame")) {
            Debug.Log("set end game");
            FindObjectOfType<GameManager>().endGameImmediately = true;
        }
    }

    public void Clear() {
        foreach (Text t in lines) {
            if (t != null)
                Destroy(t);
        }
        lines.Clear();
        lineStartTimes.Clear();
    }
}