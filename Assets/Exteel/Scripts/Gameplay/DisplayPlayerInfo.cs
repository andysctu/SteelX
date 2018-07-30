using UnityEngine;
using UnityEngine.UI;

public class DisplayPlayerInfo : DisplayInfo{
    private Text Text;
    private Image bar;
    private Combat Combat;
    private PhotonView pv;
    private string thisPlayerName;

    protected override void Awake() {
        InitComponents();
        pv = Combat.photonView;
        if (pv.isMine && tag != "Drone") {
            return;
        } else {
            base.Awake();
            RegisterOnMechEnabled();
        }         
    }

    private void InitComponents() {
        Combat = transform.root.GetComponent<Combat>();
        Text = GetComponentInChildren<Text>();        
        bar = FindBar();
    }

    private Image FindBar() {
        Image[] child_images = GetComponentsInChildren<Image>();
        foreach (Image i in child_images) {
            if (i.type == Image.Type.Filled) {
                return i;
            }
        }
        Debug.LogError("Can't find hp bar");
        return null;
    }

    private void RegisterOnMechEnabled() {
        Combat.OnMechEnabled += OnMechEnabled;
    }

    private void OnMechEnabled(bool b) {
        EnableDisplay(b);
    }

    protected override void Start() {       
        //Do not show my name & hp bar
        if(pv.isMine && tag != "Drone") {
            gameObject.SetActive(false);
            return;
        } else {
            gameObject.SetActive(true);
        }
        base.Start();

        //Init name
        thisPlayerName = (tag == "Drone") ? "Drone" + Random.Range(0, 999) : pv.owner.NickName;
        Text.text = thisPlayerName;

        //set the info name
        SetName(thisPlayerName + "_Infos");

        if (tag != "Drone" && GameManager.isTeamMode && PhotonNetwork.player.GetTeam() == pv.owner.GetTeam()) {
            bar.color = Color.white;
            Text.color = Color.white;
        } else {
            bar.color = Color.red; //enemy
            Text.color = Color.red;
        }        
    }

    private void Update() {
        //update bar value
        bar.fillAmount = (float)Combat.CurrentHP / Combat.MAX_HP;
    }
}