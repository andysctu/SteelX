using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class CTFMsgDisplayer : MonoBehaviour{
    [SerializeField] private GameObject Msg;
    [SerializeField] private Image WaitOtherPlayer;
    [SerializeField] private Text Ping;
    [SerializeField] private Animator ObjectiveAnimator;
    private AudioSource AudioSource;
    private Text MsgText;
    private Image MsgImage;    
    //private PunTeams.Team thisPlayer_team = PunTeams.Team.none;

    private const float DisplayDuration = 3 ;    
    private Coroutine DisplayCoroutine;

    private int ping;

    private enum PingColor { Green, Yellow, Red, None };//For efficiency
    private PingColor curPingColor = PingColor.None;

    private AudioClip radioNoise;

    private void Awake() {
        InitComponents();
        GetThisPlayerTeam();
        LoadAudioClips();
    }

    private void InitComponents() {
        MsgText = Msg.GetComponentInChildren<Text>();
        MsgImage = Msg.GetComponent<Image>();
        AudioSource =GetComponent<AudioSource>();

        MsgText.enabled = false;
        MsgImage.enabled = false;
    }

    private void GetThisPlayerTeam() {
        //if (PhotonNetwork.player.GetTeam() == PunTeams.Team.red) {
        //    thisPlayer_team = PunTeams.Team.red;
        //} else {
        //    thisPlayer_team = PunTeams.Team.blue;
        //}
    }

    private void LoadAudioClips() {
        radioNoise = Resources.Load<AudioClip>("GFM/GFM_UI/Sound/Radio_noise");
    }

    private void FixedUpdate() {
        UpdatePingText();
    }

    private void UpdatePingText() {
        //ping = PhotonNetwork.GetPing();
        Ping.text = "Ping : " + ping;

        if (ping < 100) {
            if (curPingColor != PingColor.Green) {
                Ping.color = Color.green;
                curPingColor = PingColor.Green;
            }
        } else if (ping < 200) {
            if (curPingColor != PingColor.Yellow) {
                Ping.color = Color.yellow;
                curPingColor = PingColor.Yellow;
            }
        } else {
            if (curPingColor != PingColor.Red) {
                Ping.color = Color.red;
                curPingColor = PingColor.Red;
            }
        }
    }

    public void ShowWaitOtherPlayer(bool b) {
        WaitOtherPlayer.enabled = b;
    }

    public void OnGameStart() {
        ShowWaitOtherPlayer(false);
        ObjectiveAnimator.SetBool("start",true);
    }

    //public void ShowGameOver(PunTeams.Team winTeam) { 
    //    if(winTeam == thisPlayer_team) {
    //        //Display win
	//
    //    }else if(winTeam == PunTeams.Team.none) {
    //        //Display draw
	//
    //    } else {
    //        //Display Lose
	//
    //    }
    //}

    //public void PlayerGetFlag(PhotonPlayer player) {
    //    if(player.GetTeam() == thisPlayer_team) {
    //        DisplayMsg(player.NickName + " captured the enemy's flag.");
    //    } else {
    //        DisplayMsg("Our team flag was snatched.", Color.red);
    //    }
    //}

    //public void PlayerReturnFlag(PhotonPlayer player) {
    //    if (player.GetTeam() == thisPlayer_team) {
    //        DisplayMsg(player.NickName + " retrived our flag.");
    //    }
    //}

    //public void PlayerDroppedFlag(PhotonPlayer player) {
    //    if (player.GetTeam() == thisPlayer_team) {
    //        DisplayMsg(player.NickName + " lost the enemy's flag.", Color.red);
    //    }
    //}

    //public void PlayerGetScore(PhotonPlayer player) {
    //    if(player.GetTeam() == thisPlayer_team) {
    //        DisplayMsg(player.NickName + " succeeded in capturing the enemy's flag.");
	//
    //        DisplayMsg("We scored 1 point because of our success in capturing the enemy's flag.");
    //    } else {
    //        DisplayMsg("The enemy successfully took our team's flag and obtained 1 point.", Color.red);
    //    }
	//
    //    //The flag respawns after 10 seconds.
    //}

    //public void OnNeutralizingAerogate(PunTeams.Team team) {
    //    if (team != thisPlayer_team) {
    //        DisplayMsg("We are attempting to neutralizing the enemy's Aerogate.", Color.red);
    //    } else {
	//
    //    }
    //}

    //public void OnNeutralizedAerogate(PunTeams.Team pre_team) {
    //    if (pre_team == thisPlayer_team) {
    //        DisplayMsg("Our Aerogate has been neutralized.", Color.red);
    //    } else {
    //        DisplayMsg("We have occupied the enemy's Aerogate");
    //    }
    //}

    public void OnDefendAerogate() {
        DisplayMsg("We have successfully defended the Aerogate!");
    }

    private void DisplayMsg(string msg, Color color = default(Color), AudioClip clip = null) {
        if(AudioSource.isPlaying)
            AudioSource.Stop();

        //Set default clip
        if(clip == null) {
            clip = radioNoise;
        }

        AudioSource.clip = clip;
        AudioSource.Play();

        if(DisplayCoroutine != null) {
            StopCoroutine(DisplayCoroutine);
        }

        DisplayCoroutine = StartCoroutine(DisplayMsgCoroutine(msg, color));
    }

    IEnumerator DisplayMsgCoroutine(string msg, Color color = default(Color)) {
        MsgText.color = (color == default(Color)) ? Color.white : color;
        MsgText.text = msg;
        MsgText.enabled = true;
        MsgImage.enabled = true;

        yield return new WaitForSeconds(DisplayDuration);

        MsgText.enabled = false;
        MsgImage.enabled = false;
    }
}