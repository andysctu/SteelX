using UnityEngine;
using UnityEngine.UI;

public class DisplayPlayerInfo : MonoBehaviour {
    private TextMesh textMesh;
    private Image bar;
    private Combat Combat;
    private Camera cam;
    private GameObject targetPlayer;
    private string thisPlayerName; 

    private float LastInitRequestTime;

    private void Awake() {
        InitComponents();
        RegisterOnMechEnabled();
    }

    private void InitComponents() {
        Combat = transform.root.GetComponent<Combat>();
        textMesh = GetComponentInChildren<TextMesh>();
        bar = FindBar();
    }

    private void RegisterOnMechEnabled() {
        Combat.OnMechEnabled += OnMechEnabled;
    }

    private void OnMechEnabled(bool b) {
        Image[] child_images = GetComponentsInChildren<Image>();
        foreach(Image i in child_images) {
            i.enabled = b;
        }
    }

    private void Start() {
        PhotonView pv = Combat.photonView;

        //Do not show my name & hp bar
        gameObject.SetActive(!pv.isMine || tag=="Drone");
        
        //Init name
        thisPlayerName = (pv.owner == null) ? "Drone" + Random.Range(0, 999) : pv.owner.NickName;
        textMesh.text = thisPlayerName;

        if (GameManager.isTeamMode && (tag == "Drone" || PhotonNetwork.player.GetTeam() != pv.owner.GetTeam()) ) {
            bar.color = Color.white;
            textMesh.color = Color.white;
        } else {
            bar.color = Color.red; //enemy
            textMesh.color = Color.red;
        }
    }

    private Image FindBar() {
        Image[] child_images = GetComponentsInChildren<Image>();
        foreach(Image i in child_images) {
            if(i.type == Image.Type.Filled) {
                return i;
            }
        }

        Debug.LogError("Can't find hp bar");
        return null;
    }

    private void Update() {
        if (cam != null) {
            transform.LookAt(cam.transform);
            Vector3 angle = transform.rotation.eulerAngles;

            transform.rotation = Quaternion.Euler(0, angle.y, angle.z);

            //update scale
            float distance = Vector3.Distance(transform.position, cam.transform.position);
            distance = Mathf.Clamp(distance, 0, 200f);
            transform.localScale = new Vector3(1 + distance / 100 * 1.5f, 1 + distance / 100 * 1.5f, 1);
        } else {
            if (Time.time - LastInitRequestTime > 0.5f) { //some other player's mechs may get built first , so they can't find the target player's cam
                targetPlayer = GameObject.Find(PhotonNetwork.playerName);
                if (targetPlayer != null)
                    cam = targetPlayer.GetComponentInChildren<Camera>();
                LastInitRequestTime = Time.time;
            }
        }

        //update bar value
        bar.fillAmount = (float)Combat.CurrentHP / Combat.MAX_HP;
    }
}